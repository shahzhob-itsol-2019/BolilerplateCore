using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Data.Constants.Enums;

namespace BoilerplateCore.Data.Models
{
    public class StatusModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public StatusType Type { get; set; }
    }
}
