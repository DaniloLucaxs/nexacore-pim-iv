namespace PIM_3sem_backend.Models;
public class Solicitacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IdFuncionario { get; set; }
    public Funcionario Funcionario { get; set; } = null!;
    public Guid IdSolicitante { get; set; }
    public Usuario Solicitante { get; set; } = null!;
    public Guid? IdResponsavel { get; set; }
    public Usuario? Responsavel { get; set; }
    public decimal SalarioAnterior { get; set; }
    public decimal NovoSalario { get; set; }
    public string Motivo { get; set; } = "";
    public string Status { get; set; } = "Pendente";
    public string? Justificativa { get; set; }
    public DateTimeOffset CriadaEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DecididaEm { get; set; }
}
public class Auditoria
{
    public long Id { get; set; }
    public string Entidade { get; set; } = "Funcionario";
    public Guid RegistroId { get; set; }
    public string Operacao { get; set; } = "";
    public string? ValorAnterior { get; set; }
    public string? ValorNovo { get; set; }
    public DateTimeOffset DataHora { get; set; }
    public Guid? IdUsuario { get; set; }
    public string Responsavel { get; set; } = "Sistema";
}
public class IndicadorFuncionario
{
    public Guid Id { get; set; }
    public Guid IdDepartamento { get; set; }
    public string Departamento { get; set; } = "";
    public Guid? IdGerente { get; set; }
    public bool Ativo { get; set; }
    public decimal Salario { get; set; }
}
