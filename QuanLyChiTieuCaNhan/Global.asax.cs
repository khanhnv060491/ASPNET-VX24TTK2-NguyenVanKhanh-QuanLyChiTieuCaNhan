using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using QuanLyChiTieuCaNhan.Models;
using QuanLyChiTieuCaNhan.Helpers;

namespace QuanLyChiTieuCaNhan
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Database.SetInitializer(new DbInitializer());
            using (var db = new AppDbContext())
            {
                db.Database.Initialize(false);
                DbInitializer.EnsureSampleUser(db);
            }
        }
    }
}
