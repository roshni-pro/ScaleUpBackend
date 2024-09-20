using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationModels.Master
{
    public class State : BaseAuditableEntity
    {
        [StringLength(200)]
        public required string Name { get; set; }
        [StringLength(20)]

        public required string StateCode { get; set; }
        public required long CountryId { get; set; }
        [ForeignKey("CountryId")]
        public Country Country { get; set; }

        public ICollection<City> Cities { get; }
    }
}
