using PedidoWeb.Modelos;

namespace PedidoWeb.Modelos
{
    public class DashboardViewModel
    {
        public int QtdNaoAutorizados { get; set; }
        public int QtdAutorizados { get; set; }
        public decimal ValorTotalPedidos { get; set; }
        public int QtdProdutos { get; set; }
        public List<Pedido> ÚltimosPedidos { get; set; } = new List<Pedido>();
    }
}
