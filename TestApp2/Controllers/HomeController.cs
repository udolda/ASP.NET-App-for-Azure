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
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var tokens = new TokenModel();
            tokens.AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiJiYTYxM2E0NC0zM2M5LTY0MGUtYjg0Yy1kNWFlNGU3NmIwMzIiLCJzY3AiOiJ2c28uYWdlbnRwb29scyB2c28uYW5hbHl0aWNzIHZzby5hdWRpdGxvZyB2c28uYnVpbGQgdnNvLmNvZGUgdnNvLmRhc2hib2FyZHMgdnNvLmVudGl0bGVtZW50cyB2c28uZXh0ZW5zaW9uIHZzby5leHRlbnNpb24uZGF0YSB2c28uZ3JhcGggdnNvLmlkZW50aXR5IHZzby5sb2FkdGVzdCB2c28ubm90aWZpY2F0aW9uX2RpYWdub3N0aWNzIHZzby5wYWNrYWdpbmcgdnNvLnByb2plY3QgdnNvLnJlbGVhc2UgdnNvLnNlcnZpY2VlbmRwb2ludCB2c28uc3ltYm9scyB2c28udGFza2dyb3Vwc19yZWFkIHZzby50ZXN0IHZzby50b2tlbmFkbWluaXN0cmF0aW9uIHZzby50b2tlbnMgdnNvLnZhcmlhYmxlZ3JvdXBzX3JlYWQgdnNvLndpa2kgdnNvLndvcmsiLCJhdWkiOiI2NGZmOWZhNi1jYjQ0LTRmMzMtOTY5My1hZGYyOTIxYWI1OGEiLCJhcHBpZCI6IjY4Y2UxOTJlLTM2YmEtNGJhZS1iNGJkLTA3NTY2ODViN2I1YSIsImlzcyI6ImFwcC52c3Rva2VuLnZpc3VhbHN0dWRpby5jb20iLCJhdWQiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNjA4ODQzOTc4LCJleHAiOjE2MDg4NDc1Nzh9.RZpctivXOu7v6LUQ-c2iW_IhbHi4aTJcYog5Du12sKUTkkkQ8ondBin8p6gPVOEFsQL2rzq7HzS1yj0OkOpNJLUQBi1CpefwtG-vfEqrp_BYW7Grw9m9wG81vkWlKiyNBRwTgStkHuN_m6NatlUP5to97zjjvFUo0cWQiwalUpHTJPR84iVYDOJ8pHpDoHFpy3y66hop7WvMwPdfuI3cihiCIbSEB0uwMg5O3dz7xehvw89fiaFc4EeLGkX_juzKP7vZmUdc6BetfBTbyLX7W6nGs3B3M0mcAdVnFxIlSsofbumCKb8ur91r7e_6RY1RFUdno15dwO3A07Ypuo79HA";
            Session["token"] = tokens;
            if (Session["token"] != null)
            {
                var token = (TokenModel)Session["token"];
                VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential(token.AccessToken));
                string name = connection.AuthorizedIdentity.DisplayName;
                Session["connect"] = connection;
                Session["info"] += connection.HasAuthenticated.ToString() + " " + name + " " + token.AccessToken;

            }
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
            Session["token"] = "";
            VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential((string)Session["token"]));
            Session["connect"] = connection;
            string s = connection.AuthorizedIdentity.DisplayName;
            bool t = connection.HasAuthenticated;

            
            var res = GetInfo.SampleREST(connection);
            Session["info"] += res;
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