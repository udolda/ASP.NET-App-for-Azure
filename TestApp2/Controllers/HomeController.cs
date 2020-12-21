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
            Session["token"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiJiYTYxM2E0NC0zM2M5LTY0MGUtYjg0Yy1kNWFlNGU3NmIwMzIiLCJzY3AiOiJ2c28uYWdlbnRwb29scyB2c28uYW5hbHl0aWNzIHZzby5hdWRpdGxvZyB2c28uYnVpbGQgdnNvLmNvZGUgdnNvLmRhc2hib2FyZHMgdnNvLmVudGl0bGVtZW50cyB2c28uZXh0ZW5zaW9uIHZzby5leHRlbnNpb24uZGF0YSB2c28uZ3JhcGggdnNvLmlkZW50aXR5IHZzby5sb2FkdGVzdCB2c28ubm90aWZpY2F0aW9uX2RpYWdub3N0aWNzIHZzby5wYWNrYWdpbmcgdnNvLnByb2plY3QgdnNvLnJlbGVhc2UgdnNvLnNlcnZpY2VlbmRwb2ludCB2c28uc3ltYm9scyB2c28udGFza2dyb3Vwc19yZWFkIHZzby50ZXN0IHZzby50b2tlbmFkbWluaXN0cmF0aW9uIHZzby50b2tlbnMgdnNvLnZhcmlhYmxlZ3JvdXBzX3JlYWQgdnNvLndpa2kgdnNvLndvcmsiLCJhdWkiOiI3MDIzYjJjNy0xZWZiLTQ0NjItYjVjNS1lNWQ3MDAwMmJhNTIiLCJhcHBpZCI6IjY4Y2UxOTJlLTM2YmEtNGJhZS1iNGJkLTA3NTY2ODViN2I1YSIsImlzcyI6ImFwcC52c3Rva2VuLnZpc3VhbHN0dWRpby5jb20iLCJhdWQiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNjA4NTc3NzE4LCJleHAiOjE2MDg1ODEzMTh9.umW9QkUz7lHayZ7X_LLM27pQDJHDHpJqxUL5yA6Qf2AO0X7ez1TUzGSaOQBjdyPp2QLzNBkIMGKZifS7oO0Wq2PlAKP6aFdxGpphax_lBo6T1VY8_N86bHk3v2lxtJ07EuXPYJsQ-Cqm9Wop__HF0kfN_Zm-9Sr6ruHicedEsvtDN8pnZIFJxduz8Z6PU1oewgZmYSUCDGhwsiykAnaNe-0qQI9zlSCXTrsfu5agb4ZAfDSFrTgCH_pQPXUPNphAgWErIpwoXM6HwezuC1zp5W07a493ml7plGZeYJPqmBfuQTX5Ka6S2V3z_qitXFpxVVXkqk6tfhc7UsYy2LrdWw";
            VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential((string)Session["token"]));
            Session["connect"] = connection;
            string s = connection.AuthorizedIdentity.DisplayName;
            bool t = connection.HasAuthenticated;

            
            GetInfo.SampleREST(connection);
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