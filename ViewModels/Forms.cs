using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.ViewModels;

public class LoginForm
{
    [Required(ErrorMessage = "Informe o login."), StringLength(80)] public string Login { get; set; } = "";
    [Required(ErrorMessage = "Informe a senha."), DataType(DataType.Password)] public string Senha { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class CadastroForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o nome."), StringLength(100, MinimumLength = 2)] public string Nome { get; set; } = "";
    [StringLength(20)] public string? Telefone { get; set; }
    public int? CategoriaId { get; set; }
    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Preço deve ser entre 0,01 e 999.999,99.")] public decimal? PrecoAtual { get; set; }
    public bool Ativo { get; set; } = true;
    public bool DisponivelVenda { get; set; } = true;
}
public class EnderecoForm
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    [Required, StringLength(150)] public string Logradouro { get; set; } = "";
    [Required, StringLength(20)] public string Numero { get; set; } = "";
    [Required, StringLength(80)] public string Bairro { get; set; } = "";
    [StringLength(100)] public string? Complemento { get; set; }
    [StringLength(40)] public string? Rotulo { get; set; }
}
public class ItemForm
{
    public int ProdutoId { get; set; }
    [Range(1, 999)] public int Quantidade { get; set; }
}
public class PedidoForm
{
    [Required] public string ChaveOperacao { get; set; } = Guid.NewGuid().ToString();
    [Required, RegularExpression("^(Mesa|Retirada|Delivery)$")] public string TipoAtendimento { get; set; } = "Retirada";
    public int? NumeroMesa { get; set; }
    public int? ClienteId { get; set; }
    public int? EnderecoId { get; set; }
    [Range(typeof(decimal), "0", "9999.99")] public decimal TaxaEntrega { get; set; }
    [Required, RegularExpression("^(PIX|Cartão|Dinheiro)$")] public string FormaPagamento { get; set; } = "PIX";
    public bool Finalizar { get; set; } = true;
    [MinLength(1), MaxLength(100)] public List<ItemForm> Itens { get; set; } = new();
}
public class MovimentoForm
{
    public int ProdutoId { get; set; }
    [Required, RegularExpression("^(ENTRADA|SAIDA)$")] public string Tipo { get; set; } = "ENTRADA";
    [Range(typeof(decimal), "0.001", "999999.999")] public decimal Quantidade { get; set; }
    [Range(typeof(decimal), "0", "999999.999")] public decimal QuantidadeMinima { get; set; }
    [Required, StringLength(300)] public string Observacao { get; set; } = "";
}
public class ConsumoForm
{
    public int ProdutoId { get; set; }
    public int EstoqueBebidaId { get; set; }
    [Range(typeof(decimal), "0.001", "9999.999")] public decimal Quantidade { get; set; }
}
public class UsuarioForm
{
    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2)] public string Nome { get; set; } = "";
    [Required, RegularExpression("^[a-zA-Z0-9._-]{3,80}$", ErrorMessage = "Use 3 a 80 letras, números, ponto, hífen ou sublinhado.")] public string Login { get; set; } = "";
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Use de 12 a 128 caracteres."), DataType(DataType.Password)] public string? Senha { get; set; }
    [Required, RegularExpression("^(Administrador|Caixa)$")] public string Perfil { get; set; } = "Caixa";
    public bool Ativo { get; set; } = true;
    public bool PodeCadastrarClientes { get; set; } = true;
}
public record LinhaRelatorio(DateTime Dia, string Modalidade, string Pagamento, string Produto, string Categoria, int Quantidade, decimal Valor);
public record PivotLinha(DateTime Dia, decimal Mesa, decimal Retirada, decimal Delivery);
public class RelatorioVm
{
    public List<AreaGourmet.Api.Models.Pedido> Pedidos { get; set; } = new();
    public List<LinhaRelatorio> Itens { get; set; } = new();
    public List<PivotLinha> Pivot { get; set; } = new();
}
