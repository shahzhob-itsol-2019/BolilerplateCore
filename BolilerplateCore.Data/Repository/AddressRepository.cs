﻿using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.Repository
{
    public class AddressRepository : BaseRepository<Addresses, int>, IAddressRepository
    {
        public AddressRepository(IDbContext context) : base(context)
        {
        }
    }
}
