using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadCompanyGenerateOfferDTO
    {
        public long NbfcCompanyId { get; set; }
        public long LeadOfferId { get; set; }
        public string LeadOfferStatus { get; set; }
        public double? CreditLimit { get; set; }
        public string ComapanyName { get; set; } //CompanyName
        public string? LeadOfferErrorMessage { get; set; }
        public long LeadId { get; set; }

        public List<LeadCompanyGenerateOfferSubactivityDTO> SubactivityList { get; set; }
    }

    public class LeadCompanyGenerateOfferSubactivityDTO
    {
        public long LeadNBFCSubActivityId { get; set; }
        public long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public int Sequence { get; set; }
        public string ActivityName { get; set; }
        public string SubActivityName { get; set; }
        public string Status { get; set; }

        public List<LeadCompanyGenerateOfferApiDTO> ApiList { get; set; }
    }
    public class LeadCompanyGenerateOfferApiDTO
    {
        public long? ApiId { get; set; }
        public string Code { get; set; }
        public string? ApiUrl { get; set; }
        public int? Sequence { get; set; }
        public string? ApiStatus { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }



    }

    public class LeadCompanyGenerateOfferDc
    {
        public long NBFCCompanyId { get; set; } //
        public string ActivityName{ get; set; } //

        public string ComapanyName { get; set; }
        public long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public int SubActivitySequence { get; set; }
        public string Code { get; set; }
        public string SubActivityStatus { get; set; }
        public long LeadOfferId { get; set; }
        public long LeadNBFCSubActivityId { get; set; }
        public string LeadOfferStatus { get; set; }
        public double? CreditLimit { get; set; }
        public string? ErrorMessage { get; set; }
        public long? APIId { get; set; }
        public string? ApiCode { get; set; }
        public string? ApiStatus { get; set; }
        public string? APIUrl { get; set; }
        public int? APISequence { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }

    }

    public class LeadActivityOfferStatus
    {
        public bool isOfferGenerated { get; set; }
    }
}
