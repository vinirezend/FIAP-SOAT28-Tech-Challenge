using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oficina.Domain.ValueObjects
{
    public sealed record Documento
    {
        public string Numero { get; }
        public TipoDocumento Tipo { get; }

        private Documento(string numero, TipoDocumento tipo) => (Numero, Tipo) = (numero, tipo);

        public static Documento Criar(string? valor)
        {
            var caracteres = new string((valor ?? "")
                .Where(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                .Select(char.ToUpperInvariant)
                .ToArray());

            return caracteres.Length switch
            {
                11 when caracteres.All(char.IsDigit) && CpfValido(caracteres) => new Documento(caracteres, TipoDocumento.Cpf),
                14 when CnpjValido(caracteres) => new Documento(caracteres, TipoDocumento.Cnpj),
                _ => throw new ArgumentException("CPF/CNPJ inválido.")
            };
        }


        private static bool CpfValido(string cpf)
        {
            if (cpf.Distinct().Count() == 1)
                return false;

            var soma1 = 0;
            for (var i = 0; i <= 8; i++)
                soma1 += (cpf[i] - '0') * (10 - i);

            var resto1 = soma1 % 11;
            var dv1 = resto1 < 2 ? 0 : 11 - resto1;

            var soma2 = 0;
            for (var i = 0; i <= 9; i++)
                soma2 += (cpf[i] - '0') * (11 - i);

            var resto2 = soma2 % 11;
            var dv2 = resto2 < 2 ? 0 : 11 - resto2;

            return dv1 == cpf[9] - '0' && dv2 == cpf[10] - '0';
        }
        private static bool CnpjValido(string cnpj)
        {
            if (cnpj.Distinct().Count() == 1)
                return false;

            if (!cnpj.Take(12).All(c => char.IsDigit(c) || (c >= 'A' && c <= 'Z')))
                return false;

            if (!cnpj.Skip(12).All(char.IsDigit))
                return false;

            var multiplicadores1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicadores2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var soma1 = 0;
            for (var i = 0; i < 12; i++)
                soma1 += (cnpj[i] - '0') * multiplicadores1[i];

            var resto1 = soma1 % 11;
            var dv1 = resto1 < 2 ? 0 : 11 - resto1;

            var soma2 = 0;
            for (var i = 0; i < 13; i++)
                soma2 += (cnpj[i] - '0') * multiplicadores2[i];

            var resto2 = soma2 % 11;
            var dv2 = resto2 < 2 ? 0 : 11 - resto2;

            return dv1 == cnpj[12] - '0' && dv2 == cnpj[13] - '0';
        }
    }
}