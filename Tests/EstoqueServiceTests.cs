using Domain;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Repository;
using Service;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class EstoqueServiceTests
    {
        private readonly Mock<ILogger<EstoqueService>> _loggerMock;
        private readonly AppDbContext _dbContext;
        private readonly ProdutoRepository _produtoRepository;
        private readonly MovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly EstoqueService _estoqueService;

        public EstoqueServiceTests()
        {
            // Configurar o banco de dados em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _produtoRepository = new ProdutoRepository(_dbContext);
            _movimentacaoRepository = new MovimentacaoEstoqueRepository(_dbContext);
            _loggerMock = new Mock<ILogger<EstoqueService>>();
            _estoqueService = new EstoqueService(_produtoRepository, _movimentacaoRepository, _loggerMock.Object);
        }

        [Fact]
        public async Task ProdutoPerecivelSemDataValidade_DeveLancarExcecao()
        {
            // Arrange
            var produto = new Produto
            {
                CodigoSku = "SKU001",
                Nome = "Produto Teste",
                Categoria = CategoriaProduto.PERECIVEL,
                PrecoUnitario = 10.0m,
                QuantidadeMinima = 5
            };
            await _produtoRepository.AddAsync(produto);

            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 10,
                Lote = "LOTE001",
                // DataValidade não informada
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ProdutoPerecivelSemDataValidadeException>(
                () => _estoqueService.RegistrarEntradaAsync(movimentacao));
        }

        [Fact]
        public async Task SaidaComQuantidadeMaiorQueEstoque_DeveLancarExcecao()
        {
            // Arrange
            var produto = new Produto
            {
                CodigoSku = "SKU002",
                Nome = "Produto Teste 2",
                Categoria = CategoriaProduto.NAO_PERECIVEL,
                PrecoUnitario = 15.0m,
                QuantidadeMinima = 5
            };
            await _produtoRepository.AddAsync(produto);

            var movimentacaoSaida = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 10,
                Tipo = TipoMovimentacao.SAIDA
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EstoqueInsuficienteException>(
                () => _estoqueService.RegistrarSaidaAsync(movimentacaoSaida));
        }

        [Fact]
        public async Task MovimentacaoComQuantidadeNegativa_DeveLancarExcecao()
        {
            // Arrange
            var produto = new Produto
            {
                CodigoSku = "SKU003",
                Nome = "Produto Teste 3",
                Categoria = CategoriaProduto.NAO_PERECIVEL,
                PrecoUnitario = 20.0m,
                QuantidadeMinima = 5
            };
            await _produtoRepository.AddAsync(produto);

            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = -5,
                Tipo = TipoMovimentacao.ENTRADA
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<QuantidadeInvalidaException>(
                () => _estoqueService.RegistrarEntradaAsync(movimentacao));
        }

        [Fact]
        public async Task ProdutoPerecivelAposDataValidade_DeveLancarExcecao()
        {
            // Arrange
            var produto = new Produto
            {
                CodigoSku = "SKU004",
                Nome = "Produto Perecível",
                Categoria = CategoriaProduto.PERECIVEL,
                PrecoUnitario = 25.0m,
                QuantidadeMinima = 5
            };
            await _produtoRepository.AddAsync(produto);

            var dataVencida = DateTime.Now.AddDays(-1);
            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 10,
                Lote = "LOTE002",
                DataValidade = dataVencida,
                Tipo = TipoMovimentacao.ENTRADA
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ProdutoVencidoException>(
                () => _estoqueService.RegistrarEntradaAsync(movimentacao));
        }

        [Fact]
        public async Task CalculoSaldoAposMovimentacoes_DeveEstarCorreto()
        {
            // Arrange
            var produto = new Produto
            {
                CodigoSku = "SKU005",
                Nome = "Produto Teste 5",
                Categoria = CategoriaProduto.NAO_PERECIVEL,
                PrecoUnitario = 30.0m,
                QuantidadeMinima = 5
            };
            await _produtoRepository.AddAsync(produto);

            var movimentacaoEntrada = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 20,
                Tipo = TipoMovimentacao.ENTRADA
            };
            await _estoqueService.RegistrarEntradaAsync(movimentacaoEntrada);

            var movimentacaoSaida = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 8,
                Tipo = TipoMovimentacao.SAIDA
            };
            await _estoqueService.RegistrarSaidaAsync(movimentacaoSaida);

            // Act
            var saldoAtual = await _estoqueService.GetEstoqueAtualAsync(produto.Id);

            // Assert
            Assert.Equal(12, saldoAtual); // 20 (entrada) - 8 (saída) = 12
        }

        [Fact]
        public async Task AlertaEstoqueMinimo_DeveSerGerado()
        {
            // Arrange
            var produto = new Produto
            {
                CodigoSku = "SKU006",
                Nome = "Produto Teste 6",
                Categoria = CategoriaProduto.NAO_PERECIVEL,
                PrecoUnitario = 40.0m,
                QuantidadeMinima = 10
            };
            await _produtoRepository.AddAsync(produto);

            var movimentacaoEntrada = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 15,
                Tipo = TipoMovimentacao.ENTRADA
            };
            await _estoqueService.RegistrarEntradaAsync(movimentacaoEntrada);

            var movimentacaoSaida = new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Quantidade = 10,
                Tipo = TipoMovimentacao.SAIDA
            };

            // Act
            await _estoqueService.RegistrarSaidaAsync(movimentacaoSaida);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("ALERTA: Produto Produto Teste 6")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}