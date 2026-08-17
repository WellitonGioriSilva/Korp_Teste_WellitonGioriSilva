using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace estoque_api.Models
{
    public class EventoProcessado
    {
        private string _eventoId;
        public string EventoId
        {
            get { return _eventoId; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("O ID do evento não pode ser nulo ou vazio.", nameof(value));
                }
                _eventoId = value;
            }
        }

        private string _eventoType;
        public string EventoType
        {
            get {return _eventoType;}
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("O Tipo do evento não pode ser nulo ou vazio.", nameof(value));
                }
                _eventoType = value;
            }
        }

        public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;
    }
}
