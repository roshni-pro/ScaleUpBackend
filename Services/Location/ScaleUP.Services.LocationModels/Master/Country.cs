using Microsoft.Extensions.Hosting;
using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ScaleUP.Services.LocationModels.Master
{
    public class Country : BaseAuditableEntity
    {
        [StringLength(500)]
        public required string Name { get; set; }

        [MaxLength(12)]
        public required string CountryCode { get; set; }

        [MaxLength(3)]
        public required string CurrencyCode { get; set; }

        public ICollection<State> States { get; }
    }
}