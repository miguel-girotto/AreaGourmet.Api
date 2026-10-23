namespace AreaGourmet.Api.Models;

public class Produto
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal PrecoAtual { get; set; }

    public string? Imagem { get; set; }

    public bool DisponivelVenda { get; set; } = true;

    public bool Ativo { get; set; } = true;

    public Categoria Categoria { get; set; } = null!;

    public List<ItemPedido> ItensPedido { get; set; } = new();

    public EstoqueBebida? EstoqueBebida { get; set; }
}