using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events
{
    public class BaixaEstoqueSolicitadaEvent
    {
        public string EventoId { get; set; } = null!;
        public string EventoType { get; set; } = null!;
        public DateTime DataSolicitacao { get; set; }
        public List<BaixaEstoqueItemEvent> Itens { get; set; } = [];
    }

    public class BaixaEstoqueItemEvent
    {
        public int ProdutoId  { get; set; }
        public int Quantidade { get; set; }
    }
}