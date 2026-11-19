Imports MySql.Data.MySqlClient

Imports System.Windows.Forms

Module db
    Private ReadOnly ConnectionString As String = "Server=localhost;Database=dengannama;Uid=root;Pwd=;"

    Public Function GetConnection() As MySqlConnection
        Dim conn As New MySqlConnection(ConnectionString)
        Try
            conn.Open()
            Return conn
        Catch ex As MySqlException
            MessageBox.Show("Koneksi ke database gagal: " & ex.Message, "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function
End Module