using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlobTest.Services.Abstract;
using BlobTest.Models;


namespace BlobTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILoginService _loginService;

        public HomeController(ILoginService loginService)
        {
            _loginService = loginService;

        }

        public ActionResult Index()
        {
            HttpContextBase httpContext = ControllerContext.HttpContext;
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            int? permission = (int?)httpContext.Session["permission"];
            if (email == null || password == null || permission == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Upload");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            string email = Request.Form["email"];
            string password = Request.Form["password"];

            if (email == null || password == null)
            {
                return RedirectToAction("Index");
            }
            else if(_loginService.SendinfoToSQL(email, password))
            {
                return RedirectToAction("Upload");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Upload()
        {
            HttpContextBase httpContext = ControllerContext.HttpContext;
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            int? permission = (int?)httpContext.Session["permission"];
            if (email == null || password == null || permission == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var model = new UploadFileModel();
                return View(model);
            }
        }

        public ActionResult SaveData(UploadFileModel fileModel)
        {
            HttpContextBase httpContext = ControllerContext.HttpContext;
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            int? permission = (int?)httpContext.Session["permission"];
            if (email == null || password == null || permission == null)
            {
                return RedirectToAction("Index");
            }
            else if(fileModel.File == null || fileModel.File.FileName == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Upload");
            }
        }
    }
}