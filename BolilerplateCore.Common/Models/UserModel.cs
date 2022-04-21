using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BoilerplateCore.Common.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? CompanyId { get; set; }
        public int StatusId { get; set; }


        public virtual CompanyModel Company { get; set; }

        public virtual StatusModel Status { get; set; }

        public virtual ICollection<AddressModel> Addresses { get; set; }
    }
}
