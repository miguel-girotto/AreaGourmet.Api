using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.Models;

public class Pedido
{
    public int Id { get; set; }

    public int NumeroComanda { get; set; }

    public int UsuarioId { get; set; }

    public int? ClienteId { get; set; }

    public int? EnderecoId { get; set; }

    [Required(ErrorMessage = "Informe o tipo de atendimento")]
    [StringLength(50)]
    public string TipoAtendimento { get; set; } = string.Empty;

    public int? NumeroMesa { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxaEntrega { get; set; }

    public decimal ValorTotal { get; set; }

    [Required(ErrorMessage = "Informe o metodo de pagamento")]
    [StringLength(30)]
    public string FormaPagamento { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime DataHora { get; set; } = DateTime.Now;


    public Usuario Usuario { get; set; } = null!;

    public Cliente? Cliente { get; set; }

    public Endereco? Endereco { get; set; }

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}