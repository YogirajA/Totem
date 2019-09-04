using AutoMapper;
using Totem.Models;

namespace Totem.Features.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Contract, TestMessage.ContractDto>();
        }
    }
}
