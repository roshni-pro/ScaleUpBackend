using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LocationModels.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationModels.Master
{
    public class City : BaseAuditableEntity
    {
        [StringLength(200)]
        public required string Name { get; set; }
        public  string? CityCode { get; set; }

        public required long StateId { get; set; }
        [ForeignKey("StateId")]
        public State State { get; set; }

        public ICollection<Address> Addresses { get; set;}
    }
}
