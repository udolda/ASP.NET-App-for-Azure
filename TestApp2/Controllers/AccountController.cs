using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TestApp2.Models;
using Microsoft.TeamFoundation.Client;
using System.Net;
using TestApp2.Tools;

namespace TestApp2.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            var auth = new Authenticate();
            Session["tfs"] =  auth.PerformAuthentication(); 

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        // получает обьект (моделт) сфорированный на странице Login
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            

            var collectionUri = new Uri("https://dev.azure.com/LATeamInc/");

            var credential = new NetworkCredential(model.Email, model.Password); //("username", "password");
            var teamProjectCollection = new TfsTeamProjectCollection(collectionUri, credential);
            teamProjectCollection.EnsureAuthenticated();
            return View(model);
            // Сбои при входе не приводят к блокированию учетной записи
            // Чтобы ошибки при вводе пароля инициировали блокирование учетной записи, замените на shouldLockout: true
            //var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            //switch (result)
            //{
            //    case SignInStatus.Success:
            //        return RedirectToLocal(returnUrl);
            //    case SignInStatus.LockedOut:
            //        return View("Lockout");
            //    case SignInStatus.RequiresVerification:
            //        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            //    case SignInStatus.Failure:
            //    default:
            //        ModelState.AddModelError("", "Неудачная попытка входа.");
            //        return View(model);
            //}
        }
    }
}