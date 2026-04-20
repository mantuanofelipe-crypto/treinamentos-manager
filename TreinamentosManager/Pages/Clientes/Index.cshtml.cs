using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Clientes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cliente> Clientes { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Clientes = await _context.Clientes
                .Include(c => c.Turmas)
                .ToListAsync();
        }
    }
}