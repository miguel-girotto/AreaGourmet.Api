namespace AreaGourmet.Api.Models;

public class Pedido
{
    public int Id { get; set; }

    public int NumeroComanda { get; set; }

    public int UsuarioId { get; set; }

    public int? ClienteId { get; set; }

    public int? EnderecoId { get; set; }

    public string TipoAtendimento { get; set; } = string.Empty;

    public int? NumeroMesa { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxaEntrega { get; set; }

    public decimal ValorTotal { get; set; }

    public string FormaPagamento { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime DataHora { get; set; } = DateTime.Now;

    public Usuario Usuario { get; set; } = null!;

    public Cliente? Cliente { get; set; }

    public Endereco? Endereco { get; set; }

    public List<ItemPedido> Itens { get; set; } = new();
}