using Domain;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Produto>> GetAllAsync()
        {
            return await _context.Produtos.ToListAsync();
        }

        public async Task<Produto> GetByIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Movimentacoes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Produto> GetBySkuAsync(string sku)
        {
            return await _context.Produtos
                .Include(p => p.Movimentacoes)
                .FirstOrDefaultAsync(p => p.CodigoSku == sku);
        }

        public async Task AddAsync(Produto produto)
        {
            await _context.Produtos.AddAsync(produto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Produto produto)
        {
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Produtos.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsBySkuAsync(string sku)
        {
            return await _context.Produtos.AnyAsync(p => p.CodigoSku == sku);
        }

        public async Task<int> GetEstoqueAtualAsync(int produtoId)
        {
            var movimentacoes = await _context.MovimentacoesEstoque
                .Where(m => m.ProdutoId == produtoId)
                .ToListAsync();

            int estoque = 0;
            foreach (var movimentacao in movimentacoes)
            {
                if (movimentacao.Tipo == TipoMovimentacao.ENTRADA)
                    estoque += movimentacao.Quantidade;
                else
                    estoque -= movimentacao.Quantidade;
            }

            return estoque;
        }
    }
}