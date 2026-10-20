using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.Models;

public class Produto
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Nome Obrigatorio")]
    [StringLength(50)]
    public string Nome { get; set; } = string.Empty;

    public decimal PrecoAtual { get; set; }

    public string? Imagem { get; set; }

    public bool Ativo { get; set; } = true;


    public Categoria Categoria { get; set; } = null!;

    public EstoqueBebida? EstoqueBebida { get; set; }

    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();

}