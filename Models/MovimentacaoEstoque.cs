namespace AreaGourmet.Api.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int EstoqueBebidaId { get; set; }
    public EstoqueBebida EstoqueBebida { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }
    public string Tipo { get; set; } = "";
    public decimal Quantidade { get; set; }
    public decimal SaldoApos { get; set; }
    public string Observacao { get; set; } = "";
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}

// O estoque existente mantém seu nome por compatibilidade e também comporta embalagens.
public class ConsumoEstoque
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int EstoqueBebidaId { get; set; }
    public EstoqueBebida EstoqueBebida { get; set; } = null!;
    public decimal Quantidade { get; set; }
}

public class Auditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Acao { get; set; } = "";
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}

public class ControleOperacao
{
    public int Id { get; set; }
    public long Versao { get; set; }
}
