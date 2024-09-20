using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;

namespace ScaleUP.Services.KYCAPI.KYCFactory
{
    public interface IDocType<T, U>
    {
        //bool IsValidTOSave(T doc, KYCMaster kycMaster);
        //Task<long> SaveDoc(T doc, string userId);
        //Task<Dictionary<string, dynamic>> GetDoc(string userId);
        //Task<bool> ValidateByUniqueId(U input);
        //Task<T> GetByUniqueId(U input);
        //Task<ResultViewModel<long>> GetAndSaveByUniqueId(U input, string userId);
        //Task<long> IsDocumentExist(string UniqueId, string userId);
        //Task<bool> RemoveDocInfo(string userId);
        ResultViewModel<bool> IsValidTOSave(T doc, KYCMaster kycMaster, string userId, string productCode);
        Task<ResultViewModel<long>> SaveDoc(T doc, string userId, string CreatedBy, string productCode);
        Task<ResultViewModel<Dictionary<string, dynamic>>> GetDoc(string userId);
        Task<ResultViewModel<bool>> ValidateByUniqueId(U input);
        Task<ResultViewModel<T>> GetByUniqueId(U input);
        Task<ResultViewModel<long>> GetAndSaveByUniqueId(U input, string userId,string CreatedBy, string productCode);
        Task<ResultViewModel<long>> IsDocumentExist(string UniqueId, string userId);
        Task<ResultViewModel<bool>> RemoveDocInfo(string userId);
    }

    //public interface IDoc
    //{
    //    public string UserId { get; set; }
    //    public abstract U GetDoc<T, U>(T userId);
    //}


    //public abstract class DocAbsCls<T, U> : IDoc
    //{
    //    public string UserId { get; set; }
    //    //public abstract U GetDoc(T userId);

    //    public abstract U1 GetDoc<T1, U1>(T1 userId)
    //    {
           
    //    }

    //    //public abstract bool IsValidTOSave(T doc, KYCMaster kycMaster) ;
    //    //public abstract Task<long> SaveDoc(T doc, string userId);
    //    //public abstract Task<Dictionary<string, dynamic>> GetDoc(string userId);
    //    //public abstract Task<bool> ValidateByUniqueId(U input);
    //    //public abstract Task<T> GetByUniqueId(U input);
    //    //public abstract Task<long> GetAndSaveByUniqueId(U input, string userId);
    //}

    //public class Concrete1 : DocAbsCls<AppyFlowGSTDTO, KYCActivityGST>
    //{
       


    //}


    //public class factory
    //{
    //    public void GetuwithT()
    //    {
    //        IDoc doc = new Concrete1();
    //        var r = doc.GetDoc(null);
    //    }
    //}

}
