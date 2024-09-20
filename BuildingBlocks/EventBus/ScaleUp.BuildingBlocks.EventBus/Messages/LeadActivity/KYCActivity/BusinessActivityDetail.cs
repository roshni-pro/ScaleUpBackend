using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class BusinessActivityDetail
    {
        public long LeadMasterId { get; set; }
        public string BusName { get; set; }
        public string DOI { get; set; }
        public string? BusGSTNO { get; set; }
        public string BusEntityType { get; set; }
        //public string BusPan { get; set; }
        public string BusAddCorrLine1 { get; set; }
        public string BusAddCorrLine2 { get; set; }
        public string BusAddCorrPincode { get; set; }
        public string BusAddCorrCity { get; set; }
        public string BusAddCorrState { get; set; }
        public string BusAddPerLine1 { get; set; }
        public string BusAddPerLine2 { get; set; }
        public string BusAddPerPincode { get; set; }
        public string BusAddPerCity { get; set; }
        public string BusAddPerState { get; set; }

        //extra field
        public int? SequenceNo { get; set; }
        public string UserId { get; set; }
        public long? CurentLocationId { get; set; }
        public long? PermanentLocationId { get; set; }
        public double? BuisnessMonthlySalary { get; set; }
        public string IncomeSlab { get; set; }
        public string BuisnessProof { get; set; }
        public long BuisnessProofDocId { get; set; }
        public string BuisnessDocumentNo { get; set; }
        public double? InquiryAmount { get; set; }

        public string? SurrogateType { get; set; }

        //public string OwnershipType { get; set; }
        //public string? CustomerElectricityNumber { get; set; }
    }
}
