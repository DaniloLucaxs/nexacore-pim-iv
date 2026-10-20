using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Security;
using PIM_3sem_backend.Middlewares;
using PIM_3sem_backend.Repositories.Departamentos;
using PIM_3sem_backend.Repositories.Funcionarios;
using PIM_3sem_backend.Repositories.Perfis;
using PIM_3sem_backend.Repositories.Usuarios;
using PIM_3sem_backend.Repositories.Gestao;
using PIM_3sem_backend.Services.Departamentos;
using PIM_3sem_backend.Services.Funcionarios;
using PIM_3sem_backend.Services.Perfis;
using PIM_3sem_backend.Services.Usuarios;
using PIM_3sem_backend.Services.Gestao;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath,"../../.local/keys"))));
builder.Services.AddControllers();builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<AtorAtual>();builder.Services.AddScoped<ContextoAuditoriaInterceptor>();
builder.Services.AddScoped<IPasswordHasher<Usuario>,PasswordHasher<Usuario>>();
builder.Services.AddScoped<IPerfilRepository,PerfilRepository>();builder.Services.AddScoped<IPerfilService,PerfilService>();
builder.Services.AddScoped<IDepartamentoRepository,DepartamentoRepository>();builder.Services.AddScoped<IDepartamentoService,DepartamentoService>();
builder.Services.AddScoped<IUsuarioRepository,UsuarioRepository>();builder.Services.AddScoped<IUsuarioService,UsuarioService>();
builder.Services.AddScoped<IFuncionarioRepository,FuncionarioRepository>();builder.Services.AddScoped<IFuncionarioService,FuncionarioService>();
builder.Services.AddScoped<GestaoRepository>();builder.Services.AddScoped<GestaoService>();
builder.Services.AddDbContext<AppDbContext>((sp,options)=>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=NexaCore;Integrated Security=true;Encrypt=true;TrustServerCertificate=true").AddInterceptors(sp.GetRequiredService<ContextoAuditoriaInterceptor>()));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{
    o.Cookie.Name="NexaCore.Session";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Strict;o.Cookie.SecurePolicy=CookieSecurePolicy.SameAsRequest;
    o.ExpireTimeSpan=TimeSpan.FromHours(4);o.SlidingExpiration=false;
    o.Events.OnRedirectToLogin=c=>{c.Response.StatusCode=401;return Task.CompletedTask;};
    o.Events.OnRedirectToAccessDenied=c=>{c.Response.StatusCode=403;return Task.CompletedTask;};
    o.Events.OnValidatePrincipal=async c=>{
        var db=c.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        if(!Guid.TryParse(c.Principal?.FindFirstValue(ClaimTypes.NameIdentifier),out var id)||!await db.Usuarios.AnyAsync(u=>u.Id==id && u.Ativo && u.Perfil.Nome==c.Principal!.FindFirstValue(ClaimTypes.Role))) c.RejectPrincipal();
    };
});
builder.Services.AddAuthorization(o=>o.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddAntiforgery(o=>{o.HeaderName="X-CSRF-TOKEN";o.Cookie.SameSite=SameSiteMode.Strict;});
builder.Services.AddRateLimiter(o=>o.AddPolicy("login",c=>RateLimitPartition.GetFixedWindowLimiter(c.Connection.RemoteIpAddress?.ToString()??"local",_=>new(){PermitLimit=10,Window=TimeSpan.FromMinutes(1),QueueLimit=0})));
var app=builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
if(app.Environment.IsDevelopment()){app.UseSwagger();app.UseSwaggerUI();}
var front=Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath,"../../frontend"));
if(Directory.Exists(front)){var provider=new PhysicalFileProvider(front);app.UseDefaultFiles(new DefaultFilesOptions{FileProvider=provider});app.UseStaticFiles(new StaticFileOptions{FileProvider=provider});}
app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.UseRateLimiter();
app.Use(async(c,next)=>{
    c.Response.Headers["X-Content-Type-Options"]="nosniff";
    c.Response.Headers["X-Frame-Options"]="DENY";
    if(c.Request.Path.StartsWithSegments("/api")){
        c.Response.Headers.CacheControl="no-store";
        if(c.Request.Method is not ("GET" or "HEAD" or "OPTIONS")){
            try{await c.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(c);}
            catch(AntiforgeryValidationException){c.Response.StatusCode=400;await c.Response.WriteAsJsonAsync(new{error="Token de segurança inválido. Recarregue a página."});return;}
        }
    }
    await next(c);
});
app.MapControllers();
app.MapGet("/api/health",async(AppDbContext db)=>{await db.Database.ExecuteSqlRawAsync("SELECT 1 FROM dbo.vw_IndicadoresFuncionarios");return Results.Ok(new{status="ok",database="SQL Server"});}).AllowAnonymous();
if(!args.Contains("--no-init")){
    using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if(builder.Configuration.GetValue<bool>("Demo:Seed")) await DemoSeeder.Seed(db,scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>(),builder.Configuration);
}
app.Run();
public partial class Program;
