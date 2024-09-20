using Microsoft.AspNetCore.Mvc;
using ScaleUP.Services.ProductAPI.Persistence;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.ProductDTO.Master;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.Services.ProductAPI.Manager;
using ScaleUP.Services.ProductModels.Master;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.ProductDTO.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts;

namespace ScaleUP.Services.ProductAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly ProductApplicationDbContext _context;
        private readonly ProductManager _ProductManager;
        private readonly NBFCCompanyApiManager _NBFCCompanyApiManager;
        private readonly NBFCSubActivityApiManager _NBFCSubActivityApiManager;

        public ProductController(ProductApplicationDbContext context)
        {
            _context = context;
            _ProductManager = new ProductManager(_context);
            _NBFCCompanyApiManager = new NBFCCompanyApiManager(_context);
            _NBFCSubActivityApiManager = new NBFCSubActivityApiManager(_context);
        }

        #region Product
        [HttpPost]
        [Route("AddProduct")]
        public async Task<ProductResponse> InsertProduct(ProductDTO.Master.ProductDTO product)
        {
            var response = await _ProductManager.InsertProduct(product);
            return response;
        }


        [HttpPost]
        [Route("UpdateProduct")]
        public async Task<ProductResponse> UpdateProduct(ProductDTO.Master.ProductDTO product)
        {
            var response = await _ProductManager.UpdateProduct(product);
            return response;
        }


        [HttpGet]
        [Route("GetProduct")]
        public async Task<ProductResponse> GetProduct(long ProductId)
        {
            var response = await _ProductManager.GetProduct(ProductId);
            return response;
        }


        [HttpGet]
        [Route("GetProductList")]
        [AllowAnonymous]
        public async Task<ProductResponse> GetProductList()
        {
            var response = await _ProductManager.GetProductList();
            return response;
        }

        [HttpGet]
        [Route("GetProductMasterListById")]
        [AllowAnonymous]
        public async Task<ProductResponse> GetProductMasterListById(long CompanyId, string CompanyType)
        {
            var response = await _ProductManager.GetProductMasterListById(CompanyId, CompanyType);
            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SaveModifyTemplateMaster")]
        public async Task<ProductTemplateResponseDc> SaveModifyTemplateMaster(ProductTemplateDc templatedc)
        {
            return await _ProductManager.SaveModifyTemplateMaster(templatedc);
        }
        #endregion


        #region  Product Activity Master

        [HttpGet]
        [Route("ProductActivityMasterList")]
        public async Task<ProductResponse> ProductActivityMasterList(long CompanyId, long ProductId)
        {
            var response = await _ProductManager.ProductActivityMasterList(CompanyId, ProductId);
            return response;

        }

        [HttpGet]
        [Route("GetProductActivitySubActivityMasterList")]
        public async Task<ProductResponse> GetProductActivitySubActivityMasterList(string CompanyType, bool IsDefault)
        {
            var response = await _ProductManager.GetProductActivitySubActivityMasterList(CompanyType, IsDefault);
            return response;
        }
        [HttpGet]
        [Route("GetProductActivityMasterList")]
        public async Task<ProductResponse> GetProductActivityMasterList(long ProductId, bool IsDefault)
        {
            var response = await _ProductManager.GetProductActivityMasterList(ProductId, IsDefault);
            return response;
        }

        [HttpPost]
        [Route("AddProductActivityMaster")]
        public async Task<ProductResponse> AddProductActivityMaster(ProductActivityMasterDTO productactivitymaster)
        {
            var response = await _ProductManager.AddProductActivityMaster(productactivitymaster);
            return response;
        }

        [HttpPost]
        [Route("AddUpdateProductActivityMaster")]
        public async Task<ProductResponse> AddUpdateProductActivityMaster(List<ProductActivityMasterDTO> productActivityMasterList)
        {
            var response = await _ProductManager.AddUpdateProductActivityMaster(productActivityMasterList);
            return response;
        }


        [HttpPost]
        [Route("UpdateProductActivityMaster")]
        public async Task<ProductResponse> UpdateProductActivityMaster(ProductActivityMasterDTO productactivitymaster)
        {
            var response = await _ProductManager.UpdateProductActivityMaster(productactivitymaster);
            return response;
        }

        #endregion


        #region Product Company
        [HttpGet]
        [Route("GetEMIOptionMasterList")]
        public async Task<ProductResponse<List<EMIOptionMasters>>> GetEMIOptionMasterList()
        {
            var response = await _ProductManager.GetEMIOptionMasterList();
            return response;
        }

        [HttpGet]
        [Route("GetCreditDayMastersList")]
        public async Task<ProductResponse<List<CreditDayMasters>>> GetCreditDayMastersList()
        {
            var response = await _ProductManager.GetCreditDayMastersList();
            return response;
        }

        #region Anchor Product
        [HttpPost]
        [Route("AddUpdateAnchorProductConfig")]
        public async Task<ProductResponse<AddUpdateAnchorProductConfigDTO>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigDTO request)
        {
            var response = await _ProductManager.AddUpdateAnchorProductConfig(request);
            return response;
        }

        [HttpGet]
        [Route("GetAnchorProductConfig")]
        public async Task<ProductResponse<List<AddUpdateAnchorProductConfigDTO>>> GetAnchorProductConfig(long CompanyId, long ProductId)
        {
            var response = await _ProductManager.GetAnchorProductConfig(CompanyId, ProductId);
            return response;
        }

        [HttpGet]
        [Route("GetAnchorProductList")]
        public async Task<ProductResponse<List<AnchorProductListDTO>>> GetAnchorProductList(long CompanyId)
        {
            var response = await _ProductManager.GetAnchorProductList(CompanyId);
            return response;
        }

        [HttpGet]
        [Route("AnchorProductActiveInactive")]
        public async Task<ProductResponse<bool>> AnchorProductActiveInactive(long AnchorProductId, bool IsActive)
        {
            var response = await _ProductManager.AnchorProductActiveInactive(AnchorProductId, IsActive);
            return response;
        }
        #endregion
        #region NBFC Product
        [HttpPost]
        [Route("AddUpdateNBFCProductConfig")]
        public async Task<ProductResponse<AddUpdateNBFCProductConfigDTO>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigDTO request)
        {
            var response = await _ProductManager.AddUpdateNBFCProductConfig(request);
            return response;
        }

        [HttpGet]
        [Route("GetNBFCProductConfig")]
        public async Task<ProductResponse<List<GetNBFCProductConfigDTO>>> GetNBFCProductConfig(long CompanyId, long ProductId)
        {
            var response = await _ProductManager.GetNBFCProductConfig(CompanyId, ProductId);
            return response;
        }

        [HttpGet]
        [Route("GetNBFCProductList")]
        public async Task<ProductResponse<List<NBFCProductListDTO>>> GetNBFCProductList(long CompanyId)
        {
            var response = await _ProductManager.GetNBFCProductList(CompanyId);
            return response;
        }

        [HttpGet]
        [Route("NBFCProductActiveInactive")]
        public async Task<ProductResponse<bool>> NBFCProductActiveInactive(long NBFCProductId, bool IsActive)
        {
            var response = await _ProductManager.NBFCProductActiveInactive(NBFCProductId, IsActive);
            return response;
        }
        #endregion
        #endregion

        #region NBFC Company

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveNBFCCompanyApiData")]
        public long SaveNBFCCompanyApiData(NBFCCompanyApiRequest nBFCCompanyApiRequest)
        {
            var saveData = _NBFCCompanyApiManager.SaveNBFCCompanyApiData(nBFCCompanyApiRequest);
            return saveData;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetNBFCComapanyApiData")]
        public async Task<List<NBFCCompanyGetData>> GetNBFCComapanyApiData(long NBFCComapanyId)
        {
            var GetData = await _NBFCCompanyApiManager.GetNBFCComapanyApiData(NBFCComapanyId);
            return GetData;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateNBFCCompanyApi")]
        public async Task<NBFCCompanyResponseMsg> UpdateNBFCCompanyApi(NBFCCompanyGetData nBFCCompanyGetData)
        {
            var UpdtaeData = await _NBFCCompanyApiManager.UpdateNBFCCompanyApi(nBFCCompanyGetData);
            return UpdtaeData;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveNBFCSubActivityApi")]
        public ResultViewModel<bool> SaveNBFCSubActivityApi(List<NBFCSubActivityApiDTO> nBFCSubActivityApiDTOs)
        {
            var SaveData = _NBFCSubActivityApiManager.SaveNBFCSubActivityApi(nBFCSubActivityApiDTOs);
            return SaveData;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetNBFCSubActivityApi")]
        public async Task<List<NBFCSubActivityApiDTO>> GetNBFCSubActivityApi(NBFCSubActivityApiRequest nBFCSubActivityApiRequest)
        {
            var GetData = await _NBFCSubActivityApiManager.GetNBFCSubActivityApi(nBFCSubActivityApiRequest);
            return GetData;
        }
        #endregion


        [AllowAnonymous]
        [HttpGet]
        [Route("SaveBlackSoilCompanyApiData")]
        public bool SaveBlackSoilCompanyApiData(long CompanyId)
        {
            return _NBFCCompanyApiManager.SaveBlackSoilCompanyApiData(CompanyId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SaveArthmateCompanyApiData")]
        public bool SaveArthmateCompanyApiData(long CompanyId)
        {
            return _NBFCCompanyApiManager.SaveArthmateCompanyApiData(CompanyId);
        }


        #region DSA

        [Route("GetDSASalesAgentList")]
        [HttpGet]
        public async Task<ResultViewModel<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList()
        {
            var request = new GRPCRequest<string> { Request = UserId };
            var res = await _ProductManager.GetDSASalesAgentList(request);
            return res;
        }


        [Route("GetDSAUsersList")]
        [HttpGet]
        public async Task<ResultViewModel<List<GetDSAAgentUsersListResponseDc>>> GetDSAUsersList(string UserId, int Skip, int Take)
        {
            var res = await _ProductManager.GetDSAUsersList(UserId, Skip, Take);
            return res;
        }

        [Route("DSAUserStatusChange")]
        [HttpPost]
        public async Task<ResultViewModel<string>> DSAUserStatusChange(DSAUserActivationDc req)
        {
            var res = await _ProductManager.DSAUserStatusChange(req);
            return res;
        }

        [Route("SaveDSAPayoutPercentage")]
        [HttpPost]
        public async Task<ResultViewModel<string>> SaveDSAPayoutPercentage(SavePayoutDc req)
        {
            var res = await _ProductManager.SaveDSAPayoutPercentage(req);
            return res;
        }

        [Route("GetSalesAgentListByAnchorId")]
        [HttpGet]
        public async Task<ResultViewModel<List<GetSalesAgentDataDc>>> GetSalesAgentListByAnchorId(long anchorCompanyId)
        {
            var res = await _ProductManager.GetSalesAgentListByAnchorId(anchorCompanyId);
            return res;
        }

        [Route("GetDSAUsersName")]
        [HttpGet]
        public async Task<ResultViewModel<string>> GetDSAUsersName(string UserId)
        {
            var res = await _ProductManager.DSAUserName(UserId);
            return res;
        }

        #endregion
    }
}
