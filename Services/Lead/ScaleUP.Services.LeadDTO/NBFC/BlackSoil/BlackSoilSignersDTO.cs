using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilSignersDTO
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
        public List<BlackSoilSigner> signers { get; set; }
    }
    public class BlackSoilSigner
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? sign_url { get; set; }
        public string? document_id { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? document_to_be_signed { get; set; }
        public SignerPosition signer_position { get; set; }
        public string? signer_ref_id { get; set; }
        public string? signer_email { get; set; }
        public string? signer_mobile { get; set; }
        public string? signer_name { get; set; }
        public string? sequence { get; set; }
        public string? page_number { get; set; }
        public string? signature_type { get; set; }
        public string? esign_type { get; set; }
        public BlackSoilSignerValidationInputs signer_validation_inputs { get; set; }
        public string? signer_id { get; set; }
        public string? type { get; set; }
        public string? status { get; set; }
        public string? response { get; set; }
        public string? error_code { get; set; }
        public string? error_desc { get; set; }
        public string? legal_desk_status { get; set; }
        public long? failed_attempt_count { get; set; }
        public long? esign { get; set; }
    }
    public class BlackSoilAppearance
    {
        public long? x1 { get; set; }
        public long? x2 { get; set; }
        public long? y1 { get; set; }
        public long? y2 { get; set; }
    }

    public class SignerPosition
    {
        public List<BlackSoilAppearance> appearance { get; set; }
    }

    public class BlackSoilSignerValidationInputs
    {
        public string gender { get; set; }
        public string year_of_birth { get; set; }
        public string name_as_per_aadhaar { get; set; }
    }


}
