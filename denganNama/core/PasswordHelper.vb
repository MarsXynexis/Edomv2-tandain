Imports System.Security.Cryptography
Imports System.Text

Public Module PasswordHelper

    ''' <summary>Hasilnya sama dengan SHA2(teks, 256) di MySQL (hex huruf kecil).</summary>
    Public Function Sha256Hex(teks As String) As String
        Dim bytes As Byte() = SHA256.HashData(Encoding.UTF8.GetBytes(teks))
        Return Convert.ToHexString(bytes).ToLowerInvariant()
    End Function

    Public Function CekPassword(plain As String, hashDb As String) As Boolean
        Return String.Equals(Sha256Hex(plain), hashDb, StringComparison.OrdinalIgnoreCase)
    End Function

End Module