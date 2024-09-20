using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class BlackSoilUpdate : BaseAuditableEntity
    {
        public long BusinessId { get; set; }
        [StringLength(300)]
        public string BusinessUpdateUrl { get; set; }

        public long BusinessAddressId { get; set; }
        [StringLength(300)]
        public string BusinessAddressUpdateUrl { get; set; }

        public long PanId { get; set; }
        [StringLength(300)]
        public string PanUpdateUrl { get; set; }

        public long AadhaarId { get; set; }
        [StringLength(300)]
        public string AadhaarUpdateUrl { get; set; }

        public long PersonId { get; set; }
        [StringLength(300)]
        public string PersonUpdateUrl { get; set; }


        public long PersonAddressId { get; set; }
        [StringLength(300)]
        public string PersonAddressUpdateUrl { get; set; }



        public long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }

        public long? ApplicationId { get; set; }
        public long? AgreementDocId { get; set; }
        public long? SanctionDocId { get; set; }
        public long? StampId { get; set; }
        public string? SingingUrl { get; set; }
        public string? ApplicationCode { get; set; }
        public string? BussinessCode { get; set; }
        public long? ESingingId { get; set; }

    }
}
