using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;

namespace ScaleUP.Services.KYCAPI.Managers
{
    public class ThirdPartyAPIConfigManager : BaseManager
    {
        public ThirdPartyAPIConfigManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ThirdPartyAPIConfig?> GetByCode(string code)
        {
            ThirdPartyAPIConfig? thirdPartyAPIConfigData = await _context.ThirdPartyAPIConfigs.Where(x => x.Code == code && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
            return thirdPartyAPIConfigData;
        }
        public async Task<bool> InsertApiReqRes(ThirdPartyApiRequestDTO request)
        {
            var res = await _context.ApiRequestResponse.AddAsync(new ApiRequestResponse
            {
                UserId = request.UserId,
                APIConfigId = request.APIConfigId,
                CompanyId = request.CompanyId,
                ProcessedResponse= request.ProcessedResponse,
                Request=request.Request, 
                Response=request.Response,
                Created = request.Created,
                IsActive = true,
                IsDeleted = false
            });
            return _context.SaveChanges() > 0;

        }

    }
}
