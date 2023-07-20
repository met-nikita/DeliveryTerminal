using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using DeliveryTerminal.Models;

namespace DeliveryTerminal.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserRolesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserRolesController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (IdentityUser user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }
        private async Task<List<string>> GetUserRoles(IdentityUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }
        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new ManageUserRolesViewModel();
            model.Email = user.Email;
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesEntry
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Roles.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageUserRolesViewModel model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            ViewBag.UserName = user.UserName;
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Невозможно удалить существующие роли");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Roles.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Невозможно добавить роли");
                return View(model);
            }
            var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            result = await _userManager.ChangeEmailAsync(user, model.Email, emailToken);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Невозможно изменить Email");
                return View(model);
            }
            result = await _userManager.SetUserNameAsync(user, model.Email);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Невозможно изменить username");
                return View(model);
            }
            if (!String.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Невозможно изменить пароль. Пароль должен содержать строчные и заглавные буквы латинского алфавита, цифры и символы.");
                    return View(model);
                }
            }
            await _userManager.UpdateSecurityStampAsync(user);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Create(string? email)
        {
            var model = new CreateUserViewModel();
            if (!String.IsNullOrEmpty(email))
            {
                model.Email = email;
            }
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesEntry
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (!String.IsNullOrEmpty(email) && userRolesViewModel.RoleName == "Client")
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Roles.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if(ModelState.IsValid)
            {
                var defaultUser = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                if (_userManager.Users.All(u => u.Id != defaultUser.Id))
                {
                    var user = await _userManager.FindByEmailAsync(defaultUser.Email);
                    if (user == null)
                    {
                        IdentityResult result = await _userManager.CreateAsync(defaultUser, model.Password);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRolesAsync(defaultUser, model.Roles.Where(r => r.Selected).Select(r=>r.RoleName));
                        }
                        else
                        {
                            ModelState.AddModelError("", result.Errors.FirstOrDefault().Description);
                            return View(model);
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
