using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.MediaDTO.Transaction
{
    public class FileUploadReply
    {
        public bool status { get; set; }
        public string Message { get; set; }
        public long DocId { get; set; }
        public string FilePath { get; set; }
    }
    public class FileUploadReplyDc
    {
        public bool status { get; set; }
        public string Message { get; set; }
        public List<DocDc> DocList { get; set; }
    }
    public class DocDc
    {
        public string DocId { get; set; }
        public string FilePath { get; set; }
    }
}
