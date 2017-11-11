namespace UnitTests
{
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Services;
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
            InlineData("qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??qwertyuiop[]asdfgjkl;'\\zxcvbm,./\\z12345790-=!@#$%&*()_+QWETUIOP{}ASDGJKL:|ZXCVBM<>??"),
            InlineData("pass@word1")
            ]
        public void EncryptDecrypt_ShouldReturnCorrectData(string originalString)
        {
            // Arrange
            IEncryptionService encryptionService;

            // Act
            encryptionService = new EncryptionService();
            var encryptedString = encryptionService.EncryptString(originalString);
            var decryptedString = encryptionService.DecryptString(encryptedString);
            var decryptedString2 = encryptionService.DecryptString(encryptedString);
            var decryptedString3 = encryptionService.DecryptString(encryptedString);

            // Assert
            Assert.Equal<string>(originalString, decryptedString);
            Assert.Equal<string>(decryptedString2, decryptedString3);

            if (originalString.Equals(string.Empty) == false)
            {
                Assert.NotEqual<string>(string.Empty, encryptedString);
                Assert.NotEqual<string>(string.Empty, decryptedString);
            }
        }
    }
}
