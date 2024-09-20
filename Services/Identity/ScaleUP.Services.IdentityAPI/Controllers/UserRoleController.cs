using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.IdentityAPI.Manager;
using ScaleUP.Services.IdentityAPI.Persistence;
using ScaleUP.Services.IdentityDTO.Master;
using ScaleUP.Services.IdentityModels.Master;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ScaleUP.Services.IdentityAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserRoleController : BaseController
    {
        private readonly ApplicationDBContext _context;
        private readonly IUserCreateManager _usercreatemanager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRoleController(ApplicationDBContext context, IUserCreateManager usercreatemanager, IUserStore<ApplicationUser> userStore, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usercreatemanager = usercreatemanager;
            _userStore = userStore;
            _userManager = userManager;
        }


        [HttpPost]
        [Route("CreateUser")]
        public async Task<bool> CreateUser()
        {
            //var res = await _userManager.CreateAsync(new ApplicationUser { UserName = "amit22", UserType = "Customer" }, "Amit@1234");
            //new Client

            var user = await _userStore.FindByNameAsync(_userManager.NormalizeName("amit22"), new CancellationToken());

            //_userStore.CreateAsync()


            //var user = await _userManager.FindByNameAsync("amit22");




            //var dbclient = new Client
            //{
            //    AllowedGrantTypes = new List<string> { GrantType.ClientCredentials },
            //    ClientId = "client1",
            //    ClientSecrets = new List<ClientSecret> { new IdentityServer4.EntityFramework.Entities.ClientSecret { Value = "secret".Sha256() } },
            //    AllowedScopes = new List<IdentityServer4.EntityFramework.Entities.ClientScope> { new IdentityServer4.EntityFramework.Entities.ClientScope { Scope = "movie" } }


            //};
            //_configurationDbContext.Clients.Add(client.ToEntity());
            //await _configurationDbContext.SaveChangesAsync();




            return true;
        }

        [HttpGet]
        [Route("GetPageMasterList")]
        [AllowAnonymous]
        public async Task<IdentityResponse> GetPageMasterList(string userName)
        {

            IdentityResponse identityResponse = new IdentityResponse();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                identityResponse.Status = false;
                identityResponse.Message = "User Not Found";
                return identityResponse;
            }

            var pages = new List<PageListDTO>();
            if (user.UserType == UserTypeConstants.SuperAdmin)
            {
                var query = from pm in _context.AspNetPageMasters
                            where pm.IsActive && !pm.IsDeleted
                            select new PageListDTO
                            {
                                PageMasterId = pm.Id,
                                PageName = pm.PageName,
                                ClassName = pm.ClassName,
                                RouteName = pm.RouteName,
                                Sequence = pm.Sequence,
                                IsAdd = true,
                                IsDelete = true,
                                IsEdit = true,
                                IsView = true,
                                IsAll = true,
                                ParentId = pm.ParentId,
                                IsMaster = pm.IsMaster
                            };
                pages = await query.OrderBy(x => x.Sequence).ToListAsync();


                foreach (var pg in pages)
                {
                    if (pg.ParentId > 0)
                    {
                        var main = pages.Where(x => x.PageMasterId == pg.ParentId).FirstOrDefault();
                        if (main != null)
                        {
                            var subpage = new SubPageListDTO
                            {
                                PageMasterId = pg.PageMasterId,
                                PageName = pg.PageName,
                                ClassName = pg.ClassName,
                                RouteName = pg.RouteName,
                                Sequence = pg.Sequence,
                                IsAdd = true,
                                IsDelete = true,
                                IsEdit = true,
                                IsView = true,
                                IsAll = true,
                                ParentId = pg.ParentId,
                                IsMaster = pg.IsMaster
                            };
                            if (main.subPageMaster != null && main.subPageMaster.Count > 0)
                            {
                                main.subPageMaster.Add(subpage);
                            }
                            else
                            {
                                var subpages = new List<SubPageListDTO>();
                                subpages.Add(subpage);
                                main.subPageMaster = subpages;
                            }
                        }

                    }
                };
                pages = pages.Where(x => x.ParentId == 0).ToList();
            }
            else
            {

                var roleList = await _userManager.GetRolesAsync(user);
                if (roleList != null && roleList.Any())
                {
                    string queryString = $"select b.Id from AspNetRoles b where b.Name in ({string.Join(',', roleList.Select(role => $"'{role}'"))})";
                    var roleIdList = await _context.Database.SqlQueryRaw<string>(queryString).ToListAsync();
                    pages = await _context.AspNetPageMasters
                                    .Join(_context.AspNetRolePagePermission, pm => pm.Id, pp => pp.PageMasterId, (pm, pp) => new { pm, pp })
                                    .Where(x => x.pp.IsActive && !x.pp.IsDeleted && x.pm.IsActive && !x.pm.IsDeleted && roleIdList.Contains(x.pp.RoleId) && (x.pp.IsAdd || x.pp.IsEdit || x.pp.IsView || x.pp.IsDelete || x.pp.IsAll))
                                    .GroupBy(x => new { x.pp.PageMasterId, x.pm.PageName, x.pm.ClassName, x.pm.RouteName, x.pm.Sequence, x.pm.ParentId, x.pm.IsMaster })
                                    .Select(x => new PageListDTO
                                    {
                                        PageMasterId = x.Key.PageMasterId,
                                        PageName = x.Key.PageName,
                                        ClassName = x.Key.ClassName,
                                        RouteName = x.Key.RouteName,
                                        Sequence = x.Key.Sequence,
                                        ParentId = x.Key.ParentId,
                                        IsMaster = x.Key.IsMaster,
                                        IsAdd = x.Any(x => x.pp.IsAdd),
                                        IsView = x.Any(x => x.pp.IsView),
                                        IsDelete = x.Any(x => x.pp.IsDelete),
                                        IsEdit = x.Any(x => x.pp.IsEdit),
                                        IsAll = x.Any(x => x.pp.IsAll),
                                    }).OrderBy(x => x.Sequence).ToListAsync();
                    foreach (var pg in pages)
                    {
                        if (pg.ParentId > 0)
                        {
                            var main = pages.Where(x => x.PageMasterId == pg.ParentId).FirstOrDefault();
                            if (main != null)
                            {
                                var subpage = new SubPageListDTO
                                {
                                    PageMasterId = pg.PageMasterId,
                                    PageName = pg.PageName,
                                    ClassName = pg.ClassName,
                                    RouteName = pg.RouteName,
                                    Sequence = pg.Sequence,
                                    IsAdd = pg.IsAdd,
                                    IsDelete = pg.IsDelete,
                                    IsEdit = pg.IsEdit,
                                    IsView = pg.IsView,
                                    IsAll = pg.IsAll,
                                    ParentId = pg.ParentId,
                                    IsMaster = pg.IsMaster
                                };
                                if (main.subPageMaster != null && main.subPageMaster.Count > 0)
                                {
                                    main.subPageMaster.Add(subpage);
                                }
                                else
                                {
                                    var subpages = new List<SubPageListDTO>();
                                    subpages.Add(subpage);
                                    main.subPageMaster = subpages;
                                }
                            }

                        }
                    };
                    pages = pages.Where(x => x.ParentId == 0).ToList();
                }
            }
            if (pages != null && pages.Any())
            {
                identityResponse.Status = true;
                identityResponse.Message = "Data Found";
                identityResponse.ReturnObject = pages;
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "No Data Found";
            }
            return identityResponse;
        }

        [HttpGet]
        [Route("GetRolePagePermissions")]
        [AllowAnonymous]
        public async Task<List<RolePagePermissionDTO>> GetRolePagePermissions(string RoleId, string? Keyword)
        {
            List<RolePagePermissionDTO> pagePermissionDTOs = new List<RolePagePermissionDTO>();

            var query = from pm in _context.AspNetPageMasters
                        join rolepages in _context.AspNetRolePagePermission
                        on new { pm.Id, RoleId, IsActive = true, IsDeleted = false } equals new { Id = rolepages.PageMasterId, rolepages.RoleId, rolepages.IsActive, rolepages.IsDeleted } into rolePages
                        from rp in rolePages.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(Keyword) || pm.PageName.Contains(Keyword)) && pm.IsActive && !pm.IsDeleted
                        select new RolePagePermissionDTO
                        {
                            PageMasterId = pm.Id,
                            PageName = pm.PageName,
                            IsView = rp != null ? rp.IsView : false,
                            IsAdd = rp != null ? rp.IsAdd : false,
                            IsEdit = rp != null ? rp.IsEdit : false,
                            IsDelete = rp != null ? rp.IsDelete : false,
                            IsAll = rp != null ? rp.IsAll : false,
                            RoleId = RoleId//rp != null ? rp.RoleId : null
                        };
            pagePermissionDTOs = await query.ToListAsync();
            return pagePermissionDTOs;
        }

        [HttpPost]
        [Route("AddUpdateRolePagePermissions")]
        [AllowAnonymous]
        public async Task<IdentityResponse> AddUpdateRolePagePermissions(List<RolePagePermissionDTO> rolePagePermissionDTOs)
        {
            IdentityResponse identityResponse = new IdentityResponse();
            foreach (var rolePage in rolePagePermissionDTOs)
            {
                var existing = await _context.AspNetRolePagePermission.FirstOrDefaultAsync(x => x.RoleId == rolePage.RoleId && x.PageMasterId == rolePage.PageMasterId);
                if (existing != null)
                {
                    existing.IsView = rolePage.IsView;
                    existing.IsAdd = rolePage.IsAdd;
                    existing.IsEdit = rolePage.IsEdit;
                    existing.IsDelete = rolePage.IsDelete;
                    existing.IsAll = !rolePage.IsView || !rolePage.IsAdd || !rolePage.IsEdit || !rolePage.IsDelete ? false : true;
                    existing.LastModified = DateTime.Now;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    AspNetRolePagePermission aspNetRolePagePermission = new AspNetRolePagePermission
                    {
                        PageMasterId = rolePage.PageMasterId,
                        RoleId = rolePage.RoleId,
                        IsView = rolePage.IsView,
                        IsAdd = rolePage.IsAdd,
                        IsEdit = rolePage.IsEdit,
                        IsDelete = rolePage.IsDelete,
                        IsAll = !rolePage.IsView || !rolePage.IsAdd || !rolePage.IsEdit || !rolePage.IsDelete ? false : true,
                        IsActive = true,
                        IsDeleted = false,
                        Created = DateTime.Now
                    };
                    _context.Add(aspNetRolePagePermission);
                }
            }
            var rowChanged = await _context.SaveChangesAsync();
            if (rowChanged > 0)
            {
                identityResponse.Status = true;
                identityResponse.Message = "Permissions Updated Successfully";
                identityResponse.ReturnObject = rolePagePermissionDTOs;
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "Failed to Update Permissions";
            }

            return identityResponse;
        }


        //[HttpPost]
        //[Route("UpdateUser")]
        //[AllowAnonymous]
        //public async Task<IdentityResponse> UpdateUser(UpdateUserReqDTO req)
        //{
        //    return await _usercreatemanager.UpdateUserByUserId(req);
        //}

        [HttpPost]
        [Route("ChangeUserPassword")]
        [AllowAnonymous]
        public async Task<IdentityResponse> ChangeUserPassword(string UserId, string OldPassword, string NewPassword)
        {
            return await _usercreatemanager.ChangeUserPassword(UserId, OldPassword, NewPassword);
        }

        [HttpGet]
        [Route("GetUserById")]
        [AllowAnonymous]
        public async Task<UpdateUserResponseDTO> GetUserById(string UserId)
        {

            return await _usercreatemanager.GetUser(UserId);
        }

        [HttpGet]
        [Route("GetRoleList")]
        [AllowAnonymous]
        public async Task<IdentityResponse> GetRoleList(string? Keyword)
        {
            return await _usercreatemanager.GetRoleList(Keyword);
        }

        [HttpGet]
        [Route("CreateRoleByName")]
        [AllowAnonymous]
        public async Task<RoleResponse> CreateRoleByName(string RoleName)
        {
            return await _usercreatemanager.CreateRoleByName(RoleName);
        }

        [HttpPost]
        [Route("CreateRoleWithPermissions")]
        [AllowAnonymous]
        public async Task<RoleResponse> CreateRoleWithPermissions(RoleWithPermissionsDTO request)
        {
            return await _usercreatemanager.CreateRoleWithPermissions(request);
        }

        [HttpGet]
        [Route("UpdateRole")]
        [AllowAnonymous]
        public async Task<IdentityResponse> UpdateRole(string RoleId, string NewRoleName)
        {
            return await _usercreatemanager.UpdateRole(RoleId, NewRoleName);
        }

        [HttpGet]
        [Route("DeleteRole")]
        [AllowAnonymous]
        public async Task<IdentityResponse> DeleteRole(string RoleId)
        {
            return await _usercreatemanager.DeleteRole(RoleId);
        }

        [HttpGet]
        [Route("GetUserList")]
        [AllowAnonymous]
        public async Task<List<ApplicationUser>> GetUserList(string Keyword)
        {
            return await _usercreatemanager.GetUserList(Keyword);
        }
    }
}
