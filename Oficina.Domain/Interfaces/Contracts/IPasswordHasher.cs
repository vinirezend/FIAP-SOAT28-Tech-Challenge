using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Domain.Interfaces.Contracts
{
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool Verify(string password, string hashedPassword);
    }
}
