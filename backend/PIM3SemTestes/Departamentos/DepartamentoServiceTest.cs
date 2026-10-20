using Moq;
using PIM_3sem_backend.Exceptions.NotFound;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Repositories.Departamentos;
using PIM_3sem_backend.Services.Departamentos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIM3SemTestes.Departamentos
{
    public class DepartamentoServiceTest
    {
        private Guid IdTeste = Guid.NewGuid();
        private Departamento CriarDepartamentoModel()
        {
            return new Departamento("Departamento");
        }

        [Fact]
        public async Task DeveBuscarDepartamentoExistente()
        {
            var departamento = CriarDepartamentoModel();

            var mock = new Mock<IDepartamentoRepository>();
            mock.Setup(x => x.ObterPorId(IdTeste))
                .ReturnsAsync(departamento);

            var service = new DepartamentoService(mock.Object);

            var resultado = await service.ObterPorId(IdTeste);

            Assert.NotNull(resultado);
            Assert.Equal(departamento.Nome, resultado.Nome);
            Assert.Equal(departamento.Id, resultado.Id);
        }

        [Fact]
        public async Task DeveBuscarDepartamentoInexistente()
        {
            var mock = new Mock<IDepartamentoRepository>();
            mock.Setup(x => x.ObterPorId(IdTeste))
                .ReturnsAsync((Departamento?)null);

            var service = new DepartamentoService(mock.Object);

            await Assert.ThrowsAsync<DepartamentoNotFoundException>(() => service.ObterPorId(IdTeste));
        }

        [Fact]
        public async Task DeveBuscarTodosDepartamentos()
        {
            var departamentos = new List<Departamento>()
            {
                CriarDepartamentoModel(),
                CriarDepartamentoModel(),
                CriarDepartamentoModel(),
                CriarDepartamentoModel()
            };

            var mock = new Mock<IDepartamentoRepository>();
            mock.Setup(x => x.ObterTodos())
                .ReturnsAsync(departamentos);

            var service = new DepartamentoService(mock.Object);

            var resultado = await service.ObterTodos();

            Assert.NotNull(resultado);
            Assert.Equal(departamentos.Count, resultado.Count);
        }
    }
}

