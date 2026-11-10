using System.ComponentModel.DataAnnotations;
namespace AreaGourmet.ViewModels;

public class LoginForm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Senha { get; set; } = "";
}
public class UsuarioForm
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = "";
    [StringLength(100, MinimumLength = 10), DataType(DataType.Password)] public string? Senha { get; set; }
    [Required, RegularExpression("Administrador|Caixa")] public string Perfil { get; set; } = "Caixa";
    public bool Ativo { get; set; } = true;
}
public class PedidoForm
{
    public Guid ChaveOperacao { get; set; }
    [Required, RegularExpression("Mesa|Retirada|Delivery")] public string TipoAtendimento { get; set; } = "Retirada";
    [Range(1, 999)] public int? MesaNumero { get; set; }
    public int? ClienteId { get; set; }
    public int? EnderecoId { get; set; }
    [Required, RegularExpression("Pix|Cartão|Dinheiro")] public string FormatoPagamento { get; set; } = "Pix";
    [MinLength(1), MaxLength(100)] public List<ItemForm> Itens { get; set; } = [];
}
public class ItemForm
{
    [Range(1, int.MaxValue)] public int ProdutoId { get; set; }
    [Range(1, 999)] public int Quantidade { get; set; }
}
public record VendaLinha(int Id, DateTime DataHora, string Cliente, string Operador, string Tipo, string Pagamento, int Quantidade, decimal Produtos, decimal Entrega, decimal Total);
public record Grupo(string Nome, decimal Total);
public class RelatorioVM
{
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public List<VendaLinha> Vendas { get; set; } = [];
    public List<Grupo> Pagamentos { get; set; } = [];
    public Dictionary<string, Dictionary<string, decimal>> Pivot { get; set; } = [];
    public List<string> Meses { get; set; } = [];
}
