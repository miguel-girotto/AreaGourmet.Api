using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome e obrigatorio")]
    [StringLength(30)]
    public string Nome { get; set; } = string.Empty;

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}