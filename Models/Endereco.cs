using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.Models;

public class Endereco
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Endereco Obrigatorio")]
    [StringLength(40)]
    public string Logradouro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numero Obrigatorio")]
    [StringLength(5)]
    public string Numero { get; set; } = string.Empty;

    [StringLength(30)]
    public string Bairro { get; set; } = string.Empty;

    [StringLength(40)]
    public string? Complemento { get; set; }

    [StringLength(30)]
    public string? Rotulo { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public ICollection<Pedido> Pedidos { get; set;} = new List<Pedido>();
}