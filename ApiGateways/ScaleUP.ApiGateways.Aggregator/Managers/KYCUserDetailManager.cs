using Grpc.Core;
using MassTransit.Futures.Contracts;
using Microsoft.EntityFrameworkCore;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using static MassTransit.ValidationResultExtensions;
//using ScaleUP.Services.KYCDTO.Constant;
//using ScaleUP.Services.KYCDTO.Constant;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class KYCUserDetailManager
    {
        private IKycService _iKycService;
        private ILocationService _iLocationService;
        private IMediaService _iMediaService;
        private ILeadService _leadService;
        public KYCUserDetailManager(IKycService iKycService, ILocationService iLocationService, IMediaService mediaService, ILeadService leadService)
        {
            _iKycService = iKycService;
            _iLocationService = iLocationService;
            _iMediaService = mediaService;
            _leadService = leadService;
        }
        public async Task<List<KYCAllInfoResponse>> GetLeadDetails(string Usersid, List<string> kycMasterCodes = null, string productCode="")
        {
            var data = await _iKycService.GetUserLeadInfo(new KYCAllInfoRequest
            {
                UserId = Usersid,
                KycMasterCode = 0,
                ProductCode = productCode
            });

            if (data != null && data.Any() && kycMasterCodes != null)
            {
                data = data.Where(x => kycMasterCodes.Contains(x.KYCMasterCode)).ToList();
            }

            if (data != null && data.Any())
            {
                await GetAllAddress(data);
                await GetAllMediaDetails(data);
                await GetAllMultiMediaDetails(data);

                foreach (var item in data)
                {
                    if ((kycMasterCodes == null || kycMasterCodes.Any(x => x == item.KYCMasterCode)) && item.KycDetailInfoList != null && item.KycDetailInfoList.Any())
                    {
                        foreach (var detailItem in item.KycDetailInfoList)
                        {
                            if (!string.IsNullOrEmpty(detailItem.FieldValue))
                            {
                                switch (detailItem.FieldInfoType)
                                {
                                    case KYCDetailFieldInfoTypeConstants.Location:
                                        //detailItem.CustomerAddressReplyDC = await _iLocationService.GetCompanyAddress(new List<long> { long.Parse(detailItem.FieldValue) });
                                        break;
                                    case KYCDetailFieldInfoTypeConstants.Document:
                                        //detailItem.Document = await _iMediaService.GetMediaDetail(new BuildingBlocks.GRPC.Contracts.Media.DataContracts.DocRequest
                                        //{
                                        //    DocId = long.Parse(detailItem.FieldValue)
                                        //});
                                        break;
                                    case KYCDetailFieldInfoTypeConstants.MultiDocument:
                                        //var list = detailItem.FieldValue.Split(',').ToList();
                                        //var docList = list.Select(x => new BuildingBlocks.GRPC.Contracts.Media.DataContracts.DocRequest
                                        //{
                                        //    DocId = long.Parse((string)x)
                                        //}).ToList();
                                        //List<DocReply> documentList = new List<DocReply>();
                                        //foreach (var docitem in docList)
                                        //{
                                        //    var document = await _iMediaService.GetMediaDetail(docitem);
                                        //    documentList.Add(document);
                                        //}
                                        //detailItem.DocumentList = documentList;
                                        break;

                                }
                            }
                        }

                    }

                }
            }

            if (data != null && data.Any())
            {
                data = data.Where(x => kycMasterCodes == null || kycMasterCodes.Contains(x.KYCMasterCode)).ToList();
            }
            return data;
        }
        public async Task<UserDetailsReply> GetLeadDetailAll(string Usersid, string productCode, List<string> kycMasterCodes = null, bool IsGetBankStatementDetail = true, bool IsGetCreditBureau = true, bool IsGetAgreement = true)
        {

            List<KYCAllInfoResponse> result = await GetLeadDetails(Usersid, kycMasterCodes: kycMasterCodes, productCode: productCode);
            UserDetailsReply userDetailsReply = new UserDetailsReply();

            if (result != null && result.Any())
            {
                var panInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.PAN);
                if (panInfo != null)
                {
                    #region PAN Detail
                    PanDetailsDc panDetailsDc = new PanDetailsDc();
                    userDetailsReply.panDetail = panDetailsDc;
                    panDetailsDc.UniqueId = panInfo.UniqueId;
                    if (panInfo.KycDetailInfoList != null)
                    {
                        var age = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.Age);
                        if (age != null && !string.IsNullOrEmpty(age.FieldValue))
                        {
                            int intAge;
                            bool isSuccess = int.TryParse(age.FieldValue, out intAge);
                            if (isSuccess)
                            {
                                panDetailsDc.Age = intAge;
                            }
                        }

                        var nameOnCard = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.NameOnCard);
                        if (nameOnCard != null && !string.IsNullOrEmpty(nameOnCard.FieldValue))
                        {
                            panDetailsDc.NameOnCard = nameOnCard.FieldValue;
                        }

                        var DOB = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DOB);
                        if (DOB != null && !string.IsNullOrEmpty(DOB.FieldValue))
                        {
                            panDetailsDc.DOB = DateConvertHelper.DateFormatReturn(DOB.FieldValue);
                        }

                        var DateOfIssue = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DateOfIssue);
                        if (DateOfIssue != null && !string.IsNullOrEmpty(DateOfIssue.FieldValue))
                        {
                            panDetailsDc.DateOfIssue = DateConvertHelper.DateFormatReturn(DateOfIssue.FieldValue);
                        }

                        var FatherName = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            panDetailsDc.FatherName = FatherName.FieldValue;
                        }


                        var Minor = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.Minor);
                        if (Minor != null && !string.IsNullOrEmpty(Minor.FieldValue))
                        {
                            bool boolMinor;
                            bool isSuccess = bool.TryParse(Minor.FieldValue, out boolMinor);
                            if (isSuccess)
                            {
                                panDetailsDc.Minor = boolMinor;
                            }

                        }

                        var PanType = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.PanType);
                        if (PanType != null && !string.IsNullOrEmpty(PanType.FieldValue))
                        {
                            panDetailsDc.PanType = PanType.FieldValue;
                        }

                        var DocumentId = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DocumentId);
                        if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        {
                            long docid;
                            bool IsSucess = long.TryParse(DocumentId.FieldValue, out docid);
                            if (IsSucess)
                            {
                                panDetailsDc.DocumentId = docid;
                            }
                            //var pandetail = panInfo.KycDetailInfoList.FirstOrDefault();
                            if (DocumentId.Document != null)
                            {
                                if (DocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(DocumentId.Document.ImagePath))
                                {
                                    panDetailsDc.FrontImageUrl = DocumentId.Document.ImagePath;
                                }
                            }
                        }

                        userDetailsReply.panDetail = panDetailsDc;

                    }
                    #endregion

                }

                var aadharInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.Aadhar);
                if (aadharInfo != null)
                {
                    #region Aadhar Detail
                    AadharDetailsDc aadharDetailsDc = new AadharDetailsDc();
                    userDetailsReply.aadharDetail = aadharDetailsDc;

                    aadharDetailsDc.UniqueId = aadharInfo.UniqueId;

                    if (aadharInfo.KycDetailInfoList != null)
                    {
                        var Name = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Name);
                        if (Name != null && !string.IsNullOrEmpty(Name.FieldValue))
                        {
                            aadharDetailsDc.Name = Name.FieldValue;
                        }

                        var Gender = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Gender);
                        if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                        {
                            aadharDetailsDc.Gender = Gender.FieldValue;
                        }

                        var MobileHash = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.MobileHash);
                        if (MobileHash != null && !string.IsNullOrEmpty(MobileHash.FieldValue))
                        {
                            aadharDetailsDc.MobileHash = MobileHash.FieldValue;
                        }

                        var EmailHash = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.EmailHash);
                        if (EmailHash != null && !string.IsNullOrEmpty(EmailHash.FieldValue))
                        {
                            aadharDetailsDc.EmailHash = EmailHash.FieldValue;
                        }

                        var HouseNumber = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.HouseNumber);
                        if (HouseNumber != null && !string.IsNullOrEmpty(HouseNumber.FieldValue))
                        {
                            aadharDetailsDc.HouseNumber = HouseNumber.FieldValue;
                        }

                        var Street = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Street);
                        if (Street != null && !string.IsNullOrEmpty(Street.FieldValue))
                        {
                            aadharDetailsDc.Street = Street.FieldValue;
                        }

                        var Subdistrict = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Subdistrict);
                        if (Subdistrict != null && !string.IsNullOrEmpty(Subdistrict.FieldValue))
                        {
                            aadharDetailsDc.Subdistrict = Subdistrict.FieldValue;
                        }

                        var State = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.State);
                        if (State != null && !string.IsNullOrEmpty(State.FieldValue))
                        {
                            aadharDetailsDc.State = State.FieldValue;
                        }

                        var Country = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Country);
                        if (Country != null && !string.IsNullOrEmpty(Country.FieldValue))
                        {
                            aadharDetailsDc.Country = Country.FieldValue;
                        }

                        var Pincode = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Pincode);
                        if (Pincode != null && !string.IsNullOrEmpty(Pincode.FieldValue))
                        {
                            int pin;
                            bool IsSuccess = int.TryParse(Pincode.FieldValue, out pin);
                            if (IsSuccess)
                            {
                                aadharDetailsDc.Pincode = pin;
                            }
                        }

                        var CombinedAddress = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.CombinedAddress);
                        if (CombinedAddress != null && !string.IsNullOrEmpty(CombinedAddress.FieldValue))
                        {
                            aadharDetailsDc.CombinedAddress = CombinedAddress.FieldValue;
                        }

                        var MaskedAadhaarNumber = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.MaskedAadhaarNumber);
                        if (MaskedAadhaarNumber != null && !string.IsNullOrEmpty(MaskedAadhaarNumber.FieldValue))
                        {
                            aadharDetailsDc.MaskedAadhaarNumber = MaskedAadhaarNumber.FieldValue;
                        }

                        var FrontDocumentId = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.FrontDocumentId);
                        if (FrontDocumentId != null && !string.IsNullOrEmpty(FrontDocumentId.FieldValue))
                        {
                            //var detail = aadharInfo.KycDetailInfoList.FirstOrDefault();
                            int docid;
                            bool IsSucess = int.TryParse(FrontDocumentId.FieldValue, out docid);
                            if (IsSucess)
                            {
                                aadharDetailsDc.FrontDocumentId = docid;
                            }
                            if (FrontDocumentId.Document != null)
                            {
                                if (FrontDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(FrontDocumentId.Document.ImagePath))
                                {
                                    aadharDetailsDc.FrontImageUrl = FrontDocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var BackDocumentId = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.BackDocumentId);
                        if (BackDocumentId != null && !string.IsNullOrEmpty(BackDocumentId.FieldValue))
                        {
                            //var detail = aadharInfo.KycDetailInfoList.FirstOrDefault();
                            int docid;
                            bool IsSucess = int.TryParse(BackDocumentId.FieldValue, out docid);
                            if (IsSucess)
                            {
                                aadharDetailsDc.BackDocumentId = docid;
                            }
                            if (BackDocumentId.Document != null)
                            {
                                if (BackDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(BackDocumentId.Document.ImagePath))
                                {
                                    aadharDetailsDc.BackImageUrl = BackDocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var DOB = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.DOB);
                        if (DOB != null && !string.IsNullOrEmpty(DOB.FieldValue))
                        {
                            DateTime dateDOB;
                            bool isSuccess = DateTime.TryParse(DOB.FieldValue, out dateDOB);
                            if (isSuccess)
                            {
                                aadharDetailsDc.DOB = dateDOB;
                            }
                        }

                        var GeneratedDateTime = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.GeneratedDateTime);
                        if (GeneratedDateTime != null && !string.IsNullOrEmpty(GeneratedDateTime.FieldValue))
                        {
                            DateTime dateGeneratedDateTime;
                            bool isSuccess = DateTime.TryParse(GeneratedDateTime.FieldValue, out dateGeneratedDateTime);
                            if (isSuccess)
                            {
                                aadharDetailsDc.GeneratedDateTime = dateGeneratedDateTime;
                            }
                        }

                        var LocationId = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.LocationId);
                        if (LocationId != null && !string.IsNullOrEmpty(LocationId.FieldValue))
                        {
                            int LocAdd;
                            bool isSuccess = int.TryParse(LocationId.FieldValue, out LocAdd);
                            if (isSuccess)
                            {
                                aadharDetailsDc.LocationID = LocAdd;
                            }
                            if (LocationId.CustomerAddressReplyDC != null && LocationId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = LocationId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                aadharDetailsDc.LocationAddress = address;
                            }

                        }

                        var FatherName = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            aadharDetailsDc.FatherName = FatherName.FieldValue;
                        }
                        userDetailsReply.aadharDetail = aadharDetailsDc;
                    }


                    #endregion
                }

                var personalInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.PersonalDetail);
                if (personalInfo != null)
                {
                    #region Personal Detail
                    PersonalDetailsDc personalDetailsDc = null;
                    if (personalInfo.KycDetailInfoList != null)
                    {

                        personalDetailsDc = new PersonalDetailsDc();
                        var FirstName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.FirstName);
                        if (FirstName != null && !string.IsNullOrEmpty(FirstName.FieldValue))
                        {
                            personalDetailsDc.FirstName = FirstName.FieldValue;
                        }
                        var MiddleName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.MiddleName);
                        if (MiddleName != null && !string.IsNullOrEmpty(MiddleName.FieldValue))
                        {
                            personalDetailsDc.MiddleName = MiddleName.FieldValue;
                        }

                        var LastName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.LastName);
                        if (LastName != null && !string.IsNullOrEmpty(LastName.FieldValue))
                        {
                            personalDetailsDc.LastName = LastName.FieldValue;
                        }

                        //var FatherName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.FatherName);
                        //if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        //{
                        //    personalDetailsDc.FatherName = FatherName.FieldValue;
                        //}

                        //var FatherLastName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.FatherLastName);
                        //if (FatherLastName != null && !string.IsNullOrEmpty(FatherLastName.FieldValue))
                        //{
                        //    personalDetailsDc.FatherLastName = FatherLastName.FieldValue;
                        //}

                        //var DOB = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.DOB);
                        //if (DOB != null && !string.IsNullOrEmpty(DOB.FieldValue))
                        //{
                        //    DateTime datedob;
                        //    bool isSuccess = DateTime.TryParse(DOB.FieldValue, out datedob);
                        //    if (isSuccess)
                        //    {
                        //        personalDetailsDc.DOB = datedob;
                        //    }
                        //}

                        var Gender = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.Gender);
                        if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                        {
                            personalDetailsDc.Gender = Gender.FieldValue;
                        }

                        var AlternatePhoneNo = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.AlternatePhoneNo);
                        if (AlternatePhoneNo != null && !string.IsNullOrEmpty(AlternatePhoneNo.FieldValue))
                        {
                            personalDetailsDc.AlternatePhoneNo = AlternatePhoneNo.FieldValue;
                        }

                        var EmailId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.EmailId);
                        if (EmailId != null && !string.IsNullOrEmpty(EmailId.FieldValue))
                        {
                            personalDetailsDc.EmailId = EmailId.FieldValue;
                        }

                        var PermanentAddressId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.PermanentAddressId);
                        if (PermanentAddressId != null && !string.IsNullOrEmpty(PermanentAddressId.FieldValue))
                        {
                            int PermanentAdd;
                            bool isSuccess = int.TryParse(PermanentAddressId.FieldValue, out PermanentAdd);
                            if (isSuccess)
                            {
                                personalDetailsDc.PermanentAddressId = PermanentAdd;
                            }
                            if (PermanentAddressId.CustomerAddressReplyDC != null && PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.PermanentAddress = address;
                            }

                        }

                        var CurrentAddressId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.CurrentAddressId);
                        if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        {
                            int intCurrentAddressId;
                            bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                            if (isSuccess)
                            {
                                personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                            }
                            if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.CurrentAddress = address;
                                //personalDetailsDc.PermanentAddressLineOne = address.AddressLineOne;
                                //personalDetailsDc.PermanentAddressLineTwo = address.AddressLineTwo;
                                //personalDetailsDc.PermanentAddressLineThree = address.AddressLineThree;                          
                            }
                        }

                        var MobileNo = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.MobileNo);
                        if (MobileNo != null && !string.IsNullOrEmpty(MobileNo.FieldValue))
                        {
                            personalDetailsDc.MobileNo = MobileNo.FieldValue;
                        }

                        var OwnershipType = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.OwnershipType);
                        if (OwnershipType != null && !string.IsNullOrEmpty(OwnershipType.FieldValue))
                        {
                            personalDetailsDc.OwnershipType = OwnershipType.FieldValue;
                        }

                        var OwnershipTypeProof = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.OwnershipTypeProof);
                        if (OwnershipTypeProof != null && !string.IsNullOrEmpty(OwnershipTypeProof.FieldValue))
                        {
                            personalDetailsDc.OwnershipTypeProof = OwnershipTypeProof.FieldValue;
                        }

                        var Martial = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.Marital);
                        if (Martial != null && !string.IsNullOrEmpty(Martial.FieldValue))
                        {
                            personalDetailsDc.Marital = Martial.FieldValue;
                        }

                        var ElectricityBillDocumentId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.ElectricityBillDocumentId);
                        if (ElectricityBillDocumentId != null && !string.IsNullOrEmpty(ElectricityBillDocumentId.FieldValue))
                        {
                            long longElectricityBillDocumentId;
                            bool IsSucess = long.TryParse(ElectricityBillDocumentId.FieldValue, out longElectricityBillDocumentId);
                            if (IsSucess)
                            {
                                personalDetailsDc.ElectricityBillDocumentId = longElectricityBillDocumentId;
                            }
                            if (ElectricityBillDocumentId.Document != null)
                            {
                                if (ElectricityBillDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(ElectricityBillDocumentId.Document.ImagePath))
                                {
                                    personalDetailsDc.ManualElectricityBillImage = ElectricityBillDocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var IVRSNumber = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.IVRSNumber);
                        if (IVRSNumber != null && !string.IsNullOrEmpty(IVRSNumber.FieldValue))
                        {
                            personalDetailsDc.IVRSNumber = IVRSNumber.FieldValue;
                        }

                        var OwnershipTypeName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.OwnershipTypeName);
                        if (OwnershipTypeName != null && !string.IsNullOrEmpty(OwnershipTypeName.FieldValue))
                        {
                            personalDetailsDc.OwnershipTypeName = OwnershipTypeName.FieldValue;
                        }

                        var OwnershipTypeAddress = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.OwnershipTypeAddress);
                        if (OwnershipTypeAddress != null && !string.IsNullOrEmpty(OwnershipTypeAddress.FieldValue))
                        {
                            personalDetailsDc.OwnershipTypeAddress = OwnershipTypeAddress.FieldValue;
                        }

                        var OwnershipTypeResponseId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.OwnershipTypeResponseId);
                        if (OwnershipTypeResponseId != null && !string.IsNullOrEmpty(OwnershipTypeResponseId.FieldValue))
                        {
                            personalDetailsDc.OwnershipTypeResponseId = OwnershipTypeResponseId.FieldValue;
                        }
                        var ElectricityServiceProvider = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.ElectricityServiceProvider);
                        if (ElectricityServiceProvider != null && !string.IsNullOrEmpty(ElectricityServiceProvider.FieldValue))
                        {
                            personalDetailsDc.ElectricityServiceProvider = ElectricityServiceProvider.FieldValue;
                        }
                        var ElectricityState = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.ElectricityState);
                        if (ElectricityState != null && !string.IsNullOrEmpty(ElectricityState.FieldValue))
                        {
                            personalDetailsDc.ElectricityState = ElectricityState.FieldValue;
                        }
                        userDetailsReply.PersonalDetail = personalDetailsDc;
                    }
                    #endregion
                }

                var BuisnessInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.BuisnessDetail);
                if (BuisnessInfo != null)
                {
                    #region Buisness Detail
                    BuisnessDetailDc buisnessDetailDc = new BuisnessDetailDc();
                    userDetailsReply.BuisnessDetail = buisnessDetailDc;

                    if (BuisnessInfo.KycDetailInfoList != null)
                    {
                        var BusinessName = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BusinessName);
                        if (BusinessName != null && !string.IsNullOrEmpty(BusinessName.FieldValue))
                        {
                            buisnessDetailDc.BusinessName = BusinessName.FieldValue;
                        }

                        var BusGSTNO = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BusGSTNO);
                        if (BusGSTNO != null && !string.IsNullOrEmpty(BusGSTNO.FieldValue))
                        {
                            buisnessDetailDc.BusGSTNO = BusGSTNO.FieldValue;
                        }

                        var DOI = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.DOI);
                        if (DOI != null && !string.IsNullOrEmpty(DOI.FieldValue))
                        {
                            DateTime datedoi;
                            bool isSuccess = DateTime.TryParse(DOI.FieldValue, out datedoi);
                            if (isSuccess)
                            {
                                buisnessDetailDc.DOI = datedoi;
                            }
                        }

                        //var BusPan = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BusPan);
                        //if (BusPan != null && !string.IsNullOrEmpty(BusPan.FieldValue))
                        //{
                        //    buisnessDetailDc.BusPan = BusPan.FieldValue;
                        //}

                        var BusEntityType = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BusEntityType);
                        if (BusEntityType != null && !string.IsNullOrEmpty(BusEntityType.FieldValue))
                        {
                            buisnessDetailDc.BusEntityType = BusEntityType.FieldValue;
                        }

                        //var PermanentAddressId = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.PermanentAddressId);
                        //if (PermanentAddressId != null && !string.IsNullOrEmpty(PermanentAddressId.FieldValue))
                        //{

                        //}
                        var CurrentAddressId = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.CurrentAddressId);
                        if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        {
                            long CurrentAdd;
                            bool isSuccess = long.TryParse(CurrentAddressId.FieldValue, out CurrentAdd);
                            if (isSuccess)
                            {
                                buisnessDetailDc.CurrentAddressId = CurrentAdd;
                            }
                            if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                buisnessDetailDc.CurrentAddress = address;
                            }
                        }
                        var buisnessMonthlySalary = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BuisnessMonthlySalary);
                        if (buisnessMonthlySalary != null && !string.IsNullOrEmpty(buisnessMonthlySalary.FieldValue))
                        {
                            double monthlySalary;
                            bool isSuccess = double.TryParse(buisnessMonthlySalary.FieldValue, out monthlySalary);
                            if (isSuccess)
                            {
                                buisnessDetailDc.BuisnessMonthlySalary = monthlySalary;
                            }
                        }

                        var IncomeSlab = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.IncomeSlab);
                        if (IncomeSlab != null && !string.IsNullOrEmpty(IncomeSlab.FieldValue))
                        {
                            buisnessDetailDc.IncomeSlab = IncomeSlab.FieldValue;
                        }

                        var BuisnessDocumentNo = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BuisnessDocumentNo);
                        if (BuisnessDocumentNo != null && !string.IsNullOrEmpty(BuisnessDocumentNo.FieldValue))
                        {
                            buisnessDetailDc.BuisnessDocumentNo = BuisnessDocumentNo.FieldValue;
                        }

                        //var BuisnessDocumentNo = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BuisnessDocumentNo);
                        //if (BuisnessDocumentNo != null && !string.IsNullOrEmpty(BuisnessDocumentNo.FieldValue))
                        //{
                        //    long BuisnessDocNo;
                        //    bool isSuccess = long.TryParse(BuisnessDocumentNo.FieldValue, out BuisnessDocNo);
                        //    if (isSuccess)
                        //    {
                        //        buisnessDetailDc.BuisnessDocumentNo = BuisnessDocNo;
                        //    }
                        //}

                        var BuisnessProofDocId = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BuisnessProofDocId);
                        if (BuisnessProofDocId != null && !string.IsNullOrEmpty(BuisnessProofDocId.FieldValue))
                        {
                            long BusProofDocId;
                            bool isSuccess = long.TryParse(BuisnessProofDocId.FieldValue, out BusProofDocId);
                            if (isSuccess)
                            {
                                buisnessDetailDc.BuisnessProofDocId = BusProofDocId;
                            }
                            if (BuisnessProofDocId.Document != null)
                            {
                                if (BuisnessProofDocId.Document.ImagePath != null && !string.IsNullOrEmpty(BuisnessProofDocId.Document.ImagePath))
                                {
                                    buisnessDetailDc.BuisnessProofUrl = BuisnessProofDocId.Document.ImagePath;
                                }
                            }
                        }

                        var BuisnessProof = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BuisnessProof);
                        if (BuisnessProof != null && !string.IsNullOrEmpty(BuisnessProof.FieldValue))
                        {
                            buisnessDetailDc.BuisnessProof = BuisnessProof.FieldValue;
                        }

                        var InquiryAmount = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.InquiryAmount);
                        if (InquiryAmount != null && !string.IsNullOrEmpty(InquiryAmount.FieldValue))
                        {
                            double InquiryAmt;
                            bool isSuccess = double.TryParse(InquiryAmount.FieldValue, out InquiryAmt);
                            if (isSuccess)
                            {
                                buisnessDetailDc.InquiryAmount = InquiryAmt;
                            }
                        }

                        var SurrogateType = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.SurrogateType);
                        if (SurrogateType != null && !string.IsNullOrEmpty(SurrogateType.FieldValue))
                        {
                            buisnessDetailDc.SurrogateType = SurrogateType.FieldValue;
                        }


                        var BuisnessPhotoDocId = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBuisnessDetailConstants.BuisnessPhotoDocId);
                        if (BuisnessPhotoDocId != null && !string.IsNullOrEmpty(BuisnessPhotoDocId.FieldValue))
                        {
                            long BuisPhotoDocId;
                            bool isSuccess = long.TryParse(BuisnessPhotoDocId.FieldValue, out BuisPhotoDocId);
                            if (isSuccess)
                            {
                                buisnessDetailDc.BuisnessPhotoDocId = BuisPhotoDocId.ToString();
                            }
                            else
                            {
                                buisnessDetailDc.BuisnessPhotoDocId = BuisnessPhotoDocId.FieldValue;
                            }
                            if (BuisnessPhotoDocId.DocumentList != null && BuisnessPhotoDocId.DocumentList.Count > 0)
                            {
                                buisnessDetailDc.BuisnessPhotoUrl = new List<string>();
                                buisnessDetailDc.BuisnessPhotoUrl = BuisnessPhotoDocId.DocumentList.Select(x => x.ImagePath).ToList();
                                //foreach (var doclist in BuisnessPhotoDocId.DocumentList)
                                //{
                                //    buisnessDetailDc.BuisnessPhotoUrl.Add(doclist.ImagePath);
                                //}
                            }
                        }
                        //var OwnershipType = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.OwnershipType);
                        //if (OwnershipType != null && !string.IsNullOrEmpty(OwnershipType.FieldValue))
                        //{
                        //    buisnessDetailDc.OwnershipType = OwnershipType.FieldValue;
                        //}

                        //var CustomerElectricityNumber = BuisnessInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.CustomerElectricityNumber);
                        //if (CustomerElectricityNumber != null && !string.IsNullOrEmpty(CustomerElectricityNumber.FieldValue))
                        //{
                        //    buisnessDetailDc.CustomerElectricityNumber = CustomerElectricityNumber.FieldValue;
                        //}
                        userDetailsReply.BuisnessDetail = buisnessDetailDc;
                    }
                    #endregion
                }

                var BankInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.BankStatement);
               

                var MSMEInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.MSME);
                if (MSMEInfo != null)
                {
                    #region MSME Detail
                    MSMEDetailDc mSMEDetailDc = new MSMEDetailDc();
                    userDetailsReply.MSMEDetail = mSMEDetailDc;

                    if (MSMEInfo.KycDetailInfoList != null)
                    {
                        var FrontDocumentId = MSMEInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCMSMEConstants.FrontDocumentId);
                        if (FrontDocumentId != null && !string.IsNullOrEmpty(FrontDocumentId.FieldValue))
                        {
                            if (FrontDocumentId.FieldValue != null)
                            {
                                if (FrontDocumentId.Document!=null && FrontDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(FrontDocumentId.Document.ImagePath))
                                {
                                    mSMEDetailDc.MSMECertificateUrl = FrontDocumentId.Document.ImagePath;
                                }
                            }
                            int FrontDocId;
                            bool IsSuccess = int.TryParse(FrontDocumentId.FieldValue, out FrontDocId);
                            if (IsSuccess)
                            {
                                mSMEDetailDc.FrontDocumentId = FrontDocId;
                            }

                        }

                        var BusinessName = MSMEInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCMSMEConstants.BusinessName);
                        if (BusinessName != null && !string.IsNullOrEmpty(BusinessName.FieldValue))
                        {
                            mSMEDetailDc.BusinessName = BusinessName.FieldValue;
                        }

                        var BusinessType = MSMEInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCMSMEConstants.BusinessType);
                        if (BusinessType != null && !string.IsNullOrEmpty(BusinessType.FieldValue))
                        {
                            mSMEDetailDc.BusinessType = BusinessType.FieldValue;
                        }

                        var MSMERegNum = MSMEInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCMSMEConstants.MSMERegNum);
                        if (MSMERegNum != null && !string.IsNullOrEmpty(MSMERegNum.FieldValue))
                        {
                            mSMEDetailDc.MSMERegNum = MSMERegNum.FieldValue;
                        }

                        var Vintage = MSMEInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCMSMEConstants.Vintage);
                        if (Vintage != null && !string.IsNullOrEmpty(Vintage.FieldValue))
                        {
                            int Vin;
                            bool IsSuccess = int.TryParse(Vintage.FieldValue, out Vin);
                            if (IsSuccess)
                            {
                                mSMEDetailDc.Vintage = Vin;
                            }

                        }
                    }
                    #endregion
                }

                var SelfieInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.Selfie);
                if (SelfieInfo != null)
                {
                    #region Selfie Detail
                    SelfieDetailDc selfieDetailDc = new SelfieDetailDc();
                    userDetailsReply.SelfieDetail = selfieDetailDc;

                    if (SelfieInfo.KycDetailInfoList != null)
                    {
                        var FrontDocumentId = SelfieInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == SelfieConstant.FrontDocumentId);
                        if (FrontDocumentId != null && !string.IsNullOrEmpty(FrontDocumentId.FieldValue))
                        {
                            if (FrontDocumentId.Document != null)
                            {
                                if (FrontDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(FrontDocumentId.Document.ImagePath))
                                {
                                    selfieDetailDc.FrontImageUrl = FrontDocumentId.Document.ImagePath;
                                }
                            }
                            int FrontDocId;
                            bool IsSuccess = int.TryParse(FrontDocumentId.FieldValue, out FrontDocId);
                            if (IsSuccess)
                            {
                                selfieDetailDc.FrontDocumentId = FrontDocId;
                            }
                        }
                    }
                    #endregion
                }

                var BankStatementCreditLendingInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.BankStatementCreditLending);
                if (BankStatementCreditLendingInfo != null)
                {
                    #region BankStatementCreditLending Detail
                    BankDetailList bankStatementCreditLendingList = new BankDetailList();
                    bankStatementCreditLendingList.StatementList = new List<BankStatementCreditLendingDc>();
                    userDetailsReply.BankStatementCreditLendingDeail = bankStatementCreditLendingList;

                    if (BankStatementCreditLendingInfo.KycDetailInfoList != null)
                    {
                        var DocumentId = BankStatementCreditLendingInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBankStatementCreditLendingConstant.DocumentId);
                        if (DocumentId != null && DocumentId.DocumentList != null)
                        {
                            bankStatementCreditLendingList.StatementList = DocumentId.DocumentList.Select(x => new BankStatementCreditLendingDc
                            {
                                DocumentId = (int)x.Id,
                                ImageUrl = x.ImagePath
                            }).ToList();
                        }
                        var SarrogateDocId = BankStatementCreditLendingInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBankStatementCreditLendingConstant.SarrogateDocId);
                        if (SarrogateDocId != null && SarrogateDocId.DocumentList != null)
                        {
                            bankStatementCreditLendingList.SarrogateStatementList = SarrogateDocId.DocumentList.Select(x => new BankStatementCreditLendingDc
                            {
                                DocumentId = (int)x.Id,
                                ImageUrl = x.ImagePath
                            }).ToList();
                        }
                        //var GSTDocumentId = BankStatementCreditLendingInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBankStatementCreditLendingConstant.GSTDocumentId);
                        //if (GSTDocumentId != null && GSTDocumentId.DocumentList != null)
                        //{
                        //    bankStatementCreditLendingList.GSTStatementList = GSTDocumentId.DocumentList.Select(x => new BankStatementCreditLendingDc
                        //    {
                        //        DocumentId = (int)x.Id,
                        //        ImageUrl = x.ImagePath
                        //    }).ToList();
                        //}
                        //var ITRDocumentId = BankStatementCreditLendingInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBankStatementCreditLendingConstant.ITRDocumentId);
                        //if (ITRDocumentId != null && ITRDocumentId.DocumentList != null)
                        //{
                        //    bankStatementCreditLendingList.ITRStatementList = ITRDocumentId.DocumentList.Select(x => new BankStatementCreditLendingDc
                        //    {
                        //        DocumentId = (int)x.Id,
                        //        ImageUrl = x.ImagePath
                        //    }).ToList();
                        //}

                        var SurrogateType = BankStatementCreditLendingInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCBankStatementCreditLendingConstant.SurrogateType);
                        if (SurrogateType != null && !string.IsNullOrEmpty(SurrogateType.FieldValue))
                        {
                            bankStatementCreditLendingList.SurrogateType = SurrogateType.FieldValue;
                        }
                    }
                    #endregion
                }

                var DSAprofileInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.DSAProfileType);
                if (DSAprofileInfo != null)
                {
                    #region DSA profile Type
                    if (DSAprofileInfo.KycDetailInfoList != null)
                    {
                        var dsatype = DSAprofileInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAProfileTypeConstants.DSA);
                        DSAprofileInfoDC info = null;
                        if (dsatype != null && !string.IsNullOrEmpty(dsatype.FieldValue))
                        {
                            info = new DSAprofileInfoDC(); 
                            info.DSAType = dsatype.FieldValue;
                        }
                        userDetailsReply.DSAProfileInfo = info;
                    }
                    #endregion
                }

                var ConnectorPersonalDetails = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.ConnectorPersonalDetail);
                if (ConnectorPersonalDetails != null)
                {
                    #region Connector Personal Details
                    ConnectorPersonalDetailDc personalDetailsDc = null;
                    if (ConnectorPersonalDetails.KycDetailInfoList != null)
                    {

                        personalDetailsDc = new ConnectorPersonalDetailDc();
                        var FullName = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.FullName);
                        if (FullName != null && !string.IsNullOrEmpty(FullName.FieldValue))
                        {
                            personalDetailsDc.FullName = FullName.FieldValue;
                        }
                        var FatherName = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            personalDetailsDc.FatherName = FatherName.FieldValue;
                        }

                        var AlternatePhoneNo = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.AlternateContactNumber);
                        if (AlternatePhoneNo != null && !string.IsNullOrEmpty(AlternatePhoneNo.FieldValue))
                        {
                            personalDetailsDc.AlternateContactNo = AlternatePhoneNo.FieldValue;
                        }

                        var EmailId = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.EmailId);
                        if (EmailId != null && !string.IsNullOrEmpty(EmailId.FieldValue))
                        {
                            personalDetailsDc.EmailId = EmailId.FieldValue;
                        }

                        var PresentEmployment = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.PresentEmployment);
                        if (PresentEmployment != null && !string.IsNullOrEmpty(PresentEmployment.FieldValue))
                        {
                            personalDetailsDc.PresentEmployment = PresentEmployment.FieldValue;
                        }
                        var LanguagesKnown = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.LanguagesKnown);
                        if (LanguagesKnown != null && !string.IsNullOrEmpty(LanguagesKnown.FieldValue))
                        {
                            personalDetailsDc.LanguagesKnown = LanguagesKnown.FieldValue;
                        }

                        var WorkingWithOther = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.WorkingWithOther);
                        if (WorkingWithOther != null && !string.IsNullOrEmpty(WorkingWithOther.FieldValue))
                        {
                            personalDetailsDc.WorkingWithOther = WorkingWithOther.FieldValue;
                        }
                        var ReferneceName = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.ReferneceName);
                        if (ReferneceName != null && !string.IsNullOrEmpty(ReferneceName.FieldValue))
                        {
                            personalDetailsDc.ReferenceName = ReferneceName.FieldValue;
                        }
                        var ReferneceContact = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.ReferneceContact);
                        if (ReferneceContact != null && !string.IsNullOrEmpty(ReferneceContact.FieldValue))
                        {
                            personalDetailsDc.ReferneceContact = ReferneceContact.FieldValue;
                        }
                        var ReferneceLocation = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.WorkingLocation);
                        if (ReferneceLocation != null && !string.IsNullOrEmpty(ReferneceLocation.FieldValue))
                        {
                            personalDetailsDc.WorkingLocation = ReferneceLocation.FieldValue;
                        }

                        var MobileNo = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.MobileNo);
                        if (MobileNo != null && !string.IsNullOrEmpty(MobileNo.FieldValue))
                        {
                            personalDetailsDc.MobileNo = MobileNo.FieldValue;
                        }
                        if(aadharInfo!=null && aadharInfo.KycDetailInfoList != null)
                        {
                            var Gender = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Gender);
                            if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                            {
                                personalDetailsDc.Gender = Gender.FieldValue;
                            }
                        }
                        var PermanentAddressId = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.CurrentAddressId);
                        if (PermanentAddressId != null && !string.IsNullOrEmpty(PermanentAddressId.FieldValue))
                        {
                            int PermanentAdd;
                            bool isSuccess = int.TryParse(PermanentAddressId.FieldValue, out PermanentAdd);
                            if (isSuccess)
                            {
                                personalDetailsDc.CurrentAddressId = PermanentAdd;
                            }
                            if (PermanentAddressId.CustomerAddressReplyDC != null && PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.CurrentAddress = address;
                            }

                        }
                        //var CurrentAddressId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.CurrentAddressId);
                        //if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        //{
                        //    int intCurrentAddressId;
                        //    bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                        //    if (isSuccess)
                        //    {
                        //        personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                        //    }
                        //    if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                        //    {
                        //        var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                        //        personalDetailsDc.CurrentAddress = address;
                        //    }
                        //}

                        userDetailsReply.ConnectorPersonalDetail = personalDetailsDc;
                    }
                    #endregion
                }
                var DSAPersonalDetail = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.DSAPersonalDetail);
                if (DSAPersonalDetail != null)
                {
                    #region DSA Personal details

                    DSAPersonalDetailDc personalDetailsDc = null;
                    if (DSAPersonalDetail.KycDetailInfoList != null)
                    {

                        personalDetailsDc = new DSAPersonalDetailDc();
                        var GSTStatus = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.GSTRegistrationStatus);
                        if (GSTStatus != null && !string.IsNullOrEmpty(GSTStatus.FieldValue))
                        {
                            personalDetailsDc.GSTStatus = GSTStatus.FieldValue;
                        }
                        var GSTNumber = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.GSTNumber);
                        if (GSTNumber != null && !string.IsNullOrEmpty(GSTNumber.FieldValue))
                        {
                            personalDetailsDc.GSTNumber = GSTNumber.FieldValue;
                        }
                        var FirmType = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.FirmType);
                        if (FirmType != null && !string.IsNullOrEmpty(FirmType.FieldValue))
                        {
                            personalDetailsDc.FirmType = FirmType.FieldValue;
                        }
                        var PresentOccupation = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.PresentOccupation);
                        if (PresentOccupation != null && !string.IsNullOrEmpty(PresentOccupation.FieldValue))
                        {
                            personalDetailsDc.PresentOccupation = PresentOccupation.FieldValue;
                        }

                        var CompanyName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.CompanyName);
                        if (CompanyName != null && !string.IsNullOrEmpty(CompanyName.FieldValue))
                        {
                            personalDetailsDc.CompanyName = CompanyName.FieldValue;
                        }
                        var FullName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.FullName);
                        if (FullName != null && !string.IsNullOrEmpty(FullName.FieldValue))
                        {
                            personalDetailsDc.FullName = FullName.FieldValue;
                        }
                        var FatherOrHusbandName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.FatherOrHusbandName);
                        if (FatherOrHusbandName != null && !string.IsNullOrEmpty(FatherOrHusbandName.FieldValue))
                        {
                            personalDetailsDc.FatherOrHusbandName = FatherOrHusbandName.FieldValue;
                        }


                        var altNo = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.AlternatePhoneNo);
                        if (altNo != null && !string.IsNullOrEmpty(altNo.FieldValue))
                        {
                            personalDetailsDc.AlternatePhoneNo = altNo.FieldValue;
                        }
                        var EmailId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.EmailId);
                        if (EmailId != null && !string.IsNullOrEmpty(EmailId.FieldValue))
                        {
                            personalDetailsDc.EmailId = EmailId.FieldValue;
                        }
                        var Occupation = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.PresentOccupation);
                        if (Occupation != null && !string.IsNullOrEmpty(Occupation.FieldValue))
                        {
                            personalDetailsDc.Qualification = Occupation.FieldValue;
                        }
                        var years = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.NoOfYearsInCurrentEmployment);
                        if (years != null && !string.IsNullOrEmpty(years.FieldValue))
                        {
                            personalDetailsDc.NoOfYearsInCurrentEmployment = years.FieldValue;
                        }
                        var Qualification = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.Qualification);
                        if (Qualification != null && !string.IsNullOrEmpty(Qualification.FieldValue))
                        {
                            personalDetailsDc.Qualification = Qualification.FieldValue;
                        }
                        var LanguagesKnown = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.LanguagesKnown);
                        if (LanguagesKnown != null && !string.IsNullOrEmpty(LanguagesKnown.FieldValue))
                        {
                            personalDetailsDc.LanguagesKnown = LanguagesKnown.FieldValue;
                        }

                        var WorkingWithOther = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.WorkingWithOther);
                        if (WorkingWithOther != null && !string.IsNullOrEmpty(WorkingWithOther.FieldValue))
                        {
                            personalDetailsDc.WorkingWithOther = WorkingWithOther.FieldValue;
                        }
                        var ReferneceName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.ReferneceName);
                        if (ReferneceName != null && !string.IsNullOrEmpty(ReferneceName.FieldValue))
                        {
                            personalDetailsDc.ReferneceName = ReferneceName.FieldValue;
                        }
                        var ReferneceContact = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.ReferneceContact);
                        if (ReferneceContact != null && !string.IsNullOrEmpty(ReferneceContact.FieldValue))
                        {
                            personalDetailsDc.ReferneceContact = ReferneceContact.FieldValue;
                        }
                        var ReferneceLocation = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.WorkingLocation);
                        if (ReferneceLocation != null && !string.IsNullOrEmpty(ReferneceLocation.FieldValue))
                        {
                            personalDetailsDc.WorkingLocation = ReferneceLocation.FieldValue;
                        }
                        if (aadharInfo != null && aadharInfo.KycDetailInfoList != null)
                        {
                            var Gender = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Gender);
                            if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                            {
                                personalDetailsDc.Gender = Gender.FieldValue;
                            }
                        }
                        //var DocumentId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.DocumentId);
                        //if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        //{
                        //    personalDetailsDc.DocumentId = DocumentId.FieldValue;
                        //}
                        var DocumentId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.DocumentId);
                        if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        {
                            long longDocumentId;
                            bool IsSucess = long.TryParse(DocumentId.FieldValue, out longDocumentId);
                            if (IsSucess)
                            {
                                personalDetailsDc.DocumentId = longDocumentId.ToString();
                            }
                            if (DocumentId.Document != null)
                            {
                                if (DocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(DocumentId.Document.ImagePath))
                                {
                                    personalDetailsDc.BuisnessDocImg = DocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var BuisnessDocument = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.BuisnessDocument);
                        if (BuisnessDocument != null && !string.IsNullOrEmpty(BuisnessDocument.FieldValue))
                        {
                            personalDetailsDc.BuisnessDocument = BuisnessDocument.FieldValue;
                        }

                        var MobileNo = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.MobileNo);
                        if (MobileNo != null && !string.IsNullOrEmpty(MobileNo.FieldValue))
                        {
                            personalDetailsDc.MobileNo = MobileNo.FieldValue;
                        }

                        var PermanentAddressId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.PermanentAddressId);
                        if (PermanentAddressId != null && !string.IsNullOrEmpty(PermanentAddressId.FieldValue))
                        {
                            int PermanentAdd;
                            bool isSuccess = int.TryParse(PermanentAddressId.FieldValue, out PermanentAdd);
                            if (isSuccess)
                            {
                                personalDetailsDc.PermanentAddressId = PermanentAdd;
                            }
                            if (PermanentAddressId.CustomerAddressReplyDC != null && PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.PermanentAddress = address;
                            }

                        }

                        var CurrentAddressId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.CurrentAddressId);
                        if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        {
                            int intCurrentAddressId;
                            bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                            if (isSuccess)
                            {
                                personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                            }
                            if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.CurrentAddress = address;
                                //personalDetailsDc.PermanentAddressLineOne = address.AddressLineOne;
                                //personalDetailsDc.PermanentAddressLineTwo = address.AddressLineTwo;
                                //personalDetailsDc.PermanentAddressLineThree = address.AddressLineThree;                          
                            }
                        }


                        //var CurrentAddressId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.CurrentAddressId);
                        //if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        //{
                        //    int intCurrentAddressId;
                        //    bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                        //    if (isSuccess)
                        //    {
                        //        personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                        //    }
                        //    if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                        //    {
                        //        var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                        //        personalDetailsDc.CurrentAddress = address;
                        //    }
                        //}

                        userDetailsReply.DSAPersonalDetail = personalDetailsDc;
                    }
                    #endregion
                }


            }

            if (IsGetBankStatementDetail == true)
            {
                List<BankStatementDetailDc> bankDataDc = new List<BankStatementDetailDc>();
                GRPCRequest<string> gRPCRequest = new GRPCRequest<string>();
                GRPCRequest<BankStatementRequestDc> request = new GRPCRequest<BankStatementRequestDc>
                {
                    Request = new BankStatementRequestDc
                    {
                        UserId = Usersid,
                        ProductCode = productCode
                    }
                };
                var bankdata = await _leadService.GetBankStatementDetailByLeadId(request);

                if (bankdata != null && bankdata.Response != null)
                {
                    bankDataDc = bankdata.Response;
                    userDetailsReply.BankStatementDetail = bankDataDc;
                }
            }

            if (IsGetCreditBureau == true)
            {
                CreditBureauListDc creditBureauListDc = new CreditBureauListDc();
                GRPCRequest<string> request = new GRPCRequest<string>();
                request.Request = Usersid;
                var creditBureauData = await _leadService.GetCreditBureauDetails(request);

                if (creditBureauData != null)
                {
                    creditBureauListDc = creditBureauData.Response;
                    userDetailsReply.CreditBureauDetails = creditBureauListDc;
                }
            }

            if (IsGetAgreement == true)
            {
                GRPCRequest<string> request = new GRPCRequest<string>();
                var AgreementData = await _leadService.GetAgreementDetail(request);

                AgreementDetailDc agreementDetailDc = new AgreementDetailDc();
                if (AgreementData != null)
                {
                    agreementDetailDc = AgreementData.Response;
                    userDetailsReply.AgreementDetail = agreementDetailDc;
                }
            }
            return userDetailsReply;
        }

        public async Task<dynamic> GetLeadDetails(string Usersid)
        {
            UserDetailsReply userDetailsReply = new UserDetailsReply();
            var result = await _iKycService.GetUserLeadInfo(new KYCAllInfoRequest
            {
                UserId = Usersid
            });
            return result;
        }

        public async Task<LeadPANDTO> GetLeadPANImage(long DocumentId)
        {
            LeadPANDTO leadPANDTO = new LeadPANDTO();
            leadPANDTO.Status = false;
            leadPANDTO.Message = "";
            if (DocumentId.ToString() != null)
            {
                var docReply = await _iMediaService.GetMediaDetail(new BuildingBlocks.GRPC.Contracts.Media.DataContracts.DocRequest
                {
                    DocId = DocumentId
                });
                if (docReply != null && docReply.Status)
                {
                    leadPANDTO.Status = true;
                    leadPANDTO.ImageURL = docReply.ImagePath;
                    leadPANDTO.DocumentId = DocumentId.ToString();
                }
            }
            return leadPANDTO;
        }

        public async Task<ExperianOTPRegistrationRequestDC> GetBureau(ExperianOTPRegistrationInput experianOTPRegistrationInput)
        {
            List<string> MasterCodeList = new List<string>
            {
               KYCMasterConstants.PersonalDetail,
               KYCMasterConstants.PAN,
               KYCMasterConstants.Aadhar
            };

            ExperianOTPRegistrationRequestDC experianOTPRegistrationRequestDC = new ExperianOTPRegistrationRequestDC();
            var GetLeadAllData = await GetLeadDetailAll(experianOTPRegistrationInput.Usersid, experianOTPRegistrationInput.ProductCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);

            if (GetLeadAllData != null && GetLeadAllData.aadharDetail != null && experianOTPRegistrationInput.IsTestingPurpose == false)
            {
                int gender = 0;
                if (GetLeadAllData.aadharDetail != null && !string.IsNullOrEmpty(GetLeadAllData.aadharDetail.Gender))
                {
                    gender = GetLeadAllData.aadharDetail.Gender.ToUpper() == "MALE" ? 2 : 1;

                }
                var stateId = await _leadService.GetExperianStateId(new GRPCRequest<long>
                {
                    Request = GetLeadAllData.aadharDetail.LocationAddress.StateId
                });

                string middleN = "";
                if (string.IsNullOrEmpty(GetLeadAllData.panDetail.NameOnCard))
                {
                    var NameList = GetLeadAllData.panDetail.NameOnCard.Split(',').ToList();
                    if (NameList.Count > 2)
                    {
                        middleN = NameList[1];
                    }
                }
                experianOTPRegistrationRequestDC.LeadId = experianOTPRegistrationInput.LeadId.Value;
                experianOTPRegistrationRequestDC.activityId = experianOTPRegistrationInput.ActivityId.Value;
                experianOTPRegistrationRequestDC.companyId = experianOTPRegistrationInput.CompanyId.Value;
                experianOTPRegistrationRequestDC.subActivityId = experianOTPRegistrationInput.SubActivityId != null ? experianOTPRegistrationInput.SubActivityId.Value : null;
                experianOTPRegistrationRequestDC.aadhaar = GetLeadAllData.aadharDetail == null ? "" : GetLeadAllData.aadharDetail.UniqueId;
                experianOTPRegistrationRequestDC.city = GetLeadAllData.aadharDetail == null ? "" : GetLeadAllData.aadharDetail.LocationAddress.CityName;
                experianOTPRegistrationRequestDC.dateOfBirth = GetLeadAllData.aadharDetail == null ? "" : GetLeadAllData.aadharDetail.DOB.ToString();//chng
                experianOTPRegistrationRequestDC.email = GetLeadAllData.PersonalDetail == null ? "" : GetLeadAllData.PersonalDetail.EmailId;
                experianOTPRegistrationRequestDC.firstName = string.IsNullOrEmpty(GetLeadAllData.panDetail.NameOnCard) ? "" : GetLeadAllData.panDetail.NameOnCard.Split(',').FirstOrDefault();
                experianOTPRegistrationRequestDC.surName = string.IsNullOrEmpty(GetLeadAllData.panDetail.NameOnCard) ? "" : GetLeadAllData.panDetail.NameOnCard.Split(',').LastOrDefault();
                experianOTPRegistrationRequestDC.flatno = GetLeadAllData.aadharDetail == null ? "" : GetLeadAllData.aadharDetail.HouseNumber;
                experianOTPRegistrationRequestDC.gender = GetLeadAllData.aadharDetail == null ? null : gender;
                experianOTPRegistrationRequestDC.middleName = middleN;
                experianOTPRegistrationRequestDC.mobileNo = GetLeadAllData.PersonalDetail == null ? "" : GetLeadAllData.PersonalDetail.AlternatePhoneNo; //Infuture chng
                experianOTPRegistrationRequestDC.pincode = GetLeadAllData.aadharDetail == null ? "" : GetLeadAllData.aadharDetail.LocationAddress.ZipCode.ToString();
                experianOTPRegistrationRequestDC.state = GetLeadAllData.aadharDetail == null ? null : GetLeadAllData.aadharDetail.LocationAddress.StateId;
                experianOTPRegistrationRequestDC.pan = GetLeadAllData.panDetail == null ? "" : GetLeadAllData.panDetail.UniqueId;
                experianOTPRegistrationRequestDC.cityId = GetLeadAllData.aadharDetail == null ? null : GetLeadAllData.aadharDetail.LocationAddress.CityId;

                if (stateId != null && stateId.Response != null)
                {
                    experianOTPRegistrationRequestDC.experianState = stateId.Response.ExperianStateId;
                }

            }
            else if (experianOTPRegistrationInput.IsTestingPurpose == true)
            {
                experianOTPRegistrationRequestDC.LeadId = experianOTPRegistrationInput.LeadId.Value;
                experianOTPRegistrationRequestDC.activityId = experianOTPRegistrationInput.ActivityId.Value;
                experianOTPRegistrationRequestDC.companyId = experianOTPRegistrationInput.CompanyId.Value;
                experianOTPRegistrationRequestDC.subActivityId = experianOTPRegistrationInput.SubActivityId.HasValue ? experianOTPRegistrationInput.SubActivityId.Value : null;
                experianOTPRegistrationRequestDC.aadhaar = "";
                experianOTPRegistrationRequestDC.city = "Wagholi";
                experianOTPRegistrationRequestDC.dateOfBirth = "11-Oct-1990";
                experianOTPRegistrationRequestDC.email = "harry@shopkirana.com";
                experianOTPRegistrationRequestDC.firstName = "Hari";
                experianOTPRegistrationRequestDC.surName = "Shinde";
                experianOTPRegistrationRequestDC.flatno = "D 571";
                experianOTPRegistrationRequestDC.gender = 2;
                experianOTPRegistrationRequestDC.middleName = "";
                experianOTPRegistrationRequestDC.mobileNo = "8319524344";
                experianOTPRegistrationRequestDC.pincode = "412207";
                experianOTPRegistrationRequestDC.state = 27;
                experianOTPRegistrationRequestDC.pan = "BQGPM7296M";
                experianOTPRegistrationRequestDC.experianState = 23;
                experianOTPRegistrationRequestDC.cityId = 1;

            }
            return experianOTPRegistrationRequestDC;
        }

        public async Task<LeadPersonalDetailDTO> GetLeadPersonalDetail(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.PersonalDetail,
                KYCMasterConstants.PAN,
                KYCMasterConstants.Aadhar,
                KYCMasterConstants.BuisnessDetail
            };

            LeadPersonalDetailDTO leadPersonalDetailDTO = null;
            List<string> masterCodes = new List<string>();
            var reply = await GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.PersonalDetail != null)
            {
                leadPersonalDetailDTO = new LeadPersonalDetailDTO
                {
                    AlternatePhoneNo = reply.PersonalDetail.AlternatePhoneNo,
                    City = reply.PersonalDetail.CurrentAddress.CityId,
                    EmailId = reply.PersonalDetail.EmailId,
                    FirstName = reply.PersonalDetail.FirstName,
                    Gender = reply.PersonalDetail.Gender,
                    LastName = reply.PersonalDetail.LastName,
                    PermanentAddressLine1 = reply.PersonalDetail.PermanentAddress.AddressLineOne,
                    PermanentAddressLine2 = reply.PersonalDetail.PermanentAddress.AddressLineTwo,
                    PermanentCity = reply.PersonalDetail.PermanentAddress.CityId,
                    PermanentPincode = reply.PersonalDetail.PermanentAddress.ZipCode,
                    PermanentState = reply.PersonalDetail.PermanentAddress.StateId,
                    Pincode = reply.PersonalDetail.CurrentAddress.ZipCode,
                    ResAddress1 = reply.PersonalDetail.CurrentAddress.AddressLineOne,
                    ResAddress2 = reply.PersonalDetail.CurrentAddress.AddressLineTwo,
                    State = reply.PersonalDetail.PermanentAddress.StateId,
                    MobileNo = reply.PersonalDetail.MobileNo,
                    BusGSTNO = reply.BuisnessDetail != null ? reply.BuisnessDetail.BusGSTNO : "",
                    OwnershipType = reply.PersonalDetail.OwnershipType,
                    Marital = reply.PersonalDetail.Marital,
                    ElectricityBillDocumentId = reply.PersonalDetail.ElectricityBillDocumentId,
                    OwnershipTypeAddress = reply.PersonalDetail.OwnershipTypeAddress,
                    IVRSNumber = reply.PersonalDetail.IVRSNumber,
                    OwnershipTypeName = reply.PersonalDetail.OwnershipTypeName,
                    OwnershipTypeProof = reply.PersonalDetail.OwnershipTypeProof,
                    OwnershipTypeResponseId = reply.PersonalDetail.OwnershipTypeResponseId,
                    FatherName = reply.panDetail != null && !string.IsNullOrEmpty(reply.panDetail.FatherName) ? reply.panDetail.FatherName.Split(' ').FirstOrDefault() : "",
                    FatherLastName = reply.panDetail != null && !string.IsNullOrEmpty(reply.panDetail.FatherName) ? reply.panDetail.FatherName.Split(' ').LastOrDefault() : "",
                    ManulaElectrictyBillImage = reply.PersonalDetail.ManualElectricityBillImage ?? "",
                    MiddleName = reply.PersonalDetail.MiddleName,
                    ElectricityServiceProvider = reply.PersonalDetail.ElectricityServiceProvider,
                    ElectricityState = reply.PersonalDetail.ElectricityState
                };
            }
            else if (reply != null && reply.aadharDetail != null)
            {
                string MiddleName = "";
                var nameParts = reply.aadharDetail.Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //var panNameParts = reply.panDetail.NameOnCard.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 2)
                {
                    MiddleName = nameParts[1];
                }
                //if (nameParts.Length > 0)
                //{
                //    var FirstName = nameParts[0];
                //}
                //if (nameParts.Length > 2)
                //{
                //    var LastName = nameParts[2];
                //}
                leadPersonalDetailDTO = new LeadPersonalDetailDTO
                {
                    AlternatePhoneNo = "",
                    City = null,
                    //DOB = reply.panDetail != null ? reply.panDetail.DOB : null,
                    EmailId = "",
                    FatherLastName = reply.panDetail != null && !string.IsNullOrEmpty(reply.panDetail.FatherName) ? reply.panDetail.FatherName.Split(' ').LastOrDefault() : "",
                    FatherName = reply.panDetail != null && !string.IsNullOrEmpty(reply.panDetail.FatherName) ? reply.panDetail.FatherName.Split(' ').FirstOrDefault() : "",
                    FirstName = !string.IsNullOrEmpty(reply.aadharDetail.Name) ? reply.aadharDetail.Name.Split(' ').FirstOrDefault() : "",
                    Gender = reply.aadharDetail.Gender,
                    LastName = !string.IsNullOrEmpty(reply.aadharDetail.Name) ? reply.aadharDetail.Name.Split(' ').LastOrDefault() : "",
                    MiddleName = MiddleName,
                    PermanentAddressLine1 = reply.aadharDetail.LocationAddress.AddressLineOne,
                    PermanentAddressLine2 = reply.aadharDetail.LocationAddress.AddressLineTwo,
                    PermanentCity = reply.aadharDetail.LocationAddress.CityId,
                    PermanentPincode = reply.aadharDetail.Pincode,
                    Pincode = null,
                    PermanentState = reply.aadharDetail.LocationAddress.StateId,
                    ResAddress1 = "",
                    ResAddress2 = "",
                    State = null,
                    TypeOfAddress = reply.aadharDetail.LocationAddress.AddressTypeId.ToString(),
                    ResidenceStatus = "",
                    Status = true,
                    MobileNo = "",
                    BusGSTNO = reply.BuisnessDetail != null ? reply.BuisnessDetail.BusGSTNO : "",
                    OwnershipType = "",
                    Marital = "",
                    ElectricityBillDocumentId = null,
                    OwnershipTypeAddress = "",
                    IVRSNumber = "",
                    OwnershipTypeName = "",
                    OwnershipTypeProof = "",
                    OwnershipTypeResponseId = "",
                };
            }
            //var reply = await _iKycService.GetKYCPersonalDetail(new BuildingBlocks.GRPC.Contracts.KYC.DataContracts.KYCPersonalDetailRequest
            //{
            //    UserId = UserId
            //});
            //if (reply.Status)
            //{
            //}

            return leadPersonalDetailDTO;
        }

        public async Task<ConnectorPersonalDetailDTO> GetConnectorPersonalDetail(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.PAN,
                KYCMasterConstants.Aadhar,
                KYCMasterConstants.ConnectorPersonalDetail
            };

            ConnectorPersonalDetailDTO leadPersonalDetailDTO = null;
            List<string> masterCodes = new List<string>();
            var reply = await GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.ConnectorPersonalDetail != null)
            {
                GRPCRequest<long> gRPCRequest = new GRPCRequest<long>();
                gRPCRequest.Request = long.Parse(reply.ConnectorPersonalDetail.WorkingLocation);
                var cityData = await _iLocationService.GetCityDetails(gRPCRequest);
                leadPersonalDetailDTO = new ConnectorPersonalDetailDTO
                {
                    FullName = reply.ConnectorPersonalDetail.FullName,
                    FatherName = reply.ConnectorPersonalDetail.FatherName,
                    DOB = reply.panDetail != null ? reply.panDetail.DOB : null,
                    Age = GetAge(reply.panDetail.DOB),
                    Address = reply.aadharDetail.LocationAddress.AddressLineOne + " " + reply.aadharDetail.LocationAddress.AddressLineTwo + " " + reply.aadharDetail.LocationAddress.AddressLineThree,
                    CityId = reply.ConnectorPersonalDetail.CurrentAddress.CityId.ToString(),
                    Pincode = reply.ConnectorPersonalDetail.CurrentAddress.ZipCode.ToString(),
                    StateId = reply.ConnectorPersonalDetail.CurrentAddress.StateId.ToString(),
                    AlternatePhoneNo = reply.ConnectorPersonalDetail.AlternateContactNo,
                    EmailId = reply.ConnectorPersonalDetail.EmailId,
                    LanguagesKnown = reply.ConnectorPersonalDetail.LanguagesKnown,
                    PresentEmployment = reply.ConnectorPersonalDetail.PresentEmployment,
                    ReferenceName = reply.ConnectorPersonalDetail.ReferenceName,
                    ReferneceContact = reply.ConnectorPersonalDetail.ReferneceContact,
                    WorkingLocation = cityData.Response.CityName,
                    WorkingWithOther = reply.ConnectorPersonalDetail.WorkingWithOther,
                    State = reply.ConnectorPersonalDetail.CurrentAddress.StateName,
                    City = reply.ConnectorPersonalDetail.CurrentAddress.CityName,
                    WorkingLocationStateName = cityData.Response.StateName,
                    WorkingLocationCityId = cityData.Response.Id,
                    WorkingLocationStateId = cityData.Response.stateId
                };
            }
            else if (reply != null && reply.aadharDetail != null)
            {
                string MiddleName = "";
                var nameParts = reply.aadharDetail.Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 2)
                {
                    MiddleName = nameParts[1];
                }
                leadPersonalDetailDTO = new ConnectorPersonalDetailDTO
                {
                    FullName = !string.IsNullOrEmpty(reply.aadharDetail.Name) ? reply.aadharDetail.Name.Split(' ').FirstOrDefault() + " " + reply.aadharDetail.Name.Split(' ').LastOrDefault() : "",
                    FatherName = reply.panDetail != null && !string.IsNullOrEmpty(reply.panDetail.FatherName) ? reply.panDetail.FatherName.Trim(): "",
                    DOB = reply.panDetail != null ? reply.panDetail.DOB : null,
                    Age = GetAge(reply.panDetail.DOB),
                    Address = reply.aadharDetail.LocationAddress.AddressLineOne + " " + reply.aadharDetail.LocationAddress.AddressLineTwo + " " + reply.aadharDetail.LocationAddress.AddressLineThree,
                    City = reply.aadharDetail.LocationAddress.CityName,
                    Pincode = reply.aadharDetail.Pincode.ToString(),
                    StateId = reply.aadharDetail.LocationAddress.StateId.ToString(),
                    State = reply.aadharDetail.LocationAddress.StateName,
                    CityId = reply.aadharDetail.LocationAddress.CityId.ToString(),
                };

            }
                return leadPersonalDetailDTO;
        }

        public async Task<DSAPersonalDetailDTO> GetDSAPersonalDetail(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.PAN,
                KYCMasterConstants.Aadhar,
                KYCMasterConstants.DSAPersonalDetail,

            };

            DSAPersonalDetailDTO leadPersonalDetailDTO = null;
            List<string> masterCodes = new List<string>();
            var reply = await GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.DSAPersonalDetail != null)
            {
                GRPCRequest<long> gRPCRequest = new GRPCRequest<long>();
                gRPCRequest.Request = long.Parse(reply.DSAPersonalDetail.WorkingLocation);
                var cityData = await _iLocationService.GetCityDetails(gRPCRequest);
                leadPersonalDetailDTO = new DSAPersonalDetailDTO
                {
                    GSTStatus = reply.DSAPersonalDetail.GSTStatus,
                    GSTNumber = reply.DSAPersonalDetail.GSTNumber,
                    FirmType = reply.DSAPersonalDetail.FirmType,
                    BuisnessDocument = reply.DSAPersonalDetail.BuisnessDocument,
                    DocumentId = reply.DSAPersonalDetail.DocumentId,
                    CompanyName = reply.DSAPersonalDetail.CompanyName,
                    FullName = reply.DSAPersonalDetail.FullName,
                    FatherOrHusbandName = reply.DSAPersonalDetail.FatherOrHusbandName,
                    DOB = reply.panDetail != null ? reply.panDetail.DOB : null,
                    Age = GetAge(reply.panDetail.DOB),
                    Address = reply.DSAPersonalDetail.PermanentAddress != null ? reply.DSAPersonalDetail.PermanentAddress.AddressLineOne + " " + reply.DSAPersonalDetail.PermanentAddress.AddressLineTwo + " " + reply.DSAPersonalDetail.PermanentAddress.AddressLineThree : "",
                    PinCode = reply.DSAPersonalDetail.PermanentAddress != null ? reply.DSAPersonalDetail.PermanentAddress.ZipCode.ToString() : "",
                    City= reply.DSAPersonalDetail.PermanentAddress != null ? reply.DSAPersonalDetail.PermanentAddress.CityName : "",
                    State = reply.DSAPersonalDetail.PermanentAddress != null ? reply.DSAPersonalDetail.PermanentAddress.StateName : "",
                    CompanyAddress = reply.DSAPersonalDetail.CurrentAddress != null ? reply.DSAPersonalDetail.CurrentAddress.AddressLineOne + " " + reply.DSAPersonalDetail.CurrentAddress.AddressLineTwo + " " + reply.DSAPersonalDetail.CurrentAddress.AddressLineThree : "",
                    CompanyPinCode = reply.DSAPersonalDetail.CurrentAddress != null ? reply.DSAPersonalDetail.CurrentAddress.ZipCode.ToString() : "",
                    CompanyCity = reply.DSAPersonalDetail.CurrentAddress != null ? reply.DSAPersonalDetail.CurrentAddress.CityName : "",
                    CompanyState = reply.DSAPersonalDetail.CurrentAddress != null ? reply.DSAPersonalDetail.CurrentAddress.StateName : "",
                    CompanyCityId = reply.DSAPersonalDetail.CurrentAddress != null ? reply.DSAPersonalDetail.CurrentAddress.CityId.ToString() : "",
                    CompanyStateId = reply.DSAPersonalDetail.CurrentAddress != null ? reply.DSAPersonalDetail.CurrentAddress.StateId.ToString() : "",
                    AlternatePhoneNo = reply.DSAPersonalDetail.AlternatePhoneNo,
                    EmailId= reply.DSAPersonalDetail.EmailId,
                    PresentOccupation= reply.DSAPersonalDetail.PresentOccupation,
                    NoOfYearsInCurrentEmployment= reply.DSAPersonalDetail.NoOfYearsInCurrentEmployment,
                    Qualification = reply.DSAPersonalDetail.Qualification,
                    LanguagesKnown = reply.DSAPersonalDetail.LanguagesKnown,
                    WorkingWithOther = reply.DSAPersonalDetail.WorkingWithOther,
                    ReferneceName = reply.DSAPersonalDetail.ReferneceName,
                    ReferneceContact = reply.DSAPersonalDetail.ReferneceContact,
                    WorkingLocation= cityData.Response.CityName,
                    CityId = reply.DSAPersonalDetail.PermanentAddress.CityId.ToString(),
                    StateId = reply.DSAPersonalDetail.PermanentAddress.StateId.ToString(),
                    BuisnessDocImg = reply.DSAPersonalDetail.BuisnessDocImg,
                    WorkingLocationStateName = cityData.Response.StateName,
                    WorkingLocationCityId = cityData.Response.Id,
                    WorkingLocationStateId = cityData.Response.stateId
                };
            }
            else if (reply != null && reply.aadharDetail != null)
            {
                string MiddleName = "";
                var nameParts = reply.aadharDetail.Name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 2)
                {
                    MiddleName = nameParts[1];
                }
                leadPersonalDetailDTO = new DSAPersonalDetailDTO
                {
                    FullName = !string.IsNullOrEmpty(reply.aadharDetail.Name) ? reply.aadharDetail.Name.Split(' ').FirstOrDefault() + " " + reply.aadharDetail.Name.Split(' ').LastOrDefault() : "",
                    FatherOrHusbandName = reply.panDetail != null && !string.IsNullOrEmpty(reply.panDetail.FatherName) ? reply.panDetail.FatherName.Trim() : "",
                    DOB = reply.panDetail != null ? reply.panDetail.DOB : null,
                    Age = GetAge(reply.panDetail.DOB),
                    Address = reply.aadharDetail != null ? reply.aadharDetail.LocationAddress.AddressLineOne + " " + reply.aadharDetail.LocationAddress.AddressLineTwo + " " + reply.aadharDetail.LocationAddress.AddressLineThree : "",
                    PinCode = reply.aadharDetail != null && reply.aadharDetail.Pincode > 0 ? reply.aadharDetail.Pincode.ToString() : "",
                    City = reply.aadharDetail != null ? reply.aadharDetail.LocationAddress.CityName : "",
                    State = reply.aadharDetail != null ? reply.aadharDetail.LocationAddress.StateName : "",
                    CityId = reply.aadharDetail != null ? reply.aadharDetail.LocationAddress.CityId.ToString() : "",
                    StateId = reply.aadharDetail != null ? reply.aadharDetail.LocationAddress.StateId.ToString() : "",
                };
            }

            return leadPersonalDetailDTO;
        }

        //public async Task<LeadPersonalDetailDTO> GetLeadPersonalDetail(string UserId)
        //{
        //    KYCUserDetailManager kYCUserDetailManager = new KYCUserDetailManager(_iKycService, _iLocationService, _iMediaService);
        //    LeadPersonalDetailDTO leadPersonalDetailDTO = null;
        //    var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId);
        //    if (reply != null && reply.PersonalDetail != null)
        //    {
        //        leadPersonalDetailDTO = new LeadPersonalDetailDTO
        //        {
        //            AlternatePhoneNo = reply.PersonalDetail.AlternatePhoneNo,
        //            City = reply.PersonalDetail.CurrentAddress.CityId,
        //            DOB = reply.PersonalDetail.DOB,
        //            EmailId = reply.PersonalDetail.EmailId,
        //            FatherLastName = reply.PersonalDetail.FatherLastName,
        //            FatherName = reply.PersonalDetail.FatherName,
        //            FirstName = reply.PersonalDetail.FirstName,
        //            Gender = reply.PersonalDetail.Gender,
        //            LastName = reply.PersonalDetail.LastName,
        //            PermanentAddressLine1 = reply.PersonalDetail.PermanentAddress.AddressLineOne,
        //            PermanentAddressLine2 = reply.PersonalDetail.PermanentAddress.AddressLineTwo,
        //            PermanentCity = reply.PersonalDetail.PermanentAddress.CityId,
        //            PermanentPincode = reply.PersonalDetail.PermanentAddress.ZipCode,
        //            PermanentState = reply.PersonalDetail.PermanentAddress.StateId,
        //            Pincode = reply.PersonalDetail.CurrentAddress.ZipCode,
        //            ResAddress1 = reply.PersonalDetail.CurrentAddress.AddressLineOne,
        //            ResAddress2 = reply.PersonalDetail.CurrentAddress.AddressLineTwo,
        //            State = reply.PersonalDetail.PermanentAddress.StateId
        //        };
        //    }
        //    else
        //{
        //    leadPersonalDetailDTO = new LeadPersonalDetailDTO();
        //    if (reply != null && reply.aadharDetail != null)
        //    {
        //        leadPersonalDetailDTO.AlternatePhoneNo = "";
        //        //_iLocationService.GetStateByName()

        //    }
        //    if (reply != null && reply.panDetail != null)
        //    {

        //    }
        //}
        //var reply = await _iKycService.GetKYCPersonalDetail(new BuildingBlocks.GRPC.Contracts.KYC.DataContracts.KYCPersonalDetailRequest
        //{
        //    UserId = UserId
        //});
        //if (reply.Status)
        //{
        //}

        //    return leadPersonalDetailDTO;
        //}

        public async Task<GRPCReply<Dictionary<string, List<KYCSpecificDetailResponse>>>> GetKYCSpecificDetail(GRPCRequest<KYCSpecificDetailRequest> request)
        {
            return await _iKycService.GetKYCSpecificDetail(request);
        }
        public async Task<LeadBusinessDetailDTO> GetCustomerDetailUsingGST(string GSTNO)
        {
            LeadBusinessDetailDTO leadBusinessDetailDTO = new LeadBusinessDetailDTO();
            var GSTDetail = await GSTDetailHelper.GetGSTVerify(GSTNO, EnvironmentConstants.GSTURL);
            if (GSTDetail != null && GSTDetail.custverify != null)
            {
                StateReply stateData = new StateReply();
                GRPCReply<CityReply> cityData = new GRPCReply<CityReply>();
                if (GSTDetail.custverify.State != null)
                {
                    try
                    {
                        GSTverifiedRequest gSTverifiedRequest = new GSTverifiedRequest
                        {
                            PlotNo = GSTDetail.custverify.PlotNo,
                            City = GSTDetail.custverify.City,
                            Name = GSTDetail.custverify.Name,
                            Active = GSTDetail.custverify.Active,
                            Citycode = GSTDetail.custverify.Citycode,
                            CreateDate = GSTDetail.custverify.CreateDate,
                            CustomerBusiness = GSTDetail.custverify.CustomerBusiness,
                            Delete = GSTDetail.custverify.Delete,
                            HomeName = GSTDetail.custverify.HomeName,
                            HomeNo = GSTDetail.custverify.HomeNo,
                            LastUpdate = GSTDetail.custverify.LastUpdate,
                            lat = GSTDetail.custverify.lat,
                            lg = GSTDetail.custverify.lg,
                            Message = GSTDetail.custverify.Message,
                            RefNo = GSTDetail.custverify.RefNo,
                            RegisterDate = GSTDetail.custverify.RegisterDate,
                            RequestPath = GSTDetail.custverify.RequestPath,
                            ShippingAddress = GSTDetail.custverify.ShippingAddress,
                            ShopName = GSTDetail.custverify.ShopName,
                            State = GSTDetail.custverify.State,
                            stateId = GSTDetail.custverify.stateId,
                            UpdateDate = GSTDetail.custverify.UpdateDate,
                            Zipcode = GSTDetail.custverify.Zipcode
                        };
                        stateData = await _iLocationService.GetStateByName(gSTverifiedRequest);

                        if (stateData != null)
                        {
                            gSTverifiedRequest.stateId = stateData.stateId;
                        }
                        cityData = await _iLocationService.GetCityByName(gSTverifiedRequest);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //cityData = await _iLocationService.GetCityByName(new CityRequest
                    //{
                    //   cityName = GSTDetail.custverify.State,
                    //   stateId = stateData.stateId
                    //});
                }

                leadBusinessDetailDTO.addressLineOne = string.Format("{0}, {1}, {2}", GSTDetail.custverify.HomeNo, GSTDetail.custverify.HomeName, GSTDetail.custverify.ShippingAddress);
                leadBusinessDetailDTO.addressLineTwo = GSTDetail.custverify.ShippingAddress;
                leadBusinessDetailDTO.busGSTNO = GSTDetail.custverify.RefNo;
                leadBusinessDetailDTO.stateId = stateData.stateId > 0 ? stateData.stateId : 0;
                leadBusinessDetailDTO.doi = DateConvertHelper.DateFormatReturn(GSTDetail.custverify.RegisterDate);
                leadBusinessDetailDTO.businessName = GSTDetail.custverify.ShopName;
                leadBusinessDetailDTO.cityId = stateData.stateId > 0 && cityData.Response != null ? cityData.Response.cityId : 0;
                leadBusinessDetailDTO.zipCode = Convert.ToInt32(GSTDetail.custverify.Zipcode);
            }
            leadBusinessDetailDTO.Status = GSTDetail.Status;
            leadBusinessDetailDTO.Message = GSTDetail.Message;
            return leadBusinessDetailDTO;
        }

        public async Task<GRPCReply<string>> GetLeadName(string UserId,string productCode)
        {
            string UserName = "";
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.PersonalDetail
            };
            List<KYCAllInfoResponse> result = await GetLeadDetails(UserId, kycMasterCodes: MasterCodeList, productCode);
            if (result != null && result.Any())
            {
                var personalInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.PersonalDetail);
                if (personalInfo != null)
                {
                    #region Personal Detail
                    PersonalDetailsDc personalDetailsDc = null;
                    if (personalInfo.KycDetailInfoList != null)
                    {

                        personalDetailsDc = new PersonalDetailsDc();
                        var FirstName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.FirstName);
                        if (FirstName != null && !string.IsNullOrEmpty(FirstName.FieldValue))
                        {
                            personalDetailsDc.FirstName = FirstName.FieldValue;
                        }

                        var MiddleName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.MiddleName);
                        if (MiddleName != null && !string.IsNullOrEmpty(MiddleName.FieldValue))
                        {
                            personalDetailsDc.MiddleName = MiddleName.FieldValue;
                        }

                        var LastName = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.LastName);
                        if (LastName != null && !string.IsNullOrEmpty(LastName.FieldValue))
                        {
                            personalDetailsDc.LastName = LastName.FieldValue;
                        }

                        if (!string.IsNullOrEmpty(personalDetailsDc.MiddleName))
                        { UserName = personalDetailsDc.FirstName + " " + personalDetailsDc.MiddleName + " " + personalDetailsDc.LastName; }
                        else
                        { UserName = personalDetailsDc.FirstName + " " + personalDetailsDc.LastName; }
                        if (!string.IsNullOrEmpty(UserName))
                        {
                            gRPCReply.Response = UserName;
                            gRPCReply.Status = true;
                        }
                    }
                    #endregion
                }
            }
            return gRPCReply;
        }

        private async Task GetAllAddress(List<KYCAllInfoResponse> request)
        {
            List<KYCDetailInfoResponse> detailList = new List<KYCDetailInfoResponse>();

            if (request != null && request.Any())
            {
                foreach (var master in request)
                {
                    if (master.KycDetailInfoList != null && master.KycDetailInfoList.Any())
                    {
                        var tempdetailList = master.KycDetailInfoList.Where(x => x.FieldInfoType == KYCDetailFieldInfoTypeConstants.Location && !string.IsNullOrEmpty(x.FieldValue)).ToList();
                        if (tempdetailList != null && tempdetailList.Any())
                        {
                            detailList.AddRange(tempdetailList);
                        }
                    }
                }

                if (detailList != null && detailList.Any())
                {
                    Dictionary<long, KYCDetailInfoResponse> addressIds = new Dictionary<long, KYCDetailInfoResponse>();
                    foreach (var id in detailList)
                    {
                        long locationId;
                        bool success = long.TryParse(id.FieldValue, out locationId);
                        if (success)
                        {
                            addressIds.Add(locationId, id);
                        }
                    }

                    if (addressIds.Count == 0)
                    {
                        return;
                    }

                    var result = await _iLocationService.GetCompanyAddress(new List<long>(addressIds.Keys));

                    if (result != null && result.Status == true && result.GetAddressDTO != null)
                    {
                        foreach (var address in result.GetAddressDTO)
                        {
                            var val = addressIds.GetValueOrDefault(address.Id);
                            val.CustomerAddressReplyDC = new CompanyAddressReply
                            {
                                GetAddressDTO = new List<GetAddressDTO> { address },
                                Message = "found",
                                Status = true
                            };
                        }

                    }
                }

            }
        }

        private async Task GetAllMediaDetails(List<KYCAllInfoResponse> request)
        {
            List<KYCDetailInfoResponse> detailList = new List<KYCDetailInfoResponse>();

            if (request != null && request.Any())
            {
                foreach (var master in request)
                {
                    if (master.KycDetailInfoList != null && master.KycDetailInfoList.Any())
                    {
                        var tempdetailList = master.KycDetailInfoList.Where(x => x.FieldInfoType == KYCDetailFieldInfoTypeConstants.Document && !string.IsNullOrEmpty(x.FieldValue)).ToList();
                        if (tempdetailList != null && tempdetailList.Any())
                        {
                            detailList.AddRange(tempdetailList);
                        }
                    }
                }

                if (detailList != null && detailList.Any())
                {
                    Dictionary<long, List<KYCDetailInfoResponse>> mediaIds = new Dictionary<long, List<KYCDetailInfoResponse>>();
                    foreach (var id in detailList)
                    {
                        long locationId;
                        bool success = long.TryParse(id.FieldValue, out locationId);
                        if (success)
                        {
                            if (!mediaIds.ContainsKey(locationId))
                            {
                                mediaIds.TryAdd(locationId, new List<KYCDetailInfoResponse> { id });
                            }
                            else
                            {
                                var value = mediaIds[locationId];
                                value.Add(id);
                            }
                            
                        }
                    }

                    if (mediaIds.Count == 0)
                    {
                        return;
                    }
                    var result = await _iMediaService.GetMediaDetails(new MultiDocRequest
                    {
                        DocIdList = new List<long>(mediaIds.Keys)
                    });

                    if (result != null && result.Any())
                    {
                        foreach (var media in result)
                        {
                            var val = mediaIds.GetValueOrDefault(media.Id.Value);
                            foreach (var item in val)
                            {
                                item.Document = media;
                            }
                            
                        }

                    }
                }

            }
        }
        private async Task GetAllMultiMediaDetails(List<KYCAllInfoResponse> request)
        {
            List<KYCDetailInfoResponse> detailList = new List<KYCDetailInfoResponse>();

            if (request != null && request.Any())
            {
                foreach (var master in request)
                {
                    if (master.KycDetailInfoList != null && master.KycDetailInfoList.Any())
                    {
                        var tempdetailList = master.KycDetailInfoList.Where(x => x.FieldInfoType == KYCDetailFieldInfoTypeConstants.MultiDocument && !string.IsNullOrEmpty(x.FieldValue)).ToList();
                        if (tempdetailList != null && tempdetailList.Any())
                        {
                            detailList.AddRange(tempdetailList);
                        }
                    }
                }

                if (detailList != null && detailList.Any())
                {
                    Dictionary<List<long>, KYCDetailInfoResponse> mediaIds = new Dictionary<List<long>, KYCDetailInfoResponse>();
                    foreach (var id in detailList)
                    {
                        List<string> mediaIdList = id.FieldValue.Split(',').ToList();
                        List<long> detalMediaIdList = new List<long>();
                        foreach (var location in mediaIdList)
                        {
                            long locationId;
                            bool success = long.TryParse(location, out locationId);
                            if (success)
                            {
                                detalMediaIdList.Add(locationId);
                            }
                        }
                        if (mediaIdList.Any())
                        {
                            mediaIds.Add(detalMediaIdList, id);
                        }

                    }
                    if (mediaIds.Count == 0)
                    {
                        return;
                    }

                    var result = await _iMediaService.GetMediaDetails(new MultiDocRequest
                    {
                        DocIdList = new List<long>(mediaIds.Keys.SelectMany(x => x))
                    });

                    if (result != null && result.Any())
                    {
                        foreach (var media in mediaIds)
                        {
                            List<long> ids = media.Key;
                            foreach (var id in ids)
                            {
                                var mediaFile = result.Where(x => x.Id == id).FirstOrDefault();

                                if (mediaFile != null)
                                {
                                    if (media.Value.DocumentList == null)
                                    {
                                        media.Value.DocumentList = new List<DocReply>();
                                    }
                                    media.Value.DocumentList.Add(mediaFile);
                                }
                            }

                        }

                    }
                }

            }
        }

        public static Int32 GetAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;

            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

            return (a - b) / 10000;
        }

        public async Task<GRPCReply<bool>> RemoveDSAPersonalDetails(string UserId)
        {
            var result = await _iKycService.RemoveDSAPersonalDetails(new GRPCRequest<string>
            {
                Request = UserId
            });
            return result;
        }

        public async Task<List<KYCAllInfoResponse>> GetAllKycInfoByUserIdsList(List<string> Usersid, List<string> kycMasterCodes = null, string productCode = "")
        {
            var data = await _iKycService.GetAllKycInfoByUserIdsList(new KYCAllInfoByUserIdsRequest
            {
                UserId = Usersid,
                KycMasterCode = 0,
                ProductCode = productCode
            });

            if (data != null && data.Any() && kycMasterCodes != null)
            {
                data = data.Where(x => kycMasterCodes.Contains(x.KYCMasterCode)).ToList();
            }
            if (data != null && data.Any())
            {
                data = data.Where(x => kycMasterCodes == null || kycMasterCodes.Contains(x.KYCMasterCode)).ToList();
            }
            return data;
        }

        public async Task<UserDetailsReply> GetAllLeadsDsaDetails(List<string> Usersids, string productCode, List<string> kycMasterCodes = null)
        {
            List<KYCAllInfoResponse> result = await GetAllKycInfoByUserIdsList(Usersids, kycMasterCodes: kycMasterCodes, productCode: productCode);
            UserDetailsReply userDetailsReply = new UserDetailsReply();

            if (result != null && result.Any())
            {
                var panInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.PAN);
                if (panInfo != null)
                {
                    #region PAN Detail
                    PanDetailsDc panDetailsDc = new PanDetailsDc();
                    userDetailsReply.panDetail = panDetailsDc;
                    panDetailsDc.UniqueId = panInfo.UniqueId;
                    if (panInfo.KycDetailInfoList != null)
                    {
                        var age = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.Age);
                        if (age != null && !string.IsNullOrEmpty(age.FieldValue))
                        {
                            int intAge;
                            bool isSuccess = int.TryParse(age.FieldValue, out intAge);
                            if (isSuccess)
                            {
                                panDetailsDc.Age = intAge;
                            }
                        }

                        var nameOnCard = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.NameOnCard);
                        if (nameOnCard != null && !string.IsNullOrEmpty(nameOnCard.FieldValue))
                        {
                            panDetailsDc.NameOnCard = nameOnCard.FieldValue;
                        }

                        var DOB = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DOB);
                        if (DOB != null && !string.IsNullOrEmpty(DOB.FieldValue))
                        {
                            panDetailsDc.DOB = DateConvertHelper.DateFormatReturn(DOB.FieldValue);
                        }

                        var DateOfIssue = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DateOfIssue);
                        if (DateOfIssue != null && !string.IsNullOrEmpty(DateOfIssue.FieldValue))
                        {
                            panDetailsDc.DateOfIssue = DateConvertHelper.DateFormatReturn(DateOfIssue.FieldValue);
                        }

                        var FatherName = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            panDetailsDc.FatherName = FatherName.FieldValue;
                        }


                        var Minor = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.Minor);
                        if (Minor != null && !string.IsNullOrEmpty(Minor.FieldValue))
                        {
                            bool boolMinor;
                            bool isSuccess = bool.TryParse(Minor.FieldValue, out boolMinor);
                            if (isSuccess)
                            {
                                panDetailsDc.Minor = boolMinor;
                            }

                        }

                        var PanType = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.PanType);
                        if (PanType != null && !string.IsNullOrEmpty(PanType.FieldValue))
                        {
                            panDetailsDc.PanType = PanType.FieldValue;
                        }

                        var DocumentId = panInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCDetailConstants.DocumentId);
                        if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        {
                            long docid;
                            bool IsSucess = long.TryParse(DocumentId.FieldValue, out docid);
                            if (IsSucess)
                            {
                                panDetailsDc.DocumentId = docid;
                            }
                            //var pandetail = panInfo.KycDetailInfoList.FirstOrDefault();
                            if (DocumentId.Document != null)
                            {
                                if (DocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(DocumentId.Document.ImagePath))
                                {
                                    panDetailsDc.FrontImageUrl = DocumentId.Document.ImagePath;
                                }
                            }
                        }

                        userDetailsReply.panDetail = panDetailsDc;

                    }
                    #endregion

                }

                var aadharInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.Aadhar);
                if (aadharInfo != null)
                {
                    #region Aadhar Detail
                    AadharDetailsDc aadharDetailsDc = new AadharDetailsDc();
                    userDetailsReply.aadharDetail = aadharDetailsDc;

                    aadharDetailsDc.UniqueId = aadharInfo.UniqueId;

                    if (aadharInfo.KycDetailInfoList != null)
                    {
                        var Name = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Name);
                        if (Name != null && !string.IsNullOrEmpty(Name.FieldValue))
                        {
                            aadharDetailsDc.Name = Name.FieldValue;
                        }

                        var Gender = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Gender);
                        if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                        {
                            aadharDetailsDc.Gender = Gender.FieldValue;
                        }

                        var MobileHash = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.MobileHash);
                        if (MobileHash != null && !string.IsNullOrEmpty(MobileHash.FieldValue))
                        {
                            aadharDetailsDc.MobileHash = MobileHash.FieldValue;
                        }

                        var EmailHash = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.EmailHash);
                        if (EmailHash != null && !string.IsNullOrEmpty(EmailHash.FieldValue))
                        {
                            aadharDetailsDc.EmailHash = EmailHash.FieldValue;
                        }

                        var HouseNumber = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.HouseNumber);
                        if (HouseNumber != null && !string.IsNullOrEmpty(HouseNumber.FieldValue))
                        {
                            aadharDetailsDc.HouseNumber = HouseNumber.FieldValue;
                        }

                        var Street = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Street);
                        if (Street != null && !string.IsNullOrEmpty(Street.FieldValue))
                        {
                            aadharDetailsDc.Street = Street.FieldValue;
                        }

                        var Subdistrict = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Subdistrict);
                        if (Subdistrict != null && !string.IsNullOrEmpty(Subdistrict.FieldValue))
                        {
                            aadharDetailsDc.Subdistrict = Subdistrict.FieldValue;
                        }

                        var State = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.State);
                        if (State != null && !string.IsNullOrEmpty(State.FieldValue))
                        {
                            aadharDetailsDc.State = State.FieldValue;
                        }

                        var Country = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Country);
                        if (Country != null && !string.IsNullOrEmpty(Country.FieldValue))
                        {
                            aadharDetailsDc.Country = Country.FieldValue;
                        }

                        var Pincode = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Pincode);
                        if (Pincode != null && !string.IsNullOrEmpty(Pincode.FieldValue))
                        {
                            int pin;
                            bool IsSuccess = int.TryParse(Pincode.FieldValue, out pin);
                            if (IsSuccess)
                            {
                                aadharDetailsDc.Pincode = pin;
                            }
                        }

                        var CombinedAddress = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.CombinedAddress);
                        if (CombinedAddress != null && !string.IsNullOrEmpty(CombinedAddress.FieldValue))
                        {
                            aadharDetailsDc.CombinedAddress = CombinedAddress.FieldValue;
                        }

                        var MaskedAadhaarNumber = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.MaskedAadhaarNumber);
                        if (MaskedAadhaarNumber != null && !string.IsNullOrEmpty(MaskedAadhaarNumber.FieldValue))
                        {
                            aadharDetailsDc.MaskedAadhaarNumber = MaskedAadhaarNumber.FieldValue;
                        }

                        var FrontDocumentId = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.FrontDocumentId);
                        if (FrontDocumentId != null && !string.IsNullOrEmpty(FrontDocumentId.FieldValue))
                        {
                            //var detail = aadharInfo.KycDetailInfoList.FirstOrDefault();
                            int docid;
                            bool IsSucess = int.TryParse(FrontDocumentId.FieldValue, out docid);
                            if (IsSucess)
                            {
                                aadharDetailsDc.FrontDocumentId = docid;
                            }
                            if (FrontDocumentId.Document != null)
                            {
                                if (FrontDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(FrontDocumentId.Document.ImagePath))
                                {
                                    aadharDetailsDc.FrontImageUrl = FrontDocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var BackDocumentId = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.BackDocumentId);
                        if (BackDocumentId != null && !string.IsNullOrEmpty(BackDocumentId.FieldValue))
                        {
                            //var detail = aadharInfo.KycDetailInfoList.FirstOrDefault();
                            int docid;
                            bool IsSucess = int.TryParse(BackDocumentId.FieldValue, out docid);
                            if (IsSucess)
                            {
                                aadharDetailsDc.BackDocumentId = docid;
                            }
                            if (BackDocumentId.Document != null)
                            {
                                if (BackDocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(BackDocumentId.Document.ImagePath))
                                {
                                    aadharDetailsDc.BackImageUrl = BackDocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var DOB = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.DOB);
                        if (DOB != null && !string.IsNullOrEmpty(DOB.FieldValue))
                        {
                            DateTime dateDOB;
                            bool isSuccess = DateTime.TryParse(DOB.FieldValue, out dateDOB);
                            if (isSuccess)
                            {
                                aadharDetailsDc.DOB = dateDOB;
                            }
                        }

                        var GeneratedDateTime = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.GeneratedDateTime);
                        if (GeneratedDateTime != null && !string.IsNullOrEmpty(GeneratedDateTime.FieldValue))
                        {
                            DateTime dateGeneratedDateTime;
                            bool isSuccess = DateTime.TryParse(GeneratedDateTime.FieldValue, out dateGeneratedDateTime);
                            if (isSuccess)
                            {
                                aadharDetailsDc.GeneratedDateTime = dateGeneratedDateTime;
                            }
                        }

                        var LocationId = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.LocationId);
                        if (LocationId != null && !string.IsNullOrEmpty(LocationId.FieldValue))
                        {
                            int LocAdd;
                            bool isSuccess = int.TryParse(LocationId.FieldValue, out LocAdd);
                            if (isSuccess)
                            {
                                aadharDetailsDc.LocationID = LocAdd;
                            }
                            if (LocationId.CustomerAddressReplyDC != null && LocationId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = LocationId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                aadharDetailsDc.LocationAddress = address;
                            }

                        }

                        var FatherName = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            aadharDetailsDc.FatherName = FatherName.FieldValue;
                        }
                        userDetailsReply.aadharDetail = aadharDetailsDc;
                    }


                    #endregion
                }

                var DSAprofileInfo = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.DSAProfileType);
                if (DSAprofileInfo != null)
                {
                    #region DSA profile Type
                    if (DSAprofileInfo.KycDetailInfoList != null)
                    {
                        var dsatype = DSAprofileInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAProfileTypeConstants.DSA);
                        DSAprofileInfoDC info = null;
                        if (dsatype != null && !string.IsNullOrEmpty(dsatype.FieldValue))
                        {
                            info = new DSAprofileInfoDC();
                            info.DSAType = dsatype.FieldValue;
                        }
                        userDetailsReply.DSAProfileInfo = info;
                    }
                    #endregion
                }

                var ConnectorPersonalDetails = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.ConnectorPersonalDetail);
                if (ConnectorPersonalDetails != null)
                {
                    #region Connector Personal Details
                    ConnectorPersonalDetailDc personalDetailsDc = null;
                    if (ConnectorPersonalDetails.KycDetailInfoList != null)
                    {

                        personalDetailsDc = new ConnectorPersonalDetailDc();
                        var FullName = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.FullName);
                        if (FullName != null && !string.IsNullOrEmpty(FullName.FieldValue))
                        {
                            personalDetailsDc.FullName = FullName.FieldValue;
                        }
                        var FatherName = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.FatherName);
                        if (FatherName != null && !string.IsNullOrEmpty(FatherName.FieldValue))
                        {
                            personalDetailsDc.FatherName = FatherName.FieldValue;
                        }

                        var AlternatePhoneNo = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.AlternateContactNumber);
                        if (AlternatePhoneNo != null && !string.IsNullOrEmpty(AlternatePhoneNo.FieldValue))
                        {
                            personalDetailsDc.AlternateContactNo = AlternatePhoneNo.FieldValue;
                        }

                        var EmailId = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.EmailId);
                        if (EmailId != null && !string.IsNullOrEmpty(EmailId.FieldValue))
                        {
                            personalDetailsDc.EmailId = EmailId.FieldValue;
                        }

                        var PresentEmployment = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.PresentEmployment);
                        if (PresentEmployment != null && !string.IsNullOrEmpty(PresentEmployment.FieldValue))
                        {
                            personalDetailsDc.PresentEmployment = PresentEmployment.FieldValue;
                        }
                        var LanguagesKnown = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.LanguagesKnown);
                        if (LanguagesKnown != null && !string.IsNullOrEmpty(LanguagesKnown.FieldValue))
                        {
                            personalDetailsDc.LanguagesKnown = LanguagesKnown.FieldValue;
                        }

                        var WorkingWithOther = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.WorkingWithOther);
                        if (WorkingWithOther != null && !string.IsNullOrEmpty(WorkingWithOther.FieldValue))
                        {
                            personalDetailsDc.WorkingWithOther = WorkingWithOther.FieldValue;
                        }
                        var ReferneceName = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.ReferneceName);
                        if (ReferneceName != null && !string.IsNullOrEmpty(ReferneceName.FieldValue))
                        {
                            personalDetailsDc.ReferenceName = ReferneceName.FieldValue;
                        }
                        var ReferneceContact = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.ReferneceContact);
                        if (ReferneceContact != null && !string.IsNullOrEmpty(ReferneceContact.FieldValue))
                        {
                            personalDetailsDc.ReferneceContact = ReferneceContact.FieldValue;
                        }
                        var ReferneceLocation = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.WorkingLocation);
                        if (ReferneceLocation != null && !string.IsNullOrEmpty(ReferneceLocation.FieldValue))
                        {
                            personalDetailsDc.WorkingLocation = ReferneceLocation.FieldValue;
                        }

                        var MobileNo = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.MobileNo);
                        if (MobileNo != null && !string.IsNullOrEmpty(MobileNo.FieldValue))
                        {
                            personalDetailsDc.MobileNo = MobileNo.FieldValue;
                        }
                        if (aadharInfo != null && aadharInfo.KycDetailInfoList != null)
                        {
                            var Gender = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Gender);
                            if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                            {
                                personalDetailsDc.Gender = Gender.FieldValue;
                            }
                        }
                        var PermanentAddressId = ConnectorPersonalDetails.KycDetailInfoList.FirstOrDefault(x => x.FieldName == ConnectorPersonalDetailConstants.CurrentAddressId);
                        if (PermanentAddressId != null && !string.IsNullOrEmpty(PermanentAddressId.FieldValue))
                        {
                            int PermanentAdd;
                            bool isSuccess = int.TryParse(PermanentAddressId.FieldValue, out PermanentAdd);
                            if (isSuccess)
                            {
                                personalDetailsDc.CurrentAddressId = PermanentAdd;
                            }
                            if (PermanentAddressId.CustomerAddressReplyDC != null && PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.CurrentAddress = address;
                            }

                        }
                        //var CurrentAddressId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.CurrentAddressId);
                        //if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        //{
                        //    int intCurrentAddressId;
                        //    bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                        //    if (isSuccess)
                        //    {
                        //        personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                        //    }
                        //    if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                        //    {
                        //        var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                        //        personalDetailsDc.CurrentAddress = address;
                        //    }
                        //}

                        userDetailsReply.ConnectorPersonalDetail = personalDetailsDc;
                    }
                    #endregion
                }
                var DSAPersonalDetail = result.FirstOrDefault(x => x.KYCMasterCode == KYCMasterConstants.DSAPersonalDetail);
                if (DSAPersonalDetail != null)
                {
                    #region DSA Personal details

                    DSAPersonalDetailDc personalDetailsDc = null;
                    if (DSAPersonalDetail.KycDetailInfoList != null)
                    {

                        personalDetailsDc = new DSAPersonalDetailDc();
                        var GSTStatus = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.GSTRegistrationStatus);
                        if (GSTStatus != null && !string.IsNullOrEmpty(GSTStatus.FieldValue))
                        {
                            personalDetailsDc.GSTStatus = GSTStatus.FieldValue;
                        }
                        var GSTNumber = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.GSTNumber);
                        if (GSTNumber != null && !string.IsNullOrEmpty(GSTNumber.FieldValue))
                        {
                            personalDetailsDc.GSTNumber = GSTNumber.FieldValue;
                        }
                        var FirmType = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.FirmType);
                        if (FirmType != null && !string.IsNullOrEmpty(FirmType.FieldValue))
                        {
                            personalDetailsDc.FirmType = FirmType.FieldValue;
                        }
                        var PresentOccupation = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.PresentOccupation);
                        if (PresentOccupation != null && !string.IsNullOrEmpty(PresentOccupation.FieldValue))
                        {
                            personalDetailsDc.PresentOccupation = PresentOccupation.FieldValue;
                        }

                        var CompanyName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.CompanyName);
                        if (CompanyName != null && !string.IsNullOrEmpty(CompanyName.FieldValue))
                        {
                            personalDetailsDc.CompanyName = CompanyName.FieldValue;
                        }
                        var FullName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.FullName);
                        if (FullName != null && !string.IsNullOrEmpty(FullName.FieldValue))
                        {
                            personalDetailsDc.FullName = FullName.FieldValue;
                        }
                        var FatherOrHusbandName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.FatherOrHusbandName);
                        if (FatherOrHusbandName != null && !string.IsNullOrEmpty(FatherOrHusbandName.FieldValue))
                        {
                            personalDetailsDc.FatherOrHusbandName = FatherOrHusbandName.FieldValue;
                        }


                        var altNo = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.AlternatePhoneNo);
                        if (altNo != null && !string.IsNullOrEmpty(altNo.FieldValue))
                        {
                            personalDetailsDc.AlternatePhoneNo = altNo.FieldValue;
                        }
                        var EmailId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.EmailId);
                        if (EmailId != null && !string.IsNullOrEmpty(EmailId.FieldValue))
                        {
                            personalDetailsDc.EmailId = EmailId.FieldValue;
                        }
                        var Occupation = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.PresentOccupation);
                        if (Occupation != null && !string.IsNullOrEmpty(Occupation.FieldValue))
                        {
                            personalDetailsDc.Qualification = Occupation.FieldValue;
                        }
                        var years = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.NoOfYearsInCurrentEmployment);
                        if (years != null && !string.IsNullOrEmpty(years.FieldValue))
                        {
                            personalDetailsDc.NoOfYearsInCurrentEmployment = years.FieldValue;
                        }
                        var Qualification = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.Qualification);
                        if (Qualification != null && !string.IsNullOrEmpty(Qualification.FieldValue))
                        {
                            personalDetailsDc.Qualification = Qualification.FieldValue;
                        }
                        var LanguagesKnown = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.LanguagesKnown);
                        if (LanguagesKnown != null && !string.IsNullOrEmpty(LanguagesKnown.FieldValue))
                        {
                            personalDetailsDc.LanguagesKnown = LanguagesKnown.FieldValue;
                        }

                        var WorkingWithOther = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.WorkingWithOther);
                        if (WorkingWithOther != null && !string.IsNullOrEmpty(WorkingWithOther.FieldValue))
                        {
                            personalDetailsDc.WorkingWithOther = WorkingWithOther.FieldValue;
                        }
                        var ReferneceName = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.ReferneceName);
                        if (ReferneceName != null && !string.IsNullOrEmpty(ReferneceName.FieldValue))
                        {
                            personalDetailsDc.ReferneceName = ReferneceName.FieldValue;
                        }
                        var ReferneceContact = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.ReferneceContact);
                        if (ReferneceContact != null && !string.IsNullOrEmpty(ReferneceContact.FieldValue))
                        {
                            personalDetailsDc.ReferneceContact = ReferneceContact.FieldValue;
                        }
                        var ReferneceLocation = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.WorkingLocation);
                        if (ReferneceLocation != null && !string.IsNullOrEmpty(ReferneceLocation.FieldValue))
                        {
                            personalDetailsDc.WorkingLocation = ReferneceLocation.FieldValue;
                        }
                        if (aadharInfo != null && aadharInfo.KycDetailInfoList != null)
                        {
                            var Gender = aadharInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCAadharConstants.Gender);
                            if (Gender != null && !string.IsNullOrEmpty(Gender.FieldValue))
                            {
                                personalDetailsDc.Gender = Gender.FieldValue;
                            }
                        }
                        //var DocumentId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.DocumentId);
                        //if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        //{
                        //    personalDetailsDc.DocumentId = DocumentId.FieldValue;
                        //}
                        var DocumentId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.DocumentId);
                        if (DocumentId != null && !string.IsNullOrEmpty(DocumentId.FieldValue))
                        {
                            long longDocumentId;
                            bool IsSucess = long.TryParse(DocumentId.FieldValue, out longDocumentId);
                            if (IsSucess)
                            {
                                personalDetailsDc.DocumentId = longDocumentId.ToString();
                            }
                            if (DocumentId.Document != null)
                            {
                                if (DocumentId.Document.ImagePath != null && !string.IsNullOrEmpty(DocumentId.Document.ImagePath))
                                {
                                    personalDetailsDc.BuisnessDocImg = DocumentId.Document.ImagePath;
                                }
                            }
                        }

                        var BuisnessDocument = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.BuisnessDocument);
                        if (BuisnessDocument != null && !string.IsNullOrEmpty(BuisnessDocument.FieldValue))
                        {
                            personalDetailsDc.BuisnessDocument = BuisnessDocument.FieldValue;
                        }

                        var MobileNo = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.MobileNo);
                        if (MobileNo != null && !string.IsNullOrEmpty(MobileNo.FieldValue))
                        {
                            personalDetailsDc.MobileNo = MobileNo.FieldValue;
                        }

                        var PermanentAddressId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.PermanentAddressId);
                        if (PermanentAddressId != null && !string.IsNullOrEmpty(PermanentAddressId.FieldValue))
                        {
                            int PermanentAdd;
                            bool isSuccess = int.TryParse(PermanentAddressId.FieldValue, out PermanentAdd);
                            if (isSuccess)
                            {
                                personalDetailsDc.PermanentAddressId = PermanentAdd;
                            }
                            if (PermanentAddressId.CustomerAddressReplyDC != null && PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = PermanentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.PermanentAddress = address;
                            }

                        }

                        var CurrentAddressId = DSAPersonalDetail.KycDetailInfoList.FirstOrDefault(x => x.FieldName == DSAPersonalDetailConstants.CurrentAddressId);
                        if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        {
                            int intCurrentAddressId;
                            bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                            if (isSuccess)
                            {
                                personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                            }
                            if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                            {
                                var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                                personalDetailsDc.CurrentAddress = address;
                                //personalDetailsDc.PermanentAddressLineOne = address.AddressLineOne;
                                //personalDetailsDc.PermanentAddressLineTwo = address.AddressLineTwo;
                                //personalDetailsDc.PermanentAddressLineThree = address.AddressLineThree;                          
                            }
                        }


                        //var CurrentAddressId = personalInfo.KycDetailInfoList.FirstOrDefault(x => x.FieldName == KYCPersonalDetailConstants.CurrentAddressId);
                        //if (CurrentAddressId != null && !string.IsNullOrEmpty(CurrentAddressId.FieldValue))
                        //{
                        //    int intCurrentAddressId;
                        //    bool isSuccess = int.TryParse(CurrentAddressId.FieldValue, out intCurrentAddressId);
                        //    if (isSuccess)
                        //    {
                        //        personalDetailsDc.CurrentAddressId = intCurrentAddressId;
                        //    }
                        //    if (CurrentAddressId.CustomerAddressReplyDC != null && CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO != null)
                        //    {
                        //        var address = CurrentAddressId.CustomerAddressReplyDC.GetAddressDTO.FirstOrDefault();
                        //        personalDetailsDc.CurrentAddress = address;
                        //    }
                        //}

                        userDetailsReply.DSAPersonalDetail = personalDetailsDc;
                    }
                    #endregion
                }
            }
            return userDetailsReply;
        }

        public async Task<GRPCReply<bool>> GetGSTExistinKYCForDSA(DSAGSTExistRequest dSAGSTExistRequest, List<string> kycMasterCodes = null, bool IsGetBankStatementDetail = true, bool IsGetCreditBureau = true, bool IsGetAgreement = true)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            gRPCReply = await _iKycService.GetDSAGSTExist(new GRPCRequest<DSAGSTExistRequest> { Request = dSAGSTExistRequest }); 
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> RemoveKYCMasterInfo(GRPCRequest<string> UserId)
        {
            var result = await _iKycService.RemoveKYCMaterInfos(UserId);
            return result;
        }

        public async Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(GRPCRequest<string> req)
        {
            var result = await _iKycService.GetDSACityList(req);
            return result;
        }

    }
}
