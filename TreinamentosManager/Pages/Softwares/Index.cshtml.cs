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
        public Dictionary<int, int> ACPCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Softwares = await _context.Softwares
                .Include(s => s.Turmas)
                .ToListAsync();

            ACPCounts = await _context.InstrutorACPs
                .GroupBy(a => a.SoftwareId)
                .Select(g => new { SoftwareId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SoftwareId, x => x.Count);
        }
    }
}
