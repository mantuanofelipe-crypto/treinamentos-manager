using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;
using TreinamentosManager.Services;
using TreinamentosManager.Services.Comunicados;

namespace TreinamentosManager.Pages.Turmas
{
    public class ComunicadoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly ConviteTemplateService _conviteTemplateService;

        public ComunicadoModel(
            ApplicationDbContext context,
            EmailService emailService,
            ConviteTemplateService conviteTemplateService)
        {
            _context = context;
            _emailService = emailService;
            _conviteTemplateService = conviteTemplateService;
        }

        [BindProperty]
        public int TurmaId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Informe ao menos um email.")]
        public string Emails { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Assunto { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Conteudo { get; set; } = string.Empty;

        [BindProperty]
        public bool AnexarConvitePptx { get; set; }

        [TempData]
        public string? Mensagem { get; set; }

        [TempData]
        public string? TipoMensagem { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var turma = await CarregarTurma(id);
            if (turma == null)
                return NotFound();

            TurmaId = turma.Id;
            Assunto = ComunicadoBuilder.CriarAssunto(turma);
            Conteudo = ComunicadoBuilder.CriarTexto(turma);
            AnexarConvitePptx = false;
            return Page();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int id)
        {
            var turma = await CarregarTurma(id);
            if (turma == null)
                return NotFound();

            var bytes = _conviteTemplateService.GerarConvitePptx(turma);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _conviteTemplateService.CriarNomeArquivo(turma));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var emails = SepararEmails(Emails).ToList();

            if (!emails.Any())
                ModelState.AddModelError(nameof(Emails), "Informe ao menos um email válido.");

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var turma = await CarregarTurma(TurmaId);
                if (turma == null)
                    return NotFound();

                var anexos = new List<EmailAttachment>();
                anexos.AddRange(_conviteTemplateService.CriarImagensInlineEmail());

                if (AnexarConvitePptx)
                {
                    anexos.Add(new EmailAttachment
                    {
                        FileName = _conviteTemplateService.CriarNomeArquivo(turma),
                        ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        Content = _conviteTemplateService.GerarConvitePptx(turma)
                    });
                }

                var conviteHtml = _conviteTemplateService.CriarHtmlEmail(turma);
                await _emailService.EnviarComunicadoAsync(emails, Assunto, Conteudo, anexos, conviteHtml);
                TipoMensagem = "success";
                Mensagem = $"Comunicado enviado para {emails.Count} destinatário(s).";
                return RedirectToPage(new { id = TurmaId });
            }
            catch (Exception ex)
            {
                TipoMensagem = "warning";
                Mensagem = $"Não foi possível enviar o comunicado: {ex.Message}";
                return Page();
            }
        }

        private async Task<Turma?> CarregarTurma(int id)
        {
            return await _context.Turmas
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .Include(t => t.Instrutor)
                .Include(t => t.Datas)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        private static IEnumerable<string> SepararEmails(string emails)
        {
            return Regex.Split(emails ?? "", @"[\s,;]+")
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e) && new EmailAddressAttribute().IsValid(e))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
