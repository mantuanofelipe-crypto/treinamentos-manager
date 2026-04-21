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
    public int PrevisaoAtual { get; set; }
    public int PrevisaoFutura { get; set; }

    // Dados para o gráfico: meses passados + meses futuros
    public List<string> GraficoLabels { get; set; } = new();
    public List<int> GraficoHistorico { get; set; } = new();
    public List<int?> GraficoTendencia { get; set; } = new();

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        var turmas = await _context.Turmas.ToListAsync();

        TurmasAtivas = turmas.Count(t => t.Fim < now);
        TurmasEmAndamento = turmas.Count(t => t.Inicio <= now && t.Fim >= now);
        TurmasFuturas = turmas.Count(t => t.Inicio > now);
        TotalClientes = await _context.Clientes.CountAsync();

        // Previsão atual = carga horária das turmas que terminam este mês
        PrevisaoAtual = turmas
            .Where(t => t.Fim.Year == now.Year && t.Fim.Month == now.Month)
            .Sum(t => t.CargaHoraria);

        // Previsão futura = carga horária das turmas que terminam no próximo mês
        var proximo = now.AddMonths(1);
        PrevisaoFutura = turmas
            .Where(t => t.Fim.Year == proximo.Year && t.Fim.Month == proximo.Month)
            .Sum(t => t.CargaHoraria);

        // Gráfico: 6 meses atrás até 3 meses à frente
        var inicio = new DateTime(now.Year, now.Month, 1).AddMonths(-6);
        var fim = new DateTime(now.Year, now.Month, 1).AddMonths(3);

        for (var m = inicio; m <= fim; m = m.AddMonths(1))
        {
            var mes = m;
            var carga = turmas
                .Where(t => t.Fim.Year == mes.Year && t.Fim.Month == mes.Month)
                .Sum(t => t.CargaHoraria);

            GraficoLabels.Add(mes.ToString("MMM/yy"));

            if (mes <= new DateTime(now.Year, now.Month, 1))
            {
                GraficoHistorico.Add(carga);
                GraficoTendencia.Add(null);
            }
            else
            {
                GraficoHistorico.Add(0);
                GraficoTendencia.Add(carga);
            }
        }

        // Conectar a linha de tendência ao último ponto histórico
        int lastIdx = GraficoHistorico.Count - 1;
        for (int i = lastIdx; i >= 0; i--)
        {
            if (GraficoHistorico[i] > 0 || i == lastIdx)
            {
                GraficoTendencia[i] = GraficoHistorico[i];
                break;
            }
        }
    }
}
