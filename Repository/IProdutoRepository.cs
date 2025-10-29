using Domain;

namespace Repository
{
    public interface IProdutoRepository
    {
        Task<List<Produto>> GetAllAsync();
        Task<Produto> GetByIdAsync(int id);
        Task<Produto> GetBySkuAsync(string sku);
        Task AddAsync(Produto produto);
        Task UpdateAsync(Produto produto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsBySkuAsync(string sku);
        Task<int> GetEstoqueAtualAsync(int produtoId);
    }
}
