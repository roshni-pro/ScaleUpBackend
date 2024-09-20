
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationDTO.City;
using ScaleUP.Services.LocationDTO.Master;
using ScaleUP.Services.LocationModels.Master;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ScaleUP.Services.LocationAPI.Managers
{
    public class CityManager
    {
        private ApplicationDbContext _context;

        public CityManager(ApplicationDbContext context) { _context = context; }

        public async Task<CityReply> GetCityByName(GSTverifiedRequest gSTverifiedRequestDTO)
        {
            CityReply cityReply = new CityReply();
            var query = from s in _context.Cities
                        where s.Name.ToLower() == gSTverifiedRequestDTO.City.Trim().ToLower()
                            && s.StateId == gSTverifiedRequestDTO.stateId
                        select new CityReply
                        {
                            cityId = s.Id,
                            cityName = s.Name
                        };
            cityReply = query.FirstOrDefault();
            if (cityReply != null)
            {
                return cityReply;
            }
            else
            {
                cityReply = await postCity(gSTverifiedRequestDTO);
                return cityReply;
            }

        }

        private async Task<CityReply> postCity(GSTverifiedRequest gSTverifiedRequestDTO)
        {
            CityReply cityReply = new CityReply();
            var stateId = _context.States.FirstOrDefault(x => x.Name.ToLower() == gSTverifiedRequestDTO.State.ToLower() && x.IsActive && !x.IsDeleted).Id;
            City city = new City
            {
                Name = gSTverifiedRequestDTO.City,
                StateId = stateId,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Now,
                CreatedBy = null
            };
            var res = _context.Cities.Add(city);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                var query = from s in _context.Cities
                            where s.Name.ToLower() == gSTverifiedRequestDTO.City.Trim().ToLower()
                                && s.StateId == stateId
                            select new CityReply
                            {
                                cityId = s.Id,
                                cityName = s.Name
                            };
                cityReply = query.FirstOrDefault();
                return cityReply;
            }
            else
            {
                return cityReply;
            }
        }


        public async Task<CityReply> GetCityById(GRPCRequest<long> cityId)
        {
            CityReply cityReply = new CityReply();
            var query = from s in _context.Cities
                        where s.Id == cityId.Request
                        select new CityReply
                        {
                            cityId = s.Id,
                            cityName = s.Name
                        };
            cityReply = query.FirstOrDefault();
            if (cityReply != null)
            {
                return cityReply;
            }
            else
            {
                return cityReply;
            }
        }

        public async Task<GRPCReply<bool>> AddCity(GRPCRequest<AddCityRequest> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            var state = await _context.States.FirstOrDefaultAsync(x => x.Name.ToLower() == request.Request.StateName.ToLower());
            if (state == null)
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "State Not Found";
                gRPCReply.Response = false;
                return gRPCReply;
            }
            var exist = _context.Cities.Any(x => x.Name.Trim().ToLower() == request.Request.CityName.Trim().ToLower() && x.StateId == state.Id && x.IsActive && !x.IsDeleted);
            if (exist)
            {
                gRPCReply.Status = true;
                gRPCReply.Response = true;
                gRPCReply.Message = "City Name Already Exists";
                return gRPCReply;
            }
            City city = new City
            {
                Name = request.Request.CityName,
                StateId = state.Id,
                IsActive = true,
                IsDeleted = false
            };
            var res = _context.Cities.Add(city);
            int exec = await _context.SaveChangesAsync();
            if (exec > 0)
            {
                gRPCReply.Status = true;
                gRPCReply.Response = true;
                gRPCReply.Message = "City Added";

            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Response = false;
                gRPCReply.Message = "Failed to Add City";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<List<CityMasterListReply>>> GetAllCities()
        {
            GRPCReply<List<CityMasterListReply>> gRPCReply = new GRPCReply<List<CityMasterListReply>>();
            gRPCReply.Status = false;
            gRPCReply.Message = "";
            var cityList = await _context.Cities.Join(_context.States,
                                city => city.StateId,
                                state => state.Id,
                                (city, state) => new { Cities = city, States = state })
                                .Where(o => o.Cities.IsDeleted == false)
                                .Select(y => new CityMasterListReply
                                {
                                    Id = y.Cities.Id,
                                    CityName = y.Cities.Name,
                                    status = y.Cities.IsActive,
                                    StateName = y.States.Name,
                                }).ToListAsync();
            if (cityList.Count > 0)
            {
                var reply = cityList.ToList();
                gRPCReply.Response = reply;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<CityReply>>> GetCityListByIds(GRPCRequest<List<long>> req)
        {
            GRPCReply<List<CityReply>> reply = new GRPCReply<List<CityReply>>();
            var cityList = await _context.Cities.Where(x=> req.Request.Contains(x.Id) && x.IsActive == true && x.IsDeleted==false).Select(y=> new CityReply { cityId = y.Id , cityName = y.Name}).ToListAsync();
            if (cityList != null)
            {
                reply.Response = cityList;
                reply.Status = true;
                reply.Message = "Data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Data not found";
            }
            return reply ;
        }

        public async Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities(LeadCityIds req)
        {
            GRPCReply<List<CityMasterListReply>> gRPCReply = new GRPCReply<List<CityMasterListReply>>();
            gRPCReply.Status = false;
            gRPCReply.Message = "";
            var cityList = await _context.Cities.Join(_context.States,
                                city => city.StateId,
                                state => state.Id,
                                (city, state) => new { Cities = city, States = state })
                                .Where(o => o.Cities.IsDeleted == false && req.CityIds.Contains(o.Cities.Id))
                                .Select(y => new CityMasterListReply
                                {
                                    Id = y.Cities.Id,
                                    CityName = y.Cities.Name,
                                    status = y.Cities.IsActive,
                                    StateName = y.States.Name,
                                }).ToListAsync();
            if (cityList.Count > 0)
            {
                var reply = cityList.ToList();
                gRPCReply.Response = reply;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<CityMasterListReply>> GetCityDetails(GRPCRequest<long> cityId)
        {
            GRPCReply<CityMasterListReply> gRPCReply = new GRPCReply<CityMasterListReply>();
            gRPCReply.Status = false;
            gRPCReply.Message = "";
            var cityList = await _context.Cities.Join(_context.States,
                                city => city.StateId,
                                state => state.Id,
                                (city, state) => new { Cities = city, States = state })
                                .Where(o => o.Cities.IsDeleted == false && o.Cities.Id == cityId.Request)
                                .Select(y => new CityMasterListReply
                                {
                                    Id = y.Cities.Id,
                                    CityName = y.Cities.Name,
                                    status = y.Cities.IsActive,
                                    StateName = y.States.Name,
                                    stateId = y.States.Id,
                                }).FirstOrDefaultAsync();
            if (cityList != null)
            {
                var reply = cityList;
                gRPCReply.Response = reply;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            return gRPCReply;
        }


    }
}
