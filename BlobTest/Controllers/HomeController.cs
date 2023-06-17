using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlobTest.Services.Abstract;
using BlobTest.Models;
using System.Threading.Tasks;

namespace BlobTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IUploadService _uploadService;
        private readonly IGetFileService _getfileService;

        public HomeController(ILoginService loginService, IUploadService uploadService, IGetFileService getfileService)
        {
            _loginService = loginService;
            _uploadService = uploadService;
            _getfileService = getfileService;
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

        public async Task<ActionResult> SaveData(UploadFileModel fileModel)
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
                await _uploadService.UploadFileAsync(fileModel.File, httpContext);
                return RedirectToAction("Upload");
            }
        }

        public ActionResult ViewFile()
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
                ViewBag.Files = _getfileService.GetFiles((int)permission, httpContext);
                return View();
            }
        }
    }
}