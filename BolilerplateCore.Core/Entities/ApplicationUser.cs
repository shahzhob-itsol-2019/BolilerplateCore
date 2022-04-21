using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public TwoFactorTypes TwoFactorTypeId { get; set; }


        [ForeignKey("TwoFactorTypeId")]
        public virtual TwoFactorType TwoFactorType { get; set; }

        public virtual ICollection<PreviousPassword> PreviousPasswords { get; set; }
    }
}
