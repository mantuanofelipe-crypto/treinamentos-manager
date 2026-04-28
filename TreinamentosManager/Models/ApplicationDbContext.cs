using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TreinamentosManager.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Instrutor> Instrutores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Software> Softwares { get; set; }
        public DbSet<InstrutorProficiencia> InstrutorProficiencias { get; set; }
        public DbSet<InstrutorACP> InstrutorACPs { get; set; }
        public DbSet<TurmaData> TurmaDatas { get; set; }
        public DbSet<TurmaConformidade> TurmaConformidades { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Turma>()
                .Property(t => t.TipoCliente)
                .HasDefaultValue("Turmas Gerenciável");

            builder.Entity<TurmaData>()
                .Property(t => t.DuracaoHoras)
                .HasDefaultValue(1m);

            builder.Entity<Turma>()
                .HasOne(t => t.ConformidadeDetalhes)
                .WithOne(c => c.Turma)
                .HasForeignKey<TurmaConformidade>(c => c.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TurmaConformidade>()
                .HasIndex(c => c.TurmaId)
                .IsUnique();
        }
    }
}
