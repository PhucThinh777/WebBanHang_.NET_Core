using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Repository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			if (!_context.Products.Any())
			{
				CategoryModel macbook = new CategoryModel { Name = "Apple", Slug = "Aple", Description = "Aple là công ty nổi tiếng", Status = 1 };
				CategoryModel pc = new CategoryModel { Name = "Samsung", Slug = "Samsung", Description = "Samsung là công ty nổi tiếng", Status = 1 };

				BrandModel apple = new BrandModel { Name = "Macbook", Slug = "Aple", Description = "Aple là công ty nổi tiếng", Status = 1 };
				BrandModel dell = new BrandModel { Name = "PC", Slug = "Samsung", Description = "Samsung là công ty nổi tiếng", Status = 1 };
				_context.Products.AddRange(
					new ProductModel { Name = "Macbook A", Slug = "macbook", Description = "Là máy tính bảng", Image = "1.jpg", Category = macbook, Brand = apple, Price = 20000 },
					new ProductModel { Name = "Dell A", Slug = "dell", Description = "Là máy tính bàn", Image = "2.jpg", Category = pc, Brand = dell, Price = 30000 }
				);
				_context.SaveChanges();
			}
		}
	}
}