using PedidoWeb.Modelos;

namespace PedidoWeb.Servicos
{
    public interface IIntegracaoService
    {
        Task<bool> EnviarPedidoAsync(Pedido pedido);
        Task ImportarClientesAsync();
        Task ImportarProdutosAsync();
        Task ImportarCondicoesPagamentoAsync();
    }
}
