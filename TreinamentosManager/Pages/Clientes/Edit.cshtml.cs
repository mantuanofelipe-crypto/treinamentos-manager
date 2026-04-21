using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Clientes
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            Cliente = cliente;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var cliente = await _context.Clientes.FindAsync(Cliente.Id);
            if (cliente == null) return NotFound();

            cliente.Nome = Cliente.Nome;
            cliente.Email = Cliente.Email;
            cliente.Empresa = Cliente.Empresa;
            cliente.TotalAlunos = Cliente.TotalAlunos;
            cliente.Ativo = Cliente.Ativo;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
