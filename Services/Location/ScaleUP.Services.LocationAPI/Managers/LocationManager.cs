using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LocationAPI.Helpers;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationModels.Master;
using ScaleUP.Services.LocationModels.Transaction;

namespace ScaleUP.Services.LocationAPI.Managers
{
    public class LocationManager
    {
        private ApplicationDbContext _context;

        public LocationManager(ApplicationDbContext context) { _context = context; }

        public List<LocationOnSaveResponse> SaveLocation(List<LocationOnSave> locationList)
        {
            List<LocationOnSaveResponse> responseList = new List<LocationOnSaveResponse>();

            foreach (var item in locationList)
            {
                Address loc = new Address
                {
                    AddressLineOne = item.AddressLineOne,
                    ZipCode = item.ZipCode,
                    AddressLineThree = item.AddressLineThree,
                    AddressLineTwo = item.AddressLineTwo,
                    CityId = item.CityId,
                    IsActive = true,
                    IsDeleted = false,
                    AddressTypeList = _context.AddressTypes.Where(x => x.Name == item.AddressType).ToList(),
                };
                _context.Addresses.Add(loc);
                _context.SaveChanges();

                responseList.Add(new LocationOnSaveResponse { AddressType = loc.AddressTypeList.First().Name, Id = loc.Id });


            }
            return responseList;

        }

        public LocationOnSaveResponse SaveRawLocation(LocationRawOnSave item)
        {

            string countryString = item != null && !string.IsNullOrEmpty(item.Country) ? StringHelper.RemoveSpecialCharAndSpaceFromLast(item.Country) : "";
            string StateString = item != null && !string.IsNullOrEmpty(item.State) ? StringHelper.RemoveSpecialCharAndSpaceFromLast(item.State) : "";
            string cityString = item != null && !string.IsNullOrEmpty(item.City) ? StringHelper.RemoveSpecialCharAndSpaceFromLast(item.City) : "";


            if (string.IsNullOrEmpty(countryString) || string.IsNullOrEmpty(StateString) || string.IsNullOrEmpty(cityString))
            {
                return new LocationOnSaveResponse
                {
                    AddressType = "",
                    Id = 0
                };
            }

            LocationOnSaveResponse locationOnSaveResponse = new LocationOnSaveResponse();
            var country = _context.Countries.Where(x => x.Name.ToLower() == item.Country.Trim().ToLower() && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (country == null)
            {
                country = new Country
                {
                    CountryCode = item.Country.Trim().Substring(0, 3).ToUpper(),
                    IsDeleted = false,
                    IsActive = true,
                    Name = item.Country.Trim(),
                    CurrencyCode = item.Country.Trim().Substring(0, 3).ToUpper()
                };
                _context.Countries.Add(country);
                _context.SaveChanges();
            }

            var state = _context.States.Where(x => x.Name.ToLower() == item.State.Trim().ToLower() && x.CountryId == country.Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (state == null)
            {
                state = new State
                {
                    StateCode = item.State.Trim().Substring(0, 3).ToUpper(),
                    IsDeleted = false,
                    IsActive = true,
                    Name = item.State.Trim(),
                    CountryId = country.Id
                };
                _context.States.Add(state);
                _context.SaveChanges();
            }

            var city = _context.Cities.Where(x => x.Name.ToLower() == item.City.Trim().ToLower() && x.StateId == state.Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (city == null)
            {
                city = new City
                {
                    CityCode = item.City.Trim().Substring(0, 3).ToUpper(),
                    IsDeleted = false,
                    IsActive = true,
                    Name = item.City.Trim(),
                    StateId = state.Id
                };
                _context.Cities.Add(city);
                _context.SaveChanges();
            }


            Address loc = new Address
            {
                AddressLineOne = item.AddressLineOne,
                ZipCode = item.ZipCode,
                AddressLineThree = item.AddressLineThree,
                AddressLineTwo = item.AddressLineTwo,
                CityId = city.Id,
                IsActive = true,
                IsDeleted = false,
                AddressTypeList = _context.AddressTypes.Where(x => x.Name == item.AddressType).ToList(),
            };
            _context.Addresses.Add(loc);
            _context.SaveChanges();

            locationOnSaveResponse = new LocationOnSaveResponse
            {
                Id = loc.Id,
                AddressType = item.AddressType
            };

            return locationOnSaveResponse;

        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            GRPCReply<List<AuditLogReply>> gRPCReply = new GRPCReply<List<AuditLogReply>>();
            AuditLogHelper auditLogHelper = new AuditLogHelper(_context);
            var auditLogs = await auditLogHelper.GetAuditLogs(request.Request.EntityId, request.Request.EntityName.Trim(), request.Request.Skip, request.Request.Take);
            if (auditLogs != null && auditLogs.Any())
            {
                gRPCReply.Status = true;
                gRPCReply.Response = auditLogs.Select(x => new AuditLogReply
                {
                    ModifiedDate = x.Timestamp,
                    Changes = x.Changes,
                    ModifiedBy = x.UserId,
                    TotalRecords = x.TotalRecords,
                    ActionType = x.Action
                }).ToList();
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Not Found";
            }
            return gRPCReply;
        }
    }
}

