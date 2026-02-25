using System.Security.Cryptography;
using System.Text;

namespace POSMonitor.Agent.Security;

public static class SecretProtector
{
    public static string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Plaintext cannot be empty", nameof(plaintext));
        }

        var bytes = Encoding.UTF8.GetBytes(plaintext);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(protectedBytes);
    }

    public static string Decrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
        {
            throw new ArgumentException("Encrypted value cannot be empty", nameof(encrypted));
        }

        var protectedBytes = Convert.FromBase64String(encrypted);
        var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(bytes);
    }
}
