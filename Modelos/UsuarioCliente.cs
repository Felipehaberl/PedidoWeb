using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoWeb.Modelos
{
    public class UsuarioCliente
    {
        public string UsuarioId { get; set; } = string.Empty;
        
        [ForeignKey("UsuarioId")]
        public virtual IdentityUser? Usuario { get; set; }

        public int ClienteId { get; set; }
        
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }
    }
}
