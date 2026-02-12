using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;

namespace PedidoWeb.Controllers
{
    [Authorize(Roles = RolesNomes.Admin)]
    public class CondicoesPagamentoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Servicos.IIntegracaoService _integracaoService;

        public CondicoesPagamentoController(ApplicationDbContext context, Servicos.IIntegracaoService integracaoService)
        {
            _context = context;
            _integracaoService = integracaoService;
        }

        [HttpPost]
        public async Task<IActionResult> Importar()
        {
            try
            {
                await _integracaoService.ImportarCondicoesPagamentoAsync();
                TempData["MensagemSucesso"] = "Condições de pagamento importadas do ERP com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Falha na importação: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CondicoesPagamento.ToListAsync());
        }

        public async Task<IActionResult> Criar()
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            ViewBag.ModoIntegracao = empresa?.ModoIntegracao ?? false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Descricao,QuantidadeParcelas,IntervaloDias")] CondicaoPagamento condicao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(condicao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(condicao);
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var condicao = await _context.CondicoesPagamento.FindAsync(id);
            if (condicao == null) return NotFound();
            
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            ViewBag.ModoIntegracao = empresa?.ModoIntegracao ?? false;

            return View(condicao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Descricao,QuantidadeParcelas,IntervaloDias")] CondicaoPagamento condicao)
        {
            if (id != condicao.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(condicao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CondicaoExiste(condicao.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(condicao);
        }

        public async Task<IActionResult> Excluir(int? id)
        {
            if (id == null) return NotFound();
            var condicao = await _context.CondicoesPagamento.FirstOrDefaultAsync(m => m.Id == id);
            if (condicao == null) return NotFound();
            return View(condicao);
        }

        [HttpPost, ActionName("Excluir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var condicao = await _context.CondicoesPagamento.FindAsync(id);
            if (condicao != null) _context.CondicoesPagamento.Remove(condicao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CondicaoExiste(int id)
        {
            return _context.CondicoesPagamento.Any(e => e.Id == id);
        }
    }
}
