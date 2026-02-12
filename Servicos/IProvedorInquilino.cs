namespace PedidoWeb.Servicos
{
    public interface IProvedorInquilino
    {
        string? ObterConta();
        void DefinirConta(string conta);
    }
}
