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

    public List<string> GraficoLabels { get; set; } = new();
    public List<int> GraficoHistorico { get; set; } = new();
    public List<int?> GraficoTendencia { get; set; } = new();
    public List<int> GraficoPresencial { get; set; } = new();
    public List<int?> GraficoPresencialTendencia { get; set; } = new();
    public List<int> GraficoOnline { get; set; } = new();
    public List<int?> GraficoOnlineTendencia { get; set; } = new();

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        var turmas = await _context.Turmas.ToListAsync();

        TurmasAtivas = turmas.Count(t => t.Fim < now);
        TurmasEmAndamento = turmas.Count(t => t.Inicio <= now && t.Fim >= now);
        TurmasFuturas = turmas.Count(t => t.Inicio > now);
        TotalClientes = await _context.Clientes.CountAsync();

        PrevisaoAtual = turmas
            .Where(t => t.Fim.Year == now.Year && t.Fim.Month == now.Month)
            .Sum(t => t.CargaHoraria);

        var proximo = now.AddMonths(1);
        PrevisaoFutura = turmas
            .Where(t => t.Fim.Year == proximo.Year && t.Fim.Month == proximo.Month)
            .Sum(t => t.CargaHoraria);

        var mesAtual = new DateTime(now.Year, now.Month, 1);
        var inicio = mesAtual.AddMonths(-6);
        var fim = mesAtual.AddMonths(3);

        for (var m = inicio; m <= fim; m = m.AddMonths(1))
        {
            var mes = m;
            var isFuturo = mes > mesAtual;

            var todos = turmas.Where(t => t.Fim.Year == mes.Year && t.Fim.Month == mes.Month).ToList();
            var presencial = todos.Where(t => t.Modalidade == "Presencial").Sum(t => t.CargaHoraria);
            var online = todos.Where(t => t.Modalidade == "Online").Sum(t => t.CargaHoraria);
            var carga = todos.Sum(t => t.CargaHoraria);

            GraficoLabels.Add(mes.ToString("MMM/yy"));

            GraficoHistorico.Add(isFuturo ? 0 : carga);
            GraficoTendencia.Add(isFuturo ? carga : (int?)null);
            GraficoPresencial.Add(isFuturo ? 0 : presencial);
            GraficoPresencialTendencia.Add(isFuturo ? presencial : (int?)null);
            GraficoOnline.Add(isFuturo ? 0 : online);
            GraficoOnlineTendencia.Add(isFuturo ? online : (int?)null);
        }

        // Conectar linhas históricas ao primeiro ponto de tendência
        int pivot = GraficoLabels.Count - 4; // índice do mês atual (3 futuros no final)
        if (pivot >= 0 && pivot < GraficoLabels.Count)
        {
            GraficoTendencia[pivot] = GraficoHistorico[pivot];
            GraficoPresencialTendencia[pivot] = GraficoPresencial[pivot];
            GraficoOnlineTendencia[pivot] = GraficoOnline[pivot];
        }
    }
}
