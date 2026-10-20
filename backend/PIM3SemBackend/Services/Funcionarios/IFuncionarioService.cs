using PIM_3sem_backend.DTOs.Funcionarios;

namespace PIM_3sem_backend.Services.Funcionarios
{
    public interface IFuncionarioService
    {
        Task<FuncionarioResponseDTO> CriarFuncionario(FuncionarioCreateDTO novoFuncionario);
        Task<FuncionarioResponseDTO> ObterPorId(Guid funcionarioId);
        Task<IReadOnlyCollection<FuncionarioResponseDTO>> ObterTodos();
        Task<IReadOnlyCollection<FuncionarioResponseDTO>> ObterFuncionarioDoGerente(Guid gerenteId);
        Task AtualizarFuncionario(FuncionarioUpdateDTO novasInformacoes, Guid funcionarioId);
        Task RemoverFuncionario(Guid funcionarioId);
        Task<IReadOnlyCollection<FuncionarioResponseDTO>> ObterTodosGerentes();
    }
}
