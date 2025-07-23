namespace MarkRestaurant.Data.Repositories
{
    public interface IMenuRepository
    {
        Task<List<Product>> GetMenuForAdmin();
        Task<List<Product>> GetMenuForUser();
        Task<Product?> GetProductById(Guid id);
        Task<Product?> GetProductByTitleAndCategoryAsync(string title, string category);
        Task<bool> DeleteProduct(Product product);
        Task<bool> EditProduct(Product product, string category, string title, decimal price, IFormFile? imageFile, bool inStock);
        Task<bool> AddProduct(string category, string title, decimal price, IFormFile imageFile);
    }
}
