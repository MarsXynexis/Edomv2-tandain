Imports MySql.Data.MySqlClient

Public Class MainWindow

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        ' Pengaman: kalau belum login, kembali ke halaman login
        If Not UserSession.IsLoggedIn() Then
            Dim login As New LoginWindow()
            login.Show()
            Me.Close()
            Return
        End If

        ' Isi header dari session
        txtNamaUser.Text = UserSession.Nama
        txtRoleUser.Text = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(UserSession.Role)
        MuatPeriodeAktif()

        ' Halaman awal & menu sesuai role
        Select Case UserSession.Role
            Case "admin"
                btnMenuKuesioner.Visibility = Visibility.Collapsed
                txtJudulHalaman.Text = "Dashboard Admin"
                MainFrame.Navigate(New AccountMainView())   ' nanti ganti ke AdminDashboardView

            Case "dosen"
                btnMenuKuesioner.Visibility = Visibility.Collapsed
                txtJudulHalaman.Text = "Beranda Dosen"
                MainFrame.Navigate(New DosenMainView())

            Case "mahasiswa"
                txtJudulHalaman.Text = "Beranda Mahasiswa"
                MainFrame.Navigate(New MhsMainView())

            Case Else
                MessageBox.Show("Role tidak dikenali: " & UserSession.Role, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error)
                UserSession.Clear()
                Dim login As New LoginWindow()
                login.Show()
                Me.Close()
        End Select
    End Sub

    Private Sub MuatPeriodeAktif()
        Using conn = GetConnection()
            If conn Is Nothing Then
                txtPeriodeAktif.Text = "Periode tidak tersedia"
                Return
            End If

            Using cmd As New MySqlCommand("SELECT nama_periode FROM periode_edom WHERE status_aktif = 1 LIMIT 1", conn)
                Dim hasil = cmd.ExecuteScalar()
                txtPeriodeAktif.Text = If(hasil Is Nothing,
                                          "Tidak ada periode aktif",
                                          hasil.ToString() & " (Periode Aktif)")
            End Using
        End Using
    End Sub

    Private Sub BtnMenuAkun_Click(sender As Object, e As RoutedEventArgs)
        txtJudulHalaman.Text = "Pengaturan Akun Pengguna"
        MainFrame.Navigate(New AccountMainView())
    End Sub

    Private Sub BtnLogout_Click(sender As Object, e As RoutedEventArgs)
        If MessageBox.Show("Apakah Anda yakin ingin keluar?", "Konfirmasi Logout",
                           MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            UserSession.Clear()
            Dim login As New LoginWindow()
            login.Show()
            Me.Close()
        End If
    End Sub

End Class