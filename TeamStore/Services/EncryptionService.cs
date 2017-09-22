namespace TeamStore.Services
{
    using Microsoft.AspNetCore.DataProtection;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using TeamStore.Interfaces;

    /// <summary>
    /// Coordinates data encryption and decryption through the AspNetCore.DataProtection API.
    /// Relies on a valid key existing in the \Keys folder of the running application.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        IDataProtector _protector;

        /// <summary>
        /// Constructor for the EncryptionService.
        /// Validates that a valid key exists in the \Keys folder of the running application and will 
        /// throw on any Cryptographic exception.
        /// </summary>
        public EncryptionService()
        {
            var protectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(Environment.CurrentDirectory + "\\Keys"), options => {
                    // Comment out the following to generate a key for testing and local DB encryption
                    options.DisableAutomaticKeyGeneration();
                    // really long time as we persist data forever and don't want the key to expire
                    options.SetDefaultKeyLifetime(new TimeSpan(123456, 1, 2, 2, 2)); 
                    }
                );  //protector ?? throw new ArgumentNullException(nameof(protector));

            _protector = protectionProvider.CreateProtector("Key Validation");
            var bytes = Encoding.UTF8.GetBytes("Validates if there is a key in the Keys folder, throws if the key is not found.");
            var base64 = Convert.ToBase64String(bytes);
            try
            {
                var result = _protector.Protect(base64);
            }
            catch (CryptographicException)
            {
                throw new Exception("The applications database encryption key is not configured. Terminating.");
            }
        }

        /// <summary>
        /// Encrypts a string using AES.
        /// </summary>
        /// <param name="stringToEncrypt">The string/text to encrypt</param>
        /// <returns>The encrypted cipher</returns>
        public string EncryptStringAsync(string stringToEncrypt)
        {
            return _protector.Protect(stringToEncrypt);
        }

        /// <summary>
        /// Decrypts a passed cipher to its original text/string
        /// </summary>
        /// <param name="stringToDecrypt">The cipher to be decrypted</param>
        /// <returns>The decrypted text/ciper</returns>
        public string DecryptStringAsync(string stringToDecrypt)
        {
            var bytes = Encoding.UTF8.GetBytes(stringToDecrypt);
            var base64 = Convert.ToBase64String(bytes);
            return _protector.Unprotect(stringToDecrypt);
        }
    }
}
