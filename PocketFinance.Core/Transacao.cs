using System;

namespace PocketFinance.Core
{
    public class Transacao
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public bool EhReceita { get; set; }
    }
}