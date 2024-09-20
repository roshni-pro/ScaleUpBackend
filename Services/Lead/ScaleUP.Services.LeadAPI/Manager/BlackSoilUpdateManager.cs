using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadModels.LeadNBFC;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class BlackSoilUpdateManager
    {
        private readonly LeadApplicationDbContext _context;

        public BlackSoilUpdateManager(LeadApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<ResultViewModel<bool>> UpdateApplicationId(long Id , long ApplicationId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();

            var UpdateAppliId = _context.BlackSoilUpdates.Where(x=> x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateAppliId != null) 
            {
                UpdateAppliId.ApplicationId = ApplicationId;
                _context.SaveChanges();

                res = new ResultViewModel<bool> 
                {
                    IsSuccess = true,
                    Message = "SuccessFully updated!",
                    Result  = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed!",
                    Result = false
                };
            }
            return res;
        }

        public async Task<ResultViewModel<bool>> UpdateDocId(long Id, long AgreementDocId , long SanctionDocId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();

            var UpdateDocId = _context.BlackSoilUpdates.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateDocId != null)
            {
                UpdateDocId.AgreementDocId = AgreementDocId;
                UpdateDocId.SanctionDocId = SanctionDocId;
                _context.SaveChanges();

                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "SuccessFully updated!",
                    Result = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed!",
                    Result = false
                };
            }
            return res;
        }

        public async Task<ResultViewModel<bool>> UpdateStampId(long Id, long StampId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();

            var UpdateStampId = _context.BlackSoilUpdates.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateStampId != null)
            {
                UpdateStampId.StampId = StampId;
                _context.SaveChanges();

                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "SuccessFully updated!",
                    Result = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed!",
                    Result = false
                };
            }
            return res;
        }

        public async Task<ResultViewModel<bool>> UpdateSingingUrl(long Id, string SingingUrl)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();

            var UpdateSingingUrl = _context.BlackSoilUpdates.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateSingingUrl != null)
            {
                UpdateSingingUrl.SingingUrl = SingingUrl;
                _context.SaveChanges();

                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "SuccessFully updated!",
                    Result = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed!",
                    Result = false
                };
            }
            return res;
        }
        public async Task<ResultViewModel<bool>> UpdateESignId(long Id, long eSignId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();

            var UpdateStampId = _context.BlackSoilUpdates.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateStampId != null)
            {
                UpdateStampId.ESingingId = eSignId;
                _context.SaveChanges();

                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "SuccessFully updated!",
                    Result = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed!",
                    Result = false
                };
            }
            return res;
        }

        public async Task<BlackSoilUpdate> GetById(long LeadId)
        {
            BlackSoilUpdate blackSoilUpdate = new BlackSoilUpdate();
            blackSoilUpdate = _context.BlackSoilUpdates.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            return blackSoilUpdate;
             
        }

    }
}

