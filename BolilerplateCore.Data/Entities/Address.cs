//using BoilerplateCore.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
//using ApplicationUser = BoilerplateCore.Core.Entities.ApplicationUser;

namespace BoilerplateCore.Data.Entities
{
    public class Addresses
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? CompanyId { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public bool IsDefault { get; set; }


        [ForeignKey("UserId")]
        public virtual ApplicationUser AppUser { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }
    }
}
