using AutoMapper;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.Models;
using BoilerplateCore.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Data.Constants.Enums;

namespace BoilerplateCore.Services
{
    public class StatusService : IStatusService
    {
        private readonly BoilerplateCore.Data.Database.IDbContext dbContext;
        private readonly IMapper mapper;

        public StatusService(BoilerplateCore.Data.Database.IDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<List<StatusModel>> Get(StatusType? type = null)
        {
            IQueryable<Status> query = dbContext.Statuses;

            if (type.HasValue)
                query = query.Where(x => x.Type.Equals(type.Value));

            var entities = await query.OrderBy(x => x.Code)
                                      .ToListAsync();
            //if (entities.Any())
            //    return null; // new BaseModel { Success = false, Message = "No record found." };

            var result = mapper.Map<List<StatusModel>>(entities);
            return result;
        }

        public async Task<StatusModel> Get(string code, StatusType type)
        {
            var entity = await dbContext.Statuses
                                        .FirstOrDefaultAsync(x => x.Code.Equals(code) && x.Type.Equals(type));
            if (entity == null)
                return null; // new BaseModel { Success = false, Message = "No record found." };

            var result = mapper.Map<StatusModel>(entity);
            return result;
        }
    }
}
