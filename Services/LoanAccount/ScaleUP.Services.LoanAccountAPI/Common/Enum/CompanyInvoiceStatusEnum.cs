namespace ScaleUP.Services.LoanAccountAPI.Common.Enum
{
    public enum CompanyInvoiceStatusEnum
    {
        Inprocess = 0,
        MakerApproved = 1,
        CheckerApproved = 2,
        MakerReject = 3,
        CheckerReject = 4,
        Settled = 5,

        All = 100 //For Filter Only
    }
}
