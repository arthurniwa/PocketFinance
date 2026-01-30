using System;
using System.Linq;
using PocketFinance.Core;

namespace PocketFinance.API
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                bool continuar = true;

                while (continuar)
                {
                    Console.WriteLine("--SISTEMA DE FINANÇAS--");
                    Console.WriteLine("1 - Adicionar Transação");
                    Console.WriteLine("2 - Ver Extrato");
                    Console.WriteLine("3 - Sair");
                    Console.WriteLine("Escolha uma opção: ");

                    string opcao = Console.ReadLine();

                    switch (opcao)
                    {
                        case "1":
                            AdicionarTransacao(db);
                            break;
                        case "2":
                            ListarTransacoes(db);
                            break;
                        case "3":
                            continuar = false;
                            break;
                        default:
                            Console.WriteLine("Opção inválida");
                            break;
                    }
                }
            }
        }
        static void AdicionarTransacao(AppDbContext db)
        {
            Console.Clear();
            Console.WriteLine("--NOVA TRANSAÇÃO--");

            Console.WriteLine("Descrição (ex: Almoço): ");
            string descricao = Console.ReadLine();

            Console.WriteLine("Valor (ex: 10.00): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
            {
                valor = 0m;
            }

            Console.WriteLine("É receita? (s/n):");
            bool ehReceita = Console.ReadLine().ToLower() == "s";

            var t = new Transacao
            {
                Descricao = descricao,
                Valor = valor,
                Data = DateTime.Now,
                EhReceita = ehReceita
            };

            db.Transacoes.Add(t);
            db.SaveChanges();

            Console.WriteLine("Transação adicionada com sucesso!");
            Console.ReadKey();
        }

        static void ListarTransacoes(AppDbContext db)
        {
            Console.Clear();
            Console.WriteLine("--EXTRATO--");

            var lista = db.Transacoes
            .OrderByDescending(t => t.Data)
            .ToList();

            foreach (var t in lista)
            {
                string sinal = t.EhReceita ? "+" : "-";
                Console.WriteLine($"{t.Data.ToShortDateString()} - {t.Descricao} - {sinal} {t.Valor:C2}");
            }

            Console.WriteLine("\nEnter para voltar.");
            Console.ReadKey();
        }
    }
    
}