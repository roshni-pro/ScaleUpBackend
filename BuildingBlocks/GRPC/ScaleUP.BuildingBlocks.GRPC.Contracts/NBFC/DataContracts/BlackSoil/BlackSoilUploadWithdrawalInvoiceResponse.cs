using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    

    public class BlackSoilUploadWithdrawalInvoiceFileDetails
    {
    }

    public class BlackSoilUploadWithdrawalInvoiceResponse
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? invoice_date { get; set; }
        public string? due_date { get; set; }
        public string? amount { get; set; }
        public string? disbursed_amount { get; set; }
        public string? invoice_number { get; set; }
        public string? is_verified_by_retailer { get; set; }
        public string? file { get; set; }
        public string? doc_id { get; set; }
        public string issue_comment { get; set; }
        public bool? is_approved_by_distributor { get; set; }
        //public BlackSoilUploadWithdrawalInvoiceFileDetails invoice_file_details { get; set; }
        public int? invoice { get; set; }
    }
}
