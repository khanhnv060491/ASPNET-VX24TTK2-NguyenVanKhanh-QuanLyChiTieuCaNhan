using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace QuanLyChiTieuCaNhan.Models.ViewModels
{
    public class BudgetCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục chi")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hạn mức")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hạn mức phải lớn hơn 0")]
        [Display(Name = "Hạn mức")]
        public decimal LimitAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tháng")]
        [Range(1, 12)]
        [Display(Name = "Tháng")]
        public byte Month { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm")]
        [Display(Name = "Năm")]
        public short Year { get; set; }

        public List<SelectListItem> ExpenseCategories { get; set; }
    }

    public class BudgetEditViewModel : BudgetCreateViewModel
    {
        public int BudgetId { get; set; }
    }

    public class BudgetListViewModel
    {
        public List<BudgetItemViewModel> Budgets { get; set; }
        public byte FilterMonth { get; set; }
        public short FilterYear { get; set; }
    }

    public class BudgetItemViewModel
    {
        public int BudgetId { get; set; }
        public string CategoryName { get; set; }
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public double Percentage => LimitAmount > 0 ? (double)(SpentAmount / LimitAmount * 100) : 0;

        public string ProgressBarClass
        {
            get
            {
                if (Percentage > 90) return "bg-danger";
                if (Percentage > 70) return "bg-warning";
                return "bg-success";
            }
        }
    }
}
