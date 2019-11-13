using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

/* See the "http://avoex.com/avoex/default-license/" for the full license governing this code. */

namespace Pikl.Utils {
    // based on http://stackoverflow.com/questions/165808/simple-two-way-encryption-for-c-sharp
    public static class AesEncryptor {
        // only the 128, 192, and 256-bit key sizes are specified in the AES standard. https://en.wikipedia.org/wiki/Advanced_Encryption_Standard
        const int keySize = 16; // keySize must be 16, 24 or 32 bytes.
        const string keyString = "SomeSuperSecureKeystringLol"; // EDIT 'keyString' BEFORE RELEASE. keyString must be longer than keySize.
        // DO NOT EDIT 'keySize, keyString' AFTER RELEASE YOUR PROJECT.
        // if you change keyString, you can not decrypt saved data encrypted by old keyString.

        // The size of the IV property must be the same as the BlockSize property divided by 8.
        // https://msdn.microsoft.com/ko-kr/library/system.security.cryptography.symmetricalgorithm.iv(v=vs.110).aspx
        const int IvLength = 16;

        static readonly UTF8Encoding encoder;
        static readonly RijndaelManaged rijndael;

        static AesEncryptor() {
            encoder = new UTF8Encoding();
            rijndael = new RijndaelManaged { Key = encoder.GetBytes("I AM A BANANA AND I NEED TO GET REPLACED").Take(keySize).ToArray() };
            rijndael.BlockSize = IvLength * 8; // only the 128-bit block size is specified in the AES standard.
        }

        public static byte[] GenerateIV() {
            rijndael.GenerateIV();
            return rijndael.IV;
        }

        #region PREPEND_VECTOR
        /// <summary>
        /// encrypt bytes with random vector. prepend vector to result.
        /// </summary>
        public static byte[] Encrypt(byte[] buffer) {
            rijndael.GenerateIV();
            using (ICryptoTransform encryptor = rijndael.CreateEncryptor()) {
                byte[] inputBuffer = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                return rijndael.IV.Concat(inputBuffer).ToArray();
            }
        }

        /// <summary>
        /// decrypt bytes, encrypted by Encrypt(byte[]).
        /// </summary>
        public static byte[] Decrypt(byte[] buffer) {
            byte[] iv = buffer.Take(IvLength).ToArray();
            using (ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, iv)) {
                return decryptor.TransformFinalBlock(buffer, IvLength, buffer.Length - IvLength);
            }
        }
        #endregion PREPEND_VECTOR

        #region CUSTOM_KEY
        /// <summary>
        /// not prepend vector to result. you must use DecryptIV(byte[], byte[]) to decrypt.
        /// </summary>
        public static byte[] EncryptIV(byte[] buffer, byte[] IV) {
            return EncryptKeyIV(buffer, rijndael.Key, IV);
        }

        /// <summary>
        /// decrypt bytes, encrypted by EncryptIV(byte[], byte[]).
        /// </summary>
        public static byte[] DecryptIV(byte[] buffer, byte[] IV) {
            return DecryptKeyIV(buffer, rijndael.Key, IV);
        }

        public static byte[] EncryptKeyIV(byte[] buffer, byte[] key, byte[] IV) {
            using (ICryptoTransform encryptor = rijndael.CreateEncryptor(key, IV)) {
                return encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            }
        }

        public static byte[] DecryptKeyIV(byte[] buffer, byte[] key, byte[] IV) {
            using (ICryptoTransform decryptor = rijndael.CreateDecryptor(key, IV)) {
                return decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            }
        }
        #endregion CUSTOM_KEY

        #region ENCRYPT_TO_STRING
        // string
        /// <summary>
        /// encrypt string with random vector. prepend vector to result.
        /// </summary>
        public static string Encrypt(string unencrypted) {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        /// <summary>
        /// decrypt string, encrypted by Encrypt(string).
        /// </summary>
        public static string Decrypt(string encrypted) {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        /// <summary>
        /// not prepend vector to result. you must use DecryptIV(string, byte[]) to decrypt.
        /// </summary>
        public static string EncryptIV(string unencrypted, byte[] vector) {
            return Convert.ToBase64String(EncryptIV(encoder.GetBytes(unencrypted), vector));
        }

        /// <summary>
        /// decrypt string, encrypted by EncryptIV(string, byte[]).
        /// </summary>
        public static string DecryptIV(string encrypted, byte[] vector) {
            return encoder.GetString(DecryptIV(Convert.FromBase64String(encrypted), vector));
        }

        // bool
        public static string Encrypt(bool unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static bool DecryptBool(string encrypted) {
            return BitConverter.ToBoolean(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // char
        public static string Encrypt(char unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static char DecryptChar(string encrypted) {
            return BitConverter.ToChar(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // double
        public static string Encrypt(double unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static double DecryptDouble(string encrypted) {
            return BitConverter.ToDouble(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // float
        public static string Encrypt(float unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static float DecryptFloat(string encrypted) {
            return BitConverter.ToSingle(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // int
        public static string Encrypt(int unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static int DecryptInt(string encrypted) {
            return BitConverter.ToInt32(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // long
        public static string Encrypt(long unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static long DecryptLong(string encrypted) {
            return BitConverter.ToInt64(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // short
        public static string Encrypt(short unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static short DecryptShort(string encrypted) {
            return BitConverter.ToInt16(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // uint
        public static string Encrypt(uint unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static uint DecryptUInt(string encrypted) {
            return BitConverter.ToUInt32(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // ulong
        public static string Encrypt(ulong unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static ulong DecryptULong(string encrypted) {
            return BitConverter.ToUInt64(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }

        // ushort
        public static string Encrypt(ushort unencrypted) {
            return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
        }

        public static ushort DecryptUShort(string encrypted) {
            return BitConverter.ToUInt16(Decrypt(Convert.FromBase64String(encrypted)), 0);
        }
        #endregion ENCRYPT_TO_STRING
    }
}