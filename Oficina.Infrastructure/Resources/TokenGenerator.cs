using Microsoft.IdentityModel.Tokens;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Infrastructure.Configurations;
using Oficina.Domain.Interfaces.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Oficina.Infrastructure.Resources
{
    public class TokenGenerator(
        TokenConfiguration configuration,
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher) : ITokenGenerator
    {
        public string GerarTokenAcesso(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Secret));
            var signCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var options = new JwtSecurityToken(
                    issuer: configuration.Issuer,
                    audience: configuration.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(configuration.Minutes),
                    signingCredentials: signCredentials);

            return new JwtSecurityTokenHandler().WriteToken(options);
        }

        private const string Date_Format = "yyyy-MM-ddTHH:mm:ss";
        
        public async Task<TokenDTO?> ValidarCredenciais(UsuarioCredenciaisDTO usuarioCredenciais, CancellationToken cancellationToken = default)
        {
            var usuario = await usuarioRepository.ObterPorLoginAsync(usuarioCredenciais.Login, cancellationToken);

            if (usuario is null || !usuario.UsuarioAtivo || !passwordHasher.Verificar(usuarioCredenciais.Senha, usuario.Senha)) return null;

            if(usuario.IdColaborador.HasValue && usuario.ColaboradorAtivo != true)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, usuario.Login),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            if (usuario.IdColaborador.HasValue)
            {
                claims.Add(new Claim("id_colaborador", usuario.IdColaborador.Value.ToString()));

                if (!string.IsNullOrWhiteSpace(usuario.Cpf))
                {
                    claims.Add(new Claim("cpf", usuario.Cpf));
                }

                if (!string.IsNullOrWhiteSpace(usuario.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, usuario.Email));
                }

                if (usuario.IdDepartamento.HasValue)
                {
                    claims.Add(new Claim(
                        "id_departamento",
                        usuario.IdDepartamento.Value.ToString()));
                }
            }

            var token = GerarTokenAcesso(claims);

            DateTime dataCriacao = DateTime.Now;
            DateTime DataExpiracao = dataCriacao.AddMinutes(configuration.Minutes);

            return new TokenDTO(
                true,
                dataCriacao.ToString(Date_Format),
                DataExpiracao.ToString(Date_Format),
                token
            );
        }
    }
}
