using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;

namespace PedidoWeb.Controllers
{
    [Authorize(Roles = RolesNomes.Admin)]
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmpresasController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Empresas.ToListAsync());
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Cnpj,RazaoSocial,NomeFantasia,ModoIntegracao,WebServiceUrl")] Empresa empresa, IFormFile? logoFile)
        {
            if (ModelState.IsValid)
            {
                if (logoFile != null && logoFile.Length > 0)
                {
                    empresa.LogomarcaPath = await SalvarImagem(logoFile);
                }

                _context.Add(empresa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empresa);
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();
            return View(empresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Cnpj,RazaoSocial,NomeFantasia,ModoIntegracao,LogomarcaPath,WebServiceUrl")] Empresa empresa, IFormFile? logoFile)
        {
            if (id != empresa.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        // Remove antiga se existir
                        if (!string.IsNullOrEmpty(empresa.LogomarcaPath))
                        {
                            var oldPath = Path.Combine(_hostEnvironment.WebRootPath, empresa.LogomarcaPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                        empresa.LogomarcaPath = await SalvarImagem(logoFile);
                    }

                    _context.Update(empresa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EmpresaExists(empresa.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(empresa);
        }

        private async Task<string> SalvarImagem(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "logos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/logos/" + uniqueFileName;
        }

        private async Task<bool> EmpresaExists(int id)
        {
            return await _context.Empresas.AnyAsync(e => e.Id == id);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObterLogo(string conta)
        {
            if (string.IsNullOrEmpty(conta)) return BadRequest();
            
            // Usamos um service scope para garantir que o ProvedorInquilino e o Contexto 
            // sejam resolvidos na ordem correta para esta consulta específica
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var provedor = scope.ServiceProvider.GetRequiredService<Servicos.IProvedorInquilino>();
                provedor.DefinirConta(conta);

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    var empresa = await context.Empresas.FirstOrDefaultAsync();
                    return Ok(new { logoPath = empresa?.LogomarcaPath });
                }
                catch
                {
                    return Ok(new { logoPath = (string?)null });
                }
            }
        }
    }
}
