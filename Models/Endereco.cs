namespace AreaGourmet.Api.Models;

public class Endereco
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string Logradouro { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    public string? Rotulo { get; set; }

    public Cliente Cliente { get; set; } = null!;
}