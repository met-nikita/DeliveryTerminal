using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using DeliveryTerminal.Models;

namespace DeliveryTerminal.Data
{
    public class ApplicationDbContext : AuditableIdentityContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<DeliveryTerminal.Models.Partner> Partner { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.PartnerAssignment> PartnerAssignment { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.Region> Region { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.Tariff> Tariff { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.Registry> Registry { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.TransportingCompany> TransportingCompany { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.Return> Return { get; set; } = default!;
        public DbSet<DeliveryTerminal.Models.SupplierReturn> SupplierReturn { get; set; } = default!;
        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartnerAssignment>()
                .HasOne(pt => pt.AssignedPartner)
                .WithMany(p => p.PartnersAssignedTo)
                .HasForeignKey(pt => pt.AssignedPartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PartnerAssignment>()
                .HasOne(pt => pt.Partner)
                .WithMany(t => t.PartnersAssigned)
                .HasForeignKey(pt => pt.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registry>()
                .HasOne(m => m.Receiver)
                .WithMany(t => t.RegistryAsReceiver)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registry>()
                .HasOne(m => m.Sender)
                .WithMany(t => t.RegistryAsSender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Return>()
                .HasOne(m => m.Client)
                .WithMany(t => t.ReturnsAsClient)
                .HasForeignKey(m => m.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Return>()
                .HasOne(m => m.Supplier)
                .WithMany(t => t.ReturnsAsSupplier)
                .HasForeignKey(m => m.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}