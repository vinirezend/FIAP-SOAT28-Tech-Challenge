using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Resources
{
    public interface ILinkAssinadoGenerator
    {
        /// <summary>
        /// Gera uma URL absoluta para o caminho informado, assinada e com prazo de expiração.
        /// Quando minutosValidade não é informado, usa o padrão da configuração.
        /// </summary>
        LinkAssinadoDTO Gerar(string caminho, int? minutosValidade = null);

        StatusLinkAssinado Validar(string caminho, string? expira, string? assinatura);
    }

    public enum StatusLinkAssinado
    {
        Valido,
        Invalido,
        Expirado
    }

    public static class ParametrosLinkAssinado
    {
        public const string Expira = "expira";
        public const string Assinatura = "assinatura";
    }
}
