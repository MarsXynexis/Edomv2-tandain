Public Class PasswordChangeView

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        Dim passLama As String = txtPassLama.Password
        Dim passBaru As String = txtPassBaru.Password
        Dim konfirmasi As String = txtKonfirmasi.Password

        If String.IsNullOrEmpty(passLama) OrElse String.IsNullOrEmpty(passBaru) Then
            MessageBox.Show("Mohon isi semua field kata sandi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        If passBaru <> konfirmasi Then
            MessageBox.Show("Konfirmasi kata sandi baru tidak cocok!", "Gagal", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        MessageBox.Show("Kata sandi berhasil diperbarui!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information)
        Kembali()
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Kembali()
    End Sub

    Private Sub Kembali()
        Dim mainWin As MainWindow = TryCast(Application.Current.MainWindow, MainWindow)
        If mainWin IsNot Nothing Then
            mainWin.txtJudulHalaman.Text = "Pengaturan Akun Pengguna"
            mainWin.MainFrame.Navigate(New AccountMainView())
        End If
    End Sub

End Class