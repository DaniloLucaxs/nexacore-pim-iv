using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Security;
namespace PIM_3sem_backend.Repositories.Gestao;
public class GestaoRepository(AppDbContext db, AtorAtual ator)
{
    public AppDbContext Db => db;
    public IQueryable<Funcionario> Funcionarios => db.Funcionarios
        .Include(f => f.Departamento).Include(f => f.Gerente).Include(f => f.Usuario).ThenInclude(u => u.Perfil)
        .Where(f => !ator.Gestor || db.Funcionarios.Any(g => g.IdUsuario == ator.Id && f.IdGerente == g.Id));
    public IQueryable<Solicitacao> Solicitacoes => db.Solicitacoes.Include(s => s.Funcionario).Include(s => s.Solicitante).Include(s => s.Responsavel)
        .Where(s => !ator.Gestor || Funcionarios.Any(f => f.Id == s.IdFuncionario));
    public Task Aplicar(Guid id) => db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.usp_AprovarReajuste @Id={id}");
}
