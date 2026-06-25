using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using QuanLyChiTieuCaNhan.Models;

namespace QuanLyChiTieuCaNhan.Helpers
{
    public class DbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            // Seed system categories
            if (!context.Categories.Any(c => c.IsSystem))
            {
                var systemCategories = new List<Category>
                {
                    // Chi
                    new Category { Name = "Ăn uống", CategoryType = 2, IsSystem = true },
                    new Category { Name = "Đi lại", CategoryType = 2, IsSystem = true },
                    new Category { Name = "Nhà ở", CategoryType = 2, IsSystem = true },
                    new Category { Name = "Giải trí", CategoryType = 2, IsSystem = true },
                    new Category { Name = "Sức khỏe", CategoryType = 2, IsSystem = true },
                    new Category { Name = "Mua sắm", CategoryType = 2, IsSystem = true },
                    // Thu
                    new Category { Name = "Lương", CategoryType = 1, IsSystem = true },
                    new Category { Name = "Kinh doanh", CategoryType = 1, IsSystem = true },
                    new Category { Name = "Khác", CategoryType = 1, IsSystem = true },
                };

                context.Categories.AddRange(systemCategories);
                context.SaveChanges();
            }

            EnsureSampleUser(context);

            base.Seed(context);
        }



        public static void EnsureSampleUser(AppDbContext context)
        {

            const string sampleEmail = "khanhnv060491@tvu-onschool.edu.vn";

            if (context.Users.Any(u => u.Email == sampleEmail))
                return;

            var sampleUser = new User
            {
                FullName = "Nguyễn Văn Khánh",
                Email = sampleEmail,
                PhoneNumber = "0901234567",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12),
                Role = "User",
                CreatedAt = DateTime.Now
            };

            context.Users.Add(sampleUser);
            context.SaveChanges();

            SeedUserCategories(context, sampleUser.UserId);

            var cashWallet = new Wallet
            {
                UserId = sampleUser.UserId,
                Name = "Tiền mặt",
                WalletType = 1,
                Balance = 3500000,
                IsDefault = true,
                CreatedAt = DateTime.Now
            };

            var bankWallet = new Wallet
            {
                UserId = sampleUser.UserId,
                Name = "Tài khoản ngân hàng",
                WalletType = 2,
                Balance = 12500000,
                CreatedAt = DateTime.Now
            };

            context.Wallets.AddRange(new[] { cashWallet, bankWallet });
            context.SaveChanges();

            var salaryCategory = context.Categories
                .FirstOrDefault(c => c.UserId == sampleUser.UserId && c.Name == "Lương" && c.CategoryType == 1);
            var foodCategory = context.Categories
                .FirstOrDefault(c => c.UserId == sampleUser.UserId && c.Name == "Ăn uống" && c.CategoryType == 2);
            var transportCategory = context.Categories
                .FirstOrDefault(c => c.UserId == sampleUser.UserId && c.Name == "Đi lại" && c.CategoryType == 2);
            var shoppingCategory = context.Categories
                .FirstOrDefault(c => c.UserId == sampleUser.UserId && c.Name == "Mua sắm" && c.CategoryType == 2);

            var today = DateTime.Today;
            var transactions = new List<Transaction>();

            if (salaryCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = sampleUser.UserId,
                    WalletId = bankWallet.WalletId,
                    CategoryId = salaryCategory.CategoryId,
                    Amount = 15000000,
                    TransactionType = 1,
                    TransactionDate = today.AddDays(-5),
                    Description = "Lương tháng"
                });
            }

            if (foodCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = sampleUser.UserId,
                    WalletId = cashWallet.WalletId,
                    CategoryId = foodCategory.CategoryId,
                    Amount = 120000,
                    TransactionType = 2,
                    TransactionDate = today.AddDays(-2),
                    Description = "Ăn trưa"
                });
            }

            if (transportCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = sampleUser.UserId,
                    WalletId = cashWallet.WalletId,
                    CategoryId = transportCategory.CategoryId,
                    Amount = 70000,
                    TransactionType = 2,
                    TransactionDate = today.AddDays(-1),
                    Description = "Xăng xe"
                });
            }

            if (shoppingCategory != null)
            {
                transactions.Add(new Transaction
                {
                    UserId = sampleUser.UserId,
                    WalletId = bankWallet.WalletId,
                    CategoryId = shoppingCategory.CategoryId,
                    Amount = 450000,
                    TransactionType = 2,
                    TransactionDate = today,
                    Description = "Mua đồ dùng cá nhân"
                });
            }

            if (transactions.Any())
            {
                context.Transactions.AddRange(transactions);
            }

            if (foodCategory != null)
            {
                context.Budgets.Add(new Budget
                {
                    UserId = sampleUser.UserId,
                    CategoryId = foodCategory.CategoryId,
                    LimitAmount = 3000000,
                    SpentAmount = transactions
                        .Where(t => t.CategoryId == foodCategory.CategoryId && t.TransactionType == 2)
                        .Sum(t => t.Amount),
                    Month = (byte)today.Month,
                    Year = (short)today.Year,
                    CreatedAt = DateTime.Now
                });
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Seed default categories for a newly registered user based on system categories
        /// </summary>
        public static void SeedUserCategories(AppDbContext context, int userId)
        {
            var systemCategories = context.Categories
                .Where(c => c.IsSystem && !c.IsDeleted && c.ParentCategoryId == null)
                .ToList();

            foreach (var sc in systemCategories)
            {
                context.Categories.Add(new Category
                {
                    UserId = userId,
                    Name = sc.Name,
                    CategoryType = sc.CategoryType,
                    IsSystem = false
                });
            }

            context.SaveChanges();
        }
    }
}
