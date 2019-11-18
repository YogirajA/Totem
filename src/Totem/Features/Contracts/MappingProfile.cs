using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Create.Command, Contract>()
                .ForMember(x => x.UpdateInst, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.DisplayOnContractList, opt => opt.Ignore())
                .ForMember(x => x.Type, opt => opt.NullSubstitute(""))
                .ForMember(x => x.Namespace, opt => opt.NullSubstitute(""))
                .ReverseMap();

            CreateMap<Contract, Details.ViewModel>()
                .ForMember(x => x.ContractObject, opt => opt.MapFrom(x => SchemaObject.BuildSchemaDictionary(x.ContractString, NoOp, NoOp)));

            CreateMap<Edit.EditModel, Contract>()
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.Type, opt => opt.NullSubstitute(""))
                .ForMember(x => x.Namespace, opt => opt.NullSubstitute(""))
                .ReverseMap();

            CreateMap<Contract, TestMessage.ViewModel>()
                .ForMember(x => x.ContractId, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.ContractDescription, opt => opt.MapFrom(x => x.Description))
                .ForMember(x => x.MessageErrors, opt => opt.Ignore())
                .ForMember(x => x.TestMessage, opt => opt.Ignore())
                .ForMember(x => x.AllowSubset, opt => opt.Ignore())
                .ForMember(x => x.IsValid, opt => opt.Ignore())
                .ForMember(x => x.WarningMessage, opt => opt.Ignore())
                .ForMember(x => x.ContractObject, opt => opt.MapFrom(x => SchemaObject.BuildSchemaDictionary(x.ContractString, NoOp, NoOp)));

            CreateMap<Contract, Index.ViewModel>()
                .ForMember(x => x.ContractProperties, opt => opt.MapFrom(x => BuildPropertyList(x.ContractString)))
                .ForMember(x => x.ContractObject, opt => opt.MapFrom(x => SchemaObject.BuildSchemaDictionary(x.ContractString, NoOp, NoOp)));

            CreateMap<Contract, History.ViewModel>()
                .ForMember(x => x.ContractProperties, opt => opt.MapFrom(x => BuildPropertyList(x.ContractString)))
                .ForMember(x => x.ContractObject, opt => opt.MapFrom(x => SchemaObject.BuildSchemaDictionary(x.ContractString, NoOp, NoOp)));
        }

        public List<string> BuildPropertyList(string contractString)
        {
            // For this view, we don't care about the body of the properties (only displaying the name),
            // so it's OK if there's an error parsing the references (handle with NoOp)
            var schema = SchemaObject.BuildSchemaDictionary(contractString, NoOp, NoOp);
            var properties = new List<string>();
            if (schema.ContainsKey("Contract"))
            {
                properties = schema["Contract"].Properties.Keys.ToList();
            }
            return properties;
        }

        public static bool NoOp() => true;
    }
}
