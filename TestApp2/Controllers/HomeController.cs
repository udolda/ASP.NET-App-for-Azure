using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TestApp2.Tools;

namespace TestApp2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
            
        }

        [HttpPost]
        public ActionResult Index(string returnUrl)
        {
            return View();

        }

        public ActionResult About()
        {
            ViewBag.Message = "Поддробности";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            var collectionUri = new Uri("https://dev.azure.com/LATeamInc/");
            Session["token"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiJkMGIyMjQ3My1jYjEyLTZmYTQtYTY0ZS00NGQwMWQ1NzlhNmQiLCJzY3AiOiJ2c28uY29kZSB2c28uaWRlbnRpdHlfbWFuYWdlIiwiYXVpIjoiNjVmMGRlM2MtYzUxNC00ZjBkLTg0ZDMtMjdjYTUwNTg2YTc2IiwiYXBwaWQiOiIyODVmMWJmNC02OWZhLTQ4N2EtYWY5YS1mN2Y1OTQ5NGQzYjMiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTYwODQ4NjQ4NCwiZXhwIjoxNjA4NDkwMDg0fQ.zrntyRQw4VGOQFHbFtC_geZR4PedmI3o8JSEu0hJuMHHenic0voEFI_JRoM1ADq5k8Pre0fNCjpFWB4FNkMokS42xBdgRYT9ilpE0be8J94fCq-NBIa2xGYKlwuTSi9DGu9ZhL2tRpbqM5McTlgyNkCxry9J7iO81YeidrNh5Xl_368UPLx4mHvML_qgUVq60GkIY3Zn63E1q3QSeV_0azes6ZZeTQpoPVE4pPcM-nL3VurJkun2Ihl0g-nK1UDuTUnQ2OGLbPZZNNfLVRsUZRFNoqYOrhoqc4JlMJJjchR_LwmACnuHOEkhHvjQ3jHao-MsXuExEvjJ3ORKEGoosA";
            VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential((string)Session["token"]));
            Session["connect"] = connection;
            string s = connection.AuthorizedIdentity.DisplayName;
            bool t = connection.HasAuthenticated;
            //GetInfo.SampleREST(connection, "WorkPractice");
            //var auth = new Authenticate();
            //var teamPC = (TfsTeamProjectCollection)Session["tfs"];
            //WorkItem w = new WorkItem(teamPC);
            //w.getInfo();
            // в зависимости от того есть или нет доступ выводить
            // ту или иную страницу
            return View("Index");
        }

        [AllowAnonymous]
        public ActionResult Logout(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var teamPC = (VssConnection)Session["connect"];
            teamPC.Disconnect();

            return View("Index");
        }
    }
}