using System.ComponentModel.DataAnnotations;

namespace PedidoWeb.Modelos
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF/CNPJ é obrigatório")]
        [StringLength(20)]
        public string CpfCnpj { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string? Email { get; set; }

        public string? Telefone { get; set; }
        
        public string? Endereco { get; set; }

        [Display(Name = "ID Externo (ERP)")]
        public string? IntegracaoId { get; set; }
    }
}
