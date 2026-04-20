using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Instrutores
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Instrutor> Instrutores { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Instrutores = await _context.Instrutores
                .Include(i => i.Turmas)
                .ToListAsync();
        }
    }
}