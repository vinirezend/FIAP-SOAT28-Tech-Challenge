using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace Oficina.Api.Tests.Controllers
{
    /// <summary>
    /// Utilitários comuns aos testes de controllers.
    /// </summary>
    internal static class ControllerTestHelpers
    {
        /// <summary>Valor devolvido por <c>Url.Action(...)</c> nos testes.</summary>
        public const string LocationPadrao = "/api/recurso/1";

        /// <summary>
        /// Cria um IUrlHelper simulado. Nos testes unitários a propriedade <c>Url</c> do controller é nula,
        /// o que causaria NullReferenceException nos métodos que usam <c>Url.Action(...)</c> (os POST que retornam 201).
        /// </summary>
        public static Mock<IUrlHelper> CriarUrlHelperMock()
        {
            var mock = new Mock<IUrlHelper>();
            mock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(LocationPadrao);
            return mock;
        }

        /// <summary>
        /// Associa um HttpContext vazio ao controller (necessário quando o código acessa <c>Request</c>).
        /// </summary>
        public static DefaultHttpContext ConfigurarHttpContext(ControllerBase controller)
        {
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return httpContext;
        }
    }
}
