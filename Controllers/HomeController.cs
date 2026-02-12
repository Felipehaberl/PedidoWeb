using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;
using PedidoWeb.Models;

namespace PedidoWeb.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<IQueryable<Pedido>> GetQueryPedidos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var temVinculo = await _context.UsuariosClientes.AnyAsync(uc => uc.UsuarioId == userId);

        if (temVinculo)
        {
            var idsPermitidos = _context.UsuariosClientes
                .Where(uc => uc.UsuarioId == userId)
                .Select(uc => uc.ClienteId);

            return _context.Pedidos.Where(p => idsPermitidos.Contains(p.ClienteId));
        }

        return _context.Pedidos;
    }

    public async Task<IActionResult> Index()
    {
        var queryPedidos = await GetQueryPedidos();

        var viewModel = new DashboardViewModel
        {
            QtdNaoAutorizados = await queryPedidos.CountAsync(p => p.Status == StatusPedido.Aberto),
            QtdAutorizados = await queryPedidos.CountAsync(p => p.Status == StatusPedido.Autorizado),
            ValorTotalPedidos = await queryPedidos.SumAsync(p => p.ValorTotal),
            QtdProdutos = await _context.Produtos.CountAsync(),
            ÚltimosPedidos = await queryPedidos
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.Data)
                .Take(5)
                .ToListAsync()
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

