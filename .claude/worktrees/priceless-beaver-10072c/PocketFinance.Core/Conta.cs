using System;

using System.Dynamic;

namespace PocketFinance.Core
{
    public class Conta
    {
        public int Id { get ; set ; }

        public string Nome { get ; set ; } = string.Empty;

        public string Tipo { get ; set ; } = string.Empty;

        public decimal Saldo { get ; set ; }

        public string UsuarioId { get  ; set ; } = string.Empty;
    }
}