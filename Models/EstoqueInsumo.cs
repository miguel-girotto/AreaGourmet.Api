namespace AreaGourmet.Api.Models;

public class EstoqueBebida
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }

    public decimal QuantidadeAtual { get; set; }

    public decimal QuantidadeMinima { get; set; }

    public Produto Produto { get; set; } = null!;
}