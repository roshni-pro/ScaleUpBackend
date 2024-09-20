using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class Xml
    {
        [DataMember(Order = 1)]
        public string? Version { get; set; }
        [DataMember(Order = 2)]
        public string? Encoding { get; set; }
        [DataMember(Order = 3)]
        public string? Standalone { get; set; }
    }

    [DataContract]
    public class Header
    {
        [DataMember(Order = 1)]
        public string? SystemCode { get; set; }
        [DataMember(Order = 2)]
        public string? MessageText { get; set; }
        [DataMember(Order = 3)]
        public string? ReportDate { get; set; }
        [DataMember(Order = 4)]
        public string? ReportTime { get; set; }
    }

    [DataContract]
    public class UserMessage
    {
        [DataMember(Order = 1)]
        public string? UserMessageText { get; set; }
    }

    [DataContract]
    public class CreditProfileHeader
    {
        [DataMember(Order = 1)]
        public string? Enquiry_Username { get; set; }
        [DataMember(Order = 2)]
        public string? ReportDate { get; set; }
        [DataMember(Order = 3)]
        public string? ReportTime { get; set; }
        [DataMember(Order = 4)]
        public string? Version { get; set; }
        [DataMember(Order = 5)]
        public string? ReportNumber { get; set; }
        [DataMember(Order = 6)]
        public string? Subscriber { get; set; }
        [DataMember(Order = 7)]
        public string? Subscriber_Name { get; set; }
    }

    [DataContract]
    public class CurrentApplicantDetails
    {
        [DataMember(Order = 1)]
        public string? Last_Name { get; set; }
        [DataMember(Order = 2)]
        public string? First_Name { get; set; }
        [DataMember(Order = 3)]
        public string? Middle_Name1 { get; set; }
        [DataMember(Order = 4)]
        public string? Middle_Name2 { get; set; }
        [DataMember(Order = 5)]
        public string Middle_Name3 { get; set; }
        [DataMember(Order = 6)]
        public string? Gender_Code { get; set; }
        [DataMember(Order = 7)]
        public string? IncomeTaxPan { get; set; }
        [DataMember(Order = 8)]
        public string? PAN_Issue_Date { get; set; }
        [DataMember(Order = 9)]
        public string? PAN_Expiration_Date { get; set; }
        [DataMember(Order = 10)]
        public string? Passport_number { get; set; }
        [DataMember(Order = 11)]
        public string? Passport_Issue_Date { get; set; }
        [DataMember(Order = 12)]
        public string? Passport_Expiration_Date { get; set; }
        [DataMember(Order = 13)]
        public string? Voter_s_Identity_Card { get; set; }
        [DataMember(Order = 14)]
        public string? Voter_ID_Issue_Date { get; set; }
        [DataMember(Order = 15)]
        public string? Voter_ID_Expiration_Date { get; set; }
        [DataMember(Order = 16)]
        public string? Driver_License_Number { get; set; }
        [DataMember(Order = 17)]
        public string? Driver_License_Issue_Date { get; set; }
        [DataMember(Order = 18)]
        public string? Driver_License_Expiration_Date { get; set; }
        [DataMember(Order = 19)]
        public string? Ration_Card_Number { get; set; }
        [DataMember(Order = 20)]
        public string? Ration_Card_Issue_Date { get; set; }
        [DataMember(Order = 21)]
        public string? Ration_Card_Expiration_Date { get; set; }
        [DataMember(Order = 22)]
        public string? Universal_ID_Number { get; set; }
        [DataMember(Order = 23)]
        public string? Universal_ID_Issue_Date { get; set; }
        [DataMember(Order = 24)]
        public string? Universal_ID_Expiration_Date { get; set; }
        [DataMember(Order = 25)]
        public string? Date_Of_Birth_Applicant { get; set; }
        [DataMember(Order = 26)]
        public string? Telephone_Number_Applicant_1st { get; set; }
        [DataMember(Order = 27)]
        public string? Telephone_Extension { get; set; }
        [DataMember(Order = 28)]
        public string? Telephone_Type { get; set; }
        [DataMember(Order = 29)]
        public string? MobilePhoneNumber { get; set; }
        [DataMember(Order = 30)]
        public string? EMailId { get; set; }
    }

    [DataContract]
    public class CurrentOtherDetails
    {
        [DataMember(Order = 1)]
        public string? Income { get; set; }
        [DataMember(Order = 2)]
        public string? Marital_Status { get; set; }
        [DataMember(Order = 3)]
        public string? Employment_Status { get; set; }
        [DataMember(Order = 4)]
        public string? Time_with_Employer { get; set; }
        [DataMember(Order = 5)]
        public string? Number_of_Major_Credit_Card_Held { get; set; }
    }

    [DataContract]
    public class CurrentApplicantAddressDetails
    {
        [DataMember(Order = 1)]
        public string? FlatNoPlotNoHouseNo { get; set; }
        [DataMember(Order = 2)]
        public string? BldgNoSocietyName { get; set; }
        [DataMember(Order = 3)]
        public string? RoadNoNameAreaLocality { get; set; }
        [DataMember(Order = 4)]
        public string? City { get; set; }
        [DataMember(Order = 5)]
        public string? Landmark { get; set; }
        [DataMember(Order = 6)]
        public string? State { get; set; }
        [DataMember(Order = 7)]
        public string? PINCode { get; set; }
        [DataMember(Order = 8)]
        public string? Country_Code { get; set; }
    }

    [DataContract]
    public class CurrentApplicationDetails
    {
        [DataMember(Order = 1)]
        public string? Enquiry_Reason { get; set; }
        [DataMember(Order = 2)]
        public string? Finance_Purpose { get; set; }
        [DataMember(Order = 3)]
        public string? Amount_Financed { get; set; }
        [DataMember(Order = 4)]
        public string? Duration_Of_Agreement { get; set; }
        [DataMember(Order = 5)]
        public CurrentApplicantDetails Current_Applicant_Details { get; set; } = null;
        [DataMember(Order = 6)]
        public CurrentOtherDetails Current_Other_Details { get; set; } = null;
        [DataMember(Order = 7)]
        public CurrentApplicantAddressDetails Current_Applicant_Address_Details { get; set; } = null;
        //[DataMember(Order = 8)]
        //public object Current_Applicant_Additional_AddressDetails { get; set; } = null;
    }

    [DataContract]
    public class CurrentApplication
    {
        [DataMember(Order = 1)]
        public CurrentApplicationDetails Current_Application_Details { get; set; } = null;
    }

    [DataContract]
    public class CreditAccount
    {
        [DataMember(Order = 1)]
        public string? CADSuitFiledCurrentBalance { get; set; }
        [DataMember(Order = 2)]
        public string? CreditAccountActive { get; set; }
        [DataMember(Order = 3)]
        public string? CreditAccountClosed { get; set; }
        [DataMember(Order = 4)]
        public string? CreditAccountDefault { get; set; }
        [DataMember(Order = 5)]
        public string? CreditAccountTotal { get; set; }
    }

    [DataContract]
    public class TotalOutstandingBalance
    {
        [DataMember(Order = 1)]
        public string? Outstanding_Balance_Secured { get; set; }
        [DataMember(Order = 2)]
        public string? Outstanding_Balance_Secured_Percentage { get; set; }
        [DataMember(Order = 3)]
        public string? Outstanding_Balance_UnSecured { get; set; }
        [DataMember(Order = 4)]
        public string? Outstanding_Balance_UnSecured_Percentage { get; set; }
        [DataMember(Order = 5)]
        public string? Outstanding_Balance_All { get; set; }
    }

    [DataContract]
    public class CAISSummary
    {
        [DataMember(Order = 1)]
        public CreditAccount Credit_Account { get; set; } = null;
        [DataMember(Order = 2)]
        public TotalOutstandingBalance Total_Outstanding_Balance { get; set; } = null;
    }

    [DataContract]
    public class CAISHolderDetails
    {
        [DataMember(Order = 1)]
        public string? Surname_Non_Normalized { get; set; }
        [DataMember(Order = 2)]
        public string? First_Name_Non_Normalized { get; set; }
        [DataMember(Order = 3)]
        public string? Middle_Name_1_Non_Normalized { get; set; }
        [DataMember(Order = 4)]
        public string? Middle_Name_2_Non_Normalized { get; set; }
        [DataMember(Order = 5)]
        public string? Middle_Name_3_Non_Normalized { get; set; }
        [DataMember(Order = 6)]
        public string? Alias { get; set; }
        [DataMember(Order = 7)]
        public string? Gender_Code { get; set; }
        [DataMember(Order = 8)]
        public string? Income_TAX_PAN { get; set; }
        [DataMember(Order = 9)]
        public string? Passport_Number { get; set; }
        [DataMember(Order = 10)]
        public string? Voter_ID_Number { get; set; }
        [DataMember(Order = 11)]
        public string? Date_of_birth { get; set; }
    }

    [DataContract]
    public class CAISHolderIDDetails
    {
        [DataMember(Order = 1)]
        public string? Income_TAX_PAN { get; set; }
        [DataMember(Order = 2)]
        public string? PAN_Issue_Date { get; set; }
        [DataMember(Order = 3)]
        public string? PAN_Expiration_Date { get; set; }
        [DataMember(Order = 4)]
        public string? Passport_Number { get; set; }
        [DataMember(Order = 5)]
        public string? Passport_Issue_Date { get; set; }
        [DataMember(Order = 6)]
        public string? Passport_Expiration_Date { get; set; }
        [DataMember(Order = 7)]
        public string? Voter_ID_Number { get; set; }
        [DataMember(Order = 8)]
        public string? Voter_ID_Issue_Date { get; set; }
        [DataMember(Order = 9)]
        public string? Voter_ID_Expiration_Date { get; set; }
        [DataMember(Order = 10)]
        public string? Driver_License_Number { get; set; }
        [DataMember(Order = 11)]
        public string? Driver_License_Issue_Date { get; set; }
        [DataMember(Order = 12)]
        public string? Driver_License_Expiration_Date { get; set; }
        [DataMember(Order = 13)]
        public string? Ration_Card_Number { get; set; }
        [DataMember(Order = 14)]
        public string? Ration_Card_Issue_Date { get; set; }
        [DataMember(Order = 15)]
        public string? Ration_Card_Expiration_Date { get; set; }
        [DataMember(Order = 16)]
        public string? Universal_ID_Number { get; set; }
        [DataMember(Order = 17)]
        public string? Universal_ID_Issue_Date { get; set; }
        [DataMember(Order = 18)]
        public string? Universal_ID_Expiration_Date { get; set; }
        [DataMember(Order = 19)]
        public string? EMailId { get; set; }
    }

    [DataContract]
    public class CAISAccountDETAIL
    {
        [DataMember(Order = 1)]
        public string? Identification_Number { get; set; }
        [DataMember(Order = 2)]
        public string? Subscriber_Name { get; set; }
        [DataMember(Order = 3)]
        public string? Account_Number { get; set; }
        [DataMember(Order = 4)]
        public string? Portfolio_Type { get; set; }
        [DataMember(Order = 5)]
        public string? Account_Type { get; set; }
        [DataMember(Order = 6)]
        public string? Open_Date { get; set; }
        [DataMember(Order = 7)]
        public string? Credit_Limit_Amount { get; set; }
        [DataMember(Order = 8)]
        public string? Highest_Credit_or_Original_Loan_Amount { get; set; }
        [DataMember(Order = 9)]
        public string? Terms_Duration { get; set; }
        [DataMember(Order = 10)]
        public string? Terms_Frequency { get; set; }
        [DataMember(Order = 11)]
        public string? Scheduled_Monthly_Payment_Amount { get; set; }
        [DataMember(Order = 12)]
        public string? Account_Status { get; set; }
        [DataMember(Order = 13)]
        public string? Payment_Rating { get; set; }
        [DataMember(Order = 14)]
        public string? Payment_History_Profile { get; set; }
        [DataMember(Order = 15)]
        public string? Special_Comment { get; set; }
        [DataMember(Order = 16)]
        public string? Current_Balance { get; set; }
        [DataMember(Order = 17)]
        public string? Amount_Past_Due { get; set; }
        [DataMember(Order = 18)]
        public string? Original_Charge_Off_Amount { get; set; }
        [DataMember(Order = 19)]
        public string? Date_Reported { get; set; }
        [DataMember(Order = 20)]
        public string? Date_of_First_Delinquency { get; set; }
        [DataMember(Order = 21)]
        public string? Date_Closed { get; set; }
        [DataMember(Order = 22)]
        public string? Date_of_Last_Payment { get; set; }
        [DataMember(Order = 23)]
        public string? SuitFiledWillfulDefaultWrittenOffStatus { get; set; }
        [DataMember(Order = 24)]
        public string? SuitFiled_WilfulDefault { get; set; }
        [DataMember(Order = 25)]
        public string? Written_off_Settled_Status { get; set; }
        [DataMember(Order = 26)]
        public string? Value_of_Credits_Last_Month { get; set; }
        [DataMember(Order = 27)]
        public string? Occupation_Code { get; set; }
        [DataMember(Order = 28)]
        public string? Settlement_Amount { get; set; }
        [DataMember(Order = 29)]
        public string? Value_of_Collateral { get; set; }
        [DataMember(Order = 30)]
        public string? Type_of_Collateral { get; set; }
        [DataMember(Order = 31)]
        public string? Written_Off_Amt_Total { get; set; }
        [DataMember(Order = 32)]
        public string? Written_Off_Amt_Principal { get; set; }
        [DataMember(Order = 33)]
        public string? Rate_of_Interest { get; set; }
        [DataMember(Order = 34)]
        public string? Repayment_Tenure { get; set; }
        [DataMember(Order = 35)]
        public string? Promotional_Rate_Flag { get; set; }
        [DataMember(Order = 36)]
        public string? Income { get; set; }
        [DataMember(Order = 37)]
        public string? Income_Indicator { get; set; }
        [DataMember(Order = 38)]
        public string? Income_Frequency_Indicator { get; set; }
        [DataMember(Order = 39)]
        public string? DefaultStatusDate { get; set; }
        [DataMember(Order = 40)]
        public string? LitigationStatusDate { get; set; }
        [DataMember(Order = 41)]
        public string? WriteOffStatusDate { get; set; }
        [DataMember(Order = 42)]
        public string? DateOfAddition { get; set; }
        [DataMember(Order = 43)]
        public string? CurrencyCode { get; set; }
        [DataMember(Order = 44)]
        public string? Subscriber_comments { get; set; }
        [DataMember(Order = 45)]
        public string? Consumer_comments { get; set; }
        [DataMember(Order = 46)]
        public string? AccountHoldertypeCode { get; set; }
        //[DataMember(Order = 47)]
        //public object CAIS_Account_History { get; set; } = null;
        [DataMember(Order = 47)]
        public CAISHolderDetails CAIS_Holder_Details { get; set; } = null;
        //[DataMember(Order = 49)]
        //public object CAIS_Holder_Address_Details { get; set; } = null;
        //[DataMember(Order = 50)]
        //public object CAIS_Holder_Phone_Details { get; set; } = null;
        [DataMember(Order = 48)]
        public CAISHolderIDDetails CAIS_Holder_ID_Details { get; set; } = null;
    }

    [DataContract]
    public class CAISAccount
    {
        [DataMember(Order = 1)]
        public CAISSummary CAIS_Summary { get; set; } = null;
        [DataMember(Order = 2)]
        public List<CAISAccountDETAIL> CAIS_Account_DETAILS { get; set; } = null;
    }

    [DataContract]
    public class MatchResult
    {
        [DataMember(Order = 1)]
        public string? Exact_match { get; set; }
    }

    [DataContract]
    public class TotalCAPSSummary
    {
        [DataMember(Order = 1)]
        public string? TotalCAPSLast7Days { get; set; }
        [DataMember(Order = 2)]
        public string? TotalCAPSLast30Days { get; set; }
        [DataMember(Order = 3)]
        public string? TotalCAPSLast90Days { get; set; }
        [DataMember(Order = 4)]
        public string? TotalCAPSLast180Days { get; set; }
    }

    [DataContract]
    public class CAPSSummary
    {
        [DataMember(Order = 1)]
        public string? CAPSLast7Days { get; set; }
        [DataMember(Order = 2)]
        public string? CAPSLast30Days { get; set; }
        [DataMember(Order = 3)]
        public string? CAPSLast90Days { get; set; }
        [DataMember(Order = 4)]
        public string? CAPSLast180Days { get; set; }
    }

    [DataContract]
    public class CAPSApplicantDetails
    {
        [DataMember(Order = 1)]
        public string? Date_Of_Birth_Applicant { get; set; }
        [DataMember(Order = 2)]
        public string? Driver_License_Expiration_Date { get; set; }
        [DataMember(Order = 3)]
        public string? Driver_License_Issue_Date { get; set; }
        [DataMember(Order = 4)]
        public string? Driver_License_Number { get; set; }
        [DataMember(Order = 5)]
        public string? EMailId { get; set; }
        [DataMember(Order = 6)]
        public string? First_Name { get; set; }
        [DataMember(Order = 7)]
        public string? Gender_Code { get; set; }
        [DataMember(Order = 8)]
        public string? IncomeTaxPan { get; set; }
        [DataMember(Order = 9)]
        public string? Last_Name { get; set; }
        [DataMember(Order = 10)]
        public string? Middle_Name1 { get; set; }
        [DataMember(Order = 11)]
        public string? Middle_Name2 { get; set; }
        [DataMember(Order = 12)]
        public string? Middle_Name3 { get; set; }
        [DataMember(Order = 13)]
        public string? MobilePhoneNumber { get; set; }
        [DataMember(Order = 14)]
        public string? PAN_Expiration_Date { get; set; }
        [DataMember(Order = 15)]
        public string? PAN_Issue_Date { get; set; }
        [DataMember(Order = 16)]
        public string? Passport_Expiration_Date { get; set; }
        [DataMember(Order = 17)]
        public string? Passport_Issue_Date { get; set; }
        [DataMember(Order = 18)]
        public string? Passport_number { get; set; }
        [DataMember(Order = 19)]
        public string? Ration_Card_Expiration_Date { get; set; }
        [DataMember(Order = 20)]
        public string? Ration_Card_Issue_Date { get; set; }
        [DataMember(Order = 21)]
        public string? Ration_Card_Number { get; set; }
        [DataMember(Order = 22)]
        public string? Telephone_Type { get; set; }
        [DataMember(Order = 23)]
        public string? Universal_ID_Expiration_Date { get; set; }
        [DataMember(Order = 24)]
        public string? Universal_ID_Issue_Date { get; set; }
        [DataMember(Order = 25)]
        public string? Universal_ID_Number { get; set; }
        [DataMember(Order = 26)]
        public string? Voter_ID_Expiration_Date { get; set; }
        [DataMember(Order = 27)]
        public string? Voter_ID_Issue_Date { get; set; }
        [DataMember(Order = 28)]
        public string? Voter_s_Identity_Card { get; set; }
        [DataMember(Order = 29)]
        public string? Telephone_Extension { get; set; }
        [DataMember(Order = 30)]
        public string? Telephone_Number_Applicant_1st { get; set; }
    }

    [DataContract]
    public class CAPSOtherDetails
    {
        [DataMember(Order = 1)]
        public string? Income { get; set; }
        [DataMember(Order = 2)]
        public string? Marital_Status { get; set; }
        [DataMember(Order = 3)]
        public string? Employment_Status { get; set; }
        [DataMember(Order = 4)]
        public string? Time_with_Employer { get; set; }
        [DataMember(Order = 5)]
        public string? Number_of_Major_Credit_Card_Held { get; set; }
    }

    [DataContract]
    public class CAPSApplicantAddressDetails
    {
        [DataMember(Order = 1)]
        public string? FlatNoPlotNoHouseNo { get; set; }
        [DataMember(Order = 2)]
        public string? BldgNoSocietyName { get; set; }
        [DataMember(Order = 3)]
        public string? RoadNoNameAreaLocality { get; set; }
        [DataMember(Order = 4)]
        public string? City { get; set; }
        [DataMember(Order = 5)]
        public string? Landmark { get; set; }
        [DataMember(Order = 6)]
        public string? State { get; set; }
        [DataMember(Order = 7)]
        public string? PINCode { get; set; }
        [DataMember(Order = 8)]
        public string? Country_Code { get; set; }
    }

    [DataContract]
    public class CAPSApplicantAdditionalAddressDetails
    {
        [DataMember(Order = 1)]
        public string? BldgNoSocietyName { get; set; }
        [DataMember(Order = 2)]
        public string? City { get; set; }
        [DataMember(Order = 3)]
        public string? FlatNoPlotNoHouseNo { get; set; }
        [DataMember(Order = 4)]
        public string? Landmark { get; set; }
        [DataMember(Order = 5)]
        public string? RoadNoNameAreaLocality { get; set; }
        [DataMember(Order = 6)]
        public string? State { get; set; }
    }

    [DataContract]
    public class CAPSApplicationDetail
    {
        [DataMember(Order = 1)]
        public string? Subscriber_code { get; set; }
        [DataMember(Order = 2)]
        public string? Subscriber_Name { get; set; }
        [DataMember(Order = 3)]
        public string? Date_of_Request { get; set; }
        [DataMember(Order = 4)]
        public string? ReportTime { get; set; }
        [DataMember(Order = 5)]
        public string? ReportNumber { get; set; }
        [DataMember(Order = 6)]
        public string? Enquiry_Reason { get; set; }
        [DataMember(Order = 7)]
        public string? Finance_Purpose { get; set; }
        [DataMember(Order = 8)]
        public string? Amount_Financed { get; set; }
        [DataMember(Order = 9)]
        public string? Duration_Of_Agreement { get; set; }
        [DataMember(Order = 10)]
        public CAPSApplicantDetails CAPS_Applicant_Details { get; set; } = null;
        [DataMember(Order = 11)]
        public CAPSOtherDetails CAPS_Other_Details { get; set; } = null;
        [DataMember(Order = 12)]
        public CAPSApplicantAddressDetails CAPS_Applicant_Address_Details { get; set; } = null;
        [DataMember(Order = 13)]
        public CAPSApplicantAdditionalAddressDetails CAPS_Applicant_Additional_Address_Details { get; set; } = null;
    }

    [DataContract]
    public class CAPS
    {
        [DataMember(Order = 1)]
        public CAPSSummary CAPS_Summary { get; set; } = null;
        [DataMember(Order = 2)]
        public List<CAPSApplicationDetail> CAPS_Application_Details { get; set; } = null;
    }

    [DataContract]
    public class NonCreditCAPSSummary
    {
        [DataMember(Order = 1)]
        public string? NonCreditCAPSLast7Days { get; set; }
        [DataMember(Order = 2)]
        public string? NonCreditCAPSLast30Days { get; set; }
        [DataMember(Order = 3)]
        public string? NonCreditCAPSLast90Days { get; set; }
        [DataMember(Order = 4)]
        public string? NonCreditCAPSLast180Days { get; set; }
    }

    [DataContract]
    public class NonCreditCAPS
    {
        [DataMember(Order = 1)]
        public NonCreditCAPSSummary NonCreditCAPS_Summary { get; set; } = null;
    }

    [DataContract]
    public class SCORE
    {
        [DataMember(Order = 1)]
        public string? BureauScore { get; set; }
        [DataMember(Order = 2)]
        public string? BureauScoreConfidLevel { get; set; }
        [DataMember(Order = 3)]
        public string? BureauPLcore { get; set; }
        [DataMember(Order = 4)]
        public string? LeverageScore { get; set; }
        [DataMember(Order = 5)]
        public string? NoHitScore { get; set; }
    }

    [DataContract]
    public class INProfileResponse
    {
        [DataMember(Order = 1)]
        public Header Header { get; set; } = null;
        [DataMember(Order = 2)]
        public UserMessage UserMessage { get; set; } = null;
        [DataMember(Order = 3)]
        public CreditProfileHeader CreditProfileHeader { get; set; } = null;
        [DataMember(Order = 4)]
        public CurrentApplication Current_Application { get; set; } = null;
        [DataMember(Order = 5)]
        public CAISAccount CAIS_Account { get; set; } = null;
        [DataMember(Order = 6)]
        public MatchResult Match_result { get; set; } = null;
        [DataMember(Order = 7)]
        public TotalCAPSSummary TotalCAPS_Summary { get; set; } = null;
        [DataMember(Order = 8)]
        public CAPS CAPS { get; set; } = null;
        [DataMember(Order = 9)]
        public NonCreditCAPS NonCreditCAPS { get; set; } = null;
        [DataMember(Order = 10)]
        public SCORE SCORE { get; set; } = null;
    }

    [DataContract]
    public class CreditBureauResponseDc
    {
        [DataMember(Order = 1)]
        public Xml Xml { get; set; } = null;
        [DataMember(Order = 2)]
        public INProfileResponse INProfileResponse { get; set; } = null;
    }
}
