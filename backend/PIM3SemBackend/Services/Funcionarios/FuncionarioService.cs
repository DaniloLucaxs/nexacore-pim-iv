using PIM_3sem_backend.DTOs.Funcionarios;
using PIM_3sem_backend.DTOs.Usuarios;
using PIM_3sem_backend.Exceptions.BadRequest;
using PIM_3sem_backend.Exceptions.NotFound;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Repositories.Funcionarios;
using PIM_3sem_backend.Services.Departamentos;
using PIM_3sem_backend.Services.Usuarios;

namespace PIM_3sem_backend.Services.Funcionarios
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly IFuncionarioRepository _repository;
        private readonly IDepartamentoService _departamentoService;
        private readonly IUsuarioService _usuarioService;
        public FuncionarioService(IFuncionarioRepository repository, IDepartamentoService departamentoService, IUsuarioService usuarioService)
        {
            _repository = repository;
            _departamentoService = departamentoService;
            _usuarioService = usuarioService;
        }

        private Funcionario CriarModelFuncionario(FuncionarioCreateDTO novoFuncionario, Guid idUsuario)
        {
            return new Funcionario(novoFuncionario.Nome, novoFuncionario.Salario, novoFuncionario.Cargo, novoFuncionario.IdDepartamento, novoFuncionario.IdGerente, idUsuario);
        }

        private FuncionarioResponseDTO CriarResponse(Funcionario funcionario)
        {
            return new FuncionarioResponseDTO()
            {
                Id = funcionario.Id,
                Nome = funcionario.Nome,
                Salario = funcionario.Salario,
                Cargo = funcionario.Cargo,
                Departamento = funcionario.Departamento?.Nome ?? "N/A",
                Gerente = funcionario.Gerente?.Nome ?? "N/A",
                Email = funcionario.Usuario.Email,
                Perfil = funcionario.Usuario.Perfil.Nome
            };
        }
        private async Task<Funcionario> BuscarFuncionario(Guid funcionarioId)
        {
            var funcionario = await _repository.ObterPorId(funcionarioId);
            if (funcionario == null)
                throw new FuncionarioNotFoundException();

            return funcionario;
        }

        public async Task<FuncionarioResponseDTO> CriarFuncionario(FuncionarioCreateDTO novoFuncionario)
        {
            var departamento = await _departamentoService.ObterPorId(novoFuncionario.IdDepartamento);

            var novoUsuario = new UsuarioCreateDTO() 
            { 
                Email = novoFuncionario.Email,
                Senha = novoFuncionario.Senha,
                IdPerfil = novoFuncionario.IdPerfil
            };

            var usuarioCriado = await _usuarioService.CriarUsuario(novoUsuario);

            if (!novoFuncionario.IdGerente.HasValue && usuarioCriado.Perfil == "Funcionario")
                throw new BadRequestException("Um Funcionário deve ter um Gerente.");

            if (usuarioCriado.Perfil == "Funcionario")
            {
                _ = await BuscarFuncionario(novoFuncionario.IdGerente!.Value);
            }

            var funcionario = CriarModelFuncionario(novoFuncionario, usuarioCriado.Id);
            await _repository.CriarFuncionario(funcionario);

            return CriarResponse(funcionario);
        }

        public async Task<FuncionarioResponseDTO> ObterPorId(Guid funcionarioId)
        {
            var funcionario = await BuscarFuncionario(funcionarioId);
            return CriarResponse(funcionario);
        }

        public async Task<IReadOnlyCollection<FuncionarioResponseDTO>> ObterFuncionarioDoGerente(Guid gerenteId)
        {
            var gerente = await BuscarFuncionario(gerenteId);
            var funcionarios = await _repository.ObterFuncionarioDoGerente(gerenteId);

            var response = new List<FuncionarioResponseDTO>();
            foreach (var funcionario in funcionarios)
                response.Add(CriarResponse(funcionario));

            return response;
        }

        public async Task<IReadOnlyCollection<FuncionarioResponseDTO>> ObterTodos()
        {
            var funcionarios = await _repository.ObterTodos();

            var response = new List<FuncionarioResponseDTO>();
            foreach (var funcionario in funcionarios)
                response.Add(CriarResponse(funcionario));

            return response;
        }

        public async Task AtualizarFuncionario(FuncionarioUpdateDTO funcionarioAtualizado, Guid funcionarioId)
        {
            var funcionario = await BuscarFuncionario(funcionarioId);

            if (funcionarioAtualizado.Salario == null && funcionarioAtualizado.Cargo == null)
                throw new BadRequestException("Nenhuma informação foi inserida para atualizar");

            if (funcionarioAtualizado.Salario != null)
                funcionario.AtualizarSalario((decimal)funcionarioAtualizado.Salario);

            if (funcionarioAtualizado.Cargo != null)
                funcionario.AtualizarCargo(funcionarioAtualizado.Cargo);

            await _repository.AtualizarFuncionario(funcionario);
        }

        public async Task RemoverFuncionario(Guid funcionarioId)
        {
            var funcionario = await BuscarFuncionario(funcionarioId);

            await _repository.RemoverFuncionario(funcionario);
        }

        public async Task<IReadOnlyCollection<FuncionarioResponseDTO>> ObterTodosGerentes()
        {
            var gerentes = await _repository.ObterTodosGerentes();
            var response = new List<FuncionarioResponseDTO>();

            foreach (var gerente in gerentes)
                response.Add(CriarResponse(gerente));

            return response;
        }
    }
}
