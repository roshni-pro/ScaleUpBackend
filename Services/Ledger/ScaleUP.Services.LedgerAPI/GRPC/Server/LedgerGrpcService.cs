using ScaleUP.BuildingBlocks.GRPC.Contracts.Ledger.Interfaces;
using ScaleUP.Services.LedgerAPI.Persistence;

namespace ScaleUP.Services.LedgerAPI.GRPC.Server
{
    public class LedgerGrpcService : ILedgerGrpcService
    {
        private readonly LedgerApplicationDbContext _context;
        public LedgerGrpcService(LedgerApplicationDbContext context)
        {
            _context = context;
        }
    }
}
