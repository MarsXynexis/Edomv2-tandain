Imports System.Data
Imports MySql.Data.MySqlClient

Public Class DetailKelasView

    Private Class PilihanItem
        Public Property Id As Integer
        Public Property Teks As String

        Public Sub New(idItem As Integer, teksItem As String)
            Id = idItem
            Teks = teksItem
        End Sub

        Public Overrides Function ToString() As String
            Return Teks
        End Function
    End Class

    Private ReadOnly idKelasTarget As Integer

    Private ReadOnly prodiTarget As String

    Private ReadOnly angkatanTarget As String

    Private ReadOnly rombelTarget As String

    Private dtMahasiswa As DataTable

    Private dtPengampu As DataTable

    Private sedangFilterPeriode As Boolean = False

    Public Sub New(idKelas As Integer, prodi As String, angkatan As String, rombel As String)
        InitializeComponent()
        idKelasTarget = idKelas
        prodiTarget = prodi
        angkatanTarget = angkatan
        rombelTarget = rombel
    End Sub

    Private Sub DetailKelasView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        txtProdiHeader.Text = prodiTarget

        MuatPilihanDropdown()
        MuatMahasiswaKelas()
        MuatPengampuKelas()
    End Sub

    Private Sub btnTabMahasiswa_Click(sender As Object, e As RoutedEventArgs)
        btnTabMahasiswa.Tag = "Active"
        btnTabPengampu.Tag = Nothing
        panelMahasiswa.Visibility = Visibility.Visible
        panelPengampu.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnTabPengampu_Click(sender As Object, e As RoutedEventArgs)
        btnTabPengampu.Tag = "Active"
        btnTabMahasiswa.Tag = Nothing
        panelPengampu.Visibility = Visibility.Visible
        panelMahasiswa.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnKembali_Click(sender As Object, e As RoutedEventArgs)
        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then mainWin = TryCast(Application.Current.MainWindow, MainWindow)

        If mainWin IsNot Nothing Then
            mainWin.NavigasiKeHalaman(
                New AdminKelasView(),
                "Kelas & Mahasiswa",
                "Klik dua kali kelas untuk kelola anggota mahasiswa dan dosen pengampu",
                mainWin.btnAdminKelas
            )
        End If
    End Sub

    Private Sub MuatPilihanDropdown()
        sedangFilterPeriode = True
        cmbMatkul.Items.Clear()
        cmbDosen.Items.Clear()
        cmbPeriodeInput.Items.Clear()
        cmbFilterPeriode.Items.Clear()
        cmbFilterPeriode.Items.Add("Semua Periode")

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim qMatkul As String = "SELECT id, kode_matkul, nama_matkul, sks FROM matkul WHERE aktif = TRUE ORDER BY nama_matkul ASC"
                Using cmdM As New MySqlCommand(qMatkul, conn)
                    Using rd As MySqlDataReader = cmdM.ExecuteReader()
                        While rd.Read()
                            Dim teks As String = rd("nama_matkul").ToString() & " (" & rd("kode_matkul").ToString() & " · " & rd("sks").ToString() & " SKS)"
                            cmbMatkul.Items.Add(New PilihanItem(Convert.ToInt32(rd("id")), teks))
                        End While
                    End Using
                End Using

                Dim qDosen As String = "SELECT id, nama FROM users WHERE role = 'dosen' AND aktif = TRUE ORDER BY nama ASC"
                Using cmdD As New MySqlCommand(qDosen, conn)
                    Using rd As MySqlDataReader = cmdD.ExecuteReader()
                        While rd.Read()
                            cmbDosen.Items.Add(New PilihanItem(Convert.ToInt32(rd("id")), rd("nama").ToString()))
                        End While
                    End Using
                End Using

                Dim qPeriode As String = "SELECT id, nama_periode, status_aktif FROM periode_edom ORDER BY status_aktif DESC, tahun_ajaran DESC, semester DESC"
                Using cmdP As New MySqlCommand(qPeriode, conn)
                    Using rd As MySqlDataReader = cmdP.ExecuteReader()
                        While rd.Read()
                            Dim namaP As String = rd("nama_periode").ToString()
                            cmbPeriodeInput.Items.Add(New PilihanItem(Convert.ToInt32(rd("id")), namaP))
                            cmbFilterPeriode.Items.Add(namaP)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat pilihan penugasan: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            sedangFilterPeriode = False
        End Try

        If cmbMatkul.Items.Count > 0 Then cmbMatkul.SelectedIndex = 0
        If cmbDosen.Items.Count > 0 Then cmbDosen.SelectedIndex = 0
        If cmbPeriodeInput.Items.Count > 0 Then cmbPeriodeInput.SelectedIndex = 0
        cmbFilterPeriode.SelectedIndex = 0
    End Sub

    Private Sub PerbaruiRingkasanHeader()
        Dim jmlMhs As Integer = If(dtMahasiswa IsNot Nothing, dtMahasiswa.Rows.Count, 0)
        Dim jmlPengampu As Integer = If(dtPengampu IsNot Nothing, dtPengampu.Rows.Count, 0)
        txtRingkasanKelas.Text = jmlMhs.ToString() & " mahasiswa · " & jmlPengampu.ToString() & " pengampu"
    End Sub

    Private Sub MuatMahasiswaKelas()
        dtMahasiswa = New DataTable()
        dtMahasiswa.Columns.Add("id", GetType(Integer))
        dtMahasiswa.Columns.Add("username", GetType(String))
        dtMahasiswa.Columns.Add("nama", GetType(String))
        dtMahasiswa.Columns.Add("aktif", GetType(Boolean))
        dtMahasiswa.Columns.Add("status_label", GetType(String))
        dtMahasiswa.Columns.Add("status_color", GetType(String))

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT id, username, nama, aktif FROM users " &
                                      "WHERE role = 'mahasiswa' AND id_kelas = @idKelas " &
                                      "ORDER BY aktif DESC, nama ASC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idKelas", idKelasTarget)

                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim aktif As Boolean = Convert.ToBoolean(rd("aktif"))
                            dtMahasiswa.Rows.Add(
                                Convert.ToInt32(rd("id")),
                                rd("username").ToString(),
                                rd("nama").ToString(),
                                aktif,
                                If(aktif, "Aktif", "Nonaktif"),
                                If(aktif, "#15803D", "#B91C1C")
                            )
                        End While
                    End Using
                End Using
            End Using

            dgMahasiswaKelas.ItemsSource = dtMahasiswa.DefaultView
            FilterMahasiswa()
            PerbaruiRingkasanHeader()
        Catch ex As Exception
            MessageBox.Show("Gagal memuat mahasiswa kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub txtCariMhs_TextChanged(sender As Object, e As TextChangedEventArgs)
        lblPlaceholderCariMhs.Visibility = If(String.IsNullOrEmpty(txtCariMhs.Text), Visibility.Visible, Visibility.Collapsed)
        FilterMahasiswa()
    End Sub

    Private Sub FilterMahasiswa()
        If dtMahasiswa Is Nothing Then Exit Sub

        Dim kata As String = EscapeRowFilter(txtCariMhs.Text.Trim())
        If String.IsNullOrEmpty(kata) Then
            dtMahasiswa.DefaultView.RowFilter = ""
        Else
            dtMahasiswa.DefaultView.RowFilter = String.Format("username LIKE '%{0}%' OR nama LIKE '%{0}%'", kata)
        End If

        Dim tampil As Integer = dtMahasiswa.DefaultView.Count
        txtKosongMhs.Visibility = If(tampil = 0, Visibility.Visible, Visibility.Collapsed)
        txtFooterMhs.Text = "Menampilkan " & tampil.ToString() & " dari " & dtMahasiswa.Rows.Count.ToString() & " mahasiswa di kelas ini"
    End Sub

    Private Sub btnTambahMhs_Click(sender As Object, e As RoutedEventArgs)
        Dim frm As New MahasiswaKelasFormWindow()
        frm.Owner = Window.GetWindow(Me)

        If frm.ShowDialog() <> True Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim idExisting As Object = Nothing
                Using cmdCek As New MySqlCommand("SELECT id FROM users WHERE username = @nim LIMIT 1", conn)
                    cmdCek.Parameters.AddWithValue("@nim", frm.NimValue)
                    idExisting = cmdCek.ExecuteScalar()
                End Using

                If idExisting IsNot Nothing AndAlso Not IsDBNull(idExisting) Then
                    Dim qUpd As String = "UPDATE users SET nama = @nama, id_kelas = @idKelas, role = 'mahasiswa', aktif = TRUE WHERE id = @id"
                    Using cmdUpd As New MySqlCommand(qUpd, conn)
                        cmdUpd.Parameters.AddWithValue("@nama", frm.NamaValue)
                        cmdUpd.Parameters.AddWithValue("@idKelas", idKelasTarget)
                        cmdUpd.Parameters.AddWithValue("@id", Convert.ToInt32(idExisting))
                        cmdUpd.ExecuteNonQuery()
                    End Using
                Else
                    Dim qIns As String = "INSERT INTO users (username, password, nama, role, id_kelas, aktif) VALUES (@nim, @pass, @nama, 'mahasiswa', @idKelas, TRUE)"
                    Using cmdIns As New MySqlCommand(qIns, conn)
                        cmdIns.Parameters.AddWithValue("@nim", frm.NimValue)
                        cmdIns.Parameters.AddWithValue("@pass", PasswordHelper.Sha256Hex(frm.PasswordValue))
                        cmdIns.Parameters.AddWithValue("@nama", frm.NamaValue)
                        cmdIns.Parameters.AddWithValue("@idKelas", idKelasTarget)
                        cmdIns.ExecuteNonQuery()
                    End Using
                End If
            End Using

            MuatMahasiswaKelas()
        Catch ex As Exception
            MessageBox.Show("Gagal menambahkan mahasiswa ke kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnEditMhs_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgMahasiswaKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih baris mahasiswa yang ingin diubah terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idMhs As Integer = Convert.ToInt32(rv("id"))
        Dim frm As New MahasiswaKelasFormWindow()
        frm.Owner = Window.GetWindow(Me)
        frm.SetEditData(rv("username").ToString(), rv("nama").ToString())

        If frm.ShowDialog() <> True Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Using cmd As New MySqlCommand("UPDATE users SET nama = @nama WHERE id = @id", conn)
                    cmd.Parameters.AddWithValue("@nama", frm.NamaValue)
                    cmd.Parameters.AddWithValue("@id", idMhs)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MuatMahasiswaKelas()
        Catch ex As Exception
            MessageBox.Show("Gagal memperbarui data mahasiswa: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnResetSandiMhs_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgMahasiswaKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih mahasiswa yang ingin direset kata sandinya.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idMhs As Integer = Convert.ToInt32(rv("id"))
        Dim namaMhs As String = rv("nama").ToString()
        Dim nimMhs As String = rv("username").ToString()

        Dim konf As MessageBoxResult = MessageBox.Show(
            "Reset kata sandi mahasiswa " & namaMhs & " (" & nimMhs & ") ke 123456?",
            "Reset Kata Sandi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        )

        If konf <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Using cmd As New MySqlCommand("UPDATE users SET password = @pass WHERE id = @id", conn)
                    cmd.Parameters.AddWithValue("@pass", PasswordHelper.Sha256Hex("123456"))
                    cmd.Parameters.AddWithValue("@id", idMhs)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Kata sandi " & namaMhs & " berhasil direset menjadi 123456.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MessageBox.Show("Gagal mereset kata sandi: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnKeluarkanMhs_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgMahasiswaKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih mahasiswa yang ingin dikeluarkan dari kelas.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idMhs As Integer = Convert.ToInt32(rv("id"))
        Dim namaMhs As String = rv("nama").ToString()

        Dim konf As MessageBoxResult = MessageBox.Show(
            "Keluarkan " & namaMhs & " dari kelas " & angkatanTarget & " · " & rombelTarget & "?",
            "Konfirmasi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        )

        If konf <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Using cmd As New MySqlCommand("UPDATE users SET id_kelas = NULL WHERE id = @id", conn)
                    cmd.Parameters.AddWithValue("@id", idMhs)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MuatMahasiswaKelas()
        Catch ex As Exception
            MessageBox.Show("Gagal mengeluarkan mahasiswa dari kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub MuatPengampuKelas()
        dtPengampu = New DataTable()
        dtPengampu.Columns.Add("id", GetType(Integer))
        dtPengampu.Columns.Add("kode_matkul", GetType(String))
        dtPengampu.Columns.Add("nama_matkul", GetType(String))
        dtPengampu.Columns.Add("sks", GetType(Integer))
        dtPengampu.Columns.Add("nama_dosen", GetType(String))
        dtPengampu.Columns.Add("nama_periode", GetType(String))

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT p.id, m.kode_matkul, m.nama_matkul, m.sks, u.nama AS nama_dosen, pe.nama_periode " &
                                      "FROM pengampu p " &
                                      "INNER JOIN matkul m ON p.id_matkul = m.id " &
                                      "INNER JOIN users u ON p.id_dosen = u.id " &
                                      "INNER JOIN periode_edom pe ON p.id_periode = pe.id " &
                                      "WHERE p.id_kelas = @idKelas AND p.aktif = TRUE " &
                                      "ORDER BY pe.tahun_ajaran DESC, pe.semester DESC, m.nama_matkul ASC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idKelas", idKelasTarget)

                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            dtPengampu.Rows.Add(
                                Convert.ToInt32(rd("id")),
                                rd("kode_matkul").ToString(),
                                rd("nama_matkul").ToString(),
                                Convert.ToInt32(rd("sks")),
                                rd("nama_dosen").ToString(),
                                rd("nama_periode").ToString()
                            )
                        End While
                    End Using
                End Using
            End Using

            dgPengampuKelas.ItemsSource = dtPengampu.DefaultView
            TerapkanFilterPeriode()
            PerbaruiRingkasanHeader()
        Catch ex As Exception
            MessageBox.Show("Gagal memuat data pengampu kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub cmbFilterPeriode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangFilterPeriode Then Exit Sub
        TerapkanFilterPeriode()
    End Sub

    Private Sub TerapkanFilterPeriode()
        If dtPengampu Is Nothing OrElse cmbFilterPeriode.SelectedItem Is Nothing Then Exit Sub

        Dim periode As String = cmbFilterPeriode.SelectedItem.ToString()
        If periode = "Semua Periode" Then
            dtPengampu.DefaultView.RowFilter = ""
        Else
            dtPengampu.DefaultView.RowFilter = "nama_periode = '" & EscapeRowFilter(periode) & "'"
        End If

        Dim tampil As Integer = dtPengampu.DefaultView.Count
        txtKosongPengampu.Visibility = If(tampil = 0, Visibility.Visible, Visibility.Collapsed)
        txtFooterPengampu.Text = "Menampilkan " & tampil.ToString() & " dari " & dtPengampu.Rows.Count.ToString() & " penugasan pengampu"
    End Sub

    Private Sub btnTambahMatkul_Click(sender As Object, e As RoutedEventArgs)
        Dim frm As New MatkulFormWindow()
        frm.Owner = Window.GetWindow(Me)

        If frm.ShowDialog() <> True Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "INSERT INTO matkul (kode_matkul, nama_matkul, sks, aktif) VALUES (@kode, @nama, @sks, TRUE) " &
                                      "ON DUPLICATE KEY UPDATE nama_matkul = VALUES(nama_matkul), sks = VALUES(sks), aktif = TRUE"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@kode", frm.KodeValue)
                    cmd.Parameters.AddWithValue("@nama", frm.NamaValue)
                    cmd.Parameters.AddWithValue("@sks", frm.SksValue)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            Dim kodeBaru As String = frm.KodeValue
            MuatPilihanDropdown()

            For i As Integer = 0 To cmbMatkul.Items.Count - 1
                If cmbMatkul.Items(i).ToString().Contains(kodeBaru) Then
                    cmbMatkul.SelectedIndex = i
                    Exit For
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Gagal menambahkan mata kuliah: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnSimpanPengampu_Click(sender As Object, e As RoutedEventArgs)
        Dim itemMatkul As PilihanItem = TryCast(cmbMatkul.SelectedItem, PilihanItem)
        Dim itemDosen As PilihanItem = TryCast(cmbDosen.SelectedItem, PilihanItem)
        Dim itemPeriode As PilihanItem = TryCast(cmbPeriodeInput.SelectedItem, PilihanItem)

        If itemMatkul Is Nothing OrElse itemDosen Is Nothing OrElse itemPeriode Is Nothing Then
            MessageBox.Show("Pilih mata kuliah, dosen pengampu, dan periode semester terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "INSERT INTO pengampu (id_kelas, id_matkul, id_dosen, id_periode, aktif) " &
                                      "VALUES (@idKelas, @idMatkul, @idDosen, @idPeriode, TRUE) " &
                                      "ON DUPLICATE KEY UPDATE aktif = TRUE"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idKelas", idKelasTarget)
                    cmd.Parameters.AddWithValue("@idMatkul", itemMatkul.Id)
                    cmd.Parameters.AddWithValue("@idDosen", itemDosen.Id)
                    cmd.Parameters.AddWithValue("@idPeriode", itemPeriode.Id)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MuatPengampuKelas()
        Catch ex As Exception
            MessageBox.Show("Gagal menetapkan dosen pengampu: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnHapusPengampu_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgPengampuKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih baris penugasan yang ingin dihapus terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idPengampu As Integer = Convert.ToInt32(rv("id"))
        Dim namaMk As String = rv("nama_matkul").ToString()

        Dim konf As MessageBoxResult = MessageBox.Show(
            "Hapus penugasan " & namaMk & " untuk kelas ini?",
            "Konfirmasi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        )

        If konf <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Using cmd As New MySqlCommand("UPDATE pengampu SET aktif = FALSE WHERE id = @id", conn)
                    cmd.Parameters.AddWithValue("@id", idPengampu)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MuatPengampuKelas()
        Catch ex As Exception
            MessageBox.Show("Gagal menghapus penugasan pengampu: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Function EscapeRowFilter(nilai As String) As String
        Return nilai.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
    End Function

End Class
