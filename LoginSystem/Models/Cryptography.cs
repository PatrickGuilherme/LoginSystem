using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LoginSystem.Models
{
    public class Cryptography
    {
        /// <summary>
        /// Atributo que irá receber um tipo de algoritmo de criptografia(hash) | Algoritmos: SHA512, MD5, RIPDEM160
        /// </summary>
        public HashAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Construtor que recebe um algoritmo de criptografia(hash)
        /// Tipos de parametro: SHA512.Create(), MD5.Create(), RIPEMD160.Create()  
        /// </summary>
        public Cryptography(HashAlgorithm algorithm)
        {
            Algorithm = algorithm;
        }

        /// <summary>
        /// Metodo para converter uma string passada em hash
        /// </summary>
        /// <returns>string já convertida em hash</returns>
        public string HashGenerate(string stringToBeEncrypted)
        {
            //Encoding.UTF8: Obtém uma codificação para o formato UTF-8
            //GetBytes(stringToBeEncrypted): decodifica um conjunto de caracteres(passado por parametro) em um vetor de bytes
            var encodedValue = Encoding.UTF8.GetBytes(stringToBeEncrypted);

            //Calcula o valor do hash de um vetor de bytes especificada.
            var passwordEncrypted = Algorithm.ComputeHash(encodedValue);

            //StringBuilder: Modifica uma cadeia de caracteres sem criar um novo objeto, funciona como uma string mais leve em relação a looping de modificações
            StringBuilder sb = new StringBuilder();

            //Looping de concatenação de StringBuilder
            foreach (var caractere in passwordEncrypted)
            {
                //Acrescenta informações ao final do stringBuilder
                //O parametro passado é um byte convertido para string em um formato 
                //ToString("X2"): Formata a string como dois caracteres hexadecimais maiúsculos
                sb.Append(caractere.ToString("X2"));
            }

            //Retorna o hash convertendo de StringBuilder para String  
            return sb.ToString();
        }


        /// <summary>
        /// Verifica se as duas string são iguais em hash
        /// </summary>
        /// <returns>True: Se forem iguas as strings | False: Se forem diferentes as string </returns>
        public bool HashVerify(string string1, string strings2)
        {
            //Verifica se a string [stringstored] está vazia ou nula
            if (string.IsNullOrEmpty(string1) || string.IsNullOrEmpty(strings2))
            {
                //Exibe uma exception
                throw new NullReferenceException("A string [stringstored] esta nula ou vazia.");
            }

            //Converter em hash
            var hashString1 = this.HashGenerate(string1);
            var hashString2 = this.HashGenerate(strings2);

            //Verificar igualdade
            if (String.Equals(hashString1, hashString2)) return true;
            return false;
        }
    }
}
