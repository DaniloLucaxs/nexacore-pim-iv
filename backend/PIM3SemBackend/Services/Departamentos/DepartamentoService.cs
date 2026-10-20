using PIM_3sem_backend.DTOs.Departamentos;
using PIM_3sem_backend.Exceptions;
using PIM_3sem_backend.Exceptions.NotFound;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Repositories.Departamentos;

namespace PIM_3sem_backend.Services.Departamentos
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly IDepartamentoRepository _repository;
        public DepartamentoService(IDepartamentoRepository repository)
        {
            _repository = repository;
        }

        private DepartamentoResponseDTO CriarDepartamentoResponse(Departamento departamento)
        {
            return new DepartamentoResponseDTO() { Id = departamento.Id, Nome = departamento.Nome };
        }

        public async Task<DepartamentoResponseDTO> ObterPorId(Guid departamentoId)
        {
            var departamento = await _repository.ObterPorId(departamentoId);
            if (departamento == null)
                throw new DepartamentoNotFoundException();

            return CriarDepartamentoResponse(departamento);
        }

        public async Task<IReadOnlyCollection<DepartamentoResponseDTO>> ObterTodos()
        {
            var departamentos = await _repository.ObterTodos();
            var response = new List<DepartamentoResponseDTO>();

            foreach (var dept in departamentos)
                response.Add(CriarDepartamentoResponse(dept));

            return response;
        }
    }
}
