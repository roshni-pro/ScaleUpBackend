using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;

namespace ScaleUP.Services.KYCAPI.Helpers
{
    public class PersonalDetailHelper
    {
        private readonly ApplicationDbContext _context;
        private ThirdPartyAPIConfigManager thirdPartyAPIConfigManager;
        public PersonalDetailHelper(ApplicationDbContext context)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyAPIConfigManager(_context);
        }

        #region PersonalDetail  

        //public async Task<CommonResponseDc> AddPersonalDetail(AddPersonalDetailDc personalDetail)  //customer post , Business data , Bankdetail
        //{
        //    CommonResponseDc res = new CommonResponseDc();
        //    PersonalDetailDTO personalDetailDTO = new PersonalDetailDTO();
        //    personalDetailDTO.FirstName = personalDetail.FirstName;
        //    personalDetailDTO.LastName = personalDetail.LastName;
        //    personalDetailDTO.FatherName = personalDetail.FatherName; 
        //    personalDetailDTO.FatherLastName = personalDetail.FatherLastName;
        //    personalDetailDTO.DOB = personalDetail.DOB;
        //    personalDetailDTO.Gender = personalDetail.Gender;
        //    personalDetailDTO.AlternatePhoneNo = personalDetail.AlternatePhoneNo;
        //    personalDetailDTO.EmailId = personalDetail.EmailId;
        //    personalDetailDTO.TypeOfAddress = personalDetail.TypeOfAddress; 
        //    personalDetailDTO.ResAddress1 = personalDetail.ResAddress1;
        //    personalDetailDTO.ResAddress2 = personalDetail.ResAddress2;
        //    personalDetailDTO.Pincode = personalDetail.Pincode;
        //    personalDetailDTO.City = personalDetail.City;
        //    personalDetailDTO.State = personalDetail.State;
        //    personalDetailDTO.PermanentAddressLine1 = personalDetail.PermanentAddressLine1;
        //    personalDetailDTO.PermanentAddressLine2 = personalDetail.PermanentAddressLine2;
        //    personalDetailDTO.PermanentPincode = personalDetail.PermanentPincode;
        //    personalDetailDTO.PermanentCity = personalDetail.PermanentCity;
        //    personalDetailDTO.PermanentState = personalDetail.PermanentState;
        //    personalDetailDTO.ResidenceStatus = personalDetail.ResidenceStatus;
        //    personalDetailDTO.ModifiedBy = 1;
        //    personalDetailDTO.ModifiedDate = DateTime.Today;
                

        //            //leaddata.first_name = lead.first_name;
        //            //leaddata.last_name = lead.last_name;
        //            //leaddata.father_fname = lead.father_fname;
        //            //leaddata.father_lname = lead.father_lname;
        //            //leaddata.dob = lead.dob;
        //            //leaddata.gender = lead.gender;
        //            //leaddata.alt_phone = lead.alt_phone;
        //            //leaddata.email_id = lead.email_id;
        //            //leaddata.type_of_addr = "Permanent";
        //            //leaddata.resi_addr_ln1 = lead.resi_addr_ln1;
        //            //leaddata.resi_addr_ln2 = lead.resi_addr_ln2;
        //            //leaddata.pincode = lead.pincode;
        //            //leaddata.city = lead.city;
        //            //leaddata.state = lead.state;
        //            //leaddata.per_addr_ln1 = lead.per_addr_ln1;
        //            //leaddata.per_addr_ln2 = lead.per_addr_ln2;
        //            //leaddata.per_pincode = lead.per_pincode;
        //            //leaddata.per_city = lead.per_city;
        //            //leaddata.per_state = lead.per_state;
        //            //leaddata.residence_status = "Owned";
        //            //leaddata.ModifiedBy = 0;
        //            //leaddata.ModifiedDate = DateTime.Now;
        //            //var leadid = new SqlParameter("@LeadMasterId", leaddata.Id);

        //    //db.Entry(leaddata).State = EntityState.Modified;
        //    //await UpdateLeadCurrentActivity(leaddata.Id, db, lead.SequenceNo);
        //    //if (db.Commit() > 0)
        //    //{
        //    //    res.Status = true;
        //    //    res.Msg = "Data Saved Successfully";
        //    //    LeadActivityProgressesHistory(lead.LeadMasterId, lead.SequenceNo, 0, "", "Personal Details Data Saved Successfully");
        //    //}

        //        return res;
        //    }
        


        #endregion
    }
}
