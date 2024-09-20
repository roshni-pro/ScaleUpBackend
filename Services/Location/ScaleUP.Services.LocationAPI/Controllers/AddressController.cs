using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationDTO.Master;
using ScaleUP.Services.LocationDTO.Transaction;
using ScaleUP.Services.LocationModels.Master;
using ScaleUP.Services.LocationModels.Transaction;

namespace ScaleUP.Services.LocationAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AddressController : BaseController
    {

        private readonly ApplicationDbContext _context;
        //private readonly IPublisher _publisher;
        public AddressController(ApplicationDbContext context)
        {
            _context = context;
            //_publisher = publisher;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddAddress")]
        public async Task<LocationResponse> AddAddress(AddAddressDTO command)
        {
            LocationResponse Response = new LocationResponse();
            Address address = new Address
            {
                AddressLineOne = command.AddressLineOne,
                AddressLineThree = command.AddressLineThree,
                AddressLineTwo = command.AddressLineTwo,
                ZipCode = command.ZipCode,
                CityId = command.CityId,
                AddressTypeList = _context.AddressTypes.Where(x => x.Id == command.AddressTypeId).ToList(),
                IsActive = true,
                IsDeleted = false
            };

            //command.Status = "Pending";
            _context.Addresses.Add(address);
            int res = await _context.SaveChangesAsync();
            if (res > 0)
            {
                Response.Status = true;
                Response.Message = "Address saved.";

            }
            else
            {
                Response.Status = false;
                Response.Message = "Issue during saving.";
                Response.ReturnObject = address;
            }

            return Response;


            //return address.Id;


        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAddress")]
        public async Task<LocationResponse> GetAddress(long addressId)
        {
            LocationResponse locationResponse = new LocationResponse();
            var query =
                        from a in _context.Addresses
                        from at in a.AddressTypeList
                        join c in _context.Cities on a.CityId equals c.Id
                        join s in _context.States on c.StateId equals s.Id
                        join cn in _context.Countries on s.CountryId equals cn.Id
                        where a.Id == addressId
                        select new GetAddressDTO
                        {
                            AddressLineOne = a.AddressLineOne,
                            AddressLineTwo = a.AddressLineTwo,
                            AddressLineThree = a.AddressLineThree,
                            ZipCode = a.ZipCode,
                            CityName = c.Name,
                            CityId = c.Id,
                            StateName = s.Name,
                            StateId = s.Id,
                            CountryName = cn.Name,
                            CountryId = cn.Id,
                            Id = a.Id,
                            AddressTypeId = at.Id,
                            AddressTypeName = at.Name
                        };

            GetAddressDTO? address = await query.FirstOrDefaultAsync();
            if (address != null)
            {
                locationResponse.Status = true;
                locationResponse.ReturnObject = address;
                locationResponse.Message = "Company Location Found";
            }
            else
            {
                locationResponse.Status = false;
                locationResponse.Message = "Company Location Not Exists";
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetAddressListByIds")]
        public async Task<LocationResponse> GetAddress(List<long> addressIdList)
        {
            LocationResponse locationResponse = new LocationResponse();
            var query =
                        from a in _context.Addresses
                        from at in a.AddressTypeList
                        join c in _context.Cities on a.CityId equals c.Id
                        join s in _context.States on c.StateId equals s.Id
                        join cn in _context.Countries on s.CountryId equals cn.Id
                        join al in addressIdList on a.Id equals al
                        select new GetAddressDTO
                        {
                            AddressLineOne = a.AddressLineOne,
                            AddressLineTwo = a.AddressLineTwo,
                            AddressLineThree = a.AddressLineThree,
                            ZipCode = a.ZipCode,
                            CityName = c.Name,
                            CityId = c.Id,
                            StateName = s.Name,
                            StateId = s.Id,
                            CountryName = cn.Name,
                            CountryId = cn.Id,
                            Id = a.Id,
                            AddressTypeId = at.Id,
                            AddressTypeName = at.Name
                        };

            var addressList = await query.ToListAsync();
            if (addressList != null && addressList.Any())
            {
                locationResponse.Status = true;
                locationResponse.ReturnObject = addressList;
                locationResponse.Message = "Company Locations Found";
            }
            else
            {
                locationResponse.Status = false;
                locationResponse.Message = "Company Locations Not Exists";
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateAddress")]
        public async Task<LocationResponse> UpdateAddress(AddressDto adds)
        {
            LocationResponse Response = new LocationResponse();
            var address = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == adds.AddressId && x.IsActive && !x.IsDeleted);
            if (address != null)
            {
                address.AddressLineOne = adds.AddressLineOne;
                address.AddressLineTwo = adds.AddressLineTwo;
                address.AddressLineThree = adds.AddressLineThree;
                address.ZipCode = adds.ZipCode;
                address.LastModified = DateTime.Now;
                address.CityId = adds.CityId;

                _context.Entry(address).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    Response.Status = true;
                    Response.Message = "Updated successfully.";
                    Response.ReturnObject = address;

                }
                else
                {
                    Response.Status = false;
                    Response.Message = "Issue during updating.";
                }
            }
            return Response;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAddressType")]
        public async Task<LocationResponse> GetAddressType()
        {
            LocationResponse Response = new LocationResponse();
            var addressTypeDTOs = await _context.AddressTypes.Where(x => x.IsActive && !x.IsDeleted).Select(x => new AddressTypeDTO { AddressTypeId = x.Id, AddressTypeName = x.Name }).ToListAsync();
            if (addressTypeDTOs != null && addressTypeDTOs.Any())
            {
                Response.Status = true;
                Response.Message = "Data Found.";
                Response.ReturnObject = addressTypeDTOs;

            }
            else
            {
                Response.Status = false;
                Response.Message = "No Data Found.";
            }
            return Response;
        }
    }
}
