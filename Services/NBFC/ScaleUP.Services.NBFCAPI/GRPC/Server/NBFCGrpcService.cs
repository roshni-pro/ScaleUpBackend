using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Services.NBFCAPI.Manager;
using ScaleUP.Services.NBFCAPI.Persistence;
using ScaleUP.Services.NBFCModels.Master;
using System.Data;

namespace ScaleUP.Services.NBFCAPI.GRPC.Server
{
    [Authorize]
    public class NBFCGrpcService : INBFCGrpcService
    {
        private readonly NBFCApplicationDbContext _context;
        private readonly NBFCGrpcManager _nBFCGrpcManager;
        public NBFCGrpcService(NBFCApplicationDbContext context, NBFCGrpcManager nBFCGrpcManager)
        {
            _context = context;
            _nBFCGrpcManager = nBFCGrpcManager;
        }

        [AllowAnonymous]
        public Task<GRPCReply<List<NBFCSelfOfferReply>>> GetCompanyselfOfferList(List<GenerateOfferRequest> request, CallContext context = default)
        {
            GRPCReply<List<NBFCSelfOfferReply>> reply = new GRPCReply<List<NBFCSelfOfferReply>>();
            var NBFCCompanyIdlist = new DataTable();
            NBFCCompanyIdlist.Columns.Add("companyId");
            NBFCCompanyIdlist.Columns.Add("leadId");
            NBFCCompanyIdlist.Columns.Add("VintageDays");
            NBFCCompanyIdlist.Columns.Add("AvgMonthlyBuying");
            NBFCCompanyIdlist.Columns.Add("CibilScore");
            NBFCCompanyIdlist.Columns.Add("CustomerType");

            foreach (var item in request)
            {
                var dr = NBFCCompanyIdlist.NewRow();
                dr["companyId"] = item.NBFCCompanyId;
                dr["leadId"] = item.LeadId;
                dr["VintageDays"] = item.VintageDays;
                dr["AvgMonthlyBuying"] = item.AvgMonthlyBuying;
                dr["CibilScore"] = item.CibilScore;
                dr["CustomerType"] = item.CustomerType;
                NBFCCompanyIdlist.Rows.Add(dr);
            }
            var nbfccompany = new SqlParameter("NBFCCompanyOffers", NBFCCompanyIdlist);
            nbfccompany.SqlDbType = SqlDbType.Structured;
            nbfccompany.TypeName = "dbo.NBFCCompanyOffer";

            var result = _context.Database.SqlQueryRaw<NBFCSelfOfferReply>("exec GetQualifiedOffer @NBFCCompanyOffers", nbfccompany).ToList();
            reply.Response = result;
            reply.Status = true;

            //ar response = AsyncContext.Run(async () => await _companyGrpcManager.GetCompanyLocationById(CompanyId));
            return Task.FromResult(reply);
            //return reply;
        }

        public Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> gRPCRequest, CallContext context = default)

        {
            var response = AsyncContext.Run(() => _nBFCGrpcManager.AddUpdateSelfConfiguration(gRPCRequest));
            return Task.FromResult(response);
        }
    }
}
