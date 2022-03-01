using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Core.Entities
{
    class BaseEntity
    {
        public DateTime? CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }
    }
}
