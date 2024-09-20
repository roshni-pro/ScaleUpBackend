using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public  class BlackSoilCreateEsignDTO
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? activate_widget_token_url { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? reference_id { get; set; }
        public string? docket_id { get; set; }
        public string? document_to_sign { get; set; }
        public string? document_id { get; set; }
        public string? reference_doc_id { get; set; }
        public string? unsigned_doc { get; set; }
        public string? signed_doc { get; set; }
        public string? status { get; set; }
        public string? doc_id { get; set; }
        public string? response { get; set; }
        public string? error_code { get; set; }
        public string? error_desc { get; set; }
        public string? legal_desk_status { get; set; }
        public long? application { get; set; }
        public long? stamp_document { get; set; }
    }
}
