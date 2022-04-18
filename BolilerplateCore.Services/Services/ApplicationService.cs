using AutoMapper;
using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerplateCore.Services.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IDbContext dbContext;
        private readonly IMapper mapper;

        public ApplicationService(
            IDbContext dbContext, 
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        //public async Task<List<ApplicationModel>> GetApplications(string userId = null)
        //{
        //    IQueryable<Application> query = dbContext.Applications
        //                                             .Include(x => x.Reconnaissance)
        //                                             .Include(m => m.Exploration)
        //                                             .Include(m => m.MineralDepositRetention)
        //                                             .Include(m => m.LargeMiningLease)
        //                                             .Include(m => m.Prospecting)
        //                                             .Include(m => m.SmallMiningLease)
        //                                             .Include(m => m.LandSurrenderAndTransfer)
        //                                             .Include(m => m.Persons)
        //                                             .Include(m => m.Minerals);

        //    if (!string.IsNullOrWhiteSpace(userId))
        //        query = query.Where(x => x.UserId.Equals(userId));

        //    var entities = await query.ToListAsync();
        //    var result = mapper.Map<List<ApplicationModel>>(entities);
        //    return result;
        //}
    }
}
