using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyModels.Master
{
    public class GSTverifiedRequest
    {
        [Key]
        public int GSTVerifiedRequestId { get; set; }
        public string RequestPath { get; set; }
        [StringLength(100)]
        public string RefNo { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(300)]
        public string ShopName { get; set; }
        [StringLength(600)]
        public string ShippingAddress { get; set; }
        [StringLength(100)]
        public string City { get; set; }
        [StringLength(100)]
        public string State { get; set; }
        [StringLength(200)]
        public string Citycode { get; set; }
        [StringLength(200)]
        public string RegisterDate { get; set; }
        [StringLength(500)]
        public string CustomerBusiness { get; set; }
        [StringLength(500)]
        public string HomeName { get; set; }
        [StringLength(200)]
        public string HomeNo { get; set; }
        [StringLength(200)]
        public string LastUpdate { get; set; }
        [StringLength(200)]
        public string PlotNo { get; set; }
        [StringLength(200)]
        public string lat { get; set; }
        [StringLength(200)]
        public string lg { get; set; }
        [StringLength(200)]
        public string Zipcode { get; set; }
        public bool Delete { get; set; }
        [StringLength(200)]
        public string Active { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Message { get; set; }
    }
}
