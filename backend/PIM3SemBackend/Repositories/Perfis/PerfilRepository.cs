using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Perfis
{
    public class PerfilRepository : IPerfilRepository
    {
        private readonly AppDbContext _context;
        public PerfilRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Perfil?> ObterPorId(Guid perfilId)
        {
            return _context.Perfis.FirstOrDefaultAsync(perfil => perfil.Id.Equals(perfilId));
        }

        public Task<List<Perfil>> ObterTodos()
        {
            return _context.Perfis.ToListAsync();
        }
    }
}
