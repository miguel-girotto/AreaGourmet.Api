namespace AreaGourmet.Api.Models;

public class Categoria
{
    public int Id { get; set; }

    public bool Ativo { get; set; } = true;

    public string Nome { get; set; } = string.Empty;

    public List<Produto> Produtos { get; set; } = new();
}