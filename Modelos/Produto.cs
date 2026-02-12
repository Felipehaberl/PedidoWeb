using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoWeb.Modelos
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O código é obrigatório")]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Estoque { get; set; } // Coluna simples conforme solicitado para integração via Webservice

        [StringLength(50)]
        [Display(Name = "Código Original")]
        public string? CodigoOriginal { get; set; }

        [StringLength(50)]
        [Display(Name = "Código de Fábrica")]
        public string? CodigoFabrica { get; set; }

        [StringLength(500)]
        [Display(Name = "Descrição de Venda")]
        public string? DescricaoVenda { get; set; }

        [StringLength(50)]
        [Display(Name = "Código de Barras")]
        public string? CodigoBarras { get; set; }

        [Display(Name = "ID Externo (ERP)")]
        public string? ProdutoIdIntegracao { get; set; }
    }
}

