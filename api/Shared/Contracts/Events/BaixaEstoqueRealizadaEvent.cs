using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events
{
    public class BaixaEstoqueRealizadaEvent
    {
        public int NotaFiscalId { get; set; }
        public DateTime DataProcessamento { get; set; }
    }
}