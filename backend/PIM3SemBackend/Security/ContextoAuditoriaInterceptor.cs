using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace PIM_3sem_backend.Security;
// Toda abertura lógica, inclusive de conexão do pool, sobrescreve o contexto.
public class ContextoAuditoriaInterceptor(AtorAtual ator) : DbConnectionInterceptor
{
    private DbCommand Comando(DbConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "EXEC sys.sp_set_session_context @key=N'UsuarioId', @value=@ator";
        var p = cmd.CreateParameter(); p.ParameterName = "@ator"; p.Value = (object?)ator.Id ?? DBNull.Value; cmd.Parameters.Add(p);
        return cmd;
    }
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    { using var cmd = Comando(connection); cmd.ExecuteNonQuery(); }
    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    { await using var cmd = Comando(connection); await cmd.ExecuteNonQueryAsync(cancellationToken); }
}
