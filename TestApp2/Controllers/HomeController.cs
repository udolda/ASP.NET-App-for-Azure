using Microsoft.TeamFoundation.Client;
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

            var auth = new Authenticate();
            Session["tfs"] = auth.PerformAuthentication();

            var teamPC = (TfsTeamProjectCollection)Session["tfs"];
            WorkItem w = new WorkItem(teamPC);
            w.getInfo();
            // в зависимости от того есть или нет доступ выводить
            // ту или иную страницу
            return View("Index");
        }

        [AllowAnonymous]
        public ActionResult Logout(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var teamPC = (TfsTeamProjectCollection)Session["tfs"];
            teamPC.Disconnect();

            return View("Index");
        }
    }
}