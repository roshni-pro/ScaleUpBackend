using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{

    public class ProductManager
    {
        private IProductService _productService;

        public ProductManager(IProductService productService)
        {
            _productService = productService;
        }


        public async Task<string> GetProductName(long productId)
        {
            return await _productService.GetProductName(productId);
        }

    }
}
