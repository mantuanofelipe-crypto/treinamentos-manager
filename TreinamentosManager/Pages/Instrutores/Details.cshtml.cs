using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Instrutores
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Instrutor Instrutor { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var instrutor = await _context.Instrutores
                .Include(i => i.Proficiencias).ThenInclude(p => p.Software)
                .Include(i => i.ACPs).ThenInclude(a => a.Software)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instrutor == null) return NotFound();

            Instrutor = instrutor;
            return Page();
        }
    }
}
