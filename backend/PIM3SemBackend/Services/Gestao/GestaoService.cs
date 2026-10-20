using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PIM_3sem_backend.DTOs.Funcionarios;
using PIM_3sem_backend.DTOs.Gestao;
using PIM_3sem_backend.Exceptions.BadRequest;
using PIM_3sem_backend.Exceptions.NotFound;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Repositories.Gestao;
using PIM_3sem_backend.Security;
namespace PIM_3sem_backend.Services.Gestao;
public class GestaoService(GestaoRepository repo, AtorAtual ator, IPasswordHasher<Usuario> hasher)
{
    private static object Resumo(Funcionario f) => new { f.Id, f.Nome, f.Salario, f.Cargo, f.IdDepartamento, Departamento=f.Departamento.Nome, f.IdGerente, Gerente=f.Gerente?.Nome, Email=f.Usuario.Email, Perfil=f.Usuario.Perfil.Nome, f.Ativo, f.DataAdmissao };
    public async Task<Funcionario> Buscar(Guid id) => await repo.Funcionarios.FirstOrDefaultAsync(x => x.Id==id) ?? throw new FuncionarioNotFoundException();
    public async Task<object> Listar(ConsultaFuncionarios c)
    {
        if(c.Pagina < 1 || c.Tamanho < 1 || c.Tamanho > 100) throw new BadRequestException("Paginação inválida (1 a 100 registros).");
        var q=repo.Funcionarios.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(c.Busca)) q=q.Where(f=>f.Nome.Contains(c.Busca.Trim()) || f.Cargo.Contains(c.Busca.Trim()));
        if(c.Departamento.HasValue) q=q.Where(f=>f.IdDepartamento==c.Departamento);
        if(c.Ativo.HasValue) q=q.Where(f=>f.Ativo==c.Ativo);
        var total=await q.CountAsync();
        q=c.Ordem=="salario" ? q.OrderByDescending(f=>f.Salario).ThenBy(f=>f.Id) : q.OrderBy(f=>f.Nome).ThenBy(f=>f.Id);
        var items=await q.Skip((c.Pagina-1)*c.Tamanho).Take(c.Tamanho).ToListAsync();
        return new { items=items.Select(Resumo), total, c.Pagina, c.Tamanho };
    }
    public async Task<object> Gerentes(Guid? id=null)
    {
        var q=repo.Funcionarios.AsNoTracking();
        q=id.HasValue?q.Where(f=>f.IdGerente==id):q.Where(f=>f.Usuario.Perfil.Nome=="Gestor" && f.Ativo);
        return (await q.OrderBy(f=>f.Nome).ToListAsync()).Select(Resumo);
    }
    public async Task<object> Detalhes(Guid id)
    {
        var f=await Buscar(id);
        var historico=await repo.Db.Salarios.AsNoTracking().Where(s=>s.IdFuncionario==id).OrderByDescending(s=>s.DataInicio).ThenByDescending(s=>s.Id).ToListAsync();
        var movimentacoes=await repo.Db.Auditorias.AsNoTracking().Where(a=>a.RegistroId==id).OrderByDescending(a=>a.Id).Take(50).ToListAsync();
        return new { funcionario=Resumo(f), historico, movimentacoes };
    }
    public async Task<Guid> Criar(FuncionarioCreateDTO dto)
    {
        if(!ator.PodeGerir) throw new UnauthorizedAccessException();
        if(!await repo.Db.Departamentos.AnyAsync(x=>x.Id==dto.IdDepartamento)) throw new BadRequestException("Departamento inválido.");
        var perfil=await repo.Db.Perfis.SingleOrDefaultAsync(x=>x.Id==dto.IdPerfil) ?? throw new BadRequestException("Perfil inválido.");
        if(perfil.Nome is not ("Colaborador" or "Gestor")) throw new BadRequestException("Cadastro de colaborador permite perfis Colaborador ou Gestor.");
        if(dto.IdGerente.HasValue && !await repo.Db.Funcionarios.AnyAsync(x=>x.Id==dto.IdGerente && x.Ativo && x.Usuario.Perfil.Nome=="Gestor")) throw new BadRequestException("Gerente inválido ou inativo.");
        if(perfil.Nome=="Colaborador" && !dto.IdGerente.HasValue) throw new BadRequestException("Colaborador precisa de um gestor.");
        var email=dto.Email.Trim().ToLowerInvariant();
        if(await repo.Db.Usuarios.AnyAsync(x=>x.Email==email)) throw new BadRequestException("E-mail já cadastrado.");
        var u=new Usuario(email,"hash-pendente",dto.IdPerfil);u.DefinirSenhaHash(hasher.HashPassword(u,dto.Senha));
        var f=new Funcionario(dto.Nome.Trim(),dto.Salario,dto.Cargo.Trim(),dto.IdDepartamento,dto.IdGerente,u.Id);
        await using var tx=await repo.Db.Database.BeginTransactionAsync();
        repo.Db.Usuarios.Add(u);repo.Db.Funcionarios.Add(f);repo.Db.Salarios.Add(new Salario(f.Id,f.Salario));
        await repo.Db.SaveChangesAsync();await tx.CommitAsync();return f.Id;
    }
    public async Task Editar(Guid id, EditarFuncionario dto)
    {
        var f=await Buscar(id);
        if(!await repo.Db.Departamentos.AnyAsync(x=>x.Id==dto.IdDepartamento)) throw new BadRequestException("Departamento inválido.");
        if(!dto.Ativo && await repo.Db.Funcionarios.AnyAsync(x=>x.IdGerente==id && x.Ativo)) throw new BadRequestException("Transfira os subordinados ativos antes de inativar o gestor.");
        if(dto.IdGerente.HasValue) {
            if(dto.IdGerente==id || !await repo.Db.Funcionarios.AnyAsync(x=>x.Id==dto.IdGerente && x.Ativo && x.Usuario.Perfil.Nome=="Gestor")) throw new BadRequestException("Gestor inválido.");
            f.TrocarGerente(dto.IdGerente.Value);
        }
        f.Editar(dto.Nome,dto.Cargo,dto.IdDepartamento,dto.Ativo,dto.DataAdmissao);f.Usuario.DefinirAcesso(dto.Ativo);
        await repo.Db.SaveChangesAsync();
    }
    public async Task Inativar(Guid id)
    { var f=await Buscar(id);await Editar(id,new(f.Nome,f.Cargo,f.IdDepartamento,false,f.DataAdmissao)); }
    public async Task<object> Simular(NovaSolicitacao dto)
    { var f=await Buscar(dto.IdFuncionario);if(!f.Ativo) throw new BadRequestException("Colaborador inativo.");return new { salarioAnterior=f.Salario, novoSalario=RegrasReajuste.Calcular(f.Salario,dto.Percentual,dto.Valor) }; }
    public async Task<Guid> Solicitar(NovaSolicitacao dto)
    {
        var f=await Buscar(dto.IdFuncionario);if(!f.Ativo) throw new BadRequestException("Colaborador inativo.");
        var novo=RegrasReajuste.Calcular(f.Salario,dto.Percentual,dto.Valor);
        if(await repo.Db.Solicitacoes.AnyAsync(s=>s.IdFuncionario==f.Id && s.Status=="Pendente")) throw new BadRequestException("Já existe solicitação pendente para este colaborador.");
        var s=new Solicitacao {IdFuncionario=f.Id,IdSolicitante=ator.Id!.Value,SalarioAnterior=f.Salario,NovoSalario=novo,Motivo=dto.Motivo.Trim()};
        repo.Db.Solicitacoes.Add(s);await repo.Db.SaveChangesAsync();return s.Id;
    }
    public async Task<object> Solicitações(string? status, Guid? funcionario, int pagina=1)
    {
        if(pagina<1) throw new BadRequestException("Página inválida.");
        var q=repo.Solicitacoes.AsNoTracking();if(!string.IsNullOrEmpty(status)) q=q.Where(s=>s.Status==status);
        if(funcionario.HasValue) q=q.Where(s=>s.IdFuncionario==funcionario);
        return new { total=await q.CountAsync(), pagina, items=await q.OrderByDescending(s=>s.CriadaEm).Skip((pagina-1)*20).Take(20).Select(s=>new {s.Id,s.IdFuncionario,Funcionario=s.Funcionario.Nome,s.SalarioAnterior,s.NovoSalario,s.Motivo,s.Status,s.CriadaEm,s.DecididaEm,s.Justificativa,Solicitante=s.Solicitante.Email,Responsavel=s.Responsavel==null?null:s.Responsavel.Email,s.IdSolicitante}).ToListAsync() };
    }
    public async Task Aprovar(Guid id)
    { _=await repo.Solicitacoes.FirstOrDefaultAsync(s=>s.Id==id) ?? throw new NotFoundException("Solicitação não encontrada.");await repo.Aplicar(id); }
    public async Task Rejeitar(Guid id,string motivo)
    {
        var s=await repo.Solicitacoes.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==id) ?? throw new NotFoundException("Solicitação não encontrada.");
        if(s.IdSolicitante==ator.Id) throw new BadRequestException("O solicitante não pode decidir a própria solicitação.");
        var n=await repo.Db.Solicitacoes.Where(s=>s.Id==id && s.Status=="Pendente").ExecuteUpdateAsync(set=>set.SetProperty(s=>s.Status,"Rejeitada").SetProperty(s=>s.IdResponsavel,ator.Id).SetProperty(s=>s.DecididaEm,DateTimeOffset.UtcNow).SetProperty(s=>s.Justificativa,motivo.Trim()));
        if(n!=1) throw new BadRequestException("Solicitação já processada.");
    }
    public async Task<object> Dashboard()
    {
        var q=repo.Db.IndicadoresFuncionarios.Where(f=>!ator.Gestor || repo.Db.Funcionarios.Any(g=>g.IdUsuario==ator.Id && g.Id==f.IdGerente));
        var ativos=q.Where(f=>f.Ativo);
        var departamentos=await ativos.GroupBy(f=>new {f.IdDepartamento,f.Departamento}).Select(g=>new {g.Key.Departamento,Quantidade=g.Count(),Folha=g.Sum(f=>f.Salario)}).OrderBy(x=>x.Departamento).ToListAsync();
        var estados=await repo.Solicitacoes.GroupBy(s=>s.Status).Select(g=>new {Status=g.Key,Quantidade=g.Count()}).ToListAsync();
        var evolucao=await repo.Solicitacoes.Where(s=>s.Status=="Aprovada").GroupBy(s=>new {Ano=s.DecididaEm!.Value.Year,Mes=s.DecididaEm.Value.Month}).Select(g=>new {g.Key.Ano,g.Key.Mes,Quantidade=g.Count(),Impacto=g.Sum(s=>s.NovoSalario-s.SalarioAnterior)}).OrderBy(x=>x.Ano).ThenBy(x=>x.Mes).ToListAsync();
        var movimentos=await repo.Db.Auditorias.Where(a=>repo.Funcionarios.Any(f=>f.Id==a.RegistroId)).GroupBy(a=>a.Operacao).Select(g=>new {Operacao=g.Key,Quantidade=g.Count()}).ToListAsync();
        return new {total=await q.CountAsync(),ativos=await ativos.CountAsync(),departamentos,folha=departamentos.Sum(d=>d.Folha),solicitacoes=estados,evolucao,movimentos};
    }
    public async Task<object> Auditorias(string? busca,string? operacao,Guid? funcionario,DateTimeOffset? inicio,DateTimeOffset? fim,int pagina)
    {
        if(pagina<1) throw new BadRequestException("Página inválida.");
        var q=repo.Db.Auditorias.AsNoTracking().Where(a=>repo.Funcionarios.Any(f=>f.Id==a.RegistroId));
        if(!string.IsNullOrWhiteSpace(busca)) q=q.Where(a=>a.Responsavel.Contains(busca)||repo.Db.Funcionarios.Any(f=>f.Id==a.RegistroId && f.Nome.Contains(busca)));
        if(!string.IsNullOrEmpty(operacao)) q=q.Where(a=>a.Operacao==operacao);
        if(funcionario.HasValue) q=q.Where(a=>a.RegistroId==funcionario);
        if(inicio.HasValue)q=q.Where(a=>a.DataHora>=inicio);if(fim.HasValue)q=q.Where(a=>a.DataHora<=fim);
        return new {total=await q.CountAsync(),pagina,items=await q.OrderByDescending(a=>a.Id).Skip((pagina-1)*20).Take(20).Select(a=>new {a.Id,a.Entidade,a.RegistroId,a.Operacao,a.ValorAnterior,a.ValorNovo,a.DataHora,a.Responsavel,Funcionario=repo.Db.Funcionarios.Where(f=>f.Id==a.RegistroId).Select(f=>f.Nome).First()}).ToListAsync()};
    }
}
