Imports MySql.Data.MySqlClient

Public Class LoginWindow

    Private jumlahGagal As Integer = 0

    Private Const MaxPercobaan As Integer = 5

    Private Sub LoginWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        txtUsername.Focus()
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs)
        Dim username As String = txtUsername.Text.Trim()
        Dim password As String = txtPassword.Password.Trim()

        If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            MessageBox.Show("Isi NIM, NIP, atau username dan kata sandi terlebih dahulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If jumlahGagal >= MaxPercobaan Then
            MessageBox.Show("Sudah 5 kali salah memasukkan sandi. Aplikasi ditutup sementara.", "Akses Dibatasi", MessageBoxButton.OK, MessageBoxImage.Stop)
            Application.Current.Shutdown()
            Exit Sub
        End If

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT id, username, password, nama, role, id_kelas FROM users WHERE username = @username AND aktif = TRUE LIMIT 1"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@username", username)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim dbPassword As String = reader("password").ToString()
                            Dim sandiCocok As Boolean = PasswordHelper.CekPassword(password, dbPassword) OrElse String.Equals(password, dbPassword, StringComparison.Ordinal)

                            If Not sandiCocok Then
                                TanganiLoginGagal()
                                Exit Sub
                            End If

                            UserSession.Id = Convert.ToInt32(reader("id"))
                            UserSession.Username = reader("username").ToString()
                            UserSession.Nama = reader("nama").ToString()
                            UserSession.Role = reader("role").ToString().ToLower()

                            If Not IsDBNull(reader("id_kelas")) Then
                                UserSession.IdKelas = Convert.ToInt32(reader("id_kelas"))
                            Else
                                UserSession.IdKelas = Nothing
                            End If

                            If UserSession.Role <> "admin" AndAlso UserSession.Role <> "dosen" AndAlso UserSession.Role <> "mahasiswa" Then
                                UserSession.Clear()
                                MessageBox.Show("Peran akun ini belum terdaftar di sistem.", "Akses Ditolak", MessageBoxButton.OK, MessageBoxImage.Warning)
                                Exit Sub
                            End If

                            jumlahGagal = 0
                            txtPassword.Clear()

                            Dim mainWindow As New MainWindow()
                            Application.Current.MainWindow = mainWindow
                            mainWindow.Show()
                            Me.Close()
                        Else
                            TanganiLoginGagal()
                        End If
                    End Using
                End Using
            End Using
        Catch ex As MySqlException
            MessageBox.Show("Koneksi database bermasalah: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        Catch ex As Exception
            MessageBox.Show("Ada gangguan saat memproses login: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub TanganiLoginGagal()
        jumlahGagal += 1
        MessageBox.Show("NIM/NIP atau kata sandi salah." & vbCrLf & "Sisa percobaan: " & (MaxPercobaan - jumlahGagal).ToString(), "Gagal Masuk", MessageBoxButton.OK, MessageBoxImage.Error)
        txtPassword.Clear()
        txtPassword.Focus()
    End Sub

End Class
