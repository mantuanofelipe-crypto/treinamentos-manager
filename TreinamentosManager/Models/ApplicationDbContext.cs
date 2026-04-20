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
    }
}