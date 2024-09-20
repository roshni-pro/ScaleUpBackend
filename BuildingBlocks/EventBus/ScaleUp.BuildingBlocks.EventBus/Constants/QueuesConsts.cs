using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Constants
{
    public class QueuesConsts
    {
        // events
        public const string LeadActivityCreatedEventQueueName = "leadactivity-created-queue";
        public const string KYCSuccessEventQueueName = "kyc-success-queue";
        public const string KYCFailedEventQueueName = "kyc-failed-queue";
        public const string UpdatingAddressEventQueueName = "updating-address-queue";
        public const string UpdatingAadharAddressEventQueueName = "updating-aadhar-address-queue";
        public const string AccountDisbursementEventQueueName = "account-disbursement-queue";

        // messages
        public const string CreateLeadActivityMessageQueueName = "create-lead-activity-message-queue";
        public const string CompleteLeadActivityMessageQueueName = "completed-lead-activity-message-queue";
        public const string LeadUpdateHistoryMessageQueueName = "lead-update-history-message-queue";
        //public const string CompletePaymentMessageQueueName = "complete-payment-message-queue";
        //public const string StockRollBackMessageQueueName = "stock-rollback-message-queue";
    }
}
