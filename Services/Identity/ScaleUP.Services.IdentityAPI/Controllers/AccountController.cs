using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ScaleUP.Services.IdentityModels.Master;
using ScaleUP.Services.IdentityAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.IdentityDTO.Master;
using ScaleUP.Services.IdentityAPI.Manager;
using System.ComponentModel.Design;
using ScaleUP.Global.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.Services.IdentityAPI.Constants;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;

namespace ScaleUP.Services.IdentityAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly ISigningCredentialStore _signingCredentialStore;
        private readonly ApplicationDBContext _context;
        private readonly IUserCreateManager _usercreatemanager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, IIdentityServerInteractionService interaction,
            IClientStore clientStore, IEventService events, IUserStore<ApplicationUser> userStore, ConfigurationDbContext configurationDbContext,
            ISigningCredentialStore signingCredentialStore, ApplicationDBContext context, IUserCreateManager usercreatemanager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
            _configurationDbContext = configurationDbContext;
            _userStore = userStore;
            _signingCredentialStore = signingCredentialStore ?? throw new ArgumentNullException(nameof(signingCredentialStore));
            _context = context;
            _usercreatemanager = usercreatemanager;
            _roleManager = roleManager;
        }


        [HttpGet]
        [Route("GenerateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCustomApiToken(string username, string password)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == username || x.PhoneNumber == username || x.UserName== username);

            if (user == null)
                return BadRequest("User not found");

            bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);

            if (!isPasswordCorrect)
            {
                return BadRequest("Password is incorrect");
            }

            //var userId = GetCustomValidatedUser(credentials);
            //if (userId == null) return Unauthorized();

            var signingCredentials = await _signingCredentialStore.GetSigningCredentialsAsync();
            var tokenHandler = new JwtSecurityTokenHandler();

            //List<Claim> claims = new List<Claim>();
            //var idclaim = new Claim("userid", user.Id);
            //var scopeClaim = new Claim("scope",);


            //claims.Add(idclaim);

            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            List<Claim> claims = new List<Claim> {  new Claim("userId", user.Id),
                    new Claim("username", user.UserName??""),
                    new Claim("loggedon", DateTime.Now.ToString())
                   , new Claim("scope", "crmApi" )
                   , new Claim("usertype", user.UserType??"" )
                   , new Claim("mobile", user.PhoneNumber??"" )
                   , new Claim("email", user.Email ?? "" )
                   , new Claim("roles",  string.Join(",",userRoles) )};

            if (userClaims != null && userClaims.Any())
                claims.AddRange(userClaims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = EnvironmentConstants.IdentityUrl,
                IssuedAt = DateTime.UtcNow,
                Audience = "crmApi",
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = signingCredentials,
                TokenType = "at+jwt",
                //,Claims = new Dictionary<string, object> { { "TokenScopes", "crmApi" } }
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));

        }



        [HttpGet]
        [Route("CreateClient")]
        [AllowAnonymous]
        public async Task<CreateClientResponse> CreateClient()
        {
            var userManager = new UserCreateManager(_context, _userManager, _roleManager, _configurationDbContext);
            return (await userManager.CreateClient()).Response;
        }


    }
}
