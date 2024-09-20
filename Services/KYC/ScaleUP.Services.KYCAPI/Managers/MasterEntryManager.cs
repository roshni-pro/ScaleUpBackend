using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Enum;
using ScaleUP.Services.KYCModels.Master;
using System;
using static System.Net.WebRequestMethods;
using KYCMSMEConstants = ScaleUP.Global.Infrastructure.Constants.KYCMSMEConstants;

namespace ScaleUP.Services.KYCAPI.Managers
{
   
    public class MasterEntryManager : BaseManager
    {
        public MasterEntryManager(ApplicationDbContext context) : base(context)
        {
        }

        #region For KYC
        public void EnterPANMaster()
        {
            KYCMaster panMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.PAN && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (panMaster == null)
            {
                panMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.PAN,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(panMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == panMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;
            //DOB
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.DOB, (int)FieldTypeEnum.DateTime, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Age
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.Age, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //DateOfIssue
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.DateOfIssue, (int)FieldTypeEnum.DateTime, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //FatherName
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.FatherName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //IdScanned
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.IdScanned, (int)FieldTypeEnum.Boolean, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Minor
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.Minor, (int)FieldTypeEnum.Boolean, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //NameOnCard
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.NameOnCard, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //PanType
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.PanType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //DocumentId
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCDetailConstants.DocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            _context.SaveChanges();
        }

        public void EnterAadharMaster()
        {
            KYCMaster aadharMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.Aadhar && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (aadharMaster == null)
            {
                aadharMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.Aadhar,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(aadharMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == aadharMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;
            //DOB
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.GeneratedDateTime, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //MaskedAadhaarNumber
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.MaskedAadhaarNumber, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Name
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Name, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //DOB
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.DOB, (int)FieldTypeEnum.DateTime, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Gender
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Gender, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //MobileHash
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.MobileHash, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //EmailHash
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.EmailHash, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //FatherName
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.FatherName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //HouseNumber
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.HouseNumber, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Street
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Street, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Landmark
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Landmark, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Subdistrict
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Subdistrict, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //District
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.District, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //VtcName
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.VtcName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Location
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Location, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //PostOffice
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.PostOffice, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //State
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.State, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Country
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Country, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Pincode
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Pincode, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //CombinedAddress
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.CombinedAddress, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Image
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.Image, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //MaskedVID
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.MaskedVID, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //File
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.File, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //FrontDocumentId
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.FrontDocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            //BackDocumentId
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.BackDocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            //DocumentNumber
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.DocumentNumber, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //LocationId
            UpdateOrAddKYCDetail(detailList, aadharMaster.Id, KYCAadharConstants.LocationId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Location, sequence++);


            _context.SaveChanges();
        }

        public void EnterSelfieMaster()
        {
            KYCMaster panMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.Selfie && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (panMaster == null)
            {
                panMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.Selfie,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(panMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == panMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;
            //DOB
            UpdateOrAddKYCDetail(detailList, panMaster.Id, SelfieConstant.FrontDocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Document, sequence++);


            _context.SaveChanges();
        }

        public void EnterPersonalDetailMaster()
        {
            KYCMaster panMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.PersonalDetail && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (panMaster == null)
            {
                panMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.PersonalDetail,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(panMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == panMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;

            //FirstName
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.FirstName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //MiddleName
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.MiddleName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //LastName
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.LastName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Gender
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.Gender, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //AlternatePhoneNo
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.AlternatePhoneNo, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //EmailId
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.EmailId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //PermanentAddressId
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.PermanentAddressId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Location, sequence++);

            //CurrentAddressId
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.CurrentAddressId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Location, sequence++);

            //MobileNo
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.MobileNo, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //OwnershipType
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.OwnershipType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Marital
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.Marital, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //OwnershipTypeProof
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.OwnershipTypeProof, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //IVRSNumber
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.IVRSNumber, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //OwnershipTypeName
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.OwnershipTypeName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //OwnershipTypeAddress
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.OwnershipTypeAddress, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //OwnershipTypeResponseId
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.OwnershipTypeResponseId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            //ElectricityBillDocumentId
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.ElectricityBillDocumentId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Document, sequence++);
            
            //ElectricityServiceProvider
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.ElectricityServiceProvider, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //ElectricityState
            UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCPersonalDetailConstants.ElectricityState, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);


            _context.SaveChanges();
        }

        public void EnterBuisnessDetailMaster()
        {
            KYCMaster BuisnessDetailMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.BuisnessDetail && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (BuisnessDetailMaster == null)
            {
                BuisnessDetailMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.BuisnessDetail,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(BuisnessDetailMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == BuisnessDetailMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;
            //BusinessName
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BusinessName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //DOI
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.DOI, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //BusGSTNO
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BusGSTNO, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //BusEntityType
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BusEntityType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //BusPan
            //UpdateOrAddKYCDetail(detailList, panMaster.Id, KYCBuisnessDetailConstants.BusPan, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //CurrentAddressId
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.CurrentAddressId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Location, sequence++);

            //BuisnessMonthlySalary
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BuisnessMonthlySalary, (int)FieldTypeEnum.Double, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //IncomeSlab
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.IncomeSlab, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //BuisnessProof
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BuisnessProof, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //BuisnessProofDocId
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BuisnessProofDocId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            //BuisnessDocumentNo
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.BuisnessDocumentNo, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //InquiryAmount
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.InquiryAmount, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Surrogate Type
            UpdateOrAddKYCDetail(detailList, BuisnessDetailMaster.Id, KYCBuisnessDetailConstants.SurrogateType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);


            _context.SaveChanges();
        }

        public void EnterMSMEMaster()
        {
            KYCMaster MSMEMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.MSME && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (MSMEMaster == null)
            {
                MSMEMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.MSME,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(MSMEMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == MSMEMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;

            UpdateOrAddKYCDetail(detailList, MSMEMaster.Id, KYCMSMEConstants.FrontDocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            UpdateOrAddKYCDetail(detailList, MSMEMaster.Id, KYCMSMEConstants.BusinessName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            UpdateOrAddKYCDetail(detailList, MSMEMaster.Id, KYCMSMEConstants.BusinessType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            UpdateOrAddKYCDetail(detailList, MSMEMaster.Id, KYCMSMEConstants.Vintage, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            UpdateOrAddKYCDetail(detailList, MSMEMaster.Id, KYCMSMEConstants.MSMERegNum, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Location, sequence++);

            _context.SaveChanges();
        }

        public void EnterBankStatementCreditLendingMaster()
        {
            KYCMaster CreditLendingMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.BankStatementCreditLending && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (CreditLendingMaster == null)
            {
                CreditLendingMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.BankStatementCreditLending,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(CreditLendingMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == CreditLendingMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;

            //DocumentId
            UpdateOrAddKYCDetail(detailList, CreditLendingMaster.Id, KYCBankStatementCreditLendingConstant.DocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.MultiDocument, sequence++);

            //SarogateDocumentId
            UpdateOrAddKYCDetail(detailList, CreditLendingMaster.Id, KYCBankStatementCreditLendingConstant.SarrogateDocId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.MultiDocument, sequence++);

            ////GSTDocumentId
            //UpdateOrAddKYCDetail(detailList, CreditLendingMaster.Id, KYCBankStatementCreditLendingConstant.GSTDocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.MultiDocument, sequence++);

            ////ITRDocumentId
            //UpdateOrAddKYCDetail(detailList, CreditLendingMaster.Id, KYCBankStatementCreditLendingConstant.ITRDocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.MultiDocument, sequence++);

            //Surrogate Type
            UpdateOrAddKYCDetail(detailList, CreditLendingMaster.Id, KYCBuisnessDetailConstants.SurrogateType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);


            _context.SaveChanges();
        }


        public void EnterDSAPersonalDetailMaster()
        {
            KYCMaster dsaPDMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.DSAPersonalDetail && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (dsaPDMaster == null)
            {
                dsaPDMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.DSAPersonalDetail,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(dsaPDMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == dsaPDMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;

            //GSTRegistrationStatus
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.GSTRegistrationStatus, (int)FieldTypeEnum.Boolean, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //GSTNumber
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.GSTNumber, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //FirmType
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.FirmType, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //BuisnessDocument
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.BuisnessDocument, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);
            
            //DocumentId
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.DocumentId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Document, sequence++);

            //CompanyName
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.CompanyName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);
            
            //FullName
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.FullName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //FatherOrHusbandName
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.FatherOrHusbandName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            ////DOB
            //UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.DOB, (int)FieldTypeEnum.DateTime, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            ////Age
            //UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.Age, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //CurrentAddressId
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.CurrentAddressId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Location, sequence++);

            //AlternatePhoneNo
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.AlternatePhoneNo, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);
            
            //MobileNo
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.MobileNo, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //EmailId
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, ConnectorPersonalDetailConstants.EmailId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //PresentOccupation
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.PresentOccupation, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //NoOfYearsInCurrentEmployment
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.NoOfYearsInCurrentEmployment, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //Qualification
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.Qualification, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //LanguagesKnown
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.LanguagesKnown, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //WorkingWithOther
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.WorkingWithOther, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //ReferneceName
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.ReferneceName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //ReferneceContact
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.ReferneceContact, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //WorkingLocation
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.WorkingLocation, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);
            //PermanentAddressId
            UpdateOrAddKYCDetail(detailList, dsaPDMaster.Id, DSAPersonalDetailConstants.PermanentAddressId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Location, sequence++);


            _context.SaveChanges();
        }

        public void EnterConnectorPersonalDetailMaster()
        {
            KYCMaster connectorPDMaster = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.ConnectorPersonalDetail && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (connectorPDMaster == null)
            {
                connectorPDMaster = new KYCMaster
                {
                    Code = KYCMasterConstants.ConnectorPersonalDetail,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(connectorPDMaster);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == connectorPDMaster.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;

            //FullName
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.FullName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //FatherName
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.FatherName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            ////DOB
            //UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.DOB, (int)FieldTypeEnum.DateTime, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            ////Age
            //UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.Age, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Simple, sequence++);
            
            //CurrentAddressId
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.CurrentAddressId, (int)FieldTypeEnum.Integer, KYCDetailFieldInfoTypeConstants.Location, sequence++);
            
            //AlternatePhoneNo
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.AlternateContactNumber, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);
            
            //MobileNo
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.MobileNo, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //EmailId
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.EmailId, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //PresentEmployment
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.PresentEmployment, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //LanguagesKnown
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.LanguagesKnown, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //WorkingWithOther
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.WorkingWithOther, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //ReferneceName
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.ReferneceName, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //ReferneceContact
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.ReferneceContact, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            //WorkingLocation
            UpdateOrAddKYCDetail(detailList, connectorPDMaster.Id, ConnectorPersonalDetailConstants.WorkingLocation, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);

            _context.SaveChanges();
        }

        public void EnterDSAProfileTypeMaster()
        {
            KYCMaster dsaProfileType = _context.KYCMasters.Where(x => x.Code == KYCMasterConstants.DSAProfileType && x.IsActive && !x.IsDeleted).FirstOrDefault();

            if (dsaProfileType == null)
            {
                dsaProfileType = new KYCMaster
                {
                    Code = KYCMasterConstants.DSAProfileType,
                    IsActive = true,
                    IsDeleted = false,
                    ValidityDays = 365
                };

                _context.KYCMasters.Add(dsaProfileType);
                _context.SaveChanges();
            }

            List<KYCDetail> detailList = null;

            detailList = _context.KYCDetails.Where(x => x.KYCMasterId == dsaProfileType.Id).ToList();

            if (detailList != null)
            {
                foreach (var item in detailList)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }
            if (detailList == null)
            {
                detailList = new List<KYCDetail>();
            }
            int sequence = 1;
            //DSA/Connector
            UpdateOrAddKYCDetail(detailList, dsaProfileType.Id, DSAProfileTypeConstants.DSA, (int)FieldTypeEnum.String, KYCDetailFieldInfoTypeConstants.Simple, sequence++);


            _context.SaveChanges();
        }


        public KYCDetail CreateKYCDetail(long masterId, string fieldName, int FieldType, int FieldInfoType, int sequence)
        {
            return new KYCDetail
            {
                IsDeleted = false,
                IsActive = true,
                Field = fieldName,
                IsPrimaryField = true,
                FieldInfoType = FieldInfoType,
                KYCMasterId = masterId,
                FieldType = FieldType,
                Sequence = sequence
            };
        }

        public void UpdateOrAddKYCDetail(List<KYCDetail> detailList, long masterId, string fieldName, int FieldType, int FieldInfoType, int sequence)
        {
            KYCDetail detail = detailList.Where(x => x.KYCMasterId == masterId && x.Field == fieldName).FirstOrDefault();

            if (detail == null)
            {
                detail = CreateKYCDetail(masterId, fieldName, FieldType, FieldInfoType, sequence);
                _context.KYCDetails.Add(detail);
                _context.SaveChanges(true);
            }
            else
            {
                detail.IsDeleted = false;
                detail.IsActive = true;
                detail.FieldInfoType = FieldInfoType;
                detail.FieldType = FieldType;
                _context.SaveChanges(true);
            }
        }

        #endregion


        #region For ThirdParty Config

        public void EnterKarzaPANValidation()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x=> x.Code == ThirdPartyAPIConfigCodeConstants.KarzaPANValidation && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if(thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.KarzaPANValidation,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "Karza",
                    URL = "https://api.karza.in/v2/pan",
                    Secret = "3uyAQEnalX6ppag",
                    Token = "dsdsds"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "Karza";
                thirdPartyAPIConfigmaster.URL = "https://api.karza.in/v2/pan";
                thirdPartyAPIConfigmaster.Secret = "3uyAQEnalX6ppag";
                thirdPartyAPIConfigmaster.Token = "dsdsds";
            }
            _context.SaveChanges();
        }

        public void EnterKarzaPANOCRInfo()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x => x.Code == ThirdPartyAPIConfigCodeConstants.KarzaPANOCRInfo && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.KarzaPANOCRInfo,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "Karza",
                    URL = "https://api.karza.in/v3/ocr/kyc",
                    Secret = "3uyAQEnalX6ppag",
                    Token = "dsdsds"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "Karza";
                thirdPartyAPIConfigmaster.URL = "https://api.karza.in/v3/ocr/kyc";
                thirdPartyAPIConfigmaster.Secret = "3uyAQEnalX6ppag";
                thirdPartyAPIConfigmaster.Token = "dsdsds";
            }
            _context.SaveChanges();
        }

        public void EnterAppyFlowGSTInfo()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x => x.Code == ThirdPartyAPIConfigCodeConstants.AppyFlowGSTInfo && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.AppyFlowGSTInfo,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "AppyFlow",
                    URL = "https://appyflow.in/api/verifyGST?gstNo=[[GstNo]]&key_secret=",
                    Secret = "HtpyFGVgXZhVEgH6CdwCD9meRCS2",
                    Token = "dsdsds"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "AppyFlow";
                thirdPartyAPIConfigmaster.URL = "https://appyflow.in/api/verifyGST?gstNo=[[GstNo]]&key_secret=";
                thirdPartyAPIConfigmaster.Secret = "HtpyFGVgXZhVEgH6CdwCD9meRCS2";
                thirdPartyAPIConfigmaster.Token = "dsdsds";
            }
            _context.SaveChanges();
        }

        public void EnterKarzaAdhaarVerification()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x => x.Code == ThirdPartyAPIConfigCodeConstants.KarzaAdhaarVerification && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.KarzaAdhaarVerification,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "Karza",
                    URL = "https://api.karza.in/v3/aadhaar-xml/otp",
                    Secret = "3uyAQEnalX6ppag",
                    Token = "931h46xzlv1GT41ktdhy"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "Karza";
                thirdPartyAPIConfigmaster.URL = "https://api.karza.in/v3/aadhaar-xml/otp";
                thirdPartyAPIConfigmaster.Secret = "3uyAQEnalX6ppag";
                thirdPartyAPIConfigmaster.Token = "931h46xzlv1GT41ktdhy";
            }
            _context.SaveChanges();
        }

        public void EnterKarzaAdhaarOtp()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x => x.Code == ThirdPartyAPIConfigCodeConstants.KarzaAdhaarOtp && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.KarzaAdhaarOtp,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "Karza",
                    URL = "https://api.karza.in/v3/aadhaar-xml/file",
                    Secret = "3uyAQEnalX6ppag",
                    Token = "931h46xzlv1GT41ktdhy"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "Karza";
                thirdPartyAPIConfigmaster.URL = "https://api.karza.in/v3/aadhaar-xml/file";
                thirdPartyAPIConfigmaster.Secret = "3uyAQEnalX6ppag";
                thirdPartyAPIConfigmaster.Token = "931h46xzlv1GT41ktdhy";
            }
            _context.SaveChanges();
        }

