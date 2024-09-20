namespace ScaleUP.Services.KYCAPI.KYCFactory
{
    public abstract class KYCDocFactory
    {

        public abstract IDocType<T,U> GetDocType<T, U>(string KycMasterCode);
    }
}
