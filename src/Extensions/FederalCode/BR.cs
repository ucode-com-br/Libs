using System;
using System.Globalization;

namespace UCode.Extensions.FederalCode
{
    public static class BR
    {
        public static bool IsCnpj(this string cnpj)
        {
            try
            {
                var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int soma;
                int resto;
                string digito;
                string tempCnpj;

                cnpj = cnpj.Trim();
                cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "").PadLeft(14, '0');

                if (cnpj.Length > 14)
                {
                    return false;
                }

                long l = -1;
                if (!long.TryParse(cnpj, out l))
                {
                    return false;
                }

                cnpj = l.ToString().PadLeft(14, '0');

                tempCnpj = cnpj[..12];

                soma = 0;
                for (var i = 0; i < 12; i++)
                {
                    soma += int.Parse(tempCnpj[i].ToString(), CultureInfo.InvariantCulture) * multiplicador1[i];
                }

                resto = soma % 11;
                if (resto < 2)
                {
                    resto = 0;
                }
                else
                {
                    resto = 11 - resto;
                }

                digito = resto.ToString();

                tempCnpj += digito;
                soma = 0;
                for (var i = 0; i < 13; i++)
                {
                    soma += int.Parse(tempCnpj[i].ToString(), CultureInfo.InvariantCulture) * multiplicador2[i];
                }

                resto = soma % 11;
                if (resto < 2)
                {
                    resto = 0;
                }
                else
                {
                    resto = 11 - resto;
                }

                digito += resto;

                return cnpj.EndsWith(digito, StringComparison.InvariantCulture);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string CnpjDigit(this string cnpj)
        {
            if (cnpj.Length != 12)
            {
                throw new ArgumentException(cnpj.Length.ToString(CultureInfo.InvariantCulture), nameof(cnpj));
            }

            var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            //tempCnpj = cnpj[..12];

            soma = 0;
            for (var i = 0; i < 12; i++)
            {
                soma += int.Parse(cnpj[i].ToString(), CultureInfo.InvariantCulture) * multiplicador1[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString(CultureInfo.InvariantCulture);

            cnpj += digito;
            soma = 0;
            for (var i = 0; i < 13; i++)
            {
                soma += int.Parse(cnpj[i].ToString(), CultureInfo.InvariantCulture) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito += resto;

            return digito;
        }

        public static bool IsCpf(this string cpf)
        {
            try
            {
                var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                string tempCpf;
                string digito;
                int soma;
                int resto;

                cpf = cpf.Trim();
                cpf = cpf.Replace(".", "").Replace("-", "").PadLeft(11, '0');

                if (cpf.Length > 11)
                {
                    return false;
                }

                long l = -1;
                if (!long.TryParse(cpf, out l))
                {
                    return false;
                }

                cpf = l.ToString(CultureInfo.InvariantCulture).PadLeft(11, '0');


                tempCpf = cpf[..9];
                soma = 0;

                for (var i = 0; i < 9; i++)
                {
                    soma += int.Parse(tempCpf[i].ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) * multiplicador1[i];
                }

                resto = soma % 11;
                if (resto < 2)
                {
                    resto = 0;
                }
                else
                {
                    resto = 11 - resto;
                }

                digito = resto.ToString(CultureInfo.InvariantCulture);

                tempCpf += digito;

                soma = 0;
                for (var i = 0; i < 10; i++)
                {
                    soma += int.Parse(tempCpf[i].ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) * multiplicador2[i];
                }

                resto = soma % 11;
                if (resto < 2)
                {
                    resto = 0;
                }
                else
                {
                    resto = 11 - resto;
                }

                digito += resto;

                return cpf.EndsWith(digito, StringComparison.InvariantCulture);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string? ToCnpj(string? format)
        {
            if (format == null)
            {
                return null;
            }

            var result = format.Replace("*", "").PadLeft(14, '0');

            if (IsCnpj(result))
            {
                return CnpjFormat(result);
            }

            return null;
        }

        public static string? ToCpf(string? format)
        {
            if (format == null)
            {
                return null;
            }

            var result = format.Replace("*", "").PadLeft(11, '0');

            if (IsCpf(result))
            {
                return CpfFormat(result);
            }

            return null;
        }

        public static string? CnpjFormat(string? format)
        {
            if (format == null)
            {
                return null;
            }

            var result = format.Replace("*", "").PadLeft(14, '0');

            if (IsCnpj(result))
            {
                result = result.Insert(12, "-");
                result = result.Insert(8, "/");
                result = result.Insert(5, ".");
                result = result.Insert(2, ".");

                return result;
            }

            return null;
        }

        public static string? CpfFormat(string? format)
        {
            if (format == null)
            {
                return null;
            }

            var result = format.Replace("*", "").PadLeft(11, '0');

            if (IsCpf(result))
            {
                result = result.Insert(9, "-");
                result = result.Insert(6, ".");
                result = result.Insert(3, ".");
                return result;
            }

            return null;
        }
    }
}
