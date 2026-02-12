using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoWeb.Modelos
{
    public class ItemPedido
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public virtual Pedido? Pedido { get; set; }

        public int ProdutoId { get; set; }
        public virtual Produto? Produto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantidade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }
    }
}
