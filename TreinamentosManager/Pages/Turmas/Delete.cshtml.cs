using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Turmas
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Turma Turma { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var turma = await _context.Turmas
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .Include(t => t.Instrutor)
                .Include(t => t.Datas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turma == null)
                return NotFound();

            Turma = turma;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var turma = await _context.Turmas.FindAsync(id);

            if (turma != null)
            {
                _context.Turmas.Remove(turma);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
