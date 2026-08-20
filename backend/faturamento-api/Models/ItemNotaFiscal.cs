using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace faturamento_api.Models
{
    public class ItemNotaFiscal
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _notaFiscalId;
        public int NotaFiscalId
        {
            get { return _notaFiscalId; }
            set { _notaFiscalId = value; }
        }

        private int _produtoId;
        public int ProdutoId
        {
            get { return _produtoId; }
            set { _produtoId = value; }
        }

        private int _quantidade;
        public int Quantidade
        {
            get { return _quantidade; }
            set { if(value > 0) { _quantidade = value; } else { throw new ArgumentException("Quantidade deve ser maior que zero."); } }
        }

        private decimal _valorUnitario;
        public decimal ValorUnitario
        {
            get { return _valorUnitario; }
            set { if(value >= 0) { _valorUnitario = value; } else { throw new ArgumentException("Valor unitário não pode ser negativo."); } }
        }

        public decimal ValorTotal
        {
            get { return CalcularValorTotal(); }
        }

        private string _descricaoProduto;
        public string DescricaoProduto
        {
            get { return _descricaoProduto; }
            set { if(!string.IsNullOrWhiteSpace(value)) { _descricaoProduto = value; } else { throw new ArgumentException("Descrição do produto não pode ser vazia."); } }
        }

        private decimal CalcularValorTotal()
        {
            return _quantidade * _valorUnitario;
        }
    }
}
