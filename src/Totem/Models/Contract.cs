using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totem.Models
{
    public class Contract
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string ContractString { get; set; }
        public string VersionNumber { get; set; }
        public string Namespace { get; set; }
        public string Type { get; set; }
        public DateTime UpdateInst { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
    }
}
