Public Class MainWindow

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ' Buka Halaman Akun Saya pertama kali
        MainFrame.Navigate(New AccountMainView())
    End Sub

    Private Sub BtnMenuAkun_Click(sender As Object, e As RoutedEventArgs)
        txtJudulHalaman.Text = "Pengaturan Akun Pengguna"
        MainFrame.Navigate(New AccountMainView())
    End Sub

    Private Sub BtnLogout_Click(sender As Object, e As RoutedEventArgs)
        If MessageBox.Show("Apakah Anda yakin ingin keluar?", "Konfirmasi Logout", MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            Dim login As New LoginWindow()
            login.Show()
            Me.Close()
        End If
    End Sub

End Class