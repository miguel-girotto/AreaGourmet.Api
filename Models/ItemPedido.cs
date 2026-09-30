namespace AreaGourmet.Api.Models;

public class ItemPedido
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal SubtotalItem { get; set; }

    public Pedido Pedido { get; set; } = null!;

    public Produto Produto { get; set; } = null!;
}