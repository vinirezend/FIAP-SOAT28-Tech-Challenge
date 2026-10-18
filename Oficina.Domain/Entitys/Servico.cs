namespace Oficina.Domain.Entitys
{
    public class Servico
    {
        public int IdServico {get; private set;}
        public string Nome {get; private set;} = default!;
        public string Descricao {get; private set;} = default!;
        public decimal ValorEstipulado {get; private set;} = default!;
        public bool Ativo {get; private set;} = default!;

       private Servico() { }

       public static Servico Criar(string Nome, string Descricao, decimal ValorEstipulado)
       {
           return new Servico
           {
               Nome = Nome,
               Descricao = Descricao,
               ValorEstipulado = ValorEstipulado,
               Ativo = true
           };
       }
       public void Atualizar(string nome, string descricao, decimal valorEstipulado, bool ativo)
       {
           Nome = nome;
           Descricao = descricao;
           ValorEstipulado = valorEstipulado;
           Ativo = ativo;
       }
       public static Servico Reconstituir(int idServico, string nome, string descricao, decimal valorEstipulado, bool ativo) =>
       new() { IdServico = idServico, Nome = nome, Descricao = descricao, ValorEstipulado = valorEstipulado, Ativo = ativo };
    }
}