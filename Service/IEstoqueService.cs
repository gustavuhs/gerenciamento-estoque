using Domain;

namespace Service
{
    public interface IEstoqueService
    {
        Task<List<Produto>> GetAllProdutosAsync();
        Task<Produto> GetProdutoByIdAsync(int id);
        Task<Produto> GetProdutoBySkuAsync(string sku);
        Task<int> AddProdutoAsync(Produto produto);
        Task UpdateProdutoAsync(Produto produto);
        Task DeleteProdutoAsync(int id);
        
        Task<List<MovimentacaoEstoque>> GetAllMovimentacoesAsync();
        Task<List<MovimentacaoEstoque>> GetMovimentacoesByProdutoIdAsync(int produtoId);
        Task<int> RegistrarEntradaAsync(MovimentacaoEstoque movimentacao);
        Task<int> RegistrarSaidaAsync(MovimentacaoEstoque movimentacao);
        Task<int> GetEstoqueAtualAsync(int produtoId);
        Task<List<Produto>> GetProdutosAbaixoDoMinimoAsync();
    }
}