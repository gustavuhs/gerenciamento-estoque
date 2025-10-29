using Domain;
using Microsoft.Extensions.Logging;
using Repository;

namespace Service
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly ILogger<EstoqueService> _logger;

        public EstoqueService(
            IProdutoRepository produtoRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository,
            ILogger<EstoqueService> logger)
        {
            _produtoRepository = produtoRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _logger = logger;
        }

        public async Task<List<Produto>> GetAllProdutosAsync()
        {
            return await _produtoRepository.GetAllAsync();
        }

        public async Task<Produto> GetProdutoByIdAsync(int id)
        {
            return await _produtoRepository.GetByIdAsync(id);
        }

        public async Task<Produto> GetProdutoBySkuAsync(string sku)
        {
            return await _produtoRepository.GetBySkuAsync(sku);
        }

        public async Task<int> AddProdutoAsync(Produto produto)
        {
            if (await _produtoRepository.ExistsBySkuAsync(produto.CodigoSku))
            {
                throw new InvalidOperationException($"Já existe um produto com o SKU {produto.CodigoSku}");
            }
            
            ValidarProduto(produto);

            produto.DataCriacao = DateTime.UtcNow;
            await _produtoRepository.AddAsync(produto);
            return produto.Id;
        }
        
        private void ValidarProduto(Produto produto)
        {
            if (string.IsNullOrWhiteSpace(produto.CodigoSku))
            {
                throw new ArgumentException("O código SKU é obrigatório");
            }
            
            if (string.IsNullOrWhiteSpace(produto.Nome))
            {
                throw new ArgumentException("O nome do produto é obrigatório");
            }
            
            if (produto.PrecoUnitario <= 0)
            {
                throw new ArgumentException("O preço unitário deve ser maior que zero");
            }
            
            if (produto.QuantidadeMinima < 0)
            {
                throw new ArgumentException("A quantidade mínima não pode ser negativa");
            }
        }

        public async Task UpdateProdutoAsync(Produto produto)
        {
            var existingProduto = await _produtoRepository.GetByIdAsync(produto.Id);
            if (existingProduto == null)
            {
                throw new InvalidOperationException($"Produto com ID {produto.Id} não encontrado");
            }

            if (existingProduto.CodigoSku != produto.CodigoSku && 
                await _produtoRepository.ExistsBySkuAsync(produto.CodigoSku))
            {
                throw new InvalidOperationException($"Já existe um produto com o SKU {produto.CodigoSku}");
            }
            
            ValidarProduto(produto);

            await _produtoRepository.UpdateAsync(produto);
        }

        public async Task DeleteProdutoAsync(int id)
        {
            if (!await _produtoRepository.ExistsAsync(id))
            {
                throw new InvalidOperationException($"Produto com ID {id} não encontrado");
            }

            await _produtoRepository.DeleteAsync(id);
        }

        public async Task<List<MovimentacaoEstoque>> GetAllMovimentacoesAsync()
        {
            return await _movimentacaoRepository.GetAllAsync();
        }

        public async Task<List<MovimentacaoEstoque>> GetMovimentacoesByProdutoIdAsync(int produtoId)
        {
            return await _movimentacaoRepository.GetByProdutoIdAsync(produtoId);
        }

        public async Task<int> RegistrarEntradaAsync(MovimentacaoEstoque movimentacao)
        {
            if (movimentacao.Quantidade <= 0)
            {
                throw new InvalidOperationException("A quantidade deve ser maior que zero");
            }

            var produto = await _produtoRepository.GetByIdAsync(movimentacao.ProdutoId);
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {movimentacao.ProdutoId} não encontrado");
            }

            if (produto.Categoria == CategoriaProduto.PERECIVEL)
            {
                if (string.IsNullOrEmpty(movimentacao.Lote))
                {
                    throw new InvalidOperationException("Produtos perecíveis devem ter lote informado");
                }

                if (!movimentacao.DataValidade.HasValue)
                {
                    throw new InvalidOperationException("Produtos perecíveis devem ter data de validade");
                }
            }

            movimentacao.Tipo = TipoMovimentacao.ENTRADA;
            movimentacao.DataMovimentacao = DateTime.UtcNow;
            
            await _movimentacaoRepository.AddAsync(movimentacao);
            return movimentacao.Id;
        }

        public async Task<int> RegistrarSaidaAsync(MovimentacaoEstoque movimentacao)
        {
            if (movimentacao.Quantidade <= 0)
            {
                throw new InvalidOperationException("A quantidade deve ser maior que zero");
            }

            var produto = await _produtoRepository.GetByIdAsync(movimentacao.ProdutoId);
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {movimentacao.ProdutoId} não encontrado");
            }

            int estoqueAtual = await _produtoRepository.GetEstoqueAtualAsync(movimentacao.ProdutoId);
            if (estoqueAtual < movimentacao.Quantidade)
            {
                throw new InvalidOperationException($"Estoque insuficiente. Disponível: {estoqueAtual}, Solicitado: {movimentacao.Quantidade}");
            }

            movimentacao.Tipo = TipoMovimentacao.SAIDA;
            movimentacao.DataMovimentacao = DateTime.UtcNow;
            
            await _movimentacaoRepository.AddAsync(movimentacao);

            estoqueAtual -= movimentacao.Quantidade;
            if (estoqueAtual < produto.QuantidadeMinima)
            {
                _logger.LogWarning($"ALERTA: Produto {produto.Nome} (SKU: {produto.CodigoSku}) está abaixo do estoque mínimo. Atual: {estoqueAtual}, Mínimo: {produto.QuantidadeMinima}");
            }

            return movimentacao.Id;
        }

        public async Task<int> GetEstoqueAtualAsync(int produtoId)
        {
            return await _produtoRepository.GetEstoqueAtualAsync(produtoId);
        }

        public async Task<List<Produto>> GetProdutosAbaixoDoMinimoAsync()
        {
            var produtos = await _produtoRepository.GetAllAsync();
            var result = new List<Produto>();

            foreach (var produto in produtos)
            {
                int estoqueAtual = await _produtoRepository.GetEstoqueAtualAsync(produto.Id);
                if (estoqueAtual < produto.QuantidadeMinima)
                {
                    result.Add(produto);
                }
            }

            return result;
        }

        public async Task<decimal> CalcularValorTotalEstoqueAsync()
        {
            decimal valorTotal = 0;
            var produtos = await _produtoRepository.GetAllAsync();

            foreach (var produto in produtos)
            {
                int quantidadeEstoque = await _produtoRepository.GetEstoqueAtualAsync(produto.Id);
                valorTotal += quantidadeEstoque * produto.PrecoUnitario;
            }

            return valorTotal;
        }

        public async Task<List<Produto>> ListarProdutosAVencerEmDiasAsync(int dias)
        {
            if (dias <= 0)
            {
                throw new ArgumentException("O número de dias deve ser maior que zero");
            }

            var dataLimite = DateTime.UtcNow.AddDays(dias);
            var result = new List<Produto>();
            var produtos = await _produtoRepository.GetAllAsync();

            foreach (var produto in produtos)
            {
                if (produto.Categoria == CategoriaProduto.PERECIVEL)
                {
                    var movimentacoes = await _movimentacaoRepository.GetByProdutoIdAsync(produto.Id);
                    
                    var movimentacoesAVencer = movimentacoes
                        .Where(m => m.Tipo == TipoMovimentacao.ENTRADA && 
                                   m.DataValidade.HasValue && 
                                   m.DataValidade.Value <= dataLimite &&
                                   m.DataValidade.Value >= DateTime.UtcNow)
                        .ToList();
                    
                    if (movimentacoesAVencer.Any())
                    {
                        if (!result.Contains(produto))
                        {
                            result.Add(produto);
                        }
                    }
                }
            }

            return result;
        }
    }
}