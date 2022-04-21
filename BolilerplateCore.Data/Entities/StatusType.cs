using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Data.Entities
{
    public class StatusType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public StatusTypes Id { get; set; }
        public string Name { get; set; }
    }
}
