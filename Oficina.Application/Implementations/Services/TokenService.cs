using Microsoft.IdentityModel.Tokens;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Configurations;
using Oficina.Domain.Interfaces.Repositorys;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Oficina.Application.Implementations.Services
{
    public class TokenService : ITokenService
    {
        private TokenConfiguration _configuration;
        private IUsuarioRepository _usuarioRepository;

        public TokenService(TokenConfiguration configuration,
            IUsuarioRepository usuarioRepository)
        {
            _configuration = configuration;
            _usuarioRepository = usuarioRepository;
        }

        private const string Date_Format = "yyyy-MM-ddTHH:mm:ss";

        public string GerarTokenAcesso(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Secret));
            var signCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var options = new JwtSecurityToken(
                    issuer: _configuration.Issuer,
                    audience: _configuration.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_configuration.Minutes),
                    signingCredentials: signCredentials);

            return new JwtSecurityTokenHandler().WriteToken(options);
        }

        public async Task<TokenDTO?> ValidarCredenciais(UsuarioCredenciaisDTO usuarioCredenciais)
        {
            var usuario = await _usuarioRepository.ObterPorCredenciais(usuarioCredenciais.Login, usuarioCredenciais.Senha);

            if (usuario == null) return null;

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Login)
            };

            var token = GerarTokenAcesso(claims);

            DateTime dataCriacao = DateTime.UtcNow;
            DateTime DataExpiracao = dataCriacao.AddMinutes(_configuration.Minutes);

            return new TokenDTO(
                true,
                dataCriacao.ToString(Date_Format),
                DataExpiracao.ToString(Date_Format),
                token
            );
        }
    }
}
