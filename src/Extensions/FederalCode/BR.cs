using System;
using System.Globalization;

namespace UCode.Extensions.FederalCode
{
    /// <summary>
    /// The BR class contains static members and methods that 
    /// provide functionality related to operations or utilities
    /// within the context of a specific application domain.
    /// </summary>
    public static class BR
    {
        /// <summary>
        /// Validates if the provided string is a valid Brazilian CNPJ (Cadastro Nacional da Pessoa Jurídica).
        /// The CNPJ must contain 14 digits, and the method checks the validity based on specific rules
        /// defined for CNPJ verification.
        /// </summary>
        /// <param name="cnpj">The CNPJ string to validate, which may include formatting characters like dots, dashes, or slashes.</param>
        /// <returns>
        /// Returns true if the CNPJ is valid; otherwise, returns false.
        /// A CNPJ is considered valid if it follows the correct digits' formatting and calculations for verification.
        /// </returns>
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

        /// <summary>
        /// Calculates the check digits (the last two digits) for a Brazilian CNPJ (Cadastro Nacional da Pessoa Jurídica).
        /// This method extends the string class to provide the CNPJ digit calculation functionality.
        /// </summary>
        /// <param name="cnpj">A string representing the CNPJ number without its check digits. It must be 12 characters long.</param>
        /// <returns>
        /// A string containing the two check digits calculated for the provided CNPJ number.
        /// The resulting string will be a combination of the calculated first and second check digits.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided CNPJ string length is not equal to 12.
        /// The exception includes details about the invalid length.
        /// </exception>
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

        /// <summary>
        /// Validates a Brazilian CPF (Cadastro de Pessoas Físicas) number.
        /// A CPF is valid if it consists of 11 digits and follows a specific checksum algorithm.
        /// This extension method will trim the input, remove non-numeric characters, 
        /// pad the number to ensure it has 11 digits, and then validates 
        /// the CPF against the algorithm used for generating valid CPFs.
        /// </summary>
        /// <param name="cpf">The CPF number as a string that needs to be validated.</param>
        /// <returns>
        /// Returns true if the CPF is valid, otherwise false. 
        /// If the input is null or does not represent a valid CPF number, 
        /// it will return false as well.
        /// </returns>
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

        /// <summary>
        /// Converts a given string to a valid CNPJ format, or returns null if the input is null or invalid.
        /// </summary>
        /// <param name="format">The input string that may contain a CNPJ number in an unformatted form.</param>
        /// <returns>
        /// A formatted CNPJ string if the input is valid; otherwise, returns null.
        /// </returns>
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

        /// <summary>
        /// Converts a given string to a CPF (Cadastro de Pessoas Físicas) format.
        /// If the input is null, returns null. 
        /// Removes any asterisks from the input and pads it with leading zeros 
        /// to ensure it has a length of 11 characters. 
        /// If the resulting string is a valid CPF, it formats it and returns it; 
        /// otherwise, it returns null.
        /// </summary>
        /// <param name="format">The input string that may contain asterisks and will be converted to CPF format.</param>
        /// <returns>
        /// The formatted CPF string if the input is valid; otherwise, null.
        /// </returns>
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

        /// <summary>
        /// Formats a CNPJ number by removing asterisks, padding the result with zeros,
        /// and inserting the appropriate formatting characters if the number is valid.
        /// </summary>
        /// <param name="format">The CNPJ string to format, which may contain '*' characters.</param>
        /// <returns>
        /// A formatted CNPJ string in the format 'XX.XXX.XXX/XXXX-XX' if the input is valid,
        /// or null if the input is null or not a valid CNPJ.
        /// </returns>
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

        /// <summary>
        /// Formats the provided string into a valid Brazilian CPF (Cadastro de Pessoas Físicas) format.
        /// If the input string is null, the method returns null.
        /// If the input string after removing asterisks does not represent a valid CPF, the method returns null.
        /// Otherwise, it returns the CPF formatted as "###.###.###-##".
        /// </summary>
        /// <param name="format">A string representing the CPF, potentially containing asterisks that will be removed.</param>
        /// <returns>
        /// A formatted string representing the CPF if valid, or null if the input is invalid or null.
        /// </returns>
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
