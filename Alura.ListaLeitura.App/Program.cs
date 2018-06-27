using Alura.ListaLeitura.App.Negocio;
using Alura.ListaLeitura.App.Repositorio;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Alura.ListaLeitura.App
{
    class Program
    {
        static void Main(string[] args)
        {

            var _repo = new LivroRepositorioCSV();

            //criar um objeto para hospedar os pedidos web
            //neste caso, estamos utilizando a interface
            IWebHost host = new WebHostBuilder()
                /* especificação do servidor http
                 * servidor que implementa o servidor http*/
                .UseKestrel()

                //classe que inicializa o host
                //toda a configuração de inicialização, fica
                //por conta desta classe
                .UseStartup<Startup>()

                //cria uma implementação da interface IWebHost
                .Build();
            host.Run();

            //ImprimeLista(_repo.ParaLer);
            //ImprimeLista(_repo.Lendo);
            //ImprimeLista(_repo.Lidos);


        }

        static void ImprimeLista(ListaDeLeitura lista)
        {
            Console.WriteLine(lista);
        }
    }
}
