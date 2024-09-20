//using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
//using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.Interfaces;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.LocationAPI.Managers;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationModels.Master;
using ScaleUP.Services.LocationModels.Transaction;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ScaleUP.Services.LocationAPI.GRPC.Server
{
    public class LocationGrpcService : ILocationGrpcService
    {
        private readonly ApplicationDbContext _context;
        private readonly StateManager _stateManager;
        private readonly CityManager _cityManager;
        private readonly LocationManager _locationManager;
        public LocationGrpcService(ApplicationDbContext context, StateManager stateManager, CityManager cityManager, LocationManager locationManager)
        {
            _context = context;
            _stateManager = stateManager;
            _cityManager = cityManager;
            _locationManager = locationManager;
        }



        public  Task<LocationReply> CreateLocation(CreateLocationRequest request, CallContext context = default)
        {
            LocationReply locationReply = new LocationReply
            {
                Message = "Location not saved.",
                Status = false
            };
            bool locationExists = false;
            var AddressTypeId = _context.AddressTypes.Where(x => x.Name == "Business").Select(x=>x.Id).FirstOrDefault();
            if (request.ExistingLocationIds != null && request.ExistingLocationIds.Any())
            {
                var query = from a in _context.Addresses
                            from at in a.AddressTypeList
                            join exist in request.ExistingLocationIds on a.Id equals exist
                            select at.Id;
                var existingAddressTypes = query.ToList();

                if (existingAddressTypes != null && existingAddressTypes.Any())
                {
                    if (AddressTypeId >0)
                    {
                        locationExists = existingAddressTypes.Any(x => x == AddressTypeId);
                    }

                    if (locationExists)
                    {
                        locationReply.Message = "This Type Of Location already exists.";
                    }
                }
            }
            if (!locationExists)
            {
                Address address = new Address
                {
                    AddressLineOne = request.AddressLineOne,
                    AddressLineThree = request.AddressLineThree,
                    AddressLineTwo = request.AddressLineTwo,
                    ZipCode = request.ZipCode,
                    CityId = request.CityId,
                    AddressTypeList = _context.AddressTypes.Where(x => x.Id == AddressTypeId).ToList(),
                    IsActive = true,
                    IsDeleted = false
                };

                _context.Addresses.Add(address);
                if (_context.SaveChanges() > 0)
                {
                    locationReply.Status = true;
                    locationReply.Message = "Location saved successfully.";
                    locationReply.LocationId = address.Id;
                }
            }

            return Task.FromResult(locationReply);
        }





        public Task<CompanyAddressReply> GetCompanyAddress(List<long> request, CallContext context = default)
        {
            CompanyAddressReply locationResponse = new CompanyAddressReply();
            var query =
                        from a in _context.Addresses
                        //from at in a.AddressTypeList
                        join c in _context.Cities on a.CityId equals c.Id
                        join s in _context.States on c.StateId equals s.Id
                        join cn in _context.Countries on s.CountryId equals cn.Id
                        join lids in request on a.Id equals lids
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
                            //AddressTypeId = at.Id,
                            //AddressTypeName = at.Name
                        };

            var address = query.ToList();
            if (address != null && address.Any())
            {
                locationResponse.Status = true;
                locationResponse.GetAddressDTO = address;
                locationResponse.Message = "Company Location Found";
            }
            else
            {
                locationResponse.Status = false;
                locationResponse.Message = "Company Location Not Exists";
            }
            return Task.FromResult(locationResponse);
        }

        public Task<StateReply> GetStateByName(GSTverifiedRequest gSTverifiedRequestDTO, CallContext context = default)
        {
            //StateManager stateManager = new StateManager(_context);
            //return Task.FromResult(_stateManager.GetStateByName(gSTverifiedRequestDTO));
            var response = AsyncContext.Run(() => _stateManager.GetStateByName(gSTverifiedRequestDTO));
            return Task.FromResult(response);
        }


        public Task<GRPCReply<CityReply>> GetCityByName(GSTverifiedRequest gSTverifiedRequestDTO, CallContext context = default)
        {
            GRPCReply<CityReply> res = new GRPCReply<CityReply>();
            var response = AsyncContext.Run(() => _cityManager.GetCityByName(gSTverifiedRequestDTO));
            res.Response = response;
            res.Status = true;
            res.Message = "Found";
            return Task.FromResult(res);
        }

        public Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _locationManager.GetAuditLogs(request));
            return Task.FromResult(response);
        }


        [AllowAnonymous]
        public Task<GRPCReply<CityReply>> GetCityById(GRPCRequest<long> cityID , CallContext context = default)
        {
            GRPCReply<CityReply> res = new GRPCReply<CityReply>();
            var response = AsyncContext.Run(() => _cityManager.GetCityById(cityID));
            res.Response = response;
            res.Status = true;
            res.Message = "Found";
            return Task.FromResult(res);
        }

        public Task<GRPCReply<UpdateCompanyAddressRequest>> UpdateAddress(UpdateCompanyAddressRequest adds, CallContext context = default)
        {
            GRPCReply<UpdateCompanyAddressRequest> res = new GRPCReply<UpdateCompanyAddressRequest>();
            var address = _context.Addresses.FirstOrDefault(x => x.Id == adds.AddressId && x.IsActive && !x.IsDeleted);
            if (address != null)
            {
                address.AddressLineOne = adds.AddressLineOne;
                address.AddressLineTwo = adds.AddressLineTwo;
                address.AddressLineThree = adds.AddressLineThree;
                address.ZipCode = adds.ZipCode;
                address.LastModified = DateTime.Now;
                address.CityId = adds.CityId;
                _context.Entry(address).State = EntityState.Modified;

                if (_context.SaveChanges() > 0)
                {
                    res.Status = true;
                    res.Message = "Updated successfully.";
                    res.Response = adds;

                }
                else
                {
                    res.Status = false;
                    res.Message = "Issue during updating.";
                }
            }
            return Task.FromResult(res);
        }

        public Task<GRPCReply<bool>> AddCity(GRPCRequest<AddCityRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _cityManager.AddCity(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<CityMasterListReply>>> GetAllCities(CallContext context = default)
        {
            GRPCReply<List<CityMasterListReply>> res = new GRPCReply<List<CityMasterListReply>>();
            var response = AsyncContext.Run(async () =>await _cityManager.GetAllCities());
            res.Response = response.Response;
            res.Status = response.Status;
            res.Message = response.Message;
            return Task.FromResult(res);
        }

        [AllowAnonymous]
        public Task<GRPCReply<List<CityReply>>> GetCityListByIds(GRPCRequest<List<long>> req, CallContext context = default)
        {
            GRPCReply<CityReply> res = new GRPCReply<CityReply>();
            var response = AsyncContext.Run(() => _cityManager.GetCityListByIds(req));
            return Task.FromResult(response);
        }
        
        [AllowAnonymous]
        public Task<GRPCReply<long>> AddAddress(UpdateCompanyAddressRequest adds, CallContext context = default)
        {
            GRPCReply<long> res = new GRPCReply<long>();
            var address = _context.Addresses.FirstOrDefault(x => x.Id == adds.AddressId && x.IsActive && !x.IsDeleted);
            if (address != null)
            {
                List<LocationOnSave> locations = new List<LocationOnSave>();
                locations.Add(new LocationOnSave
                {
                    AddressLineOne = adds.AddressLineOne,
                    AddressLineTwo = adds.AddressLineTwo,
                    AddressLineThree = adds.MasterName == "PersonalDetail" ? adds.AddressLineThree : "",
                    ZipCode = adds.ZipCode,                    
                    CityId = adds.CityId,
                    AddressType = AddressTypeConstants.Current
                });
                var response = _locationManager.SaveLocation(locations);

                long CurentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Current).First().Id;
                if(CurentLocationId > 0)
                {
                    res.Response = CurentLocationId;
                    res.Status = true;
                    res.Message = "Address Saved Successfully!!";
                }
                else
                {
                    res.Response = CurentLocationId;
                    res.Status = false;
                    res.Message = "Something went wrong!!";
                }
                
            }
            return Task.FromResult(res);
        }

        //[AllowAnonymous]
        //public async Task<GRPCReply<List<KarzaElectricityStateReply>>> GetKarzaElectricityState(string state, CallContext context = default)
        //{
        //    GRPCReply<List<KarzaElectricityStateReply>> res = new GRPCReply<List<KarzaElectricityStateReply>>();  

        //    res.Response = _context.karzaElectricityDistricts.Where(x => x.State == state && x.IsActive && !x.IsDeleted).Select(x => new KarzaElectricityStateReply
        //    {
        //        DistrictCode = x.DistrictCode,
        //        DistrictName = x.DistrictName,
        //        State = x.State
        //    }).Distinct().OrderBy(x => x.DistrictName).ToList();

        //    if(res.Response.Count > 0)
        //    {
        //        res.Status = true;
        //    }

        //    return res;


        //}

        public Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities(LeadCityIds req, CallContext context = default)
        {
            GRPCReply<List<CityMasterListReply>> res = new GRPCReply<List<CityMasterListReply>>();
            var response = AsyncContext.Run(async () => await _cityManager.GetAllLeadCities(req));
            res.Response = response.Response;
            res.Status = response.Status;
            res.Message = response.Message;
            return Task.FromResult(res);
        }

        public Task<GRPCReply<CityMasterListReply>> GetCityDetails(GRPCRequest<long> req, CallContext context = default)
        {
            GRPCReply<CityMasterListReply> res = new GRPCReply<CityMasterListReply>();
            var response = AsyncContext.Run(async () => await _cityManager.GetCityDetails(req));
            res.Response = response.Response;
            res.Status = response.Status;
            res.Message = response.Message;
            return Task.FromResult(res);
        }
    }
}
