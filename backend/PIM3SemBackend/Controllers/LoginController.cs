using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Data;
using PIM_3sem_backend.Models;
namespace PIM_3sem_backend.Controllers;
[ApiController,Route("api")]
public class LoginController(AppDbContext db,IPasswordHasher<Usuario> hasher,IAntiforgery antiforgery) : ControllerBase
{
    public record Credenciais(string Email,string Senha);
    [HttpGet("session"),AllowAnonymous] public IActionResult Session()
    {
        var token=antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
        return Ok(new {autenticado=User.Identity?.IsAuthenticated==true,email=User.FindFirstValue(ClaimTypes.Email),perfil=User.FindFirstValue(ClaimTypes.Role),id=User.FindFirstValue(ClaimTypes.NameIdentifier),csrfToken=token});
    }
    [HttpPost("Login"),AllowAnonymous,EnableRateLimiting("login")]
    public async Task<IActionResult> Login(Credenciais dto)
    {
        if(string.IsNullOrWhiteSpace(dto.Email)||string.IsNullOrEmpty(dto.Senha)||dto.Senha.Length>128) return Unauthorized(new {error="Credenciais inválidas."});
        var u=await db.Usuarios.Include(x=>x.Perfil).FirstOrDefaultAsync(x=>x.Email==dto.Email.Trim().ToLower());
        PasswordVerificationResult result=PasswordVerificationResult.Failed;
        if(u is {Ativo:true}) {try { result=hasher.VerifyHashedPassword(u,u.Senha,dto.Senha); } catch(FormatException) {} }
        if(u==null||result==PasswordVerificationResult.Failed || u.Perfil.Nome is not ("Administrador" or "RH" or "Gestor")) return Unauthorized(new {error="Credenciais inválidas."});
        if(result==PasswordVerificationResult.SuccessRehashNeeded){u.DefinirSenhaHash(hasher.HashPassword(u,dto.Senha));await db.SaveChangesAsync();}
        var identity=new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,u.Id.ToString()),new Claim(ClaimTypes.Email,u.Email),new Claim(ClaimTypes.Role,u.Perfil.Nome)},CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));
        return Ok(new {u.Email,perfil=u.Perfil.Nome});
    }
    [HttpPost("logout"),Authorize] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return NoContent();}
}
