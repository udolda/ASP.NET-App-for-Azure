using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Controllers
{
    public class OAuthController : Controller
    {
        private static readonly HttpClient s_httpClient = new HttpClient();
        private static readonly Dictionary<Guid, TokenModel> s_authorizationRequests = new Dictionary<Guid, TokenModel>();

        /// <summary>
        /// Запускает новый запрос авторизации.
        ///
        /// Это создает случайное значение состояния, которое используется для корреляции/проверки запроса в обратном вызове позже.
        /// </summary>
        /// <returns></returns>
        public ActionResult Authorize()
        {
            Session["info"] += "Hi, bro, you're in Authorise().\n";
            Guid state = Guid.NewGuid();

            s_authorizationRequests[state] = new TokenModel() { IsPending = true };

            return new RedirectResult(GetAuthorizationUrl(state.ToString()));
        }

        /// <summary>
        /// Создает URL-адрес авторизации с указанным значением состояния.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static String GetAuthorizationUrl(String state)
        {
            UriBuilder uriBuilder = new UriBuilder(ConfigurationManager.AppSettings["AuthUrl"]);
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query ?? String.Empty);

            queryParams["client_id"] = ConfigurationManager.AppSettings["ClientAppId"];
            queryParams["response_type"] = "Assertion";
            queryParams["state"] = state;
            queryParams["scope"] = ConfigurationManager.AppSettings["Scope"];
            queryParams["redirect_uri"] = ConfigurationManager.AppSettings["CallbackUrl"];

            uriBuilder.Query = "client_id=285F1BF4-69FA-487A-AF9A-F7F59494D3B3&response_type=Assertion&state=" + state + "&scope=vso.code%20vso.identity_manage&redirect_uri=https://appwithidentity.azurewebsites.net/oauth/callback";//queryParams.ToString();

            return uriBuilder.ToString();
        }

        /// <summary>
        /// Действие обратного вызова. Вызывается после того, как пользователь авторизовал приложение.
        /// Получает токен.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<ActionResult> Callback(String code, Guid state)
        {
            Session["info"] += "Hi, bro, you're in Callback().\n";

            String error;
            if (ValidateCallbackValues(code, state.ToString(), out error))
            {
                // Exchange the auth code for an access token and refresh token
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, ConfigurationManager.AppSettings["TokenUrl"]);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Dictionary<String, String> form = new Dictionary<String, String>()
                {
                    { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                    { "client_assertion", ConfigurationManager.AppSettings["ClientAppSecret"] },
                    { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                    { "assertion", code },
                    { "redirect_uri", ConfigurationManager.AppSettings["CallbackUrl"] }
                };
                requestMessage.Content = new FormUrlEncodedContent(form);

                HttpResponseMessage responseMessage = await s_httpClient.SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    String body = await responseMessage.Content.ReadAsStringAsync();

                    TokenModel tokenModel = s_authorizationRequests[state];
                    JsonConvert.PopulateObject(body, tokenModel);

                    ViewBag.Token = tokenModel;
                }
                else
                {
                    error = responseMessage.ReasonPhrase;
                }
            }

            if (!String.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
            }

            ViewBag.ProfileUrl = ConfigurationManager.AppSettings["ProfileUrl"];

            return View("TokenView");
        }

        /// <summary>
        /// Гарантирует, что указанный код аутентификации и значение состояния являются допустимыми.
        /// Если оба они действительны, то значение состояния помечается так, чтобы его нельзя было использовать снова.       
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private static bool ValidateCallbackValues(String code, String state, out String error)
        {
            error = null;

            if (String.IsNullOrEmpty(code))
            {
                error = "Invalid auth code";
            }
            else
            {
                Guid authorizationRequestKey;
                if (!Guid.TryParse(state, out authorizationRequestKey))
                {
                    error = "Invalid authorization request key";
                }
                else
                {
                    TokenModel tokenModel;
                    if (!s_authorizationRequests.TryGetValue(authorizationRequestKey, out tokenModel))
                    {
                        error = "Unknown authorization request key";
                    }
                    else if (!tokenModel.IsPending)
                    {
                        error = "Authorization request key already used";
                    }
                    else
                    {
                        // отметьте значение состояния как используемое, чтобы его нельзя было использовать повторно
                        s_authorizationRequests[authorizationRequestKey].IsPending = false; 
                    }
                }
            }

            return error == null;
        }

        /// <summary>
        /// Gets a new access
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task<ActionResult> RefreshToken(string refreshToken)
        {
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
                HttpResponseMessage responseMessage = await s_httpClient.SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    // Обрабатывать успешный запрос
                    String body = await responseMessage.Content.ReadAsStringAsync();
                    ViewBag.Token = JObject.Parse(body).ToObject<TokenModel>();
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

            if (!String.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
            }

            return View("TokenView");
        }
    }
}