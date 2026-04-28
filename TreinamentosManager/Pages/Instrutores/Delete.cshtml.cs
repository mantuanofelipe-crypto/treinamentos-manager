using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Instrutores
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Instrutor Instrutor { get; set; } = default!;

        public int TotalTurmas { get; set; }
        public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var instrutor = await _context.Instrutores
                .Include(i => i.Turmas)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instrutor == null)
                return NotFound();

            Instrutor = instrutor;
            TotalTurmas = instrutor.Turmas.Count;
            if (TotalTurmas > 0)
                Erro = "Este instrutor possui turmas vinculadas. Remova ou altere o instrutor dessas turmas antes de excluir.";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var instrutor = await _context.Instrutores
                .Include(i => i.Turmas)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instrutor == null)
                return NotFound();

            if (instrutor.Turmas.Any())
            {
                Instrutor = instrutor;
                TotalTurmas = instrutor.Turmas.Count;
                Erro = "Este instrutor possui turmas vinculadas. Remova ou altere o instrutor dessas turmas antes de excluir.";
                return Page();
            }

            _context.Instrutores.Remove(instrutor);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
