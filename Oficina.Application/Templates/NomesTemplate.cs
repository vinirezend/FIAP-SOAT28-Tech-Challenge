namespace Oficina.Application.Templates;

/// <summary>
/// Templates em Templates/Html, agrupados por pasta de contexto. O valor é o caminho do arquivo, sem a extensão.
/// </summary>
public static class NomesTemplate
{
    /// <summary>Templates/Html/Comum: compartilhados pelas páginas abertas pelo cliente.</summary>
    public static class Comum
    {
        /// <summary>Estrutura da página e CSS; recebe {{titulo}} e {{conteudo}}.</summary>
        public const string Layout = "Comum/layout";
        public const string Mensagem = "Comum/mensagem";
    }

    /// <summary>Templates/Html/Orcamento</summary>
    public static class Orcamento
    {
        /// <summary>
        /// {{itens}} recebe as linhas de EmailItem já renderizadas;
        /// {{aprovar}} e {{recusar}} recebem os links assinados.
        /// </summary>
        public const string Email = "Orcamento/email";
        public const string EmailItem = "Orcamento/email-item";
        public const string PaginaAprovar = "Orcamento/pagina-aprovar";
        public const string PaginaRecusar = "Orcamento/pagina-recusar";
    }

    /// <summary>Templates/Html/AcompanhamentoOrdemServico</summary>
    public static class AcompanhamentoOrdemServico
    {
        public const string Email = "AcompanhamentoOrdemServico/email";

        /// <summary>{{valor}} recebe PaginaValor renderizado (ou vazio); {{etapas}} recebe as linhas de PaginaEtapa.</summary>
        public const string Pagina = "AcompanhamentoOrdemServico/pagina";
        public const string PaginaValor = "AcompanhamentoOrdemServico/pagina-valor";
        public const string PaginaEtapa = "AcompanhamentoOrdemServico/pagina-etapa";
    }
}
