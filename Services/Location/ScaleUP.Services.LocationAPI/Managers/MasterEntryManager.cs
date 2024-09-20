using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationModels.Master;

namespace ScaleUP.Services.LocationAPI.Managers
{
    public class MasterEntryManager
    {
        private ApplicationDbContext _context;

        public MasterEntryManager(ApplicationDbContext context) { _context = context; }

        public void EnterAddressType()
        {
            var addrestype = _context.AddressTypes.Where(x => x.IsActive && !x.IsDeleted).ToList();
            if (addrestype != null)
            {
                foreach (var item in addrestype)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.SaveChanges();
                }
            }

            var Billing = addrestype.Where(x => x.Name == AddressTypeConstants.Billing).FirstOrDefault();
            if (Billing != null)
            {
                Billing.IsActive = true;
                Billing.IsDeleted = false;
                _context.SaveChanges();
            }
            else
            {
                AddressType Addaddress = null;
                Addaddress = new AddressType
                {
                    IsActive = true,
                    Name = AddressTypeConstants.Billing,
                    IsDeleted = false,  
                };
                _context.AddressTypes.Add(Addaddress);
                _context.SaveChanges();
            }

            var Shipping = addrestype.Where(x => x.Name == AddressTypeConstants.Shipping).FirstOrDefault();
            if (Shipping != null)
            {
                Shipping.IsActive = true;
                Shipping.IsDeleted = false;
                _context.SaveChanges();
            }
            else
            {
                AddressType Addaddress = null;
                Addaddress = new AddressType
                {
                    IsActive = true,
                    Name = AddressTypeConstants.Shipping,
                    IsDeleted = false,
                };
                _context.AddressTypes.Add(Addaddress);
                _context.SaveChanges();
            }

            var Current = addrestype.Where(x => x.Name == AddressTypeConstants.Current).FirstOrDefault();
            if (Current != null)
            {
                Current.IsActive = true;
                Current.IsDeleted = false;
                _context.SaveChanges();
            }
            else
            {
                AddressType Addaddress = null;
                Addaddress = new AddressType
                {
                    IsActive = true,
                    Name = AddressTypeConstants.Current,
                    IsDeleted = false,
                };
                _context.AddressTypes.Add(Addaddress);
                _context.SaveChanges();
            }

            var Permanent = addrestype.Where(x => x.Name == AddressTypeConstants.Permanent).FirstOrDefault();
            if (Permanent != null)
            {   
                Permanent.IsActive = true;
                Permanent.IsDeleted = false;
                _context.SaveChanges();
            }
            else
            {
                AddressType Addaddress = null;
                Addaddress = new AddressType
                {
                    IsActive = true,
                    Name = AddressTypeConstants.Permanent,
                    IsDeleted = false,
                };
                _context.AddressTypes.Add(Addaddress);
                _context.SaveChanges();
            }
        }
    }
}
