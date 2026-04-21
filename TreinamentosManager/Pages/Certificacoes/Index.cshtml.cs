using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Certificacoes
{
    public class CertificacaoItem
    {
        public string InstrutorId { get; set; } = default!;
        public string InstrutorNome { get; set; } = default!;
        public string Tipo { get; set; } = default!;
        public string? SoftwareNome { get; set; }
        public DateTime DataExpiracao { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CertificacaoItem> Itens { get; set; } = new();

        public async Task OnGetAsync()
        {
            var limite = DateTime.Today.AddDays(180);

            var acis = await _context.Instrutores
                .Where(i => i.DataExpiracaoACI.HasValue && i.DataExpiracaoACI.Value <= limite)
                .Select(i => new CertificacaoItem
                {
                    InstrutorId = i.Id,
                    InstrutorNome = i.Nome,
                    Tipo = "ACI",
                    SoftwareNome = null,
                    DataExpiracao = i.DataExpiracaoACI!.Value
                }).ToListAsync();

            var acps = await _context.InstrutorACPs
                .Include(a => a.Instrutor)
                .Include(a => a.Software)
                .Where(a => a.DataExpiracao <= limite)
                .Select(a => new CertificacaoItem
                {
                    InstrutorId = a.InstrutorId,
                    InstrutorNome = a.Instrutor.Nome,
                    Tipo = "ACP",
                    SoftwareNome = a.Software.Nome,
                    DataExpiracao = a.DataExpiracao
                }).ToListAsync();

            Itens = acis.Concat(acps).ToList();
        }
    }
}
