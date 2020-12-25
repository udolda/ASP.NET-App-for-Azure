using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TestApp2.Tools;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Строки для локального тестирования
            //var tokens = new TokenModel();
            //tokens.AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiJiYTYxM2E0NC0zM2M5LTY0MGUtYjg0Yy1kNWFlNGU3NmIwMzIiLCJzY3AiOiJ2c28uYWdlbnRwb29scyB2c28uYW5hbHl0aWNzIHZzby5hdWRpdGxvZyB2c28uYnVpbGQgdnNvLmNvZGUgdnNvLmRhc2hib2FyZHMgdnNvLmVudGl0bGVtZW50cyB2c28uZXh0ZW5zaW9uIHZzby5leHRlbnNpb24uZGF0YSB2c28uZ3JhcGggdnNvLmlkZW50aXR5IHZzby5sb2FkdGVzdCB2c28ubm90aWZpY2F0aW9uX2RpYWdub3N0aWNzIHZzby5wYWNrYWdpbmcgdnNvLnByb2plY3QgdnNvLnJlbGVhc2UgdnNvLnNlcnZpY2VlbmRwb2ludCB2c28uc3ltYm9scyB2c28udGFza2dyb3Vwc19yZWFkIHZzby50ZXN0IHZzby50b2tlbmFkbWluaXN0cmF0aW9uIHZzby50b2tlbnMgdnNvLnZhcmlhYmxlZ3JvdXBzX3JlYWQgdnNvLndpa2kgdnNvLndvcmsiLCJhdWkiOiJlMDFlNTQ4Zi1lZGQ0LTQyNzMtOTkwNy0xOTQ5YmI5ZGNiMmUiLCJhcHBpZCI6IjY4Y2UxOTJlLTM2YmEtNGJhZS1iNGJkLTA3NTY2ODViN2I1YSIsImlzcyI6ImFwcC52c3Rva2VuLnZpc3VhbHN0dWRpby5jb20iLCJhdWQiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNjA4ODg3NjA0LCJleHAiOjE2MDg4OTEyMDR9.wnLxLZNXOsPdBbN5J5SX13vzbdKiWJdNW6FX7cYYpNHAQRrINT2UfAfhkK7e6CwkeDPcNNND_p636ToD6x9fvADN_T-VfLFWASrgmi0uFvypIRqj2IZWMPui5_10CA2oOPrkzZF5kRbXL4-RyeVOYRWxm464whjanFKOJ4fQYsVeqKU9n1TRfkgLmXqo5-G2bZSeJsowZqiji5MB1uUp6NYaFFW9u8x1S2Nd70NcAdBPQNT2T1fCixm9Zbp4rck7ouVsQx1pYA0iv-SDr38afbADTtFIJa9oRD1gLf-J4Ojb8cngE8Z-0ZkYLywj3U4I9F7o3Flu12vV68LhYrtwjw";
            //Session["token"] = tokens;
            if (Session["token"] != null)
            {
                Session["access"] = true;
                var token = (TokenModel)Session["token"];
                VssConnection connection = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential(token.AccessToken));
                Session["connect"] = connection;
                Session["info"] += connection.HasAuthenticated.ToString() + " " + connection.AuthorizedIdentity.DisplayName + " " + token.AccessToken;
                try
                {
                    var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
                    var hierarchyAccess = witClient.GetQueriesAsync("WorkPractice", depth: 2).Result;
                }
                catch (Exception)
                {
                    Session["access"] = false;
                }

            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Поддробности";

            return View();
        }

        public ActionResult Manage()
        {
            ViewBag.HasAccess = Session["access"];
            
            return View("Manage");
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            var teamPC = (VssConnection)Session["connect"];
            if (teamPC.HasAuthenticated)
                teamPC.Disconnect();

            return View("Index");
        }
    
    }
}