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

        public async Task OnGetAsync()
        {
            Turmas = await _context.Turmas
                .Include(t => t.Instrutor)
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .ToListAsync();
        }
    }
}