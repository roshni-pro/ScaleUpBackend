using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationModels.Master;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ScaleUP.Services.LocationAPI.Managers
{
    public class StateManager
    {
        private ApplicationDbContext _context;

        public StateManager(ApplicationDbContext context) { _context = context; }

        public async Task<StateReply> GetStateByName(GSTverifiedRequest gSTverifiedRequestDTO)
        {
            var query = from s in _context.States
                        where s.Name.ToLower() == gSTverifiedRequestDTO.State.Trim().ToLower()
                        select new StateReply
                        {
                            stateId = s.Id,
                            stateName = s.Name
                        };
            var stateData = query.FirstOrDefault();
            if (stateData != null)
            {
                return stateData;
            }
            else
            {
               stateData = await postState(gSTverifiedRequestDTO);
                return stateData;
            }
        }

        private async Task<StateReply> postState(GSTverifiedRequest gSTverifiedRequestDTO)
        {
            StateReply stateReply = new StateReply();
            State state = new State
            {
                Name = gSTverifiedRequestDTO.State,
                StateCode = gSTverifiedRequestDTO.State.Substring(0, 2),
                CountryId = 1,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Now,
            };
            _context.States.Add(state);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                var query = from s in _context.States
                            where s.Name.ToLower() == gSTverifiedRequestDTO.State.Trim().ToLower()
                            select new StateReply
                            {
                                stateId = s.Id,
                                stateName = s.Name
                            };
                stateReply = query.FirstOrDefault();
                return stateReply;
            }
            else
            {
                return stateReply;
            }
        }


    }
}
