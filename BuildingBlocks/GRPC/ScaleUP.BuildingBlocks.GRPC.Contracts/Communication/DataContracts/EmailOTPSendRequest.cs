using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts
{
    public class EmailOTPSendRequest
    {
        [DataMember(Order = 1)]
        public string From { get; set; }
        [DataMember(Order = 2)]
        public string To { get; set; }
        [DataMember(Order = 3)]
        public string BCC { get; set; }

        [DataMember(Order = 4)]
        public string Subject { get; set; }
        [DataMember(Order = 5)]
        public string Message { get; set; }
        [DataMember(Order = 6)]
        public string File { get; set; }

        [DataMember(Order = 7)]
        public int ExpiredInMin { get; set; }
        
        [DataMember(Order = 8)]
        public string OTP { get; set; }

    }
}
