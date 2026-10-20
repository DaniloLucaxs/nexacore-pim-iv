using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIM_3sem_backend.Migrations
{
    /// <inheritdoc />
    public partial class EvolucaoPimIV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Funcionarios",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataAdmissao",
                table: "Funcionarios",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Operacao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorNovo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataHora = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Responsavel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auditorias_Funcionarios_RegistroId",
                        column: x => x.RegistroId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auditorias_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFuncionario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSolicitante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdResponsavel = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalarioAnterior = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NovoSalario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Justificativa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CriadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DecididaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Funcionarios_IdFuncionario",
                        column: x => x.IdFuncionario,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Usuarios_IdResponsavel",
                        column: x => x.IdResponsavel,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Usuarios_IdSolicitante",
                        column: x => x.IdSolicitante,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salarios_IdFuncionario",
                table: "Salarios",
                column: "IdFuncionario",
                unique: true,
                filter: "[DataFim] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_Ativo_IdDepartamento",
                table: "Funcionarios",
                columns: new[] { "Ativo", "IdDepartamento" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_IdUsuario",
                table: "Auditorias",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_RegistroId_DataHora",
                table: "Auditorias",
                columns: new[] { "RegistroId", "DataHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_IdFuncionario",
                table: "Solicitacoes",
                column: "IdFuncionario",
                unique: true,
                filter: "[Status] = N'Pendente'");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_IdResponsavel",
                table: "Solicitacoes",
                column: "IdResponsavel");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_IdSolicitante",
                table: "Solicitacoes",
                column: "IdSolicitante");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_Status_CriadaEm",
                table: "Solicitacoes",
                columns: new[] { "Status", "CriadaEm" });

            migrationBuilder.AddForeignKey(
                name: "FK_Salarios_Funcionarios_IdFuncionario",
                table: "Salarios",
                column: "IdFuncionario",
                principalTable: "Funcionarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("UPDATE f SET Ativo=u.Ativo FROM dbo.Funcionarios f JOIN dbo.Usuarios u ON u.Id=f.IdUsuario;");
            migrationBuilder.Sql("INSERT dbo.Salarios (Id,IdFuncionario,Valor,DataInicio,DataFim,MotivoAlteracao) SELECT NEWID(),f.Id,f.Salario,CONVERT(date,SYSUTCDATETIME()),NULL,N'Saldo inicial PIM IV; histórico anterior indisponível' FROM dbo.Funcionarios f WHERE NOT EXISTS (SELECT 1 FROM dbo.Salarios s WHERE s.IdFuncionario=f.Id AND s.DataFim IS NULL);");
            migrationBuilder.Sql(Sql("vw_IndicadoresFuncionarios.sql"));
            migrationBuilder.Sql(Sql("tr_Funcionario_Auditoria.sql"));
            migrationBuilder.Sql(Sql("usp_AprovarReajuste.sql"));
        }

        private static string Sql(string file)
        {
            var assembly=typeof(EvolucaoPimIV).Assembly;
            var name=assembly.GetManifestResourceNames().Single(n=>n.EndsWith(file,StringComparison.Ordinal));
            using var stream=assembly.GetManifestResourceStream(name)!;
            using var reader=new System.IO.StreamReader(stream);return reader.ReadToEnd();
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_AprovarReajuste; DROP TRIGGER IF EXISTS dbo.tr_Funcionario_Auditoria; DROP VIEW IF EXISTS dbo.vw_IndicadoresFuncionarios;");
            migrationBuilder.DropForeignKey(
                name: "FK_Salarios_Funcionarios_IdFuncionario",
                table: "Salarios");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Solicitacoes");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Salarios_IdFuncionario",
                table: "Salarios");

            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_Ativo_IdDepartamento",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DataAdmissao",
                table: "Funcionarios");
        }
    }
}
