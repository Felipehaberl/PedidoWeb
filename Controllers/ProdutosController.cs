using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;

namespace PedidoWeb.Controllers
{
    [Authorize(Roles = RolesNomes.Admin)]
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Servicos.IIntegracaoService _integracaoService;

        public ProdutosController(ApplicationDbContext context, Servicos.IIntegracaoService integracaoService)
        {
            _context = context;
            _integracaoService = integracaoService;
        }

        [HttpPost]
        public async Task<IActionResult> Importar()
        {
            try
            {
                await _integracaoService.ImportarProdutosAsync();
                TempData["MensagemSucesso"] = "Produtos importados do ERP com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Falha na importação: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Produtos.ToListAsync());
        }

        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();
            var produto = await _context.Produtos.FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null) return NotFound();
            return View(produto);
        }

        public async Task<IActionResult> Criar()
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            ViewBag.ModoIntegracao = empresa?.ModoIntegracao ?? false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Codigo,Descricao,Preco,Estoque,CodigoOriginal,CodigoFabrica,DescricaoVenda,CodigoBarras,ProdutoIdIntegracao")] Produto produto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();
            
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            ViewBag.ModoIntegracao = empresa?.ModoIntegracao ?? false;

            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Codigo,Descricao,Preco,Estoque,CodigoOriginal,CodigoFabrica,DescricaoVenda,CodigoBarras,ProdutoIdIntegracao")] Produto produto)
        {
            if (id != produto.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExiste(produto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        public async Task<IActionResult> Excluir(int? id)
        {
            if (id == null) return NotFound();
            var produto = await _context.Produtos.FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null) return NotFound();
            return View(produto);
        }

        [HttpPost, ActionName("Excluir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null) _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProdutoExiste(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }
    }
}
