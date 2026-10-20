using Moq;
using PIM_3sem_backend.Exceptions.NotFound;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Repositories.Departamentos;
using PIM_3sem_backend.Repositories.Perfis;
using PIM_3sem_backend.Services.Departamentos;
using PIM_3sem_backend.Services.Perfis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIM3SemTestes.Perfis
{
    public class PerfisServiceTest
    {
        private Guid IdTeste = Guid.NewGuid();
        private Perfil CriarPerfilModel()
        {
            return new Perfil("Perfil");
        }

        [Fact]
        public async Task DeveBuscarPerfilExistente()
        {
            var perfil = CriarPerfilModel();

            var mock = new Mock<IPerfilRepository>();
            mock.Setup(x => x.ObterPorId(IdTeste))
                .ReturnsAsync(perfil);

            var service = new PerfilService(mock.Object);

            var resultado = await service.ObterPorId(IdTeste);
            Assert.NotNull(resultado);
            Assert.Equal(perfil.Nome, resultado.Nome);
            Assert.Equal(perfil.Id, resultado.Id);
        }

        [Fact]
        public async Task DeveLancarErroAoProcurarPerfilInexistente()
        {
            var mock = new Mock<IPerfilRepository>();
            mock.Setup(x => x.ObterPorId(IdTeste))
                .ReturnsAsync((Perfil?)null);

            var service = new PerfilService(mock.Object);

            await Assert.ThrowsAsync<PerfilNotFoundException>(() => service.ObterPorId(IdTeste));
        }

        [Fact]
        public async Task DeveBuscarTodosDepartamentos()
        {
            var perfis = new List<Perfil>()
            {
                CriarPerfilModel(),
                CriarPerfilModel(),
                CriarPerfilModel(),
                CriarPerfilModel()
            };

            var mock = new Mock<IPerfilRepository>();
            mock.Setup(x => x.ObterTodos())
                .ReturnsAsync(perfis);

            var service = new PerfilService(mock.Object);

            var resultado = await service.ObterTodos();

            Assert.NotNull(resultado);
            Assert.Equal(perfis.Count, resultado.Count);
        }
    }
}
