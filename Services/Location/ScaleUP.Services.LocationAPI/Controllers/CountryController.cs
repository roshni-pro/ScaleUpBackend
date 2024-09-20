using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationDTO.City;
using ScaleUP.Services.LocationDTO.Country;
using ScaleUP.Services.LocationDTO.Master;
using ScaleUP.Services.LocationModels.Master;
using System.ComponentModel.Design;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ScaleUP.Services.LocationAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CountryController : BaseController
    {
        private readonly ApplicationDbContext _context;
        //private readonly IPublisher _publisher;
        public CountryController(ApplicationDbContext context)
        {
            _context = context;
            //_publisher = publisher;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddCountry")]
        public async Task<LocationResponse> AddCountry(AddCountryDTO command)
        {
            LocationResponse locationResponse = new LocationResponse();
            var exist = _context.Countries.Any(x => x.Name.Trim().ToLower() == command.Name.Trim().ToLower() && x.IsActive && !x.IsDeleted);
            if (exist)
            {
                locationResponse.Status = false;
                locationResponse.Message = "Country Name Already Exists!!";
                return locationResponse;
            }
            Country country = new Country
            {
                Name = command.Name,
                CountryCode = command.CountryCode,
                CurrencyCode = command.CurrencyCode,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Now,
            };
            //command.Status = "Pending";
            _context.Countries.Add(country);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                locationResponse.Status = true;
                locationResponse.Message = "country added successfully.";
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllCountry")]
        public async Task<LocationResponse> GetAllCountry()
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "Data not found!";
            var countryList = await _context.Countries
                .Where(x => x.IsActive == true && x.IsDeleted == false)
                .ToListAsync();
            if (countryList != null && countryList.Any())
            {
                locationResponse.Status = true;
                locationResponse.Message = "Data found!";
                locationResponse.ReturnObject = countryList;
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetCountryById")]
        public async Task<LocationResponse> GetCountryById(long countryId)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "Country not exists.";
            var _country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == countryId);
            if (_country != null)
            {
                locationResponse.Status = true;
                locationResponse.Message = "Country found.";
                locationResponse.ReturnObject = _country;
            }
            return locationResponse;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateCountry")]
        public async Task<LocationResponse> UpdateCountry(AddCountryDTO country)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "Issue during country updating.";
            var exist = _context.Countries.Any(x => x.Name.Trim().ToLower() == country.Name.Trim().ToLower() && x.Id != country.CountryId && x.IsActive && !x.IsDeleted);
            if (exist)
            {
                locationResponse.Status = false;
                locationResponse.Message = "Country Name Already Exists!!";
                return locationResponse;
            }
            var _country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == country.CountryId);
            if (_country != null)
            {
                _country.Name = country.Name;
                _country.CountryCode = country.CountryCode;
                _country.CurrencyCode = country.CurrencyCode;
                _country.LastModified = DateTime.Now;

                _context.Entry(_country).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    locationResponse.Status = true;
                    locationResponse.Message = "country updated successfully.";
                    locationResponse.ReturnObject = country;
                }

            }
            return locationResponse;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteCountry")]
        public async Task<LocationResponse> DeleteCountry(long countryId)
        {
            LocationResponse locationResponse = new LocationResponse();
            locationResponse.Status = false;
            locationResponse.Message = "country not exists.";
            var _country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == countryId);
            if (_country != null)
            {
                _country.IsActive = false;
                _country.IsDeleted = true;
                _country.Deleted = DateTime.Now;
                _context.Entry(_country).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    locationResponse.Status = true;
                    locationResponse.Message = "country deleted successfully.";

                }
                else
                {
                    locationResponse.Status = false;
                    locationResponse.Message = "Issue during country deleting.";
                }
            }

            return locationResponse;
        }
    }
}
