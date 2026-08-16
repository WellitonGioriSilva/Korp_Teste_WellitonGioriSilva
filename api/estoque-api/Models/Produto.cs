using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace estoque_api.Models
{
    public class Produto
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        
        private string _descricao;
        public string Descricao
        {
            get { return _descricao; }
            set { if(!string.IsNullOrWhiteSpace(value)) { _descricao = value; } else { throw new ArgumentException("Descrição não pode ser vazia."); } }
        }
        
        private int _saldo;
        public int Saldo
        {
            get { return _saldo; }
            set { if(value >= 0) { _saldo = value; } else { throw new ArgumentException("Saldo não pode ser negativo."); } }
        }
    }
}