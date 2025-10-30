using Domain;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Net;

namespace GerenciamentoEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;
        private readonly ILogger<ProdutoController> _logger;

        public ProdutoController(IEstoqueService estoqueService, ILogger<ProdutoController> logger)
        {
            _estoqueService = estoqueService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Produto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var produtos = await _estoqueService.GetAllProdutosAsync();
                return Ok(produtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os produtos");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Produto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var produto = await _estoqueService.GetProdutoByIdAsync(id);
                if (produto == null)
                    return NotFound($"Produto com ID {id} não encontrado");

                return Ok(produto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter produto por ID: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpGet("sku/{sku}")]
        [ProducesResponseType(typeof(Produto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetBySku(string sku)
        {
            try
            {
                var produto = await _estoqueService.GetProdutoBySkuAsync(sku);
                if (produto == null)
                    return NotFound($"Produto com SKU {sku} não encontrado");

                return Ok(produto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter produto por SKU: {Sku}", sku);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Produto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] Produto produto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var novoProdutoId = await _estoqueService.AddProdutoAsync(produto);
                return CreatedAtAction(nameof(GetById), new { id = novoProdutoId }, novoProdutoId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar produto");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Produto produto)
        {
            try
            {
                if (id != produto.Id)
                    return BadRequest("ID na URL não corresponde ao ID no corpo da requisição");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var produtoExistente = await _estoqueService.GetProdutoByIdAsync(id);
                if (produtoExistente == null)
                    return NotFound($"Produto com ID {id} não encontrado");

                await _estoqueService.UpdateProdutoAsync(produto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar produto: {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var produtoExistente = await _estoqueService.GetProdutoByIdAsync(id);
                if (produtoExistente == null)
                    return NotFound($"Produto com ID {id} não encontrado");

                await _estoqueService.DeleteProdutoAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir produto: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }

        [HttpGet("baixo-estoque")]
        [ProducesResponseType(typeof(IEnumerable<Produto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProdutosBaixoEstoque()
        {
            try
            {
                var produtos = await _estoqueService.GetProdutosAbaixoDoMinimoAsync();
                return Ok(produtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter produtos com estoque baixo");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao processar a solicitação");
            }
        }
    }
}