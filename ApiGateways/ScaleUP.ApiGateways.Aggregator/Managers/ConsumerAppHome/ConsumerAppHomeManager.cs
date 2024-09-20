using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.ApiGateways.Aggregator.Persistence;
using ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.ApiGateways.AggregatorDTO.ConsumerAppHome;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class ConsumerAppHomeManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IMassTransitService _massTransitService;

        #region old APIs
        public ConsumerAppHomeManager(ApplicationDbContext context, IMassTransitService massTransitService)
        {
            _context = context;
            _massTransitService = massTransitService;
        }

        public async Task<ResultViewModel<List<AppHomeFunctionListDc>>> GetAppHomeFunctionList()
        {
            ResultViewModel<List<AppHomeFunctionListDc>> res = new ResultViewModel<List<AppHomeFunctionListDc>>();

            var GetList = _context.AppHomeFunctionDb.Where(x => x.IsActive && !x.IsDeleted).Select(x => new AppHomeFunctionListDc
            {
                AppHomeFunctionId = x.Id,
                FunctionName = x.FunctionName,
                Request = x.Request,
                Response = x.Response
            }).ToList();

            if (GetList.Count > 0)
            {
                res = new ResultViewModel<List<AppHomeFunctionListDc>>
                {
                    Result = GetList,
                    IsSuccess = true,
                    Message = ""
                };
            }
            else
            {
                res = new ResultViewModel<List<AppHomeFunctionListDc>>
                {
                    Result = null,
                    IsSuccess = false,
                    Message = "Data not found!"
                };
            }
            return res;
        }

        public async Task<ResultViewModel<bool>> SaveAppHome(List<AppHomeInput> input, string UserId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();
            int sequence = 0;
            foreach (var item in input)
            {
                AppHome appHome = new AppHome
                {
                    Name = item.Name,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Today,
                    CreatedBy = UserId
                };
                _context.AppHomeDb.Add(appHome);
                _context.SaveChanges();

                var appHomeItem = _context.AppHomeItemDb.Where(x => x.AppHomeId == appHome.Id).FirstOrDefault();
                appHomeItem = new AppHomeItem
                {
                    ItemType = item.ItemType,
                    AppHomeId = appHome.Id,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Today,
                    CreatedBy = UserId
                };
                _context.AppHomeItemDb.Add(appHomeItem);
                _context.SaveChanges();

                var appHomeContent = _context.AppHomeContentDb.Where(x => x.AppHomeItemId == appHomeItem.Id).FirstOrDefault();

                appHomeContent = new AppHomeContent
                {
                    ImageUrl = item.ImageUrl,
                    AppHomeFunctionId = item.AppHomeFunctionId,
                    CallBackUrl = item.CallBackUrl,
                    AppHomeItemId = appHomeItem.Id,
                    Sequence = appHomeItem.ItemType == "Slider" ? ++sequence : 1,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Today,
                    CreatedBy = UserId
                };

                _context.AppHomeContentDb.Add(appHomeContent);
                _context.SaveChanges();

            }


            res = new ResultViewModel<bool>
            {
                Result = true,
                IsSuccess = true,
                Message = "SuccesFully!"
            };
            return res;

        }

        public async Task<ResultViewModel<List<AppHomeReturnList>>> getAllHomeList()
        {
            ResultViewModel<List<AppHomeReturnList>> resultViewModel = new ResultViewModel<List<AppHomeReturnList>>();


            var result = await _context.Database.SqlQueryRaw<AppHomeReturnList>("exec getAllAppHomeList").ToListAsync();

            if (result != null)
            {
                resultViewModel.IsSuccess = true;
                resultViewModel.Result = result;
                resultViewModel.Message = "Success";
            }

            return resultViewModel;
        }
        #endregion


        #region new APIs
        public async Task<ResultViewModel<bool>> SaveAppHomeV2(AppHomeDC input, string UserId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();
            int sequence = 0;

            if (input.AppHomeId == 0)
            {
                AppHome appHome = new AppHome
                {
                    Name = input.AppHomeHeading,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Today,
                    CreatedBy = UserId
                };
                _context.AppHomeDb.Add(appHome);
                _context.SaveChanges();
                var appHomeItem = _context.AppHomeItemDb.Where(x => x.AppHomeId == appHome.Id).FirstOrDefault();

                foreach (var item in input.appHomeItemLists)
                {
                    if (item.AppHomeItemId == 0)
                    {
                        appHomeItem = new AppHomeItem
                        {
                            ItemType = item.ItemType,
                            //ItemName = item.ItemName,
                            AppHomeId = appHome.Id,
                            IsActive = true,
                            IsDeleted = false,
                            Created = DateTime.Today,
                            CreatedBy = UserId
                        };
                        _context.AppHomeItemDb.Add(appHomeItem);
                        _context.SaveChanges();
                        var appHomeContent = _context.AppHomeContentDb.Where(x => x.AppHomeItemId == appHomeItem.Id).FirstOrDefault();

                        foreach (var content in item.AppHomeItemContent)
                        {
                            if (content.AppHomeItemContentId == 0)
                            {
                                appHomeContent = new AppHomeContent
                                {
                                    ImageUrl = content.ImageUrl,
                                    AppHomeFunctionId = content.AppHomeFnId,
                                    CallBackUrl = content.CallBackUrl,
                                    AppHomeItemId = appHomeItem.Id,
                                    Sequence = appHomeItem.ItemType == "Slider" ? ++sequence : 1,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Created = DateTime.Today,
                                    CreatedBy = UserId
                                };

                                _context.AppHomeContentDb.Add(appHomeContent);
                                _context.SaveChanges();

                            }
                            else
                            {
                                //edit case here
                            }
                        }
                    }
                    else
                    {
                        //edit case here
                    }
                }

            }
            else
            {
                //edit case here
            }

            res = new ResultViewModel<bool>
            {
                Result = true,
                IsSuccess = true,
                Message = "SuccesFully!"
            };
            return res;

        }

        public async Task<ResultViewModel<List<AppHomelist>>> GetAppHomeList()
        {
            ResultViewModel<List<AppHomelist>> resultViewModel = new ResultViewModel<List<AppHomelist>>();

            var GetList = _context.AppHomeDb.Where(x => x.IsActive && !x.IsDeleted).Select(x => new AppHomelist
            {
                Id = x.Id,
                Name = x.Name,
            }).ToList();

            if (GetList.Count > 0)
            {
                resultViewModel = new ResultViewModel<List<AppHomelist>>
                {
                    Result = GetList,
                    IsSuccess = true,
                    Message = "This is the list"
                };
            }
            else
            {
                resultViewModel = new ResultViewModel<List<AppHomelist>>
                {
                    Result = null,
                    IsSuccess = false,
                    Message = "Data not found!"
                };
            }

            return resultViewModel;

        }

        public async Task<ResultViewModel<AppHomeDC>> GetAppHomeListById(long Id)
        {
            ResultViewModel<AppHomeDC> resultViewModel = new ResultViewModel<AppHomeDC>();
            List<AppHomeItemList> appHomeItemList = new List<AppHomeItemList>();


            var res = await _context.AppHomeDb
                .Where(x => x.Id == Id).Include(r => r.AppHomeItem)
                .ThenInclude(r => r.AppHomeContent).ToListAsync();

            if (res.Count > 0)
            {
                AppHomeDC returnAppHomeDC = new AppHomeDC();


                returnAppHomeDC.AppHomeId = res[0].Id;
                returnAppHomeDC.AppHomeHeading = res[0].Name;

                if (res[0].AppHomeItem.Count > 0)
                {
                    foreach (var item in res[0].AppHomeItem)
                    {
                        AppHomeItemList tempItem = new AppHomeItemList();
                        tempItem.AppHomeItemId = item.Id;
                        tempItem.ItemType = item.ItemType;
                        tempItem.ItemName = "";
                        appHomeItemList.Add(tempItem);
                        if (item.AppHomeContent.Count > 0)
                        {
                            tempItem.AppHomeItemContent = new List<AppHomeItemContent>();

                            foreach (var content in item.AppHomeContent)
                            {
                                AppHomeItemContent tempContent = new AppHomeItemContent();
                                tempContent.AppHomeItemContentId = content.Id;
                                tempContent.ImageUrl = content.ImageUrl;
                                tempContent.Sequence = content.Sequence;
                                tempContent.CallBackUrl = content.CallBackUrl;
                                tempContent.AppHomeFnId = content.AppHomeFunctionId;

                                tempItem.AppHomeItemContent.Add(tempContent);
                            }
                        }
                    }
                        returnAppHomeDC.appHomeItemLists = appHomeItemList;
                }

                resultViewModel.IsSuccess = true;
                resultViewModel.Result = returnAppHomeDC;
                resultViewModel.Message = "Items Found";
            }
            else
            {
                resultViewModel.IsSuccess = true;
                resultViewModel.Result = null;
                resultViewModel.Message = "No Items Found";
            }
            return resultViewModel;
        }
        #endregion

    }
}
