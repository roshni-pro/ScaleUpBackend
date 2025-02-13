﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts
{
    [DataContract]
    public class ValidateEmailRequest
    {
        [DataMember(Order = 1)]
        public string Email { get; set; }
        [DataMember(Order = 2)]
        public string OTP { get; set; }
    }
}
