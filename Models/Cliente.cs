using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome Obrigatorio")]
    [StringLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Insira um telefone")]
    public string Telefone { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}   