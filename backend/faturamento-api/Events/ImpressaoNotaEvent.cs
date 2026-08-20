using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace faturamento_api.Events
{
    public class ImpressaoNotaEvent
    {
        public int NotaFiscalId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Message { get; set; }
    }
}