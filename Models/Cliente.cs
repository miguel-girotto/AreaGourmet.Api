namespace AreaGourmet.Api.Models;

public class Cliente
{
    public int Id { get; set; }

    public bool Ativo { get; set; } = true;

    public string Nome { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public List<Endereco> Enderecos { get; set; } = new();
}