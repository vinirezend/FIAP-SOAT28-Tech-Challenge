using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Application.Exceptions
{
    public sealed class BusinessException : Exception
    {
        public BusinessException(string message)
            : base(message)
        {
        }
    }
}
