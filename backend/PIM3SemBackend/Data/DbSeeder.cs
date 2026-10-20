using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.Migrate();

            var perfisParaAdicionar = AdicionarPerfis(context);
            var departamentosParaAdicionar = AdicionarDepartamentos(context);

            if (perfisParaAdicionar.Any())
                context.Perfis.AddRange(perfisParaAdicionar);

            if (departamentosParaAdicionar.Any())
                context.Departamentos.AddRange(departamentosParaAdicionar);

            context.SaveChanges();
        }

        private static List<Perfil> AdicionarPerfis(AppDbContext context)
        {
            var perfisExistentes = context.Perfis.Select(p => p.Nome).ToHashSet();

            var perfisParaAdicionar = new[] { "CEO", "Gerente", "Funcionario" }
                .Where(nome => !perfisExistentes.Contains(nome))
                .Select(nome => new Perfil(nome))
                .ToList();
            return perfisParaAdicionar;
        }

        private static List<Departamento> AdicionarDepartamentos(AppDbContext context)
        {
            var departamentosExistentes = context.Departamentos.Select(d => d.Nome).ToHashSet();
            var departamentosParaAdicionar = new[] { "T.I", "RH", "Marketing", "Financeiro" }
                .Where(nome => !departamentosExistentes.Contains(nome))
                .Select(nome => new Departamento(nome))
                .ToList();
            return departamentosParaAdicionar;
        }
    }
}
