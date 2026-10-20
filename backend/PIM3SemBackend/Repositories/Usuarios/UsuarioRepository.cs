using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Usuarios
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CriarUsuario(Usuario novoUsuario)
        {
            await _context.Usuarios.AddAsync(novoUsuario);
            await _context.SaveChangesAsync();
        }

        public Task<Usuario?> ObterPorId(Guid usuarioId)
        {
            return _context.Usuarios
                    .Include(u => u.Perfil)
                .FirstOrDefaultAsync(usuario => usuario.Id.Equals(usuarioId));
        }

        public Task<List<Usuario>> ObterTodos()
        {
            return _context.Usuarios
                    .Include(u  => u.Perfil)
                .ToListAsync();
        }

        public async Task AtualizarUsuario(Usuario usuarioAtualizado)
        {
            _context.Usuarios.Update(usuarioAtualizado);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverUsuario(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> BuscarLogin(string email, string senha)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Equals(email));
        }
    }
}
