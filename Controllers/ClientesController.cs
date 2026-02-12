using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;
using System.Security.Claims;

namespace PedidoWeb.Controllers
{
    [Authorize(Roles = RolesNomes.Admin)]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Servicos.IIntegracaoService _integracaoService;

        public ClientesController(ApplicationDbContext context, Servicos.IIntegracaoService integracaoService)
        {
            _context = context;
            _integracaoService = integracaoService;
        }

        private async Task<IQueryable<Cliente>> GetQueryClientes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var temVinculo = await _context.UsuariosClientes.AnyAsync(uc => uc.UsuarioId == userId);
            
            if (temVinculo)
            {
                var idsPermitidos = _context.UsuariosClientes
                    .Where(uc => uc.UsuarioId == userId)
                    .Select(uc => uc.ClienteId);
                
                return _context.Clientes.Where(c => idsPermitidos.Contains(c.Id));
            }

            return _context.Clientes;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var query = await GetQueryClientes();
            return View(await query.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var query = await GetQueryClientes();
            var cliente = await query.FirstOrDefaultAsync(m => m.Id == id);
            
            if (cliente == null) return NotFound();

            return View(cliente);
        }


        // GET: Clientes/Create
        public async Task<IActionResult> Criar()
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            ViewBag.ModoIntegracao = empresa?.ModoIntegracao ?? false;
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Nome,CpfCnpj,Email,Telefone,Endereco")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            ViewBag.ModoIntegracao = empresa?.ModoIntegracao ?? false;

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,CpfCnpj,Email,Telefone,Endereco")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExiste(cliente.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Excluir(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Excluir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExiste(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
        [HttpPost]
        public async Task<IActionResult> Importar()
        {
            try
            {
                await _integracaoService.ImportarClientesAsync();
                TempData["MensagemSucesso"] = "Clientes importados do ERP com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Falha na importação: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
