using PIM_3sem_backend.DTOs.Login;
using PIM_3sem_backend.DTOs.Usuarios;

namespace PIM_3sem_backend.Services.Usuarios
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDTO> CriarUsuario(UsuarioCreateDTO novoUsuario);
        Task<UsuarioResponseDTO> ObterPorId(Guid usuarioId);
        Task<IReadOnlyCollection<UsuarioResponseDTO>> ObterTodos();
        Task AlterarAcesso(Guid usuarioId);
        Task RemoverUsuario(Guid usuarioId);

        Task<bool> FazerLogin(FazerLoginDTO login);
    }
}
