using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using QuanLyChiTieuCaNhan.Helpers;
using QuanLyChiTieuCaNhan.Models;
using QuanLyChiTieuCaNhan.Models.ViewModels;

namespace QuanLyChiTieuCaNhan.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (db.Users.Any(u => u.Email == model.Email && !u.IsDeleted))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(model);
            }

            try
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, 12),
                    Role = "User",
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();

                // Tạo ví "Tiền mặt" mặc định
                db.Wallets.Add(new Wallet
                {
                    UserId = user.UserId,
                    Name = "Tiền mặt",
                    WalletType = 1,
                    Balance = 0,
                    IsDefault = true,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();

                // Seed danh mục mặc định
                DbInitializer.SeedUserCategories(db, user.UserId);

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") + (ex.InnerException?.InnerException != null ? " - " + ex.InnerException.InnerException.Message : ""));
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u => u.Email == model.Email && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            if (user.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                user.LoginFailedCount++;
                if (user.LoginFailedCount >= 5)
                {
                    user.IsLocked = true;
                    db.SaveChanges();
                    ModelState.AddModelError("", "Tài khoản đã bị khóa sau 5 lần đăng nhập sai.");
                    return View(model);
                }
                db.SaveChanges();
                ModelState.AddModelError("", $"Email hoặc mật khẩu không đúng. Còn {5 - user.LoginFailedCount} lần thử.");
                return View(model);
            }

            // Reset failed count on successful login
            user.LoginFailedCount = 0;
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.Email, false);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, 12);
            db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u => u.Email == model.Email && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, 12);
            user.IsLocked = false;
            user.LoginFailedCount = 0;
            db.SaveChanges();

            TempData["Success"] = "Khôi phục mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
