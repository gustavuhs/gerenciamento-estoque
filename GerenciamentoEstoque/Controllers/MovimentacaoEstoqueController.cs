using Domain;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace GerenciamentoEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacaoEstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;
        private readonly ILogger<MovimentacaoEstoqueController> _logger;

        public MovimentacaoEstoqueController(IEstoqueService estoqueService, ILogger<MovimentacaoEstoqueController> logger)
        {
            _estoqueService = estoqueService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MovimentacaoEstoque>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var movimentacoes = await _estoqueService.GetAllMovimentacoesAsync();
                return Ok(movimentacoes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todas as movimentações de estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpGet("produto/{produtoId:int}")]
        [ProducesResponseType(typeof(IEnumerable<MovimentacaoEstoque>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByProdutoId(int produtoId)
        {
            try
            {
                var produto = await _estoqueService.GetProdutoByIdAsync(produtoId);
                if (produto == null)
                    return NotFound($"Produto com ID {produtoId} não encontrado");

                var movimentacoes = await _estoqueService.GetMovimentacoesByProdutoIdAsync(produtoId);
                return Ok(movimentacoes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter movimentações por produto ID: {ProdutoId}", produtoId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpPost("entrada")]
        [ProducesResponseType(typeof(MovimentacaoEstoque), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RegistrarEntrada([FromBody] MovimentacaoEstoque movimentacao)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Garantir que é uma entrada
                movimentacao.Tipo = TipoMovimentacao.ENTRADA;

                var novaMovimentacao = await _estoqueService.RegistrarEntradaAsync(movimentacao);
                return CreatedAtAction(nameof(GetByProdutoId), new { produtoId = movimentacao.ProdutoId }, novaMovimentacao);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao registrar entrada de estoque");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Produto não encontrado ao registrar entrada");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar entrada de estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpPost("saida")]
        [ProducesResponseType(typeof(MovimentacaoEstoque), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RegistrarSaida([FromBody] MovimentacaoEstoque movimentacao)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Garantir que é uma saída
                movimentacao.Tipo = TipoMovimentacao.SAIDA;

                var novaMovimentacao = await _estoqueService.RegistrarSaidaAsync(movimentacao);
                return CreatedAtAction(nameof(GetByProdutoId), new { produtoId = movimentacao.ProdutoId }, novaMovimentacao);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao registrar saída de estoque");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Produto não encontrado ao registrar saída");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao registrar saída");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar saída de estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpGet("estoque-atual/{produtoId:int}")]
        [ProducesResponseType(typeof(decimal), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetEstoqueAtual(int produtoId)
        {
            try
            {
                var produto = await _estoqueService.GetProdutoByIdAsync(produtoId);
                if (produto == null)
                    return NotFound($"Produto com ID {produtoId} não encontrado");

                var estoqueAtual = await _estoqueService.GetEstoqueAtualAsync(produtoId);
                return Ok(estoqueAtual);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estoque atual do produto: {ProdutoId}", produtoId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }
    }
}