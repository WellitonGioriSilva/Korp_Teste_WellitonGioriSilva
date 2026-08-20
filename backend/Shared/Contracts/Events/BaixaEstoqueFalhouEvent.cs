using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events
{
    public class BaixaEstoqueFalhouEvent
    {
        public int NotaFiscalId { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime DataProcessamento { get; set; }
    }
}