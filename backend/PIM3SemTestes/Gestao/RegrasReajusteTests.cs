using Microsoft.AspNetCore.Identity;
using PIM_3sem_backend.Models;
using PIM_3sem_backend.Exceptions.BadRequest;
using PIM_3sem_backend.Services.Gestao;
namespace PIM3SemTestes.Gestao;
public class RegrasReajusteTests
{
    [Theory]
    [InlineData(1000d,5d,null,1050d)]
    [InlineData(1000d,null,150d,1150d)]
    [InlineData(1000d,30d,null,1300d)]
    [InlineData(1000.05d,10d,null,1100.06d)]
    public void CalculaEArredonda(decimal atual,double? percentual,double? valor,decimal esperado)
        => Assert.Equal(esperado,RegrasReajuste.Calcular(atual,(decimal?)percentual,(decimal?)valor));
    [Theory]
    [InlineData(null,null)] [InlineData(5d,100d)] [InlineData(-1d,null)]
    [InlineData(31d,null)] [InlineData(0d,null)] [InlineData(null,301d)] [InlineData(null,-1d)]
    public void RejeitaAumentoInvalido(double? percentual,double? valor)
        => Assert.Throws<BadRequestException>(()=>RegrasReajuste.Calcular(1000,(decimal?)percentual,(decimal?)valor));
    [Fact] public void RejeitaAumentoSemEfeito() => Assert.Throws<BadRequestException>(()=>RegrasReajuste.Calcular(1,0.1m,null));
    [Fact] public void SenhaNaoEGravadaEmTextoPuro()
    {
        var u=new Usuario("teste@example.test","provisorio",Guid.NewGuid());var h=new PasswordHasher<Usuario>();
        var a=h.HashPassword(u,"SenhaFicticia!123");var b=h.HashPassword(u,"SenhaFicticia!123");
        Assert.NotEqual(a,b);Assert.DoesNotContain("SenhaFicticia",a);
        Assert.Equal(PasswordVerificationResult.Success,h.VerifyHashedPassword(u,a,"SenhaFicticia!123"));
        Assert.Equal(PasswordVerificationResult.Failed,h.VerifyHashedPassword(u,a,"incorreta"));
    }
}
