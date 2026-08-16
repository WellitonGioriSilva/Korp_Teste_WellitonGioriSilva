using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace faturamento_api.Models
{
    public class NotaFiscal
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _numero;
        public string Numero
        {
            get { return _numero; }
            set { if(!string.IsNullOrWhiteSpace(value)) { _numero = value; } else { throw new ArgumentException("Número da nota fiscal não pode ser vazio."); } }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { if(!string.IsNullOrWhiteSpace(value)) { _status = value; } else { _status = "Aberta"; } }
        }

        private DateTime _dataEmissao = DateTime.Now;
        public DateTime DataEmissao
        {
            get { return _dataEmissao; }
        }
    }
}