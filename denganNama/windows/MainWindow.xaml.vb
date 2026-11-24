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
            btnMhsQuestion,
            btnDosenMain,
            btnDosenDetail,
            btnAdminRekap,
            btnAdminSetup,
            btnAdminKelas,
            btnAdminDosen,
            btnAdminMahasiswa,
            btnMenuAkun
        }

        MuatIdentitasHeader()
        MuatPeriodeAktif()
        AturMenuBerdasarkanPeran()
        BukaHalamanAwal()
    End Sub

    Private Sub BukaHalamanAwal()
        Select Case UserSession.Role
            Case "admin"
                NavigasiKeHalaman(New AdminDashboardView(), "Dashboard Admin", "Ringkasan administrasi dan statistik EDOM", Nothing)

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
        txtNamaUser.Text = If(Not String.IsNullOrEmpty(UserSession.Nama), UserSession.Nama, UserSession.Username)

        Select Case UserSession.Role
            Case "admin"
                txtRoleUser.Text = "Admin Jurusan"
            Case "dosen"
                txtRoleUser.Text = "Dosen"
            Case "mahasiswa"
                txtRoleUser.Text = "Mahasiswa"
            Case Else
                txtRoleUser.Text = UserSession.Role
        End Select
    End Sub

    Private Sub MuatPeriodeAktif()
        Dim query As String = "SELECT nama_periode, status_aktif, " &
                              "(CURDATE() BETWEEN tanggal_mulai AND tanggal_selesai) AS dalam_rentang " &
                              "FROM periode_edom " &
                              "ORDER BY status_aktif DESC, tahun_ajaran DESC, semester DESC LIMIT 1"

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then
                    SetStatusPeriode("Periode: Koneksi terputus", "StatusClosedBrush")
                    Exit Sub
                End If

                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim namaPeriode As String = reader("nama_periode").ToString()
                            Dim statusAktif As Boolean = Convert.ToBoolean(reader("status_aktif"))
                            Dim dalamRentang As Boolean = Convert.ToBoolean(reader("dalam_rentang"))

                            If statusAktif AndAlso dalamRentang Then
                                SetStatusPeriode("Periode " & namaPeriode & " (Dibuka)", "StatusOpenBrush")
                            ElseIf statusAktif Then
                                SetStatusPeriode("Periode " & namaPeriode & " (Di Luar Jadwal)", "StatusClosedBrush")
                            Else
                                SetStatusPeriode("Periode " & namaPeriode & " (Ditutup)", "StatusClosedBrush")
                            End If
                        Else
                            SetStatusPeriode("Belum ada periode terdaftar", "StatusClosedBrush")
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            SetStatusPeriode("Periode gagal dimuat", "StatusClosedBrush")
        End Try
    End Sub

    Private Sub SetStatusPeriode(teks As String, resourceKey As String)
        txtPeriodeAktif.Text = teks

        Dim warna As Object = Application.Current.TryFindResource(resourceKey)
        If TypeOf warna Is System.Windows.Media.Brush Then
            indikatorPeriode.Fill = DirectCast(warna, System.Windows.Media.Brush)
        End If
    End Sub

    Private Sub AturMenuBerdasarkanPeran()
        panelNavMahasiswa.Visibility = Visibility.Collapsed
        panelNavDosen.Visibility = Visibility.Collapsed
        panelNavAdmin.Visibility = Visibility.Collapsed

        Select Case UserSession.Role
            Case "admin"
                panelNavAdmin.Visibility = Visibility.Visible
            Case "dosen"
                panelNavDosen.Visibility = Visibility.Visible
            Case "mahasiswa"
                panelNavMahasiswa.Visibility = Visibility.Visible
        End Select
    End Sub

    Public Sub NavigasiKeHalaman(halaman As Page, judul As String, subJudul As String, tombolAktif As Button)
        txtJudulHalaman.Text = judul
        txtSubJudulHalaman.Text = subJudul
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

        If tombolAktif IsNot Nothing Then
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

    Private Sub BtnMhsQuestion_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New MhsQuestionView(), "Kuesioner Mahasiswa", "Kuesioner Mahasiswa Evaluasi Dosen Mengajar", btnMhsQuestion)
        'TampilkanInfoPengembangan("Kuesioner", "Views/Mhs")
    End Sub

    Private Sub BtnDosenMain_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New DosenMainView(), "Capaian Evaluasi", "Ringkasan hasil evaluasi dosen", btnDosenMain)
    End Sub

    Private Sub BtnDosenDetail_Click(sender As Object, e As RoutedEventArgs)
        TampilkanInfoPengembangan("Rincian Kelas", "Views/Dosen")
    End Sub

    Private Sub BtnAdminRekap_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminDashboardView(), "Rekapitulasi & Dashboard", "Ringkasan administrasi dan statistik EDOM", btnAdminRekap)
    End Sub

    Private Sub BtnAdminSetup_Click(sender As Object, e As RoutedEventArgs)
        TampilkanInfoPengembangan("Setup EDOM", "Views/Admin")
    End Sub

    Private Sub BtnAdminKelas_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminKelasView(), "Daftar Kelas", "Pengelolaan data kelas jurusan", btnAdminKelas)
    End Sub

    Private Sub BtnAdminDosen_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminUserView(), "Daftar Dosen", "Pengelolaan data akun dosen dan pengguna", btnAdminDosen)
    End Sub

    Private Sub BtnAdminMahasiswa_Click(sender As Object, e As RoutedEventArgs)
        NavigasiKeHalaman(New AdminUserView(), "Daftar Mahasiswa", "Pengelolaan data akun mahasiswa dan pengguna", btnAdminMahasiswa)
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
