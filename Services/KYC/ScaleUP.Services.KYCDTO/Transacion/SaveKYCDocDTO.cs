using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class SaveKYCDocDTO<T>
    {
        public string UserId { get; set; }
        public T Input { get; set; }
    }
}
