using System.Data.Entity;

namespace QuanLyChiTieuCaNhan.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("AppDbContext")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasRequired(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transaction>()
                .HasOptional(t => t.ToWallet)
                .WithMany(w => w.TransferTransactions)
                .HasForeignKey(t => t.ToWalletId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transaction>()
                .HasRequired(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transaction>()
                .HasRequired(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Category>()
                .HasOptional(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Wallet>()
                .HasRequired(w => w.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(w => w.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Budget>()
                .HasRequired(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Budget>()
                .HasRequired(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .WillCascadeOnDelete(false);

            // Decimal precision
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance).HasPrecision(18, 2);
            modelBuilder.Entity<Budget>()
                .Property(b => b.LimitAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Budget>()
                .Property(b => b.SpentAmount).HasPrecision(18, 2);

            modelBuilder.Entity<Budget>()
                .HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year })
                .IsUnique()
                .HasName("UQ_Budget_User_Cat_Period");

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.TransactionDate })
                .HasName("IX_Transactions_UserId_Date");

            base.OnModelCreating(modelBuilder);
        }
    }
}
