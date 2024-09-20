using Microsoft.AspNetCore.Mvc;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationDTO.State;
using ScaleUP.Services.LocationModels.Master;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LocationDTO.Master;
using ScaleUP.Global.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace ScaleUP.Services.LocationAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StateController : BaseController
    {

        private readonly ApplicationDbContext _context;
        //private readonly IPublisher _publisher;
        public StateController(ApplicationDbContext context)
        {
            _context = context;
            //_publisher = publisher;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddState")]
        public async Task<LocationResponse> AddState(AddStateDTO command)
        {
            LocationResponse response = new LocationResponse();
            var exist = await _context.States.FirstOrDefaultAsync(x => (x.Name.Trim().ToLower() == command.Name.Trim().ToLower() || x.StateCode.Trim().ToLower() == command.StateCode.Trim().ToLower()) && x.CountryId == command.CountryId && x.IsActive && !x.IsDeleted);
            if (exist != null)
            {
                response.Status = false;
                response.Message = exist.Name.Trim().ToLower() == command.Name.Trim().ToLower() ? "State Name Already Exists!!": "State Code Already Exists!!";
                return response;
            }
            State state = new State
            {
                Name = command.Name,
                StateCode = command.StateCode,
                CountryId = command.CountryId,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Now,
            };
            _context.States.Add(state);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                response.Status = true;
                response.Message = "State added successfully.";
            }
            else
            {
                response.Status = false;
                response.Message = "Issue during State adding.";
            }
            return response;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllState")]
        public async Task<LocationResponse> GetAllState()
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "Data not found!";
            var stateList = await _context.States
                .Where(x => x.IsDeleted == false)
                .OrderBy(x => x.Name)
                .ToListAsync();
            if (stateList != null && stateList.Any())
            {
                locationResponse.Status = true;
                locationResponse.Message = "Data found!";
                locationResponse.ReturnObject = stateList;
            }
            return locationResponse;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetStateByCountryId")]
        public async Task<LocationResponse> GetStateByCountryId(long countryId)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "State not exists.";
            var stateList = await _context.States.Where(x => x.CountryId == countryId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (stateList != null && stateList.Any())
            {
                locationResponse.Status = true;
                locationResponse.Message = "Data found.";
                locationResponse.ReturnObject = stateList;
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetStateByStateId")]
        public async Task<LocationResponse> GetStateByStateId(long stateId)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "State not exists.";
            var stateList = await _context.States.FirstOrDefaultAsync(x => x.Id == stateId && !x.IsDeleted);
            if (stateList != null)
            {
                locationResponse.Status = true;
                locationResponse.Message = "Data found.";
                locationResponse.ReturnObject = stateList;
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateState")]
        public async Task<LocationResponse> UpdateState(AddStateDTO command)
        {
            LocationResponse locationResponse = new LocationResponse();
            var exist = await _context.States.FirstOrDefaultAsync(x => (x.Name.Trim().ToLower() == command.Name.Trim().ToLower() || x.StateCode.Trim().ToLower() == command.StateCode.Trim().ToLower()) && x.Id != command.StateId && x.CountryId == command.CountryId && x.IsActive && !x.IsDeleted);
            if (exist != null)
            {
                locationResponse.Status = false;
                locationResponse.Message = exist.Name.Trim().ToLower() == command.Name.Trim().ToLower() ? "State Name Already Exists!!" : "State Code Already Exists!!";
                return locationResponse;
            }
            var _state = await _context.States.FirstOrDefaultAsync(x => x.Id == command.StateId);
            if (_state != null)
            {

                _state.Name = command.Name;
                _state.StateCode = command.StateCode;
                _state.LastModified = DateTime.Now;
                _state.CountryId = command.CountryId;

                _context.Entry(_state).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    locationResponse.Status = true;
                    locationResponse.Message = "State updated successfully.";
                }
                else
                {
                    locationResponse.Status = false;
                    locationResponse.Message = "Issue during State updating.";
                }

            }
            return locationResponse;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteState")]
        public async Task<LocationResponse> DeleteState(long stateId)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "State not exists.";
            var _state = await _context.States.FirstOrDefaultAsync(x => x.Id == stateId);
            if (_state != null)
            {
                _state.IsActive = false;
                _state.IsDeleted = true;
                _state.Deleted = DateTime.Now;
                _context.Entry(_state).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    locationResponse.Status = true;
                    locationResponse.Message = "state deleted successfully.";

                }
                else
                {
                    locationResponse.Status = false;
                    locationResponse.Message = "Issue during state deleting.";
                }
            }

            return locationResponse;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateStateStatus")]
        public async Task<LocationResponse> UpdateCityStatus(long stateId, bool IsActive)
        {
            LocationResponse resp = new LocationResponse();
            resp.Status = false;
            
            var _state = await _context.States.FirstOrDefaultAsync(x => x.Id == stateId);
            if (_state != null)
            {
                if (IsActive)
                {
                    var exist = await _context.States.FirstOrDefaultAsync(x => (x.Name.Trim().ToLower() == _state.Name.Trim().ToLower() || x.StateCode.Trim().ToLower() == _state.StateCode.Trim().ToLower()) && x.CountryId == _state.CountryId && x.IsActive && !x.IsDeleted);
                    if (exist != null)
                    {
                        resp.Status = false;
                        resp.Message = exist.Name.Trim().ToLower() == _state.Name.Trim().ToLower() ? "State Name Already Exists!!" : "State Code Already Exists!!";
                        return resp;
                    }
                }
                _state.IsActive = IsActive;
                _state.LastModified = DateTime.Now;

                _context.Entry(_state).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    resp.Status = true;
                    resp.Message = "State status updated successfully.";

                }
                else
                {
                    resp.Status = false;
                    resp.Message = "Issue during State status updating.";
                }
            }
            else
            {
                resp.Message = "State not Exist.";
            }
            return resp;

        }
    }
}
