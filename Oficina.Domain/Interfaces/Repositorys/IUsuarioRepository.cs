using Oficina.Domain.Entitys;
using Oficina.Domain.Models;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IUsuarioRepository
    {
        public Task<Usuario?> Obter(int idUsuario);
        public Task<UsuarioAutenticacao?> ObterPorCredenciais(string login, string senha);
    }
}
