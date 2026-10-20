using PIM_3sem_backend.DTOs.Departamentos;

namespace PIM_3sem_backend.Services.Departamentos
{
    public interface IDepartamentoService
    {
        Task<DepartamentoResponseDTO> ObterPorId(Guid departamentoId);
        Task<IReadOnlyCollection<DepartamentoResponseDTO>> ObterTodos();
    }
}
