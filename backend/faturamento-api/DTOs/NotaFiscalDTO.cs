using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace faturamento_api.DTOs
{
    public class NotaFiscalCreateDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "A nota fiscal deve conter pelo menos um item.")]
        public List<ItemNotaFiscalCreateDTO> Itens { get; set; } = [];
    }

    public class ItemNotaFiscalCreateDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProdutoId deve ser maior que zero.")]
        public int ProdutoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Valor unitário não pode ser negativo.")]
        public decimal ValorUnitario { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Descrição do produto deve ter entre 1 e 100 caracteres.")]
        public string DescricaoProduto { get; set; } = string.Empty;
    }
}
