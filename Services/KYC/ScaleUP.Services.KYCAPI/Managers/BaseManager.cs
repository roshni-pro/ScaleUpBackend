using ScaleUP.Services.KYCAPI.Persistence;

namespace ScaleUP.Services.KYCAPI.Managers
{
    public abstract class BaseManager
    {
        public readonly ApplicationDbContext _context;
        public BaseManager(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
