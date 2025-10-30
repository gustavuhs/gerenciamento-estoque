namespace Domain.Exceptions
{
    public class EstoqueException : Exception
    {
        public EstoqueException(string message) : base(message) { }
    }

    public class ProdutoPerecivelSemDataValidadeException : EstoqueException
    {
        public ProdutoPerecivelSemDataValidadeException() 
            : base("Produto perecível deve ter data de validade informada.") { }
    }

    public class EstoqueInsuficienteException : EstoqueException
    {
        public EstoqueInsuficienteException() 
            : base("Quantidade insuficiente em estoque para realizar a saída.") { }
    }

    public class QuantidadeInvalidaException : EstoqueException
    {
        public QuantidadeInvalidaException() 
            : base("A quantidade da movimentação deve ser maior que zero.") { }
    }

    public class ProdutoVencidoException : EstoqueException
    {
        public ProdutoVencidoException() 
            : base("Não é possível movimentar produto com data de validade vencida.") { }
    }

    public class DadosObrigatoriosProdutoException : EstoqueException
    {
        public DadosObrigatoriosProdutoException(string campo) 
            : base($"O campo {campo} é obrigatório para o produto.") { }
    }
}