using System.Linq;
using System.Web;
using QuanLyChiTieuCaNhan.Models;

namespace QuanLyChiTieuCaNhan.Helpers
{
    public static class SessionHelper
    {
        public static int GetCurrentUserId()
        {
            var email = HttpContext.Current.User.Identity.Name;
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted);
                return user?.UserId ?? 0;
            }
        }

        public static User GetCurrentUser()
        {
            var email = HttpContext.Current.User.Identity.Name;
            using (var db = new AppDbContext())
            {
                return db.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted);
            }
        }
    }
}
