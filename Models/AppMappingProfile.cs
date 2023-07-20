using AutoMapper;
using System;

namespace DeliveryTerminal.Models
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<Partner, PartnerViewModel>().ReverseMap();
            CreateMap<Tariff, TariffViewModel>().ReverseMap();
            CreateMap<PartnerAssignment, PartnerAssignmentViewModelEntry>().ReverseMap();
            CreateMap<DeliveryRegistrationViewModel, Registry>();
            CreateMap<RegistryViewModel, Registry>().ReverseMap();
        }
    }
}
