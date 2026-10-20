using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Exceptions;
using PIM_3sem_backend.Exceptions.BadRequest;
using PIM_3sem_backend.Exceptions.NotFound;
namespace PIM_3sem_backend.Middlewares;
public class ExceptionMiddleware(RequestDelegate next,ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try {await next(context);}
        catch(Exception ex)
        {
            var sql=ex as SqlException ?? (ex as DbUpdateException)?.InnerException as SqlException;
            var (status,msg)=ex switch {
                NotFoundException => (404,ex.Message),
                BadRequestException or ModelInvalidoException => (400,ex.Message),
                UnauthorizedAccessException => (403,"Acesso não autorizado."),
                _ when sql?.Number is 2601 or 2627 => (409,"Registro duplicado ou solicitação já pendente."),
                _ when sql?.Number >= 50000 => (409,sql.Message),
                _ => (500,"Erro interno. Consulte os logs do servidor.")
            };
            if(status==500)logger.LogError(ex,"Falha na requisição {Path}",context.Request.Path);
            context.Response.StatusCode=status;await context.Response.WriteAsJsonAsync(new {error=msg,status});
        }
    }
}
