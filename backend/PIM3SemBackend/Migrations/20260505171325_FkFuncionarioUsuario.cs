using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIM_3sem_backend.Migrations
{
    /// <inheritdoc />
    public partial class FkFuncionarioUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdUsuario",
                table: "Funcionarios",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_IdUsuario",
                table: "Funcionarios",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionarios_Usuarios_IdUsuario",
                table: "Funcionarios",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funcionarios_Usuarios_IdUsuario",
                table: "Funcionarios");

            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_IdUsuario",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Funcionarios");
        }
    }
}
