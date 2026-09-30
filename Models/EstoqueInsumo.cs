namespace AreaGourmet.Api.Models;

public class EstoqueInsumo
{
    public int Id { get; set; }

    public string NomeInsumo { get; set; } = string.Empty;

    public decimal QuantidadeAtual { get; set; }

    public decimal QuantidadeMinima { get; set; }
}