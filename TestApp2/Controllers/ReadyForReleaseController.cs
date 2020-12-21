using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TestApp2.Tools;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Controllers
{
    public class ReadyForReleaseController : Controller
    {
        // GET: ReadyForRelease
        public async Task<ActionResult> TestsInfo()
        {
            // обновить токен
            var token = (TokenModel)Session["token"];
            Session["token"] = await GetInfo.RefreshToken(token.RefreshToken);
            // обновить соединение 
            var connect = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential((string)Session["token"]));
            Session["connect"] = connect;

            GetInfo.SampleREST(connect);

            return View();
        }
    }
}