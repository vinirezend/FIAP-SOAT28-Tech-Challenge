using Oficina.Domain.DTOs;
using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IUsuarioRepository
    {
        public Task<Usuario> Obter(int idUsuario);
        public Task<Usuario> ObterPorCredenciais(UsuarioCredenciaisDTO usuarioCredenciais);
    }
}
