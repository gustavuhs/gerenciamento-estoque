using Domain;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
    {
        private readonly AppDbContext _context;

        public MovimentacaoEstoqueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovimentacaoEstoque>> GetAllAsync()
        {
            return await _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .ToListAsync();
        }

        public async Task<List<MovimentacaoEstoque>> GetByProdutoIdAsync(int produtoId)
        {
            return await _context.MovimentacoesEstoque
                .Where(m => m.ProdutoId == produtoId)
                .Include(m => m.Produto)
                .ToListAsync();
        }

        public async Task<MovimentacaoEstoque> GetByIdAsync(int id)
        {
            return await _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddAsync(MovimentacaoEstoque movimentacao)
        {
            await _context.MovimentacoesEstoque.AddAsync(movimentacao);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MovimentacaoEstoque movimentacao)
        {
            _context.MovimentacoesEstoque.Update(movimentacao);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var movimentacao = await _context.MovimentacoesEstoque.FindAsync(id);
            if (movimentacao != null)
            {
                _context.MovimentacoesEstoque.Remove(movimentacao);
                await _context.SaveChangesAsync();
            }
        }
    }
}