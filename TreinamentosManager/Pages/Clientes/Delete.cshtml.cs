using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Clientes
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        public int TotalTurmas { get; set; }
        public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Turmas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            Cliente = cliente;
            TotalTurmas = cliente.Turmas.Count;
            if (TotalTurmas > 0)
                Erro = "Esta empresa possui turmas vinculadas. Remova ou altere essas turmas antes de excluir.";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Turmas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            if (cliente.Turmas.Any())
            {
                Cliente = cliente;
                TotalTurmas = cliente.Turmas.Count;
                Erro = "Esta empresa possui turmas vinculadas. Remova ou altere essas turmas antes de excluir.";
                return Page();
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
