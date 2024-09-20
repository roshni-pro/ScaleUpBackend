using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Ledger.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
   
    public class LedgerService : ILedgerService
    {
        private IConfiguration Configuration;
        private readonly ILedgerGrpcService _client;

        public LedgerService(IConfiguration _configuration
            , ILedgerGrpcService client
            )
        {
            Configuration = _configuration;
            _client = client;
        }
    }
}
