using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.MediaDTO.Transaction
{
    public class FileUploadDTO
    {
        public IFormFile FileDetails { get; set; }
        public bool IsValidForLifeTime { get; set; }
        public int? ValidityInDays { get; set; } = null;
        public string? SubFolderName { get; set; }

    }
    public class MultiFileUploadDTO
    {
        public List<IFormFile> FileDetails { get; set; }
        public bool IsValidForLifeTime { get; set; }
        public int? ValidityInDays { get; set; } = null;
        public string? SubFolderName { get; set; }

    }
}
