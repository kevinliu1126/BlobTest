using BlobTest.Services.Abstract;
using BlobTest.Services.Concrete;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity;
using Unity.AspNet.Mvc;

namespace BlobTest
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var container = new UnityContainer();

            container.RegisterType<ILoginService, LoginService>();
            container.RegisterType<IUploadService, UploadService>();
            container.RegisterType<IGetFileService, GetFileService>();
            container.RegisterType<IDownloadService, DownloadService>();
            container.RegisterType<IDeleteService, DeleteService>();
            container.RegisterType<ITempService, TempService>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
