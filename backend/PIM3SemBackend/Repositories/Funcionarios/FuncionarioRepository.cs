using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Repositories.Funcionarios
{
    public class FuncionarioRepository : IFuncionarioRepository
    {
        private readonly AppDbContext _context;
        public FuncionarioRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CriarFuncionario(Funcionario novoFuncionario)
        {
            await _context.Funcionarios.AddAsync(novoFuncionario);
            await _context.SaveChangesAsync();
        }

        public async Task<Funcionario?> ObterPorId(Guid funcionarioId)
        {
            return await _context.Funcionarios
                .Include(f => f.Departamento)
                .Include(f => f.Gerente)
                .Include(f => f.Usuario)
                    .ThenInclude(u => u.Perfil)
                .FirstOrDefaultAsync(f => f.Id.Equals(funcionarioId));
        }

        public async Task<List<Funcionario>> ObterTodos()
        {
            return await _context.Funcionarios
                .Include(f => f.Departamento)
                .Include(f => f.Gerente)
                .Include(f => f.Usuario)
                    .ThenInclude(u => u.Perfil)
                .ToListAsync();
        }

        public async Task<List<Funcionario>> ObterTodosGerentes()
        {
            return await _context.Funcionarios
                .Include(f => f.Departamento)
                .Include(f => f.Gerente)
                .Include(f => f.Usuario)
                    .ThenInclude(u => u.Perfil)
                .Where(f => f.Usuario.Perfil.Nome== "Gerente")
                .ToListAsync();
        }

        public async Task<List<Funcionario>> ObterFuncionarioDoGerente(Guid gerenteId)
        {
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Departamento)
                .Include(f => f.Gerente)
                .Include(f => f.Usuario)
                    .ThenInclude(u => u.Perfil)
                .Where(f => f.IdGerente.Equals(gerenteId))
                .AsNoTracking()
                .ToListAsync();

            return funcionarios;
        }

        public async Task AtualizarFuncionario(Funcionario funcionarioAtualizado)
        {
            _context.Funcionarios.Update(funcionarioAtualizado);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverFuncionario(Funcionario funcionario)
        {
            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
        }
    }
}
