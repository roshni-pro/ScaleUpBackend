using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.ProductAPI.Persistence;
using ScaleUP.Services.ProductDTO.Master;
using ScaleUP.Services.ProductModels.Master;

namespace ScaleUP.Services.ProductAPI.Manager
{
    public class NBFCSubActivityApiManager
    {
        private readonly ProductApplicationDbContext _context;

        public NBFCSubActivityApiManager(ProductApplicationDbContext context)
        {
            _context = context;
        }

        public ResultViewModel<bool> SaveNBFCSubActivityApi(List<NBFCSubActivityApiDTO> nBFCSubActivityApiDTOs)
        {
            if (nBFCSubActivityApiDTOs != null)
            {
                List<NBFCSubActivityApi> nBFCSubActivityApis = new List<NBFCSubActivityApi>();
                nBFCSubActivityApis = _context.NBFCSubActivityApis.Where(x => x.ActivityMasterId == nBFCSubActivityApiDTOs.First().ActivityMasterId
                && x.SubActivityMasterId == nBFCSubActivityApiDTOs.First().SubActivityMasterId
                && x.ProductCompanyActivityMasterId == nBFCSubActivityApiDTOs.First().ProductCompanyActivityMasterId
                && x.IsActive && !x.IsDeleted).ToList();
                //List<long> ids = data.Select(x => x.Id).ToList();
                if (nBFCSubActivityApis.Count > 0)
                {
                    foreach (var ele in nBFCSubActivityApis)
                    {
                        ele.IsActive = false;
                        ele.IsDeleted = true;
                    }
                    // _context.NBFCSubActivityApis.AddRange(nBFCSubActivityApis);
                }
                _context.SaveChanges();


                foreach (var item in nBFCSubActivityApiDTOs)
                {
                    NBFCSubActivityApi NBFCSubActivityApi = new NBFCSubActivityApi
                    {
                        ActivityMasterId = item.ActivityMasterId,
                        Sequence = item.Sequence,
                        SubActivityMasterId = item.SubActivityMasterId,
                        IsActive = true,
                        IsDeleted = false,
                        NBFCCompanyApiId = item.NBFCCompanyApiId,
                        ProductCompanyActivityMasterId = item.ProductCompanyActivityMasterId
                    };
                    _context.NBFCSubActivityApis.Add(NBFCSubActivityApi);
                    _context.SaveChanges();
                }
            }
            return new ResultViewModel<bool>
            {
                Result = true,
                IsSuccess = true,
                Message = "SuccesFully"
            };
        }

        public async Task<List<NBFCSubActivityApiDTO>> GetNBFCSubActivityApi(NBFCSubActivityApiRequest nBFCSubActivityApiRequest)
        {
            List<NBFCSubActivityApiDTO> NBFCSubActivityApiDTO = new List<NBFCSubActivityApiDTO>();

            var query = (from x in _context.NBFCSubActivityApis
                         join com in _context.CompanyApis on x.NBFCCompanyApiId equals com.Id
                         where x.ActivityMasterId == nBFCSubActivityApiRequest.ActivityMasterId
                         && x.SubActivityMasterId == nBFCSubActivityApiRequest.SubActivityMasterId && x.IsActive && !x.IsDeleted
                         && x.ProductCompanyActivityMasterId == nBFCSubActivityApiRequest.ProductCompanyActivityMasterId
                         select new NBFCSubActivityApiDTO
                         {
                             Sequence = x.Sequence,
                             ActivityMasterId = x.ActivityMasterId,
                             SubActivityMasterId = x.SubActivityMasterId,
                             APIUrl = com.APIUrl,
                             Code = com.Code,
                             NBFCCompanyApiId = x.NBFCCompanyApiId,
                             NBFCSubActivityApiId = x.Id,
                             ProductCompanyActivityMasterId = x.ProductCompanyActivityMasterId
                         }).OrderBy(x => x.Sequence);
            NBFCSubActivityApiDTO = query.ToList();
            return NBFCSubActivityApiDTO;
        }
    }
}
