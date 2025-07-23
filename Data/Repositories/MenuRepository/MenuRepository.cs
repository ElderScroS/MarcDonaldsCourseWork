using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarkRestaurant.Models;

namespace MarkRestaurant.Data.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly MarkRestaurantDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;

        private const string UserMenuCacheKey = "UserMenuCache";
        private const string AdminMenuCacheKey = "AdminMenuCache";

        public MenuRepository(MarkRestaurantDbContext context, IWebHostEnvironment webHostEnvironment, IMemoryCache cache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _cache = cache;
        }

        public async Task<List<Product>> GetMenuForAdmin()
        {
            if (!_cache.TryGetValue(AdminMenuCacheKey, out List<Product>? menu))
            {
                menu = await _context.Menu.ToListAsync();
                _cache.Set(AdminMenuCacheKey, menu, TimeSpan.FromMinutes(10));
            }
            return menu!;
        }

        public async Task<List<Product>> GetMenuForUser()
        {

            
            if (!_cache.TryGetValue(UserMenuCacheKey, out List<Product>? menu))
            {
                menu = await _context.Menu
                    .Where(p => p.InStock)
                    .ToListAsync();
                _cache.Set(UserMenuCacheKey, menu, TimeSpan.FromMinutes(10));
            }
            return menu!;
        }

        public async Task<Product?> GetProductByTitleAndCategoryAsync(string title, string category)
        {
            return await _context.Menu.FirstOrDefaultAsync(p => p.Title == title && p.Category == category);
        }

        public async Task<Product?> GetProductById(Guid id)
        {
            return await _context.Menu.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> DeleteProduct(Product product)
        {
            _context.Menu.Remove(product);
            await _context.SaveChangesAsync();

            _cache.Remove(UserMenuCacheKey);
            _cache.Remove(AdminMenuCacheKey);

            return true;
        }

        public async Task<bool> EditProduct(Product product, string category, string title, decimal price, IFormFile? imageFile, bool inStock)
        {
            if (product == null) return false;

            product.Category = category;
            product.Title = title;
            product.Price = price;
            product.InStock = inStock;

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                string categoryFolder = Path.Combine(uploadsFolder, category.ToLower());
                Directory.CreateDirectory(categoryFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(categoryFolder, uniqueFileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(fileStream);

                product.ImageUrl = $"/images/{category.ToLower()}/{uniqueFileName}";
            }

            _context.Menu.Update(product);
            await _context.SaveChangesAsync();

            _cache.Remove(UserMenuCacheKey);
            _cache.Remove(AdminMenuCacheKey);

            return true;
        }

        public async Task<bool> AddProduct(string category, string title, decimal price, IFormFile imageFile)
        {
            string imageUrl;

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                string categoryFolder = Path.Combine(uploadsFolder, category.ToLower());
                Directory.CreateDirectory(categoryFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(categoryFolder, uniqueFileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(fileStream);

                imageUrl = $"/images/{category.ToLower()}/{uniqueFileName}";
            }
            else
            {
                imageUrl = "/images/none.jpg";
            }

            var product = new Product(category, title, price, imageUrl, true);

            _context.Menu.Add(product);
            await _context.SaveChangesAsync();

            _cache.Remove(UserMenuCacheKey);
            _cache.Remove(AdminMenuCacheKey);

            return true;
        }
    }
}
