namespace UnitTests
{
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using TeamStore.Interfaces;
    using TeamStore.Services;
    using Xunit;

    public class EncryptionServiceTests
    {
        [Theory,
            InlineData(""),
            InlineData("0000"),
            InlineData("12345G"),
            InlineData("000 0 0 0 0 000 0 0 00 0 00 "),
            InlineData("!@#$%^&*()1234567890"),
            InlineData("qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??"),
            InlineData("pass@word1")
            ]
        public void EncryptDecrypt_ShouldReturnCorrectData(string originalString)
        {
            // Arrange
            IEncryptionService encryptionService;
            //var services = new ServiceCollection();
            //services.AddDataProtection()
            //    .DisableAutomaticKeyGeneration()
            //    .SetDefaultKeyLifetime(new TimeSpan(1230000, 12, 12))
            //    .SetApplicationName("TeamStore-UnitTests")
            //    .PersistKeysToFileSystem(new DirectoryInfo(Environment.CurrentDirectory + "\\Keys"));

            //var serviceProvider = services.BuildServiceProvider();
            //var realProtector = serviceProvider.GetService<IDataProtectionProvider>();
            //var actualProtector = realProtector.CreateProtector("poo");

            // Act
            encryptionService = new EncryptionService();
            var encryptedString = encryptionService.EncryptStringAsync(originalString);
            var decryptedString = encryptionService.DecryptStringAsync(encryptedString);
            var decryptedString2 = encryptionService.DecryptStringAsync(encryptedString);
            var decryptedString3 = encryptionService.DecryptStringAsync(encryptedString);

            // Assert
            Assert.Equal<string>(originalString, decryptedString);
            Assert.NotEqual<string>(string.Empty, encryptedString);
        }
    }
}
