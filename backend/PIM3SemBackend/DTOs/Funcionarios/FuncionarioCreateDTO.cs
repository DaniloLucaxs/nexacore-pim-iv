using System.ComponentModel.DataAnnotations;
namespace PIM_3sem_backend.DTOs.Funcionarios;
public class FuncionarioCreateDTO
{
    [Required, StringLength(50, MinimumLength=1)] public string Nome { get; set; } = "";
    [Range(typeof(decimal),"0.01","99999999.99")] public decimal Salario { get; set; }
    [Required, StringLength(50, MinimumLength=1)] public string Cargo { get; set; } = "";
    public Guid IdDepartamento { get; set; }
    public Guid? IdGerente { get; set; }
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [Required, MinLength(10), MaxLength(128)] public string Senha { get; set; } = "";
    public Guid IdPerfil { get; set; }
}
