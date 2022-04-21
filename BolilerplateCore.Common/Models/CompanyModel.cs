using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Common.Models
{
    public class CompanyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }


        public virtual ICollection<AddressModel> Addresses { get; set; }
    }
}
