using BoilerplateCore.Common.Extensions;
using BoilerplateCore.Common.Helpers;
using BoilerplateCore.RestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.RestApi.Helpers
{
    public class HttpClientHelper
    {
        private static HttpClient httpClient;
        private static readonly string WebApiUrl = string.Empty;
        static HttpClientHelper()
        {
            WebApiUrl = Startup.StaticConfiguration.GetSection("ItSolution:WebApiUrl").Value;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(WebApiUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task SetBearerToken(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<LoginResponseModel> TokenAsync(string action, object data)
        {
            try
            {
                //Create the request.
                string json = await JsonSerializer.SerializeAsync(data);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var keyValuePair = data?.ToKeyValue();
                HttpResponseMessage response = await httpClient.PostAsync(action, keyValuePair == null ? null : new FormUrlEncodedContent(keyValuePair));
                
                //Process the response.
                if (response.IsSuccessStatusCode)
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<LoginResponseModel>(await response.Content.ReadAsStringAsync());
                    return responseModel;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<LoginResponseModel>(await response.Content.ReadAsStringAsync());
                    if (responseModel == null)
                        return new LoginResponseModel { Status = LoginStatus.Failed, Message = "Something went wrong. Please try again latter." };
                    return new LoginResponseModel { Status = responseModel.Status, Message = responseModel.Message };
                }
            }
            catch (HttpRequestException ex)
            {
                return new LoginResponseModel { Message = ex.Message };
            }
        }

        public static async Task<ResponseModel<T>> GetAsync<T>(string action)
        {
            try
            {
                var response = await httpClient.GetAsync(action);

                if (response.IsSuccessStatusCode)
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message, Data = responseModel.Data };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    if (responseModel == null)
                        return new ResponseModel<T> { Success = false, Message = "Something went wrong. Please try again latter." };
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<T> { Success = false, Message = ex.Message };
            }
        }

        public static async Task<ResponseModel<T>> PostAsync<T>(string action)
        {
            try
            {
                var response = await httpClient.PostAsync(action, null);

                if (response.IsSuccessStatusCode)
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message, Data = responseModel.Data };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    if (responseModel == null)
                        return new ResponseModel<T> { Success = false, Message = "Something went wrong. Please try again latter." };
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<T> { Success = false, Message = ex.Message };
            }
        }

        public static async Task<ResponseModel<T>> PostAsync<T>(string action, object data)
        {
            try
            {
                //Create the request.
                string json = await JsonSerializer.SerializeAsync(data);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var keyValuePair = data?.ToKeyValue();
                HttpResponseMessage response = await httpClient.PostAsync(action, keyValuePair == null ? null : new FormUrlEncodedContent(keyValuePair));
                
                //Process the response.
                if (response.IsSuccessStatusCode)
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message, Data = responseModel.Data };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    if(responseModel == null)
                        return new ResponseModel<T> { Success = false, Message = "Something went wrong. Please try again latter." };

                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ResponseModel<T> { Success = false, Message = ex.Message };
            }
        }

        public static async Task<ResponseModel<T>> PutAsync<T>(string action, object data)
        {
            try
            {
                //Create the request.
                string json = await JsonSerializer.SerializeAsync(data);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var keyValuePair = data?.ToKeyValue();
                HttpResponseMessage response = await httpClient.PutAsync(action, keyValuePair == null ? null : new FormUrlEncodedContent(keyValuePair));

                //Process the response.
                if (response.IsSuccessStatusCode)
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message, Data = responseModel.Data };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    if (responseModel == null)
                        return new ResponseModel<T> { Success = false, Message = "Something went wrong. Please try again latter." };
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ResponseModel<T> { Success = false, Message = ex.Message };
            }
        }

        public static async Task<ResponseModel<T>> DeleteAsync<T>(string action)
        {
            try
            {
                var response = await httpClient.DeleteAsync(action);

                if (response.IsSuccessStatusCode)
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message, Data = responseModel.Data };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
                    if (responseModel == null)
                        return new ResponseModel<T> { Success = false, Message = "Something went wrong. Please try again latter." };
                    return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ResponseModel<T> { Success = false, Message = ex.Message };
            }
        }

        //public static async Task<ResponseModel<T>> PostDataWithUploadAsync<T>(Uri baseAddress, string action, object data, HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        if (file == null)
        //            return await PostAsync<T>(action, data);

        //        //using (HttpClient httpClient = new HttpClient())
        //        //{
        //        //    httpClient.BaseAddress = baseAddress;

        //        using (var content = new MultipartFormDataContent())
        //        {
        //            content.Add(new StreamContent(file.InputStream), "File", file.FileName.Replace(" ", ""));

        //            if (data != null)
        //                foreach (var property in data.GetType().GetProperties())
        //                {
        //                    object value = property.GetValue(data, null);
        //                    if (value != null)
        //                        content.Add(new StringContent(value.ToString()), property.Name);
        //                }

        //            //Create the request.
        //            var response = await httpClient.PostAsync(action, content);
        //            //Process the response.
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var responseModel = await JsonSerializer.DeserializeAsync<ResponseModel<T>>(await response.Content.ReadAsStringAsync());
        //                return new ResponseModel<T> { Success = responseModel.Success, Message = responseModel.Message, Data = responseModel.Data };
        //            }
        //            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        //            {
        //                throw new Exception(await response.Content.ReadAsStringAsync());
        //            }
        //        }
        //        //}
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        //await EmailManager.SendEmail("Error", ex.ToString(), ConfigurationManager.AppSettings["SMTPSupportError"]);
        //    }

        //    return default(ResponseModel<T>);
        //}
    }
}