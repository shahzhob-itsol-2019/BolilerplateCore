//using BoilerplateCore.Data.Constants;
using BoilerplateCore.Common.Utility;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Data.Entities
{
   public class ApplicationUser : IdentityUser
   {
        [MaxLength(60)]
        public string FirstName { get; set; }

        [MaxLength(60)]
        public string LastName { get; set; }

        [MaxLength(12)]
        public string AccountNumber { get; set; }   

        [Column(TypeName = "DateTime2")]
        public DateTime IssueDate { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime BirthDate { get; set; }

        public Enums.Gender Gender { get; set; }

        [MaxLength(500)]
        public string Picture { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(60)]
        public string Language { get; set; }

        public string TimeZone { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "DateTime2")]
        public DateTime? CreatedAtUtc { get; set; }
       
        [Column(TypeName = "datetime2")]
        public DateTime DisabledDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DisabledDateUtc { get; set; }

        [MaxLength(20)]
        public string AlternativeAccountId { get; set; }

        [MaxLength(60)]
        public string VerifiedFirstName { get; set; }

        [MaxLength(60)]
        public string VerifiedMiddleName { get; set; }

        [MaxLength(60)]
        public string VerifiedFirstLastName { get; set; }

        [MaxLength(60)]
        public string VerifiedSecondLastName { get; set; }
        public string NicNumber { get; set; }

        public int? CompanyId { get; set; }

        public int? StatusId { get; set; }


        public TwoFactorTypes TwoFactorTypeId { get; set; }


        [ForeignKey("TwoFactorTypeId")]
        public virtual TwoFactorType TwoFactorType { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }

        public virtual ICollection<Addresses> Addresses { get; set; }

        //public virtual ICollection<PreviousPassword> PreviousPasswords { get; set; }
    }
}
