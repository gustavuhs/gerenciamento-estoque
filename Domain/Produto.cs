namespace Domain
{
    public class Produto
    {
        public int Id { get; set; }
        public string CodigoSku { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public CategoriaProduto Categoria { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int QuantidadeMinima { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
    }
    public enum CategoriaProduto
    {
        PERECIVEL,
        NAO_PERECIVEL
    }
}