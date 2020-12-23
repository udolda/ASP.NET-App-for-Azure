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

        //[HttpPost]
        //public ActionResult Index(string returnUrl)
        //{
        //    if (Session["token"] != null)
        //    {
        //        var token = (TokenModel)Session["token"];
        //        VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential(token.AccessToken));
        //        Session["connect"] = connection;
        //    }
        //    return View();

        //}

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
            Session["token"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiJiYTYxM2E0NC0zM2M5LTY0MGUtYjg0Yy1kNWFlNGU3NmIwMzIiLCJzY3AiOiJ2c28uYWdlbnRwb29scyB2c28uYW5hbHl0aWNzIHZzby5hdWRpdGxvZyB2c28uYnVpbGQgdnNvLmNvZGUgdnNvLmRhc2hib2FyZHMgdnNvLmVudGl0bGVtZW50cyB2c28uZXh0ZW5zaW9uIHZzby5leHRlbnNpb24uZGF0YSB2c28uZ3JhcGggdnNvLmlkZW50aXR5IHZzby5sb2FkdGVzdCB2c28ubm90aWZpY2F0aW9uX2RpYWdub3N0aWNzIHZzby5wYWNrYWdpbmcgdnNvLnByb2plY3QgdnNvLnJlbGVhc2UgdnNvLnNlcnZpY2VlbmRwb2ludCB2c28uc3ltYm9scyB2c28udGFza2dyb3Vwc19yZWFkIHZzby50ZXN0IHZzby50b2tlbmFkbWluaXN0cmF0aW9uIHZzby50b2tlbnMgdnNvLnZhcmlhYmxlZ3JvdXBzX3JlYWQgdnNvLndpa2kgdnNvLndvcmsiLCJhdWkiOiI1MzYxZDUxYy1mMzFjLTQ4NjktYmZmZi03Y2FkZjhhYTJkMzEiLCJhcHBpZCI6IjY4Y2UxOTJlLTM2YmEtNGJhZS1iNGJkLTA3NTY2ODViN2I1YSIsImlzcyI6ImFwcC52c3Rva2VuLnZpc3VhbHN0dWRpby5jb20iLCJhdWQiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNjA4NzM1ODIwLCJleHAiOjE2MDg3Mzk0MjB9.zaNaYnsQ32v1ks8dYZxtnXixQb99CrofEcfFHa_dcvmrVaviVIzqD6kU46ftGB0izY7FJmxKdeU30YES_1Ojn9bPCaz08Sy8dUZ82MU_Mpxv4H6Bfi2oUSekMViih_thsijb8hEdzdczd1hqTo4xLTeBxAkiNo3nMNFl1AjUPDvraUnGt-nrQYOwr9gFipOhyU9JfgWdOobio9G3WCu--IVKYbe1eAUy93spZE2PX1xrZd4YS4UhXOmsV-rFzwUVwtRzhecN5PNBFq6QEonZ5NcM1Om3uHdEIKs7JCDXIm1qRGETFVVzjqVczrUaM4nigKqVQWoLqXFjoJhxbQqJLw";
            VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential((string)Session["token"]));
            Session["connect"] = connection;
            string s = connection.AuthorizedIdentity.DisplayName;
            bool t = connection.HasAuthenticated;

            
            var res = GetInfo.SampleREST(connection);
            Session["info"] += res;
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