using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoWeb.Modelos
{
    public enum StatusPedido
    {
        Aberto,
        Autorizado,
        Cancelado
    }

    public class Pedido
    {
        public int Id { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O cliente é obrigatório")]
        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        [Required(ErrorMessage = "A condição de pagamento é obrigatória")]
        public int CondicaoPagamentoId { get; set; }
        public virtual CondicaoPagamento? CondicaoPagamento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        public StatusPedido Status { get; set; } = StatusPedido.Aberto;

        public virtual ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}
