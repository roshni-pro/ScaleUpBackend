using Microsoft.AspNetCore.Mvc;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.KYCFactory.Implementation;
using ScaleUP.Services.KYCAPI.KYCFactory.Implementation.DSA;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;

namespace ScaleUP.Services.KYCAPI.KYCFactory
{
    public class KYCDocFactoryConcrete : KYCDocFactory
    {
        public readonly ApplicationDbContext Context;
        public readonly string _userId;
        public readonly string _filePath;
        public readonly string _createdOrModifiedBy;

        public KYCDocFactoryConcrete(ApplicationDbContext context, string userId, string filePath, string createdOrModifiedBy="")
        {
            Context = context;
            _userId = userId;
            _filePath = filePath;
            _createdOrModifiedBy = createdOrModifiedBy;
        }

        public override IDocType<T, U> GetDocType<T, U>(string KycMasterCode)
        {
            switch(KycMasterCode)
            {
                case Global.Infrastructure.Constants.KYCMasterConstants.PAN:
                    return (IDocType<T, U>)new KarzaPANDocType(Context, _userId, _filePath);
                    break;
                case Global.Infrastructure.Constants.KYCMasterConstants.Aadhar:
                    return (IDocType<T, U>)new KarzaAadharDocType(Context, _userId, _filePath);
                    break;
                case Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail:
                    return (IDocType<T, U>)new BusinessDetailDocType(Context, _userId, _filePath);
                    break;
                case Global.Infrastructure.Constants.KYCMasterConstants.MSME:
                    return (IDocType<T, U>)new MSMEDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.PersonalDetail:
                    return (IDocType<T, U>)new PersonalDetailDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.BankStatement:
                    return (IDocType<T, U>)new BankStatementDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.Selfie:
                    return (IDocType<T, U>)new SelfieDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.BankStatementCreditLending:
                    return (IDocType<T, U>)new BankStatementCreditLendingDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.DSAProfileType:
                    return (IDocType<T, U>)new DSAProfileDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.DSAPersonalDetail:
                    return (IDocType<T, U>)new DSAPersonalDetailDocType(Context, _userId, _filePath);
                    break;
                case KYCMasterConstants.ConnectorPersonalDetail:
                    return (IDocType<T, U>)new ConnectorPersonalDetailDocType(Context, _userId, _filePath);
                    break;
                default:
                    return null;
            }
        }
    }
}
