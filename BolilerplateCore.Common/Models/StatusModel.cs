using BoilerplateCore.Common.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Common.Models
{
    public class StatusModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public StatusTypes TypeId { get; set; }
    }
}
