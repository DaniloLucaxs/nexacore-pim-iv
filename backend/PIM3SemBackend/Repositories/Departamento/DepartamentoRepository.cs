using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Departamentos
{
    public class DepartamentoRepository : IDepartamentoRepository
    {
        private readonly AppDbContext _context;
        public DepartamentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Departamento?> ObterPorId(Guid departamentoId)
        {
            return _context.Departamentos.FirstOrDefaultAsync(departamento => departamento.Id.Equals(departamentoId));
        }

        public Task<List<Departamento>> ObterTodos()
        {
            return _context.Departamentos.ToListAsync();
        }
    }
}
