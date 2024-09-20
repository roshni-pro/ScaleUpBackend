using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.ApiGateways.Aggregator.Persistence;

//using ScaleUP.ApiGateways.Aggregator.Persistence;
using ScaleUP.ApiGateways.AggregatorDTO.ConsumerAppHome;
using ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Managers.ConsumerAppHome
{
    public class TestManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IMassTransitService _massTransitService;
        public TestManager(ApplicationDbContext context, IMassTransitService massTransitService)
        {
            _context = context;
            _massTransitService = massTransitService;
        }
        public async Task<ResultViewModel<bool>> SaveTestDetail(TestDTO testDTO, string UserId)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();
            Test testDetail = new Test();

            var testData = await _context.TestMaster.FirstOrDefaultAsync(x => x.Id == testDTO.Id);

            Test testDetailkd = new Test
            {
                Name = testDTO.Name,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Today,
                CreatedBy = UserId
            };
            _context.TestMaster.Add(testDetailkd);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
            res = new ResultViewModel<bool>
            {
                Result = true,
                IsSuccess = true,
                Message = "SuccesFully!"
            };
            return res;

        }

        public async Task<ResultViewModel<List<Test>>> GetTestDetail()
        {
            ResultViewModel<List<Test>> res = new ResultViewModel<List<Test>>();
            var list = new List<Test>();
            var GetList = _context.TestMaster.Where(x => x.IsActive && !x.IsDeleted).ToList();
            if (GetList.Count > 0)
            {
                res = new ResultViewModel<List<Test>>
                {
                    Result = GetList,
                    IsSuccess = true,
                    Message = ""
                };
            }
            else
            {
                res = new ResultViewModel<List<Test>>
                {
                    Result = null,
                    IsSuccess = false,
                    Message = "Data not found!"
                };
            }
            return res;
        }


    }
}
