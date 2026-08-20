using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace estoque_api.DTOs
{
    public class ProdutoCreateDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Descrição deve ter entre 1 e 100 caracteres.")]
        public string Descricao { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Saldo não pode ser negativo.")]
        public int Saldo { get; set; }
    }

    public class ProdutoUpdateDTO : ProdutoCreateDTO
    {}
}