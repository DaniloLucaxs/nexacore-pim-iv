using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;
namespace PIM3SemTestes.Gestao;
public class SqlFactAttribute : FactAttribute
{
    public SqlFactAttribute() {if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NEXACORE_TEST_URL"))) Skip="Execute python scripts/test.py para validar API com SQL Server real isolado.";}
}
[CollectionDefinition("SQL",DisableParallelization=true)] public class SqlCollection;
[Collection("SQL")]
public class ApiSqlTests
{
    private static string Url=>Environment.GetEnvironmentVariable("NEXACORE_TEST_URL")!;
    private static string Connection=>Environment.GetEnvironmentVariable("NEXACORE_TEST_CONNECTION")!;
    private static HttpClient Novo()=>new(new HttpClientHandler{CookieContainer=new CookieContainer()}){BaseAddress=new Uri(Url),Timeout=TimeSpan.FromSeconds(30)};
    private static async Task<JsonNode> Json(HttpResponseMessage response)
    {response.EnsureSuccessStatusCode();return JsonNode.Parse(await response.Content.ReadAsStringAsync())!;}
    private static async Task Token(HttpClient c){var s=await Json(await c.GetAsync("/api/session"));c.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");c.DefaultRequestHeaders.Add("X-CSRF-TOKEN",(string)s["csrfToken"]!);}
    private static async Task<HttpClient> Entrar(string email)
    {
        var c=Novo();await Token(c);
        (await c.PostAsJsonAsync("/api/Login",new{email,senha=Environment.GetEnvironmentVariable("Demo__Password")})).EnsureSuccessStatusCode();
        await Token(c);return c;
    }
    private static async Task<(string Id,decimal Salario)> Cadastrar(HttpClient rh,HttpClient gestor)
    {
        var deps=await Json(await rh.GetAsync("/api/Departamento"));var perfis=await Json(await rh.GetAsync("/api/Perfil"));
        var gestores=await Json(await rh.GetAsync("/api/Funcionario/Gerentes"));
        var g=gestores.AsArray().First(x=>(string?)x!["email"]=="gestor@nexacore.example")!;
        var col=perfis.AsArray().First(p=>(string?)p!["nome"]=="Colaborador")!;
        var response=await rh.PostAsJsonAsync("/api/Funcionario",new{nome="Teste Integrado "+Guid.NewGuid().ToString("N")[..8],cargo="Analista",salario=4000,idDepartamento=(string)g["idDepartamento"]!,idGerente=(string)g["id"]!,idPerfil=(string)col["id"]!,email=Guid.NewGuid()+"@example.test",senha="FicticiaTeste!2026"});
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);var id=(string)(await Json(response))["id"]!;
        return(id,4000);
    }
    private static async Task<string> Solicitar(HttpClient c,string id)
    {
        var r=await c.PostAsJsonAsync("/api/solicitacoes",new{idFuncionario=id,percentual=10,motivo="Teste integrado de mérito"});
        Assert.Equal(HttpStatusCode.Created,r.StatusCode);return (string)(await Json(r))["id"]!;
    }
    [SqlFact] public async Task FluxoCompletoAutorizacaoHistoricoAuditoriaDashboardEConcorrencia()
    {
        using var anon=Novo();Assert.Equal(HttpStatusCode.Unauthorized,(await anon.GetAsync("/api/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await anon.PostAsJsonAsync("/api/Login",new{email="x",senha="x"})).StatusCode);
        using var rh=await Entrar("rh@nexacore.example");using var gestor=await Entrar("gestor@nexacore.example");using var outro=await Entrar("gestor2@nexacore.example");using var admin=await Entrar("admin@nexacore.example");
        var f=await Cadastrar(rh,gestor);
        Assert.Equal(HttpStatusCode.NotFound,(await outro.GetAsync("/api/Funcionario/"+f.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await gestor.DeleteAsync("/api/Funcionario/"+f.Id)).StatusCode);
        var antes=await Json(await rh.GetAsync("/api/dashboard"));var folha=(decimal)antes["folha"]!;
        var preview=await Json(await gestor.PostAsJsonAsync("/api/solicitacoes/simular",new{idFuncionario=f.Id,percentual=10,motivo="Mérito"}));Assert.Equal(4400,(decimal)preview["novoSalario"]!);
        var id=await Solicitar(gestor,f.Id);
        Assert.Equal(HttpStatusCode.BadRequest,(await gestor.PostAsJsonAsync("/api/solicitacoes",new{idFuncionario=f.Id,percentual=5,motivo="Duplicada"})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await gestor.PostAsync($"/api/solicitacoes/{id}/aprovar",null)).StatusCode);
        var approvals=await Task.WhenAll(rh.PostAsync($"/api/solicitacoes/{id}/aprovar",null),admin.PostAsync($"/api/solicitacoes/{id}/aprovar",null));
        Assert.Single(approvals.Where(r=>r.StatusCode==HttpStatusCode.NoContent));Assert.Single(approvals.Where(r=>r.StatusCode==HttpStatusCode.Conflict));
        var detalhe=await Json(await rh.GetAsync("/api/Funcionario/"+f.Id));Assert.Equal(4400,(decimal)detalhe["funcionario"]!["salario"]!);Assert.Equal(2,detalhe["historico"]!.AsArray().Count);
        var salarios=detalhe["movimentacoes"]!.AsArray().Where(a=>(string?)a!["operacao"]=="Salário").ToList();Assert.Single(salarios);Assert.Contains((string)salarios[0]!["responsavel"]!,new[]{"rh@nexacore.example","admin@nexacore.example"});
        var depois=await Json(await rh.GetAsync("/api/dashboard"));Assert.Equal(folha+400,(decimal)depois["folha"]!);
        var rejeitar=await Solicitar(gestor,f.Id);Assert.Equal(HttpStatusCode.NoContent,(await rh.PostAsJsonAsync($"/api/solicitacoes/{rejeitar}/rejeitar",new{justificativa="Orçamento indisponível"})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsync($"/api/solicitacoes/{rejeitar}/aprovar",null)).StatusCode);
        var propria=await Solicitar(rh,f.Id);Assert.Equal(HttpStatusCode.Conflict,(await rh.PostAsync($"/api/solicitacoes/{propria}/aprovar",null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await admin.PostAsJsonAsync($"/api/solicitacoes/{propria}/rejeitar",new{justificativa="Encerramento demonstrativo"})).StatusCode);
        foreach(var c in new[]{rh,admin}){
            var detail=await Json(await c.GetAsync("/api/Funcionario/"+f.Id));var v=detail["funcionario"]!;
            var cargo=c==rh?"Analista RH":"Analista Admin";
            (await c.PatchAsJsonAsync("/api/Funcionario/"+f.Id,new{nome=(string)v["nome"]!,cargo,idDepartamento=(string)v["idDepartamento"]!,ativo=true,dataAdmissao=(string)v["dataAdmissao"]!})).EnsureSuccessStatusCode();
        }
        var audit=await Json(await rh.GetAsync("/api/auditoria?funcionario="+f.Id));
        Assert.Contains(audit["items"]!.AsArray(),a=>(string?)a!["valorNovo"]=="Analista RH"&&(string?)a["responsavel"]=="rh@nexacore.example");
        Assert.Contains(audit["items"]!.AsArray(),a=>(string?)a!["valorNovo"]=="Analista Admin"&&(string?)a["responsavel"]=="admin@nexacore.example");
        Assert.Equal(HttpStatusCode.NoContent,(await rh.DeleteAsync("/api/Funcionario/"+f.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await gestor.PostAsJsonAsync("/api/solicitacoes",new{idFuncionario=f.Id,percentual=10,motivo="Inativo"})).StatusCode);
        detalhe=await Json(await rh.GetAsync("/api/Funcionario/"+f.Id));Assert.False((bool)detalhe["funcionario"]!["ativo"]!);Assert.Equal(2,detalhe["historico"]!.AsArray().Count);
    }
    private static async Task<int> Exec(SqlConnection c,string sql)
    {using var cmd=new SqlCommand(sql,c);return await cmd.ExecuteNonQueryAsync();}
    [SqlFact] public async Task TriggerMultiplasLinhasEProcedureReverteAlteracoesQuandoAuditoriaFalha()
    {
        var cs=new SqlConnectionStringBuilder(Connection);Assert.StartsWith("NexaCoreTests_",cs.InitialCatalog);
        using var rh=await Entrar("rh@nexacore.example");using var gestor=await Entrar("gestor@nexacore.example");
        var f=await Cadastrar(rh,gestor);var id=await Solicitar(gestor,f.Id);
        await using var c=new SqlConnection(Connection);await c.OpenAsync();
        // Gatilho adicional exclusivo deste banco descartável: falha depois da escrita do histórico.
        await Exec(c,"CREATE TRIGGER dbo.tr_TestFalha ON dbo.Funcionarios AFTER UPDATE AS BEGIN IF UPDATE(Salario) THROW 50100, N'Falha de teste de atomicidade', 1; END");
        try { Assert.Equal(HttpStatusCode.Conflict,(await rh.PostAsync($"/api/solicitacoes/{id}/aprovar",null)).StatusCode); }
        finally {await Exec(c,"DROP TRIGGER dbo.tr_TestFalha");}
        var detalhe=await Json(await rh.GetAsync("/api/Funcionario/"+f.Id));Assert.Equal(4000,(decimal)detalhe["funcionario"]!["salario"]!);Assert.Single(detalhe["historico"]!.AsArray());
        var requests=await Json(await rh.GetAsync("/api/solicitacoes?funcionario="+f.Id));Assert.Equal("Pendente",(string)requests["items"]![0]!["status"]!);
        using var command=new SqlCommand("BEGIN TRAN; DECLARE @n bigint=(SELECT COUNT_BIG(*) FROM dbo.Auditorias); UPDATE TOP (2) dbo.Funcionarios SET Cargo=N'Teste multirregistro'; SELECT COUNT_BIG(*)-@n FROM dbo.Auditorias; ROLLBACK;",c);
        Assert.Equal(2L,(long)(await command.ExecuteScalarAsync())!);
        // A mesma pendência continua processável após rollback da tentativa anterior.
        Assert.Equal(HttpStatusCode.NoContent,(await rh.PostAsync($"/api/solicitacoes/{id}/aprovar",null)).StatusCode);
    }
}
