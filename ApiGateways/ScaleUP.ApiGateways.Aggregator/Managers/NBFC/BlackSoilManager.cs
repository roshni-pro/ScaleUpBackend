using Newtonsoft.Json;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Constants.NBFC;

namespace ScaleUP.ApiGateways.Aggregator.Managers.NBFC
{
    public class BlackSoilManager
    {
        private ILeadService _iLeadService;
        private ILoanAccountService _iloanAccountService;
        private LoanAccountManager _loanaccountmanager;

        public BlackSoilManager(ILeadService iLeadService, ILoanAccountService iloanAccountService, LoanAccountManager loanaccountmanager)
        {
            _iLeadService = iLeadService;
            _iloanAccountService = iloanAccountService;
            _loanaccountmanager = loanaccountmanager;

        }
        public async Task<GRPCReply<bool>> BlacksoilCallback(string requestcontent)
        {
            var result = new GRPCReply<bool>();
            var gRPCReply = new GRPCReply<long>();
            if (requestcontent != null)
            {
                var item = JsonConvert.DeserializeObject<BlackSoilCommonWebhookResponse>(requestcontent);
                if (item != null)
                {



                    GRPCRequest<BlackSoilWebhookRequest> _request = new GRPCRequest<BlackSoilWebhookRequest> { Request = new BlackSoilWebhookRequest { EventName = item.@event, Data = requestcontent } };

                    string apiName = "";
                    if (item.@event == BlackSoilWebhookConstant.ApplicationCreated || item.@event == BlackSoilWebhookConstant.LineApproved
                        || item.@event == BlackSoilWebhookConstant.LineInitiate || item.@event == BlackSoilWebhookConstant.LineActivate
                        || item.@event == BlackSoilWebhookConstant.Linerejected)
                    {
                        apiName = "LEAD";
                    }
                    else if (item.@event == BlackSoilWebhookConstant.InvoiceApproved || item.@event == BlackSoilWebhookConstant.InvoiceDisbursed
                        || item.@event == BlackSoilWebhookConstant.RepaymentCreated || item.@event == BlackSoilWebhookConstant.RepaymentCredited
                        || item.@event == BlackSoilWebhookConstant.InvoiceDisbursalProcessed || item.@event == BlackSoilWebhookConstant.RepaymentCaptured
                        || item.@event == BlackSoilWebhookConstant.InvoiceRejected || item.@event == BlackSoilWebhookConstant.LoanAccountProcessed
                        || item.@event == BlackSoilWebhookConstant.LoanAvailed
                        )
                    {
                        apiName = "LOAN";
                    }
                    else
                    {
                        apiName = "LEAD";
                    }
                    switch (apiName)
                    {
                        case "LEAD":
                            gRPCReply = await _iLeadService.BlacksoilCallback(_request);
                            // Post Disbursement
                            if (item.@event == BlackSoilWebhookConstant.LineActivate && gRPCReply.Response > 0)
                            {
                                await _loanaccountmanager.PostDisbursement(new PostDisbursementDTO
                                {

                                    leadId = gRPCReply.Response,
                                    Webhookresposne = requestcontent,

                                });

                            }
                            result.Status = gRPCReply.Status;
                            break;
                        case "LOAN":
                            gRPCReply = await _iloanAccountService.BlacksoilCallback(_request);
                            result.Status = gRPCReply.Status;

                            try
                            { // gRPCReply.Response is Id <= BlackSoilAccountTransactions
                                if (item.@event == BlackSoilWebhookConstant.InvoiceDisbursalProcessed && gRPCReply.Response > 0)
                                {
                                    await _loanaccountmanager.SendInvoiceDisbursmentSMS(gRPCReply.Response);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                            break;

                    }

                }
            }
            return result;
        }
    }
}
