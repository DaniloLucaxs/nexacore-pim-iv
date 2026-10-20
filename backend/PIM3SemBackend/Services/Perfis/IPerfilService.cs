using PIM_3sem_backend.DTOs.Perfis;

namespace PIM_3sem_backend.Services.Perfis
{
    public interface IPerfilService
    {
        Task<PerfilResponseDTO> ObterPorId(Guid perfilId);
        Task<IReadOnlyCollection<PerfilResponseDTO>> ObterTodos();
    }
}
