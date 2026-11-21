Imports MySql.Data.MySqlClient

Public Class LoginWindow

    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs)
        Dim username As String = txtUsername.Text.Trim()
        Dim password As String = txtPassword.Password

        If username = "" OrElse password = "" Then
            MessageBox.Show("Mohon isi username dan password!", "Peringatan",
                            MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Using conn = GetConnection()
            If conn Is Nothing Then Return

            Dim sql As String =
                "SELECT id, username, password, nama, role, id_kelas " &
                "FROM users WHERE username = @u AND aktif = 1 LIMIT 1"

            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@u", username)

                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        TampilGagal()
                        Return
                    End If

                    If Not CekPassword(password, rd.GetString("password")) Then
                        TampilGagal()
                        Return
                    End If

                    UserSession.Id = rd.GetInt32("id")
                    UserSession.Username = rd.GetString("username")
                    UserSession.Nama = rd.GetString("nama")
                    UserSession.Role = rd.GetString("role")
                    UserSession.IdKelas = If(rd.IsDBNull(rd.GetOrdinal("id_kelas")),
                                             CType(Nothing, Integer?),
                                             rd.GetInt32("id_kelas"))
                End Using
            End Using
        End Using

        Dim main As New MainWindow()
        main.Show()
        Me.Close()
    End Sub

    Private Sub TampilGagal()
        MessageBox.Show("Username atau password salah, atau akun tidak aktif.", "Login Gagal",
                        MessageBoxButton.OK, MessageBoxImage.Error)
        txtPassword.Clear()
    End Sub

End Class