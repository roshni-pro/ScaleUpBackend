using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts
{
    [DataContract]
    public class FileUploadRequest
    {
        [DataMember(Order = 1)]
        public byte[] FileStream { get; set; }
        [DataMember(Order = 2)]
        public bool IsValidForLifeTime { get; set; }
        [DataMember(Order = 3)]
        public int? ValidityInDays { get; set; } = null;
        [DataMember(Order = 4)]
        public string? SubFolderName { get; set; }
        [DataMember(Order = 5)]
        public string FileExtensionWithoutDot{ get; set; }

        [DataMember(Order = 6)]
        public string html { get; set; }

    }
}
