using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.Repository
{
    public class NotificationTypeRepository : BaseRepository<NotificationType, int>, INotificationTypeRepository
    {
        public NotificationTypeRepository(ISqlServerDbContext context) : base(context)
        {
        }
    }
}
