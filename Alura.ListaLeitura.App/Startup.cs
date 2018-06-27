using Alura.ListaLeitura.App.Negocio;
using Alura.ListaLeitura.App.Repositorio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.App
{
    public class Startup
    {
        /*
         * Passando a classe IServiceCollection, não precisamos
         * se preocupar de dar um new
         * isenta de saber quem está implementando o objeto
         */
        public void ConfigureServices(IServiceCollection services)
        {            
            /* serviço de roteamento do AspNet Core */
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory )
        {
            var builder = new RouteBuilder(app);
            builder.MapRoute("Livros/ParaLer", LivrosParaLer);
            builder.MapRoute("Livros/Lendo", LivrosLendo);
            builder.MapRoute("Livros/Lidos", LivrosLidos);

            /*Passando o nome e o autor, via url*/
            //builder.MapRoute("Cadastro/NovoLivro/{nome}/{autor}", NovoLivroParaLer);

            //segundo argumento, é o RequestDelegate para atender este tipo de requisição
            /*Colocar restrição para as rotas, só entro, se tipo do id for inteiro*/
            builder.MapRoute("Livros/Detalhes/{id:int}", ExibeDetalhes);

            builder.MapRoute("Cadastro/NovoLivro", ExibeFormulario);

            builder.MapRoute("Cadastro/Incluir", ProcessaFormulario);
                       
            /*
             * Rota com template, se adequa a varias requisições, com caminhos diferentes
             */
            var rotas = builder.Build();

            app.UseRouter(rotas);

            //fluxo requisição-Resposta
            //app.Run(LivrosParaLer);
            //app.Run(Roteamento);

            //cada rota, encapsulada em um objeto
            
            /*
             * Para mostrar o erro de código 500*/
                loggerFactory.AddConsole();
                env.EnvironmentName = EnvironmentName.Production;
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/error");
                }
            /**/
        }

        private Task ProcessaFormulario(HttpContext context)
        {
            var livro = new Livro()
            {
                //pegando os valores da queryString
                //Titulo = Convert.ToString(context.GetRouteValue("nome"));
                Titulo = context.Request.Form["titulo"].First(),
                Autor = context.Request.Form["autor"].First(),
            };

            var repo = new LivroRepositorioCSV();
            repo.Incluir(livro);
            return context.Response.WriteAsync("O livro foi adicionado com sucesso");
        }

        private Task ExibeFormulario(HttpContext context)
        {
            /*var html = @"
                        <html>
                            <form action='/Cadastro/Incluir'>
                                <label>Título:</label>      
                                <input name='titulo'/>
                                <br>
                                <label>Autor:</label>
                                <input name='autor'/>
                                <br>
                                <button>Gravar</button>
                            </form>
                        </html>   
                        ";*/
                        //formulario
            var html = CarregaArquivoHTML("formulario");
            return context.Response.WriteAsync(html);
        }

        private string CarregaArquivoHTML(string nomeArquivo)
        {
            /*
             verificar o que está acontecendo
             erro: IO
             */
            var nomeCompletoArquivo = $"HTML/{nomeArquivo}.html";
            using(var arquivo = File.OpenText(nomeCompletoArquivo))
            {
                return arquivo.ReadToEnd();
            }
        }

        private Task ExibeDetalhes(HttpContext context)
        {
            int id = Convert.ToInt32(context.GetRouteValue("id"));
            var repo = new LivroRepositorioCSV();
            var livro = repo.Todos.First(l => l.Id == id);
            return context.Response.WriteAsync(livro.Detalhes());
        }
        
        //REQUES DELEGATE
        public Task NovoLivroParaLer(HttpContext context)
        {
            var livro = new Livro()
            {
                //retorna um object, portanto damos um parse para string
                Titulo = Convert.ToString(context.GetRouteValue("nome")),
                Autor = Convert.ToString(context.GetRouteValue("autor")),
            };

            var repo = new LivroRepositorioCSV();
            repo.Incluir(livro);
            return context.Response.WriteAsync("O livro foi adicionado com sucesso");
        }

        public Task Roteamento(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();
            /*
             * estrutura está limitando para o tipo de problema
             * Ideal: colocar em um local isolado
             *  var caminhoAtendidos = new Dictionary<string, string>
             */
            var caminhoAtendidos = new Dictionary<string, RequestDelegate>
            {
                {"/Livros/ParaLer", LivrosParaLer},
                {"/Livros/Lendo", LivrosLendo},
                {"/Livros/Lidos", LivrosLidos}
            };

            if (caminhoAtendidos.ContainsKey(context.Request.Path))
            {
                var metodo = caminhoAtendidos[context.Request.Path];
                return metodo.Invoke(context);
            }

            /* o cara que tem o endereço da requisição (context.Request.Path)*/
            /*ao inves de mostrar a mensagem abaixo, 
             iremos mostrar o codigo de erro, atraves da propriedade response*/
            context.Response.StatusCode = 404;
            return context.Response.WriteAsync("Caminho inexistente");
        }


        /* AO chegar uma requisição, mostrar a lista de 
         * livrosp para ler
         * Passando como parametro, estou dizendo que preciso de um cara deste tipo
         */
        public Task LivrosParaLer(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();
            var conteudoArquivo = CarregaArquivoHTML("para-ler");
            
            //return context.Response.WriteAsync(_repo.ParaLer.ToString());

            foreach(var livro in _repo.ParaLer.Livros)
            {
                conteudoArquivo = conteudoArquivo.Replace("#NOVO-ITEM#", $"<li>{livro.Titulo} - {livro.Autor}</li>#NOVO-ITEM#");
            }

            conteudoArquivo = conteudoArquivo.Replace("#NOVO-ITEM#", "");

            return context.Response.WriteAsync(conteudoArquivo);
        }

        public Task LivrosLendo(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();
            return context.Response.WriteAsync(_repo.Lendo.ToString());
        }

        public Task LivrosLidos(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();
            return context.Response.WriteAsync(_repo.Lidos.ToString());
        }
    }
}