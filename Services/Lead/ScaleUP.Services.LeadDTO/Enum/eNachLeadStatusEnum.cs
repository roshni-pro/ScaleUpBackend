using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Enum
{
    public enum eNachLeadStatusEnum

    {
        Initiated = 0,
        Fresh = 1,
        KYCProcessing = 2,
        KYCSuccess = 3,
        KYCRejected = 4,
        BankFailed = 5,
        BankProcessing = 6,
        BankAdded = 7,
        Approved = 8, //
        Reject = 9,
        Disbursed = 10, //AccountCredit & Account Transaction
        AgreementAccept = 11,
        AgreementFailed = 12
    }
}
