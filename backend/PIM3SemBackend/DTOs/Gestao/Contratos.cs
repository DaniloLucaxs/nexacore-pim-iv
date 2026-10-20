using System.ComponentModel.DataAnnotations;
namespace PIM_3sem_backend.DTOs.Gestao;
public record EditarFuncionario(
    [Required, StringLength(50, MinimumLength=1)] string Nome,
    [Required, StringLength(50, MinimumLength=1)] string Cargo,
    Guid IdDepartamento, bool Ativo, DateOnly DataAdmissao, Guid? IdGerente = null);
public record NovaSolicitacao(Guid IdFuncionario, decimal? Percentual, decimal? Valor, [Required, StringLength(500, MinimumLength=3)] string Motivo);
public record Decisao([Required, StringLength(500, MinimumLength=3)] string Justificativa);
public record ConsultaFuncionarios(string? Busca = null, Guid? Departamento = null, bool? Ativo = null, string Ordem = "nome", int Pagina = 1, int Tamanho = 10);
