using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AreaGourmet.Models;

public class Usuario
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = "";
    [Required] public string SenhaHash { get; set; } = "";
    [Required, RegularExpression("Administrador|Caixa")] public string Perfil { get; set; } = "Caixa";
    public bool Ativo { get; set; } = true;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
}
public class Cliente
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    [Required, RegularExpression(@"\d{10,11}", ErrorMessage = "Informe DDD e telefone, apenas 10 ou 11 dígitos.")] public string Telefone { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public List<Endereco> Enderecos { get; set; } = [];
}
public class Endereco
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    [Required, StringLength(150)] public string Logradouro { get; set; } = "";
    [Required, StringLength(15)] public string Numero { get; set; } = "";
    [Required, StringLength(80)] public string Bairro { get; set; } = "";
    [Required, StringLength(80)] public string Cidade { get; set; } = "";
    [Required, RegularExpression(@"\d{8}", ErrorMessage = "Informe oito dígitos.")] public string CEP { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public string Resumo => $"{Logradouro}, {Numero} — {Bairro}, {Cidade}";
}
public class Produto
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    [Required, RegularExpression("Marmitas|Bebidas|Adicionais")] public string Categoria { get; set; } = "Marmitas";
    [Range(typeof(decimal), "0.01", "99999", ParseLimitsInInvariantCulture = true)] public decimal PrecoUnitario { get; set; }
    [Range(0, 1000000)] public int EstoqueInsumo { get; set; }
    public bool ControlaEstoque { get; set; } = true;
    public bool Ativo { get; set; } = true;
    [Range(1, int.MaxValue)] public int Versao { get; set; } = 1;
    [StringLength(100)] public string Imagem { get; set; } = "meal.jpg";
}
public class Pedido
{
    public int Id { get; set; }
    public Guid ChaveOperacao { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string TipoAtendimento { get; set; } = "Retirada";
    public int? MesaNumero { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int? EnderecoId { get; set; }
    public Endereco? Endereco { get; set; }
    public string EnderecoEntrega { get; set; } = "";
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public decimal TaxaEntrega { get; set; }
    public decimal ValorTotal { get; set; }
    public string FormatoPagamento { get; set; } = "Pix";
    public List<ItemPedido> Itens { get; set; } = [];
}
public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int Quantidade { get; set; }
    public decimal PrecoPraticado { get; set; }
    public string NomeProduto { get; set; } = "";
    public string CategoriaPraticada { get; set; } = "";
}
