using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PedidoWeb.Modelos;

namespace PedidoWeb.Controllers
{
    public class SetupController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SetupController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> Seed()
        {
            // 1. Garantir Roles
            if (!await _roleManager.RoleExistsAsync(RolesNomes.Admin))
                await _roleManager.CreateAsync(new IdentityRole(RolesNomes.Admin));
            
            if (!await _roleManager.RoleExistsAsync(RolesNomes.Vendedor))
                await _roleManager.CreateAsync(new IdentityRole(RolesNomes.Vendedor));

            // 2. Criar Admin
            var adminEmail = "admin@sistema.com";
            var userAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (userAdmin == null)
            {
                userAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(userAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(userAdmin, RolesNomes.Admin);
                }
            }

            // 3. Criar Vendedor
            var vendedorEmail = "vendedor@sistema.com";
            var userVendedor = await _userManager.FindByEmailAsync(vendedorEmail);
            if (userVendedor == null)
            {
                userVendedor = new IdentityUser { UserName = vendedorEmail, Email = vendedorEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(userVendedor, "Vendedor@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(userVendedor, RolesNomes.Vendedor);
                }
            }

            return "Usuários de teste criados com sucesso! \n\n" +
                   "ADMIN: admin@sistema.com / Senha: Admin@123 \n" +
                   "VENDEDOR: vendedor@sistema.com / Senha: Vendedor@123";
        }
    }
}
