using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Services.LocationAPI.Managers;
using ScaleUP.Services.LocationAPI.Persistence;

namespace ScaleUP.Services.LocationAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MasterEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MasterEntryManager _MasterEntryManager;
        public MasterEntryController(ApplicationDbContext context, MasterEntryManager masterEntryManager)
        {
            _context = context;
            _MasterEntryManager = masterEntryManager;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveAddressType")]
        public async Task SaveAddressType()
        {
            _MasterEntryManager.EnterAddressType();
        }
    }
}
