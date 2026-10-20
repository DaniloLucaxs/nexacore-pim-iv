using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Models;
namespace PIM_3sem_backend.Data;
public static class DemoSeeder
{
    public static async Task Seed(AppDbContext db,IPasswordHasher<Usuario> hasher,IConfiguration config)
    {
        var password=config["Demo:Password"];
        if(string.IsNullOrEmpty(password)||password.Length<10)throw new InvalidOperationException("Demo:Password precisa ter ao menos 10 caracteres. Seed é somente para demonstração local.");
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if(await db.Usuarios.AnyAsync(x=>x.Email=="admin@nexacore.example")){await ExemploAprovado(db);await tx.CommitAsync();return;}
        foreach(var nome in new[]{"Administrador","RH","Gestor","Colaborador"})
            if(!await db.Perfis.AnyAsync(p=>p.Nome==nome)) db.Perfis.Add(new Perfil(nome));
        foreach(var nome in new[]{"T.I","RH","Financeiro","Comercial","Operações"})
            if(!await db.Departamentos.AnyAsync(d=>d.Nome==nome)) db.Departamentos.Add(new Departamento(nome));
        await db.SaveChangesAsync();
        var perfis=await db.Perfis.ToDictionaryAsync(p=>p.Nome,p=>p.Id);
        Usuario User(string email,string perfil){var u=new Usuario(email,"hash-pendente",perfis[perfil]);u.DefinirSenhaHash(hasher.HashPassword(u,password));db.Usuarios.Add(u);return u;}
        User("admin@nexacore.example","Administrador");var rh=User("rh@nexacore.example","RH");
        var deps=await db.Departamentos.Where(d=>new[]{"T.I","RH","Financeiro","Comercial","Operações"}.Contains(d.Nome)).OrderBy(d=>d.Nome).ToListAsync();
        var nomes=new[]{"Marina Duarte","Beatriz Tavares","Caio Monteiro","Helena Freitas","Otávio Brandão"};
        var gestores=new List<Funcionario>();var funcionarios=new List<Funcionario>();
        for(int d=0;d<deps.Count;d++){
            var u=User(d==0?"gestor@nexacore.example":$"gestor{d+1}@nexacore.example","Gestor");
            var g=new Funcionario(nomes[d],9000+d*800,"Gestor de equipe",deps[d].Id,null,u.Id);gestores.Add(g);funcionarios.Add(g);db.Funcionarios.Add(g);
            for(int i=1;i<=9;i++){
                var usuario=User($"colaborador{d*9+i:00}@nexacore.example","Colaborador");
                var f=new Funcionario($"Colaborador Exemplo {d*9+i:00}",3000+d*400+i*150,"Analista",deps[d].Id,g.Id,usuario.Id);funcionarios.Add(f);db.Funcionarios.Add(f);
            }
        }
        foreach(var f in funcionarios){f.Editar(f.Nome,f.Cargo,f.IdDepartamento,true,new DateOnly(2024,1,15));db.Salarios.Add(new Salario(f.Id,f.Salario));}
        await db.SaveChangesAsync();
        foreach(var f in funcionarios.Where(f=>f.IdGerente==gestores[0].Id).Take(2))
            db.Solicitacoes.Add(new Solicitacao{IdFuncionario=f.Id,IdSolicitante=gestores[0].IdUsuario,SalarioAnterior=f.Salario,NovoSalario=decimal.Round(f.Salario*1.05m,2),Motivo="Revisão de mérito demonstrativa"});
        await db.SaveChangesAsync();await ExemploAprovado(db);await tx.CommitAsync();
    }
    private static async Task ExemploAprovado(AppDbContext db)
    {
        const string motivo="Exemplo aprovado de reajuste anual";
        if(await db.Solicitacoes.AnyAsync(s=>s.Motivo==motivo)) return;
        var f=await db.Funcionarios.Include(x=>x.Usuario).FirstOrDefaultAsync(x=>x.Usuario.Email=="colaborador03@nexacore.example");
        var rh=await db.Usuarios.FirstOrDefaultAsync(x=>x.Email=="rh@nexacore.example");
        var gestor=await db.Usuarios.FirstOrDefaultAsync(x=>x.Email=="gestor@nexacore.example");
        if(f==null||rh==null||gestor==null || await db.Solicitacoes.AnyAsync(s=>s.IdFuncionario==f.Id && s.Status=="Pendente")) return;
        var solicitacao=new Solicitacao{IdFuncionario=f.Id,IdSolicitante=gestor.Id,SalarioAnterior=f.Salario,NovoSalario=decimal.Round(f.Salario*1.03m,2),Motivo=motivo};
        db.Solicitacoes.Add(solicitacao);await db.SaveChangesAsync();
        // Seed usa a mesma procedure validada do produto, dentro da transação do seed.
        await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_set_session_context @key=N'UsuarioId', @value={rh.Id}");
        try {await db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.usp_AprovarReajuste @Id={solicitacao.Id}");}
        finally {await db.Database.ExecuteSqlRawAsync("EXEC sys.sp_set_session_context @key=N'UsuarioId', @value=NULL");}
    }

}
