namespace TeamStore.Services
{
    using Microsoft.AspNetCore.DataProtection;
    using System;
    using System.IO;
    using System.Text;
    using TeamStore.Interfaces;

    public class EncryptionService : IEncryptionService
    {
        IDataProtector _protector;

        public EncryptionService()
        {
            var protectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(Environment.CurrentDirectory + "\\Keys")
                , options => options.DisableAutomaticKeyGeneration()
                );  //protector ?? throw new ArgumentNullException(nameof(protector));

            _protector = protectionProvider.CreateProtector("Key Validation");
            var bytes = Encoding.UTF8.GetBytes("Validates if there is a key in the Keys folder, throws if the key is not found.");
            var base64 = Convert.ToBase64String(bytes);
            var result = _protector.Protect(base64);
        }

        public string EncryptStringAsync(string stringToEncrypt)
        {
            return _protector.Protect(stringToEncrypt);
        }

        public string DecryptStringAsync(string stringToDecrypt)
        {
            var bytes = Encoding.UTF8.GetBytes(stringToDecrypt);
            var base64 = Convert.ToBase64String(bytes);
            return _protector.Unprotect(stringToDecrypt);
        }
    }
}
