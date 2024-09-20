using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationDTO.City;
using ScaleUP.Services.LocationDTO.Master;
using ScaleUP.Services.LocationModels.Master;
using System.Data.Common;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ScaleUP.Services.LocationAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //private readonly IPublisher _publisher;
        public CityController(ApplicationDbContext context)
        {
            _context = context;
            //_publisher = publisher;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddCity")]
        public async Task<LocationResponse> AddCity(AddCityDTO command)
        {
            LocationResponse cityResponse = new LocationResponse();
            var exist = _context.Cities.Any(x => x.Name.Trim().ToLower() == command.CityName.Trim().ToLower() && x.StateId == command.StateId && x.IsActive && !x.IsDeleted);
            if (exist)
            {
                cityResponse.Status = false;
                cityResponse.Message = "City Name Already Exists!!";
                return cityResponse;
            }
            City city = new City
            {
                Name = command.CityName,
                StateId = command.StateId,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Now,
                CreatedBy = null
            };
            var res = _context.Cities.Add(city);
            int exec = await _context.SaveChangesAsync();
            if (exec > 0)
            {
                cityResponse.Status = true;
                cityResponse.Message = "City saved.";

            }
            else
            {
                cityResponse.Status = false;
                cityResponse.Message = "Issue during saving city.";
                cityResponse.ReturnObject = city;
            }

            return cityResponse;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetCityByStateId")]
        public async Task<ActionResult<List<LocationDTO.Master.LocationDTO>>> GetCityByStateId(long stateId)
        {

            var cityList = await _context.Cities
                .Where(x => x.StateId == stateId && x.IsActive == true && x.IsDeleted == false)
                .Select(y => new LocationDTO.Master.LocationDTO
                {
                    Id = y.Id,
                    Name = y.Name,
                    ShortName = y.CityCode
                }).OrderBy(x => x.Name)
                .ToListAsync();
            return cityList;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetCityByCityId")]
        public async Task<LocationResponse> GetCityByCityId(long cityId)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "City not exists.";
            var cityList = await _context.Cities.Where(x => x.Id == cityId).FirstOrDefaultAsync();
            if (cityList != null)
            {
                locationResponse.Status = true;
                locationResponse.Message = "Data found.";
                locationResponse.ReturnObject = cityList;
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllCities")]
        public async Task<ActionResult<List<CityMasterListDTO>>> GetAllCities()
        {
            var cityList = await _context.Cities.Join(_context.States,
                                city => city.StateId,
                                state => state.Id,
                                (city, state) => new { Cities = city, States = state })
                                .Where(o => o.Cities.IsDeleted == false)
                                .Select(y => new CityMasterListDTO
                                {
                                    Id = y.Cities.Id,
                                    CityName = y.Cities.Name,
                                    status = y.Cities.IsActive,
                                    StateName = y.States.Name,
                                }).ToListAsync();
            return cityList;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateCity")]
        public async Task<LocationResponse> UpdateCity(AddCityDTO cityobj)
        {
            LocationResponse resp = new LocationResponse();
            resp.Status = false;

            var existCityName = await _context.Cities
                .FirstOrDefaultAsync(x => x.StateId == cityobj.StateId && x.Name.Trim().ToLower() == cityobj.CityName.Trim().ToLower() && x.Id != cityobj.Id && !x.IsDeleted);
            if (existCityName == null)
            {
                var city = await _context.Cities
                    .FirstOrDefaultAsync(x => x.Id == cityobj.Id && x.IsDeleted == false);
                if (city != null)
                {
                    city.Name = cityobj.CityName;
                    city.StateId = cityobj.StateId;
                    city.LastModified = DateTime.Now;

                    _context.Entry(city).State = EntityState.Modified;
                    int rowchanged = await _context.SaveChangesAsync();
                    if (rowchanged > 0)
                    {
                        resp.Status = true;
                        resp.Message = "City updated successfully.";

                    }
                    else
                    {
                        resp.Status = false;
                        resp.Message = "Issue during city updating.";
                    }
                }
                else
                {
                    resp.Message = "city not Exist.";
                }
            }
            else
            {
                resp.Message = "city name already exist for this state.";
            }
            return resp;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteCity")]
        public async Task<LocationResponse> DeleteCity(long cityId)
        {
            LocationResponse cityResponse = new LocationResponse();
            cityResponse.Status = false;
            City? _city = await _context.Cities.FirstOrDefaultAsync(x => x.Id == cityId);
            if (_city != null)
            {
                _city.Deleted = DateTime.Now;
                _city.IsDeleted = true;
                _city.IsActive = false;
                _context.Entry(_city).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    cityResponse.Status = true;
                    cityResponse.Message = "City deleted successfully.";

                }
                else
                {
                    cityResponse.Status = false;
                    cityResponse.Message = "Issue during city deleting.";
                }
            }

            return cityResponse;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateCityStatus")]
        public async Task<LocationResponse> UpdateCityStatus(long cityId, bool IsActive)
        {
            LocationResponse resp = new LocationResponse();
            resp.Status = false;
            var _city = await _context.Cities
                .FirstOrDefaultAsync(x => x.Id == cityId);
            if (_city != null)
            {
                if (IsActive)
                {
                    var exist = _context.Cities.Any(x => x.Name.Trim().ToLower() == _city.Name.Trim().ToLower() && x.Id != cityId && x.StateId == _city.StateId && x.IsActive && !x.IsDeleted);
                    if (exist)
                    {
                        resp.Status = false;
                        resp.Message = "City Name Already Exists!!";
                        return resp;
                    }
                }
                _city.IsActive = IsActive;
                _city.LastModified = DateTime.Now;
                _context.Entry(_city).State = EntityState.Modified;

                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    resp.Status = true;
                    resp.Message = "City status updated successfully.";

                }
                else
                {
                    resp.Status = false;
                    resp.Message = "Issue during city status updating.";
                }
            }
            else
            {
                resp.Message = "city not Exist.";
            }
            return resp;

        }
    }
}
