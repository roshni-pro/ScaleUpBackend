using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.BuildingBlocks.GRPC.Contracts;

namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAggController : ControllerBase
    {
        private ProductManager productManager;
        public ProductAggController(ProductManager _productManager)
        {
            productManager = _productManager;
        }

        [HttpGet]
        [Route("GetProdName")]
        [AllowAnonymous]
        public async Task<string> GetProductName(long productId)
        {
            return await productManager.GetProductName(productId);
        }

    }
}
