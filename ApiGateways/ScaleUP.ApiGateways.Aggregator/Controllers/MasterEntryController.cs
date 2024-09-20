using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;

namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MasterEntryController : ControllerBase
    {
        private IProductService _productService;
        private ILoanAccountService _loanAccountService;

        public MasterEntryController(IProductService productService, ILoanAccountService loanAccountService)
        {
            _productService = productService;
            _loanAccountService = loanAccountService;
        }

        [HttpGet]
        [Route("GetCompanyApiData")]
        [AllowAnonymous]
        public async Task<GRPCReply<long>> GetCompanyApiData()
        {
            var reply = await _productService.GetCompanyApiData();
            if (reply != null)
            {
                GRPCRequest<List<CompanyApiReply>> request = new GRPCRequest<List<CompanyApiReply>>
                {
                    Request = reply.Response
                };
                var result = await _loanAccountService.SaveNBFCCompanyAPIs(request);
                return result;
            }
            return null;

        }
    }
}
