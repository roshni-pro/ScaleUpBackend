using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.NBFCModels.Master;
using ScaleUP.Services.NBFCAPI.Persistence;

namespace ScaleUP.Services.NBFCAPI.Manager
{
    public class NBFCGrpcManager
    {
        private readonly NBFCApplicationDbContext _context;
        public NBFCGrpcManager(NBFCApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> gRPCRequest)

        {
            GRPCReply<List<DefaultOfferSelfConfigurationDc>> response = new GRPCReply<List<DefaultOfferSelfConfigurationDc>>();
            foreach (var config in gRPCRequest.Request)
            {
                if (config.Id > 0)
                {
                    var existing = await _context.OfferSelfConfigurations.FirstOrDefaultAsync(x => x.Id == config.Id);
                    if (existing != null)
                    {
                        existing.CompanyId = config.CompanyId;
                        existing.CustomerType = config.CustomerType;
                        existing.MaxCibilScore = config.MaxCibilScore;
                        existing.MaxCreditLimit = config.MaxCreditLimit;
                        existing.MaxVintageDays = config.MaxVintageDays;
                        existing.MinCibilScore = config.MinCibilScore;
                        existing.MinCreditLimit = config.MinCreditLimit;
                        existing.MinVintageDays = config.MinVintageDays;
                        existing.MultiPlier = config.MultiPlier;
                        existing.IsActive = config.IsActive;

                        _context.Entry(existing).State = EntityState.Modified;
                    }
                }
                else
                {
                    OfferSelfConfiguration offerSelfConfiguration = new OfferSelfConfiguration
                    {
                        CompanyId = config.CompanyId,
                        CustomerType = config.CustomerType,
                        MaxCibilScore = config.MaxCibilScore,
                        MaxCreditLimit = config.MaxCreditLimit,
                        MaxVintageDays = config.MaxVintageDays,
                        MinCibilScore = config.MinCibilScore,
                        MinCreditLimit = config.MinCreditLimit,
                        MinVintageDays = config.MinVintageDays,
                        MultiPlier = config.MultiPlier,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.OfferSelfConfigurations.Add(offerSelfConfiguration);
                }
            }
            int rowChanged = await _context.SaveChangesAsync();
            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = "Configuration Updated Successfully";
                response.Response = gRPCRequest.Request;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed To Update Configs";
            }
            return response;
        }
    }
}
