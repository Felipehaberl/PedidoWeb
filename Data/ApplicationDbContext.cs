using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PedidoWeb.Servicos;


using PedidoWeb.Modelos;

namespace PedidoWeb.Data
{
    public class ApplicationDbContext
        : IdentityDbContext
    {
        private readonly IProvedorInquilino _provedorInquilino;
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IProvedorInquilino provedorInquilino,
            IConfiguration configuration)
            : base(options)

        {
            _provedorInquilino = provedorInquilino;
            _configuration = configuration;
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<CondicaoPagamento> CondicoesPagamento { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<ItemPedido> ItensPedido { get; set; } = null!;
        public DbSet<UsuarioCliente> UsuariosClientes { get; set; } = null!;
        public DbSet<Empresa> Empresas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UsuarioCliente>()
                .HasKey(uc => new { uc.UsuarioId, uc.ClienteId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        {
            var conta = _provedorInquilino.ObterConta();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrEmpty(conta))
            {
                var builder = new MySqlConnector.MySqlConnectionStringBuilder(connectionString);
                builder.Database = conta;
                optionsBuilder.UseMySql(builder.ConnectionString, ServerVersion.AutoDetect(builder.ConnectionString));
            }
            else
            {
                // Fallback para design-time (migrações)
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }

            base.OnConfiguring(optionsBuilder);
        }


        // Aqui depois entram suas entidades:
        // public DbSet<Pedido> Pedidos { get; set; }
    }

}
