

using System.Runtime.Serialization;

namespace ScaleUP.Services.ProductDTO.Master
{
    [DataContract]
    public class ProductResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic ReturnObject { get; set; }
    }
    public class ProductResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T ReturnObject { get; set; }
    }
}
