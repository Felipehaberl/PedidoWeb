using System.Security.Claims;

namespace PedidoWeb.Servicos
{
    public class ProvedorInquilino : IProvedorInquilino
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _contaManual;

        public ProvedorInquilino(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? ObterConta()
        {
            if (!string.IsNullOrEmpty(_contaManual))
            {
                return _contaManual;
            }

            // Tenta obter do claim (depois de logado)
            var conta = _httpContextAccessor.HttpContext?.User?.FindFirstValue("Conta");
            if (!string.IsNullOrEmpty(conta))
            {
                return conta;
            }

            // Tenta obter do formulário (durante o POST de login)
            if (_httpContextAccessor.HttpContext?.Request.HasFormContentType == true)
            {
                if (_httpContextAccessor.HttpContext.Request.Form.TryGetValue("Input.Conta", out var contaForm))
                {
                    return contaForm;
                }
            }

            return null;
        }


        public void DefinirConta(string conta)
        {
            _contaManual = conta;
        }
    }
}
