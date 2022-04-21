using BoilerplateCore.Common.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Data.Entities
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public StatusTypes TypeId { get; set; }

        [ForeignKey("TypeId")]
        public virtual StatusType StatusType { get; set; }
    }
}
