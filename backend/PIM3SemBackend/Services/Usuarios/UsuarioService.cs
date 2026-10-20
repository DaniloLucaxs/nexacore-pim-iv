using Microsoft.AspNetCore.Identity;
using PIM_3sem_backend.DTOs.Login;
using PIM_3sem_backend.DTOs.Usuarios;
using PIM_3sem_backend.Exceptions.NotFound;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Repositories.Usuarios;
using PIM_3sem_backend.Services.Perfis;

namespace PIM_3sem_backend.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPerfilService _perfilService;
        public UsuarioService(IUsuarioRepository repository, IPerfilService perfilService)
        {
            _repository = repository;
            _perfilService = perfilService;
        }

        private UsuarioResponseDTO CriarUsuarioResponse(Usuario usuario)
        {
            return new UsuarioResponseDTO()
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Perfil = usuario.Perfil.Nome,
                Status = usuario.Ativo ? "Ativo" : "Desativado"
            };
        }

        private Usuario CriarUsuarioModel(UsuarioCreateDTO novoUsuario)
        {
            var usuario = new Usuario(novoUsuario.Email.Trim().ToLowerInvariant(), "hash-pendente", novoUsuario.IdPerfil);
            if(novoUsuario.Senha.Length<10) throw new PIM_3sem_backend.Exceptions.BadRequest.BadRequestException("Senha deve ter pelo menos 10 caracteres.");
            usuario.DefinirSenhaHash(new PasswordHasher<Usuario>().HashPassword(usuario, novoUsuario.Senha));
            return usuario;
        }
        private async Task<Usuario> BuscarUsuario(Guid usuarioId)
        {
            var usuario = await _repository.ObterPorId(usuarioId);
            if (usuario == null)
                throw new UsuarioNotFoundException();
            return usuario;
        }

        public async Task<UsuarioResponseDTO> CriarUsuario(UsuarioCreateDTO createDTO)
        {
            var novoUsuario = CriarUsuarioModel(createDTO);
            var perfil = await _perfilService.ObterPorId(createDTO.IdPerfil);

            await _repository.CriarUsuario(novoUsuario);
            return CriarUsuarioResponse(novoUsuario);
        }

        public async Task<UsuarioResponseDTO> ObterPorId(Guid usuarioId)
        {
            var usuario = await BuscarUsuario(usuarioId);
            return CriarUsuarioResponse(usuario);
        }

        public async Task<IReadOnlyCollection<UsuarioResponseDTO>> ObterTodos()
        {
            var usuarios = await _repository.ObterTodos();
            var response = new List<UsuarioResponseDTO>();

            foreach (var usuario in usuarios)
                response.Add(CriarUsuarioResponse(usuario));

            return response;
        }

        public async Task AlterarAcesso(Guid usuarioId)
        {
            var usuario = await BuscarUsuario(usuarioId);
            usuario.AlterarAcesso();

            await _repository.AtualizarUsuario(usuario);
        }

        public async Task RemoverUsuario(Guid usuarioId)
        {
            var usuario = await BuscarUsuario(usuarioId);
            usuario.DefinirAcesso(false);
            await _repository.AtualizarUsuario(usuario);
        }

        public async Task<bool> FazerLogin(FazerLoginDTO login)
        {
            var usuario = await _repository.BuscarLogin(login.Email.Trim().ToLowerInvariant(), "");
            if(usuario is not { Ativo: true }) return false;
            try { return new PasswordHasher<Usuario>().VerifyHashedPassword(usuario, usuario.Senha, login.Senha) != PasswordVerificationResult.Failed; }
            catch(FormatException) { return false; }
        }
    }
}
