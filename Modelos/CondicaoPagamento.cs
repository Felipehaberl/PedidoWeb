using System.ComponentModel.DataAnnotations;

namespace PedidoWeb.Modelos
{
    public class CondicaoPagamento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(100)]
        public string Descricao { get; set; } = string.Empty;

        public int QuantidadeParcelas { get; set; }

        public int IntervaloDias { get; set; }

        [Display(Name = "ID Externo (ERP)")]
        public string? IntegracaoId { get; set; }
    }
}
