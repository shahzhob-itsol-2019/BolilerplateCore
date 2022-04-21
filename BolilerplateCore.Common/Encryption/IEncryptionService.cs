using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Common.Encryption
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
