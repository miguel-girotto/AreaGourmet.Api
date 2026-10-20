using System.ComponentModel.DataAnnotations;

namespace AreaGourmet.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome Obrigatorio")]
    [StringLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Login Obrigatorio")]
    [StringLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha Obrigatorio")]
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome Obrigatorio")]
    [StringLength(50)]
    public string Perfil { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}