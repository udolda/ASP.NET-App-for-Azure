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
            Session["token"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiI4YTRhNjk3My1mMGZiLTZiZDgtYjBjYS0wNjRiMDc2OWZmNzciLCJzY3AiOiJ2c28uY29kZSB2c28uaWRlbnRpdHlfbWFuYWdlIiwiYXVpIjoiMDRjYWQyZWMtYTgzYy00NjkwLTk2ZjAtZmFhNzgwMWZmMWVmIiwiYXBwaWQiOiIyODVmMWJmNC02OWZhLTQ4N2EtYWY5YS1mN2Y1OTQ5NGQzYjMiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTYwODQ5NDYzNywiZXhwIjoxNjA4NDk4MjM3fQ.iU8IMFyjEzVoDjGLj-fv1iVB-ZUUf7fi5mgY2SGyq3BrtAcvcrNc9L0sWU4lvBGi5ySFJIt4DDo_p-pIfYYi-9EtNGX6Ef5kmrd4XPX9Cny8VOMLSNemLB1Uj_56IzEeylUMSatOo_1hWDCBJUv5-fU4TDDuco-mFUrS2YqhuGiBAqhgLguHMCxDdNvD8X6WjMyrjXOOi5ea29KWTqzZeCqv1iLW4etciRdGL3vlRAzPMzJmkUS50lxc1lUPoXuPxr_QEsBH5piSoEsliIHO-gT8elCnzb8gpKEJMw4fUPVUQj2YZaMH99ZM7LX0dKlyT3IgFn_tmF_LXr5_0eablw";
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