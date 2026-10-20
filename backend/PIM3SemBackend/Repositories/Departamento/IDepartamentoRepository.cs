using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Departamentos
{
    public interface IDepartamentoRepository
    {
        Task<Departamento?> ObterPorId(Guid departamentoId);
        Task<List<Departamento>> ObterTodos();
    }
}
