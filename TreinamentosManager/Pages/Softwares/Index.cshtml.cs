using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Softwares
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Software> Softwares { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Softwares = await _context.Softwares
                .Include(s => s.Turmas)
                .ToListAsync();
        }
    }
}