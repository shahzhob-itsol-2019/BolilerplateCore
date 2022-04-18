using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Data.Constants.Enums;

namespace BoilerplateCore.Data.Entities
{
    public class Status
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public StatusType Type { get; set; }
    }
}