        public void EnterKarzaPANProfile()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x => x.Code == ThirdPartyAPIConfigCodeConstants.KarzaPANProfile && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.KarzaPANProfile,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "Karza",
                    URL = "https://api.karza.in/v3/pan-profile",
                    Secret = "3uyAQEnalX6ppag",                    
                    Token = "931h46xzlv1GT41ktdhy"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "Karza";
                thirdPartyAPIConfigmaster.URL = "https://api.karza.in/v3/pan-profile";
                thirdPartyAPIConfigmaster.Secret = "3uyAQEnalX6ppag";
                thirdPartyAPIConfigmaster.Token = "931h46xzlv1GT41ktdhy";
            }
            _context.SaveChanges();
        }

        public void KarzaElectricityBillAuthentication()
        {
            ThirdPartyAPIConfig thirdPartyAPIConfigmaster = _context.ThirdPartyAPIConfigs.Where(x => x.Code == ThirdPartyAPIConfigCodeConstants.KarzaElectricityBillAuthentication && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (thirdPartyAPIConfigmaster == null)
            {
                thirdPartyAPIConfigmaster = new ThirdPartyAPIConfig
                {
                    Code = ThirdPartyAPIConfigCodeConstants.KarzaElectricityBillAuthentication,
                    IsActive = true,
                    IsDeleted = false,
                    ProviderName = "Karza",
                    URL = "https://api.karza.in/v2/elec",
                    Secret = "3uyAQEnalX6ppag",
                    Token = "931h46xzlv1GT41ktdhy"
                };
                _context.ThirdPartyAPIConfigs.Add(thirdPartyAPIConfigmaster);
                _context.SaveChanges();
            }
            else
            {
                thirdPartyAPIConfigmaster.ProviderName = "Karza";
                thirdPartyAPIConfigmaster.URL = "https://api.karza.in/v2/elec";
                thirdPartyAPIConfigmaster.Secret = "3uyAQEnalX6ppag";
                thirdPartyAPIConfigmaster.Token = "931h46xzlv1GT41ktdhy";
            }
            _context.SaveChanges();
        }


        #endregion
    }
}
