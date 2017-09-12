namespace TeamStore.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for an Encryption Service
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a string using AES.
        /// </summary>
        /// <param name="stringToEncrypt">The string/text to encrypt</param>
        /// <returns>The encrypted cipher</returns>
        string EncryptStringAsync(string stringToEncrypt);

        /// <summary>
        /// Decrypts a passed cipher to its original text/string
        /// </summary>
        /// <param name="stringToDecrypt">The cipher to be decrypted</param>
        /// <returns>The decrypted text/ciper</returns>
        string DecryptStringAsync(string stringToDecrypt);
    }
}
