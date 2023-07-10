using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlobTest.Services.Abstract;
using BlobTest.Models;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing.Drawing2D;
using System.Web.Helpers;

namespace BlobTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IUploadService _uploadService;
        private readonly IGetFileService _getfileService;
        private readonly IDownloadService _downloadService;
        private readonly IDeleteService _deleteService;
        private readonly ITempService _tempService;

        public HomeController(ILoginService loginService, IUploadService uploadService, IGetFileService getfileService, IDownloadService downloadService, IDeleteService deleteService, ITempService tempService)
        {
            _loginService = loginService;
            _uploadService = uploadService;
            _getfileService = getfileService;
            _downloadService = downloadService;
            _deleteService = deleteService;
            _tempService = tempService;
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
            else if (_loginService.SendinfoToSQL(email, password))
            {
                return RedirectToAction("Upload");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Logout()
        {
            HttpContextBase httpContext = ControllerContext.HttpContext;
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            if (email == null || password == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                httpContext.Session.Abandon();
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

        public ActionResult CheckFileExistence(string filename)
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
                string fileExists = _tempService.FileExist(filename, email);
                return Json(new { exists = fileExists });
            }
                
        }

        public async Task<ActionResult> SaveData(UploadFileModel fileModel, string ExtraValue)
        {
            HttpContextBase httpContext = ControllerContext.HttpContext;
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            int? permission = (int?)httpContext.Session["permission"];
            if (email == null || password == null || permission == null)
            {
                return RedirectToAction("Index");
            }
            else if (fileModel.File == null || fileModel.File.FileName == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                if(ExtraValue == "")
                {
                    await _tempService.UploadFile(fileModel.File, httpContext);
                    return RedirectToAction("Upload");
                }
                else
                {
                    await _tempService.UpdateFile(fileModel.File, ExtraValue, httpContext);
                    return RedirectToAction("Upload");
                }
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

        public async Task<ActionResult> Download(string file, string save)
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
                string url = _downloadService.GetDownloadURL(save);
                string filePath = $@"D:\NJ\file\" + _downloadService.GetContainername(file) + @"\" + file;

                if (!System.IO.File.Exists(filePath))
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(url, filePath);
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return HttpNotFound();
                }

                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memoryStream);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream, "application/octet-stream", Path.GetFileName(filePath));
            }
        }

        public async Task<ActionResult> Delete(string file, string save)
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
                await _deleteService.DeleteFile(file, save);
                return RedirectToAction("ViewFile");
            }
        }

        public ActionResult AzureManagement()
        {
            HttpContextBase httpContext = ControllerContext.HttpContext;
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            int? permission = (int?)httpContext.Session["permission"];
            if (email == null || password == null || permission != 1)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Files = _getfileService.GetFiles((int)permission, httpContext);
                return View();
            }
            
        }

        public ActionResult SelectContainer()
        {
            return RedirectToAction("AzureManagement");
        }
    }
}