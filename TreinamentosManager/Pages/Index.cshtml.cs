using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public int TurmasAtivas { get; set; }
    public int TurmasEmAndamento { get; set; }
    public int TurmasFuturas { get; set; }
    public int TotalClientes { get; set; }

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        var turmas = await _context.Turmas.ToListAsync();

        TurmasAtivas = turmas.Count(t => t.Fim > now);
        TurmasEmAndamento = turmas.Count(t => t.Inicio <= now && t.Fim >= now);
        TurmasFuturas = turmas.Count(t => t.Inicio > now);
        TotalClientes = await _context.Clientes.CountAsync();
    }
}
