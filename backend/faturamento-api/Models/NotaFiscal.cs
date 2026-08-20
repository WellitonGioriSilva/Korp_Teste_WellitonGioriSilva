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

        private int _numero;
        public int Numero
        {
            get { return _numero; }
            set { if (value > 0) { _numero = value; } }
        }

        private string _status = "Aberta";
        public string Status
        {
            get { return _status; }
            set { if (!string.IsNullOrWhiteSpace(value)) { _status = value; } else { _status = "Aberta"; } }
        }

        private string? _observacao;
        public string? Observacao
        {
            get { return _observacao; }
            set { _observacao = value; }
        }

        private DateTime _dataEmissao = DateTime.UtcNow;
        public DateTime DataEmissao
        {
            get { return _dataEmissao; }
        }

        private List<ItemNotaFiscal> _itens = new List<ItemNotaFiscal>();
        public List<ItemNotaFiscal> Itens
        {
            get { return _itens; }
            set { if (value != null && value.Count > 0) { _itens = value; } else { throw new ArgumentException("A nota fiscal deve conter pelo menos um item."); } }
        }
    }
}
