using Microsoft.EntityFrameworkCore;
using PIM_3sem_backend.Models;

namespace PIM_3sem_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Salario> Salarios { get; set; }
        public DbSet<PontoRegistro> PontosRegistro { get; set; }

        public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
        public DbSet<Auditoria> Auditorias => Set<Auditoria>();
        public DbSet<IndicadorFuncionario> IndicadoresFuncionarios => Set<IndicadorFuncionario>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IndicadorFuncionario>().Property(x => x.Salario).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<IndicadorFuncionario>().HasNoKey().ToView("vw_IndicadoresFuncionarios");
            modelBuilder.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<Funcionario>().ToTable("Funcionarios", t => t.UseSqlOutputClause(false));
            modelBuilder.Entity<Funcionario>().HasIndex(x => new { x.Ativo, x.IdDepartamento });
            modelBuilder.Entity<Salario>().HasOne<Funcionario>().WithMany().HasForeignKey(x => x.IdFuncionario).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Salario>().HasIndex(x => x.IdFuncionario).IsUnique().HasFilter("[DataFim] IS NULL");
            modelBuilder.Entity<Auditoria>(e => {
                e.HasKey(x => x.Id); e.Property(x => x.Entidade).HasMaxLength(50);
                e.Property(x => x.Operacao).HasMaxLength(50); e.Property(x => x.Responsavel).HasMaxLength(100);
                e.HasIndex(x => new { x.RegistroId, x.DataHora });
                e.HasOne<Funcionario>().WithMany().HasForeignKey(x => x.RegistroId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Solicitacao>(e => {
                e.HasKey(x => x.Id); e.Property(x => x.Motivo).HasMaxLength(500);
                e.Property(x => x.Justificativa).HasMaxLength(500); e.Property(x => x.Status).HasMaxLength(20);
                e.Property(x => x.SalarioAnterior).HasColumnType("decimal(10,2)");
                e.Property(x => x.NovoSalario).HasColumnType("decimal(10,2)");
                e.HasOne(x => x.Funcionario).WithMany().HasForeignKey(x => x.IdFuncionario).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Solicitante).WithMany().HasForeignKey(x => x.IdSolicitante).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Responsavel).WithMany().HasForeignKey(x => x.IdResponsavel).OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.Status, x.CriadaEm });
                e.HasIndex(x => x.IdFuncionario).IsUnique().HasFilter("[Status] = N'Pendente'");
            });

            modelBuilder.Entity<Perfil>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Departamento>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Nome)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Senha)
                    .IsRequired();

                entity.Property(u => u.IdPerfil)
                    .IsRequired();

                entity.Property(u => u.Ativo)
                    .IsRequired();

                entity.HasOne(u => u.Perfil)
                    .WithMany()
                    .HasForeignKey(u => u.IdPerfil)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Nome)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(f => f.Salario) // Alterar caso use a tabela Salario
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(f => f.Cargo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(f => f.IdDepartamento)
                    .IsRequired();

                entity.Property(f => f.IdUsuario)
                    .IsRequired();

                entity.HasOne(f => f.Departamento)
                    .WithMany()
                    .HasForeignKey(f => f.IdDepartamento)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Gerente)
                    .WithMany()
                    .HasForeignKey(f => f.IdGerente)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany<PontoRegistro>()
                    .WithOne()
                    .HasForeignKey(pr => pr.IdFuncionario)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Usuario)
                    .WithOne()
                    .HasForeignKey<Funcionario>(f => f.IdUsuario)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Salario>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Valor)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(s => s.DataInicio)
                    .IsRequired();

                entity.Property(s => s.DataFim)
                    .IsRequired(false);

                entity.Property(s => s.MotivoAlteracao)
                    .IsRequired(false);
            });

            modelBuilder.Entity<PontoRegistro>(entity =>
            {
                entity.HasKey(pr => pr.Id);

                entity.Property(pr => pr.Data)
                    .IsRequired();

                entity.Property(pr => pr.HoraEntrada)
                    .IsRequired();

                entity.Property(pr => pr.HoraSaida)
                    .IsRequired(false);

                entity.Property(pr => pr.Status)
                    .IsRequired();
            });
        }
    }
}
