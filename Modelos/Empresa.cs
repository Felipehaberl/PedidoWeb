using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoWeb.Modelos
{
    public class Empresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório")]
        [StringLength(18)]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Razão Social é obrigatória")]
        [StringLength(200)]
        [Display(Name = "Razão Social")]
        public string RazaoSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome Fantasia é obrigatório")]
        [StringLength(200)]
        [Display(Name = "Nome Fantasia")]
        public string NomeFantasia { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Logomarca")]
        public string? LogomarcaPath { get; set; }

        [Display(Name = "Modo Integração")]
        public bool ModoIntegracao { get; set; }

        [StringLength(500)]
        [Display(Name = "URL do WebService")]
        public string? WebServiceUrl { get; set; }

        [Display(Name = "Validar Estoque")]
        public bool ValidarEstoque { get; set; }
    }
}
