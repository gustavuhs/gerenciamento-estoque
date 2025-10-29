using Domain;

namespace Repository
{
    public interface IMovimentacaoEstoqueRepository
    {
        Task<List<MovimentacaoEstoque>> GetAllAsync();
        Task<List<MovimentacaoEstoque>> GetByProdutoIdAsync(int produtoId);
        Task<MovimentacaoEstoque> GetByIdAsync(int id);
        Task AddAsync(MovimentacaoEstoque movimentacao);
        Task UpdateAsync(MovimentacaoEstoque movimentacao);
        Task DeleteAsync(int id);
    }
}