Imports MySql.Data.MySqlClient

Public Class PasswordChangeView

    Private Sub PasswordChangeView_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        txtPassLama.Focus()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If Not UserSession.IsLoggedIn() Then
            MessageBox.Show("Sesi sudah habis. Silakan masuk ulang.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim passLama As String = txtPassLama.Password.Trim()
        Dim passBaru As String = txtPassBaru.Password.Trim()
        Dim konfirmasi As String = txtKonfirmasi.Password.Trim()

        If String.IsNullOrWhiteSpace(passLama) Then
            MessageBox.Show("Isi kata sandi sekarang dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPassLama.Focus()
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(passBaru) Then
            MessageBox.Show("Isi kata sandi baru dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPassBaru.Focus()
            Exit Sub
        End If

        If passBaru.Length < 6 Then
            MessageBox.Show("Kata sandi baru minimal 6 karakter.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPassBaru.Focus()
            Exit Sub
        End If

        If passBaru <> konfirmasi Then
            MessageBox.Show("Kata sandi baru dan ulangannya tidak cocok.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtKonfirmasi.Clear()
            txtKonfirmasi.Focus()
            Exit Sub
        End If

        Dim konfirmasiDialog As MessageBoxResult = MessageBox.Show(
            "Simpan kata sandi baru sekarang?",
            "Ganti Kata Sandi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        )

        If konfirmasiDialog <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim queryCek As String = "SELECT COUNT(*) FROM users WHERE id = @id AND aktif = TRUE AND (password = @passLama OR password = SHA2(@passLama, 256))"

                Using cmdCek As New MySqlCommand(queryCek, conn)
                    cmdCek.Parameters.AddWithValue("@id", UserSession.Id)
                    cmdCek.Parameters.AddWithValue("@passLama", passLama)

                    Dim cocok As Integer = Convert.ToInt32(cmdCek.ExecuteScalar())

                    If cocok = 0 Then
                        MessageBox.Show("Kata sandi sekarang tidak sesuai.", "Verifikasi Gagal", MessageBoxButton.OK, MessageBoxImage.Error)
                        txtPassLama.Clear()
                        txtPassLama.Focus()
                        Exit Sub
                    End If
                End Using

                Dim queryUpdate As String = "UPDATE users SET password = SHA2(@password, 256) WHERE id = @id"

                Using cmdUpdate As New MySqlCommand(queryUpdate, conn)
                    cmdUpdate.Parameters.AddWithValue("@password", passBaru)
                    cmdUpdate.Parameters.AddWithValue("@id", UserSession.Id)

                    Dim barisTerpengaruh As Integer = cmdUpdate.ExecuteNonQuery()

                    If barisTerpengaruh > 0 Then
                        MessageBox.Show("Kata sandi berhasil diganti.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information)
                        Kembali()
                    Else
                        MessageBox.Show("Akun tidak ditemukan di database.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Koneksi database bermasalah: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Kembali()
    End Sub

    Private Sub Kembali()
        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then
            mainWin = TryCast(Application.Current.MainWindow, MainWindow)
        End If

        If mainWin IsNot Nothing Then
            mainWin.NavigasiKeHalaman(New AccountMainView(), "Akun Saya", "Data profil dan pengaturan kata sandi", mainWin.btnMenuAkun)
        End If
    End Sub

End Class
