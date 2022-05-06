using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Common.Models
{
    public class BaseModel
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }

        public BaseModel()
        {

        }

        public BaseModel(bool success, object data = null, string message = "", int total = 0)
        {
            this.Success = success;
            this.Data = data;
            this.Message = message;
            this.Total = total;
        }

        public static BaseModel Create(bool success, object data = null, string message = "", int total = 0)
        {
            return new BaseModel() { Success = success, Data = data, Message = message, Total = total };
        }

        public static BaseModel Failed(string message, object data = null, int total = 0)
        {
            return new BaseModel(false, data, message, total);
        }

        public static BaseModel Succeed(object data = null, int total = 0, string message = "")
        {
            return new BaseModel(true, data, message, total);
        }
    }

    public class BaseModel<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
    }

    public class LoginResponseModel
    {
        public LoginStatus Status { get; set; }
        public bool Success { get; set; }
        public System.Security.Claims.ClaimsPrincipal Data { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
    }
}