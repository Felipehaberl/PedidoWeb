using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;

namespace PedidoWeb.Controllers
{
    [Authorize(Roles = RolesNomes.Admin)]
    public class UsuariosClientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosClientesController(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Listagem de usuários e seus vínculos
        public async Task<IActionResult> Index()
        {
            await GarantirRolesExistentes();

            var usuarios = await _userManager.Users.ToListAsync();
            var vinculos = await _context.UsuariosClientes
                .Include(uc => uc.Cliente)
                .ToListAsync();

            ViewBag.Vinculos = vinculos;
            
            // Carregar roles de cada usuário
            var userRoles = new Dictionary<string, string>();
            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Sem Perfil";
            }
            ViewBag.UserRoles = userRoles;

            return View(usuarios);
        }

        private async Task GarantirRolesExistentes()
        {
            if (!await _roleManager.RoleExistsAsync(RolesNomes.Admin))
                await _roleManager.CreateAsync(new IdentityRole(RolesNomes.Admin));
            
            if (!await _roleManager.RoleExistsAsync(RolesNomes.Vendedor))
                await _roleManager.CreateAsync(new IdentityRole(RolesNomes.Vendedor));
        }

        [HttpPost]
        public async Task<IActionResult> AtribuirPerfil(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            if (roleName != "Nenhum")
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return RedirectToAction(nameof(Index));
        }

        // Gerenciar vínculos de um usuário específico
        public async Task<IActionResult> Gerenciar(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var clientesDoUsuario = await _context.UsuariosClientes
                .Where(uc => uc.UsuarioId == id)
                .Select(uc => uc.ClienteId)
                .ToListAsync();

            var todosClientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync();

            ViewBag.Usuario = usuario;
            ViewBag.ClientesDoUsuario = clientesDoUsuario;

            return View(todosClientes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarVinculos(string usuarioId, int[] clientesSelecionados)
        {
            if (string.IsNullOrEmpty(usuarioId)) return NotFound();

            // Remove todos os vínculos atuais
            var vinculosAtuais = _context.UsuariosClientes.Where(uc => uc.UsuarioId == usuarioId);
            _context.UsuariosClientes.RemoveRange(vinculosAtuais);

            // Adiciona os novos vínculos
            if (clientesSelecionados != null)
            {
                foreach (var clienteId in clientesSelecionados)
                {
                    _context.UsuariosClientes.Add(new UsuarioCliente
                    {
                        UsuarioId = usuarioId,
                        ClienteId = clienteId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
