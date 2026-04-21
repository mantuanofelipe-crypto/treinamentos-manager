using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Turmas
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Turma> Turmas { get; set; } = default!;
        public IList<Cliente> Clientes { get; set; } = default!;
        public IList<Instrutor> Instrutores { get; set; } = default!;

        public string? FiltroStatus { get; set; }
        public int? FiltroClienteId { get; set; }
        public string? FiltroInstrutorId { get; set; }
        public string? FiltroModalidade { get; set; }

        public async Task OnGetAsync(string? status, int? clienteId, string? instrutorId, string? modalidade)
        {
            FiltroStatus = status;
            FiltroClienteId = clienteId;
            FiltroInstrutorId = instrutorId;
            FiltroModalidade = modalidade;

            Clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync();
            Instrutores = await _context.Instrutores.OrderBy(i => i.Nome).ToListAsync();

            var query = _context.Turmas
                .Include(t => t.Instrutor)
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(t => t.ClienteId == clienteId.Value);

            if (!string.IsNullOrEmpty(instrutorId))
                query = query.Where(t => t.InstrutorId == instrutorId);

            if (!string.IsNullOrEmpty(modalidade))
                query = query.Where(t => t.Modalidade == modalidade);

            var turmas = await query.ToListAsync();

            if (!string.IsNullOrEmpty(status))
                turmas = turmas.Where(t => t.Status == status).ToList();

            Turmas = turmas;
        }
    }
}
