using System;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Class generates hashes for SHA1 hashing algorithm.
/// </summary>
namespace LoveSeat.Core
{
    public class HashIt
    {
        /// <summary>
        /// Computes SHA1 hash for plain text and returns a
        /// base64-encoded result. Before the hash is computed, a random salt of 25 characters.
        /// is generated and appended to the plain text.  The salt is passed by
        /// reference.
        /// </summary>
        public static string ComputeHash(string plainText, ref string salt)
        {
            // If salt is not specified, generate it on the fly.
            byte[] saltBytes = new byte[0];

            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Generate salt of 25 characters
            salt = RandomString(25);

            // Encode with UTF8
            saltBytes = Encoding.UTF8.GetBytes(salt);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            // Current versions are SHA1 for hashed password
            HashAlgorithm hash = SHA1.Create();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Convert result into a hexadecimal
            string hashValue = ByteArrayToString(hashBytes);

            // Return the result.
            return hashValue;
        }

        /// <summary>
        /// Build random string
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string RandomString(int size)
        {

            Random rnd = new Random();
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rnd.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Convert ByteArray to String
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }

    }
}