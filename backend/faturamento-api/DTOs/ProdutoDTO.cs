using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace faturamento_api.DTOs
{
    public class ProdutoResponse
    {
        public ProdutoDto Data { get; set; } = null!;
    }

    public class ProdutoDto
    {
        public int Id { get; set; }
        public int Saldo { get; set; }
    }
}