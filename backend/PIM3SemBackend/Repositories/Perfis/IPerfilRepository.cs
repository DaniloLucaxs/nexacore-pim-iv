using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Perfis

{
    public interface IPerfilRepository
    {
        Task<Perfil?> ObterPorId(Guid perfilId);
        Task<List<Perfil>> ObterTodos();
    }
}
