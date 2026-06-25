using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QuanLyChiTieuCaNhan.Helpers;
using QuanLyChiTieuCaNhan.Models;
using QuanLyChiTieuCaNhan.Models.ViewModels;

namespace QuanLyChiTieuCaNhan.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index(byte? month, short? year)
        {
            int userId = SessionHelper.GetCurrentUserId();
            byte filterMonth = month ?? (byte)DateTime.Now.Month;
            short filterYear = year ?? (short)DateTime.Now.Year;

            var budgets = db.Budgets
                .Where(b => b.UserId == userId && b.Month == filterMonth && b.Year == filterYear && !b.IsDeleted)
                .ToList();

            var model = new BudgetListViewModel
            {
                FilterMonth = filterMonth,
                FilterYear = filterYear,
                Budgets = budgets.Select(b =>
                {
                    // Tính lại SpentAmount realtime từ giao dịch
                    var startDate = new DateTime(filterYear, filterMonth, 1);
                    var endDate = startDate.AddMonths(1);
                    var spent = db.Transactions
                        .Where(t => t.UserId == userId
                            && t.CategoryId == b.CategoryId
                            && t.TransactionType == 2
                            && t.TransactionDate >= startDate
                            && t.TransactionDate < endDate
                            && !t.IsDeleted)
                        .Select(t => t.Amount)
                        .DefaultIfEmpty(0)
                        .Sum();

                    return new BudgetItemViewModel
                    {
                        BudgetId = b.BudgetId,
                        CategoryName = db.Categories.Where(c => c.CategoryId == b.CategoryId).Select(c => c.Name).FirstOrDefault() ?? "",
                        LimitAmount = b.LimitAmount,
                        SpentAmount = spent
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            int userId = SessionHelper.GetCurrentUserId();
            var model = new BudgetCreateViewModel
            {
                Month = (byte)DateTime.Now.Month,
                Year = (short)DateTime.Now.Year,
                ExpenseCategories = GetExpenseCategorySelectList(userId)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BudgetCreateViewModel model)
        {
            int userId = SessionHelper.GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                model.ExpenseCategories = GetExpenseCategorySelectList(userId);
                return View(model);
            }

            // Kiểm tra trùng
            bool exists = db.Budgets.Any(b =>
                b.UserId == userId &&
                b.CategoryId == model.CategoryId &&
                b.Month == model.Month &&
                b.Year == model.Year &&
                !b.IsDeleted);

            if (exists)
            {
                ModelState.AddModelError("CategoryId", "Đã tồn tại ngân sách cho danh mục này trong tháng/năm được chọn");
                model.ExpenseCategories = GetExpenseCategorySelectList(userId);
                return View(model);
            }

            db.Budgets.Add(new Budget
            {
                UserId = userId,
                CategoryId = model.CategoryId,
                LimitAmount = model.LimitAmount,
                SpentAmount = 0,
                Month = model.Month,
                Year = model.Year,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            TempData["Success"] = "Tạo ngân sách thành công!";
            return RedirectToAction("Index", new { month = model.Month, year = model.Year });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            int userId = SessionHelper.GetCurrentUserId();
            var budget = db.Budgets.FirstOrDefault(b => b.BudgetId == id && b.UserId == userId && !b.IsDeleted);

            if (budget == null)
                return HttpNotFound();

            var model = new BudgetEditViewModel
            {
                BudgetId = budget.BudgetId,
                CategoryId = budget.CategoryId,
                LimitAmount = budget.LimitAmount,
                Month = budget.Month,
                Year = budget.Year,
                ExpenseCategories = GetExpenseCategorySelectList(userId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BudgetEditViewModel model)
        {
            int userId = SessionHelper.GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                model.ExpenseCategories = GetExpenseCategorySelectList(userId);
                return View(model);
            }

            var budget = db.Budgets.FirstOrDefault(b => b.BudgetId == model.BudgetId && b.UserId == userId && !b.IsDeleted);
            if (budget == null)
                return HttpNotFound();

            budget.LimitAmount = model.LimitAmount;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật ngân sách thành công!";
            return RedirectToAction("Index", new { month = budget.Month, year = budget.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            int userId = SessionHelper.GetCurrentUserId();
            var budget = db.Budgets.FirstOrDefault(b => b.BudgetId == id && b.UserId == userId && !b.IsDeleted);

            if (budget == null)
                return HttpNotFound();

            budget.IsDeleted = true;
            db.SaveChanges();

            TempData["Success"] = "Xóa ngân sách thành công!";
            return RedirectToAction("Index", new { month = budget.Month, year = budget.Year });
        }

        private List<SelectListItem> GetExpenseCategorySelectList(int userId)
        {
            return db.Categories
                .Where(c => c.UserId == userId && c.CategoryType == 2 && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
