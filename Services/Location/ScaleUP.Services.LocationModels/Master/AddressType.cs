using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LocationModels.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationModels.Master
{
    public class AddressType : BaseAuditableEntity
    {
        public string Name { get; set; }

        public ICollection<Address> AddressList { get; }
    }
}
