using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Tools
{
    public class GetInfo
    {
        /// <summary>
        /// Gets a new access
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static async Task<TokenModel> RefreshToken(string refreshToken)
        {
            TokenModel token = null;
            String error = null;
            if (!String.IsNullOrEmpty(refreshToken))
            {
                // Сформировать запрос на обмен кода аутентификации на токен доступа и токен обновления
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, ConfigurationManager.AppSettings["TokenUrl"]);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Dictionary<String, String> form = new Dictionary<String, String>()
                {
                    { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                    { "client_assertion", ConfigurationManager.AppSettings["ClientAppSecret"] },
                    { "grant_type", "refresh_token" },
                    { "assertion", refreshToken },
                    { "redirect_uri", ConfigurationManager.AppSettings["CallbackUrl"] }
                };
                requestMessage.Content = new FormUrlEncodedContent(form);

                // Сделайте запрос на обмен кода аутентификации на токен доступа (и токен обновления)
                HttpResponseMessage responseMessage = await new HttpClient().SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    // Обрабатывать успешный запрос
                    String body = await responseMessage.Content.ReadAsStringAsync();
                    token = JObject.Parse(body).ToObject<TokenModel>();
                }
                else
                {
                    error = responseMessage.ReasonPhrase;
                }
            }
            else
            {
                error = "Invalid refresh token";
            }

            return token;
        }

    }
}