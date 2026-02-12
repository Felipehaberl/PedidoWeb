using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;
using System.Security.Claims;

namespace PedidoWeb.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Servicos.IIntegracaoService _integracaoService;

        public PedidosController(ApplicationDbContext context, Servicos.IIntegracaoService integracaoService)
        {
            _context = context;
            _integracaoService = integracaoService;
        }

        private IQueryable<Pedido> GetQueryPedidos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Se for admin, vê tudo
            if (User.IsInRole(RolesNomes.Admin))
            {
                return _context.Pedidos;
            }

            // Se não for admin, vê apenas o que está vinculado
            var idsPermitidos = _context.UsuariosClientes
                .Where(uc => uc.UsuarioId == userId)
                .Select(uc => uc.ClienteId);

            return _context.Pedidos.Where(p => idsPermitidos.Contains(p.ClienteId));
        }

        private IQueryable<Cliente> GetQueryClientes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Se for admin, vê tudo
            if (User.IsInRole(RolesNomes.Admin))
            {
                return _context.Clientes;
            }

            var idsPermitidos = _context.UsuariosClientes
                .Where(uc => uc.UsuarioId == userId)
                .Select(uc => uc.ClienteId);

            return _context.Clientes.Where(c => idsPermitidos.Contains(c.Id));
        }

        public async Task<IActionResult> Index()
        {
            var query = GetQueryPedidos();
            var pedidos = await query
                .Include(p => p.Cliente)
                .Include(p => p.CondicaoPagamento)
                .OrderByDescending(p => p.Data)
                .ToListAsync();
            return View(pedidos);
        }

        public async Task<IActionResult> Criar()
        {
            await CarregarDadosView();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Pedido pedido, List<ItemPedido> itens)
        {
            if (itens == null || !itens.Any())
            {
                ModelState.AddModelError("", "O pedido deve ter pelo menos um item.");
            }

            if (ModelState.IsValid && itens != null)
            {
                pedido.ValorTotal = itens.Sum(i => i.ValorTotal);
                pedido.Itens = itens;
                
                // Verifica parâmetro de validação de estoque
                var empresa = await _context.Empresas.FirstOrDefaultAsync();
                bool validarEstoque = empresa?.ValidarEstoque ?? false;

                // Processamento de itens e estoque
                foreach (var item in itens)
                {
                    var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                    
                    if (produto != null)
                    {
                        // Valida apenas se o parâmetro estiver ativo
                        if (validarEstoque && produto.Estoque < item.Quantidade)
                        {
                            ModelState.AddModelError("", $"Estoque insuficiente para o produto {produto.Descricao}. Disponível: {produto.Estoque.ToString("N2")}");
                            await CarregarDadosView();
                            return View(pedido);
                        }

                        // Subtrai estoque (pode ficar negativo se validação estiver desligada)
                        produto.Estoque -= item.Quantidade;
                    }
                }

                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CarregarDadosView();
            return View(pedido);
        }


        private async Task CarregarDadosView()
        {
            var queryClientes = GetQueryClientes();
            ViewData["ClienteId"] = new SelectList(await queryClientes.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome");
            ViewData["CondicaoPagamentoId"] = new SelectList(_context.CondicoesPagamento.OrderBy(c => c.Descricao), "Id", "Descricao");
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await GetQueryPedidos()
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            if (pedido.Status != StatusPedido.Aberto)
            {
                TempData["Erro"] = "Pedidos autorizados ou cancelados não podem ser editados.";
                return RedirectToAction(nameof(Detalhes), new { id = id });
            }

            await CarregarDadosView();
            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Pedido pedido, List<ItemPedido> itens)
        {
            if (id != pedido.Id) return NotFound();

            var pedidoOriginal = await GetQueryPedidos()
                .Include(p => p.Itens)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedidoOriginal == null) return NotFound();

            if (pedidoOriginal.Status != StatusPedido.Aberto)
            {
                ModelState.AddModelError("", "Este pedido não pode ser editado pois não está mais em Aberto.");
            }

            if (itens == null || !itens.Any())
            {
                ModelState.AddModelError("", "O pedido deve ter pelo menos um item.");
            }

            if (ModelState.IsValid && itens != null)
            {
                // Devolver estoque dos itens antigos
                if (pedidoOriginal.Itens != null)
                {
                    foreach (var itemAntigo in pedidoOriginal.Itens)
                    {
                        var produto = await _context.Produtos.FindAsync(itemAntigo.ProdutoId);
                        if (produto != null)
                        {
                            produto.Estoque += itemAntigo.Quantidade;
                        }
                    }
                }

                var empresa = await _context.Empresas.FirstOrDefaultAsync();
                bool validarEstoque = empresa?.ValidarEstoque ?? false;

                // Validar e subtrair estoque dos novos itens
                foreach (var itemNovo in itens)
                {
                    var produto = await _context.Produtos.FindAsync(itemNovo.ProdutoId);
                    
                    if (produto != null)
                    {
                        if (validarEstoque && produto.Estoque < itemNovo.Quantidade)
                        {
                            ModelState.AddModelError("", $"Estoque insuficiente para o produto {produto.Descricao}. Disponível: {produto.Estoque.ToString("N2")}");
                            await CarregarDadosView();
                            return View(pedido);
                        }
                        produto.Estoque -= itemNovo.Quantidade;
                    }
                }

                // Remover itens antigos fisicamente do banco
                var itensParaRemover = _context.ItensPedido.Where(i => i.PedidoId == id);
                _context.ItensPedido.RemoveRange(itensParaRemover);

                // Configurar novos itens
                if (itens != null)
                {
                    foreach (var item in itens)
                    {
                        item.Id = 0; // Garante que é um novo insert
                        item.PedidoId = id;
                    }

                    pedido.ValorTotal = itens.Sum(i => i.ValorTotal);
                    pedido.Itens = itens;
                }
                pedido.Data = pedidoOriginal.Data;
                pedido.Status = StatusPedido.Aberto;

                _context.Update(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CarregarDadosView();
            return View(pedido);
        }


        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await GetQueryPedidos()
                .Include(p => p.Cliente)
                .Include(p => p.CondicaoPagamento)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        public async Task<IActionResult> Imprimir(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await GetQueryPedidos()
                .Include(p => p.Cliente)
                .Include(p => p.CondicaoPagamento)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedido == null) return NotFound();

            ViewBag.Empresa = await _context.Empresas.FirstOrDefaultAsync();

            return View(pedido);
        }

        [HttpPost]

        public async Task<IActionResult> Autorizar(int id)
        {
            var pedido = await GetQueryPedidos()
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido != null)
            {
                pedido.Status = StatusPedido.Autorizado;
                await _context.SaveChangesAsync();

                // Verifica se deve integrar
                var empresa = await _context.Empresas.FirstOrDefaultAsync();
                if (empresa != null && empresa.ModoIntegracao)
                {
                    var sucesso = await _integracaoService.EnviarPedidoAsync(pedido);
                    if (sucesso) TempData["MensagemSucesso"] = "Pedido autorizado e enviado para o ERP com sucesso!";
                    else TempData["MensagemErro"] = "Pedido autorizado, mas houve uma falha ao enviar para o ERP.";
                }
                else
                {
                    TempData["MensagemSucesso"] = "Pedido autorizado com sucesso!";
                }
            }
            return RedirectToAction(nameof(Detalhes), new { id = id });
        }
        [HttpGet]
        public async Task<IActionResult> BuscarProdutos(string termo, string codigoOriginal, string codigoFabrica, string codigoBarras, string idExterno)
        {
            // Se nenhum filtro for informado, não retorna nada (pesquisa sob demanda)
            if (string.IsNullOrWhiteSpace(termo) && 
                string.IsNullOrWhiteSpace(codigoOriginal) && 
                string.IsNullOrWhiteSpace(codigoFabrica) && 
                string.IsNullOrWhiteSpace(codigoBarras) && 
                string.IsNullOrWhiteSpace(idExterno))
            {
                return Json(new List<object>());
            }

            var query = _context.Produtos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(idExterno))
                query = query.Where(p => p.ProdutoIdIntegracao.Contains(idExterno));

            if (!string.IsNullOrWhiteSpace(codigoOriginal))
                query = query.Where(p => p.CodigoOriginal.Contains(codigoOriginal));

            if (!string.IsNullOrWhiteSpace(codigoFabrica))
                query = query.Where(p => p.CodigoFabrica.Contains(codigoFabrica));
            
            if (!string.IsNullOrWhiteSpace(codigoBarras))
                query = query.Where(p => p.CodigoBarras.Contains(codigoBarras));

            if (!string.IsNullOrWhiteSpace(termo))
            {
                // Pesquisa por Código Interno, Descrição ou Descrição Venda
                query = query.Where(p => 
                    p.Codigo.Contains(termo) || 
                    p.Descricao.Contains(termo) || 
                    (p.DescricaoVenda != null && p.DescricaoVenda.Contains(termo))
                );
            }

            var produtos = await query
                .OrderBy(p => p.Descricao)
                .Take(50) // Limita resultado para performance
                .Select(p => new {
                    p.Id,
                    p.Codigo,
                    p.Descricao,
                    p.DescricaoVenda,
                    p.CodigoOriginal,
                    p.CodigoFabrica,
                    p.CodigoBarras,
                    p.ProdutoIdIntegracao,
                    p.Preco,
                    p.Estoque
                })
                .ToListAsync();

            return Json(produtos);
        }

        }
}
