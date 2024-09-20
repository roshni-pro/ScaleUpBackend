using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.ProductAPI.Persistence;

namespace ScaleUP.Services.ProductAPI.Controllers
{
    [Route("companies")]
    [ApiController]
    public class ProductCompaniesController : BaseController
    {
        private readonly ProductApplicationDbContext _context;
        public ProductCompaniesController(ProductApplicationDbContext context)
        {
            _context = context;
        }

        //[HttpPost]
        //[Route("ProductCompanie")]
        //public async Task<> ProductCompanie() 
        //{
        
        
        
        
        //}



    }
}
