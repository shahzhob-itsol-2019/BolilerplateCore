using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.Templates
{
    public static class EmailTemplateForgotPassword
    {
        public const string Subject = "Reset Password";
        public const string MessageBody = "Hi #Name </br></br>"
                                        + "Please click <a href=\"#Link\">here</a> to reset your password.";
    }
}
