using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Usuarios
{
    public interface IUsuarioRepository
    {
        Task CriarUsuario(Usuario usuario);
        Task<Usuario?> ObterPorId(Guid usuarioId);
        Task<List<Usuario>> ObterTodos();
        Task AtualizarUsuario(Usuario usuarioAtualizado);
        Task RemoverUsuario(Usuario usuario);
        Task<Usuario?> BuscarLogin(string email, string senha);
    }
}
