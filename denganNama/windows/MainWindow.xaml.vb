Imports MySql.Data.MySqlClient

Public Class MainWindow

    Private semuaTombolNav As Button()

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        If Not UserSession.IsLoggedIn() Then
            KembaliKeLogin()
            Exit Sub
        End If

        semuaTombolNav = {
            btnMhsDashboard,
            btnDosenMain,
            btnAdminRekap,
            btnAdminSetup,
            btnAdminKelas,
            btnAdminDosen,
            btnMenuAkun,
            btnTopMenuAkun
        }

        MuatIdentitasHeader()
        MuatPeriodeAktif()
        AturMenuBerdasarkanPeran()
        BukaHalamanAwal()
    End Sub

    Private Sub BukaHalamanAwal()
        Select Case UserSession.Role
            Case "admin"
                NavigasiKeHalaman(New AdminDashboardView(), "Rekapitulasi", "Evaluasi seluruh dosen dan perhitungan Fuzzy Sugeno", btnAdminRekap)

            Case "dosen"
                NavigasiKeHalaman(New DosenMainView(), "Capaian Evaluasi", "Ringkasan hasil evaluasi dosen", btnDosenMain)

            Case "mahasiswa"
                NavigasiKeHalaman(New AccountMainView(), "Akun Saya", "Data profil dan pengaturan kata sandi", btnMenuAkun)

            Case Else
                MessageBox.Show("Peran pengguna tidak dikenali: " & UserSession.Role, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                UserSession.Clear()
                KembaliKeLogin()
        End Select
    End Sub

    Private Sub KembaliKeLogin()
        Dim loginWin As New LoginWindow()
        Application.Current.MainWindow = loginWin
        loginWin.Show()
        Me.Close()
    End Sub

    Private Sub MuatIdentitasHeader()
        Dim namaTampil As String = If(Not String.IsNullOrEmpty(UserSession.Nama), UserSession.Nama, UserSession.Username)
        txtNamaUser.Text = namaTampil
        txtNamaUserTop.Text = namaTampil

        Dim labelPeran As String
        Select Case UserSession.Role
            Case "admin"
                labelPeran = "Admin Jurusan"
            Case "dosen"
                labelPeran = "Dosen"
            Case "mahasiswa"
                labelPeran = "Mahasiswa"
            Case Else
                labelPeran = UserSession.Role
        End Select

        txtRoleUser.Text = labelPeran
        txtRoleUserTop.Text = labelPeran
    End Sub

    Public Sub MuatPeriodeAktif()
        Dim query As String = "SELECT nama_periode, status_aktif, " &
                              "(CURDATE() BETWEEN tanggal_mulai AND tanggal_selesai) AS dalam_rentang " &
                              "FROM periode_edom " &
                              "ORDER BY status_aktif DESC, tahun_ajaran DESC, semester DESC LIMIT 1"

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then
                    SetStatusPeriode("Koneksi terputus", "StatusClosedOnDarkBrush")
                    Exit Sub
                End If

                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim namaPeriode As String = reader("nama_periode").ToString()
                            Dim statusAktif As Boolean = Convert.ToBoolean(reader("status_aktif"))
                            Dim dalamRentang As Boolean = Convert.ToBoolean(reader("dalam_rentang"))

                            If statusAktif AndAlso dalamRentang Then
                                SetStatusPeriode(namaPeriode & " · Dibuka", "StatusOpenOnDarkBrush")
                            ElseIf statusAktif Then
                                SetStatusPeriode(namaPeriode & " · Di Luar Jadwal", "StatusWarningOnDarkBrush")
                            Else
                                SetStatusPeriode(namaPeriode & " · Ditutup", "StatusClosedOnDarkBrush")
                            End If
                        Else
                            SetStatusPeriode("Belum ada periode", "StatusClosedOnDarkBrush")
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            SetStatusPeriode("Gagal dimuat", "StatusClosedOnDarkBrush")
        End Try
    End Sub

    Private Sub SetStatusPeriode(teks As String, resourceKey As String)
        txtPeriodeAktif.Text = teks
        txtPeriodeAktifTop.Text = teks

        Dim warna As Object = Application.Current.TryFindResource(resourceKey)
        If TypeOf warna Is System.Windows.Media.Brush Then
            Dim brushStatus As System.Windows.Media.Brush = DirectCast(warna, System.Windows.Media.Brush)
            indikatorPeriode.Fill = brushStatus
            indikatorPeriodeTop.Fill = brushStatus
        End If
    End Sub

    Private Sub AturMenuBerdasarkanPeran()
        panelNavMahasiswa.Visibility = Visibility.Collapsed
        panelNavDosen.Visibility = Visibility.Collapsed
        panelNavAdmin.Visibility = Visibility.Collapsed

        Select Case UserSession.Role
            Case "admin"
                colSidebar.Width = New GridLength(256)
                rowTopbar.Height = New GridLength(0)
                sidebarPanel.Visibility = Visibility.Visible
                topbarPanel.Visibility = Visibility.Collapsed
                panelNavAdmin.Visibility = Visibility.Visible

            Case "dosen"
                colSidebar.Width = New GridLength(0)
                rowTopbar.Height = New GridLength(58)
                sidebarPanel.Visibility = Visibility.Collapsed
                topbarPanel.Visibility = Visibility.Visible
                panelNavDosen.Visibility = Visibility.Visible

            Case "mahasiswa"
                colSidebar.Width = New GridLength(0)
                rowTopbar.Height = New GridLength(58)
                sidebarPanel.Visibility = Visibility.Collapsed
                topbarPanel.Visibility = Visibility.Visible
                panelNavMahasiswa.Visibility = Visibility.Visible
        End Select
    End Sub

    Public Sub NavigasiKeHalaman(halaman As Page, judul As String, Optional subJudul As String = "", Optional tombolAktif As Button = Nothing)
        txtJudulHalaman.Text = judul
        TandaiMenuAktif(tombolAktif)
        MainFrame.Navigate(halaman)
    End Sub

    Public Sub TandaiMenuAktif(tombolAktif As Button)
        If semuaTombolNav Is Nothing Then Exit Sub

        For Each btn As Button In semuaTombolNav
            If btn IsNot Nothing Then
                btn.Tag = Nothing
            End If
        Next

        If tombolAktif Is btnMenuAkun OrElse tombolAktif Is btnTopMenuAkun Then
            btnMenuAkun.Tag = "Active"
            btnTopMenuAkun.Tag = "Active"
        ElseIf tombolAktif IsNot Nothing Then
            tombolAktif.Tag = "Active"
        End If
    End Sub

    Private Sub TampilkanInfoPengembangan(namaModul As String, folderTarget As String)
        MessageBox.Show("Menu " & namaModul & " belum terpasang di sini (dikerjakan di " & folderTarget & ").", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub BtnMhsDashboard_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New MhsMainView(), "Dashboard Mahasiswa", "Pengelolaan data kelas jurusan", btnMhsDashboard)
        'TampilkanInfoPengembangan("Daftar Mata Kuliah", "Views/Mhs")
    End Sub


    Private Sub BtnDosenMain_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New DosenMainView(), "Capaian Evaluasi", "Ringkasan hasil evaluasi dosen", btnDosenMain)
    End Sub


    Private Sub BtnAdminRekap_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminDashboardView(), "Rekapitulasi", "Evaluasi seluruh dosen dan perhitungan Fuzzy Sugeno", btnAdminRekap)
    End Sub

    Private Sub BtnAdminSetup_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminSetupView(), "Setup EDOM", "", btnAdminSetup)
    End Sub

    Private Sub BtnAdminKelas_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminKelasView(), "Kelas & Mahasiswa", "Klik dua kali kelas untuk kelola anggota mahasiswa dan dosen pengampu", btnAdminKelas)
    End Sub

    Private Sub BtnAdminDosen_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminUserView("dosen"), "Daftar Dosen", "Akun dosen aktif (klik dua kali baris untuk lihat detail evaluasi)", btnAdminDosen)
    End Sub

    Private Sub BtnMenuAkun_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AccountMainView(), "Akun Saya", "Data profil dan pengaturan kata sandi", btnMenuAkun)
    End Sub

    Private Sub MainFrame_Navigated(sender As Object, e As NavigationEventArgs) Handles MainFrame.Navigated
        While MainFrame.CanGoBack
            MainFrame.RemoveBackEntry()
        End While
    End Sub

    Private Sub BtnLogout_Click(sender As Object, e As RoutedEventArgs)
        Dim konfirmasi As MessageBoxResult = MessageBox.Show("Keluar dari akun sekarang?", "Keluar", MessageBoxButton.YesNo, MessageBoxImage.Question)

        If konfirmasi = MessageBoxResult.Yes Then
            UserSession.Clear()
            KembaliKeLogin()
        End If
    End Sub

End Class
