Imports System.ComponentModel
Imports System.Globalization
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Imports MySql.Data.MySqlClient

Public Class AdminSetupView

    Public Class PeriodeItem
        Public Property Id As Integer
        Public Property NamaPeriode As String
        Public Property TahunAjaran As Integer
        Public Property TahunAjaranTeks As String
        Public Property Semester As String
        Public Property TanggalMulai As Date
        Public Property TanggalSelesai As Date
        Public Property TanggalMulaiTeks As String
        Public Property TanggalSelesaiTeks As String
        Public Property StatusAktif As Boolean
        Public Property DalamRentang As Boolean
        Public Property StatusTeks As String
        Public Property WarnaStatus As String
    End Class

    Public Class SoalItem
        Public Property Id As Integer
        Public Property Nomor As Integer
        Public Property Kategori As String
        Public Property LabelKategori As String
        Public Property Pertanyaan As String
    End Class

    Private semuaPeriode As List(Of PeriodeItem)
    Private semuaSoal As List(Of SoalItem)
    Private ReadOnly kulturIndonesia As CultureInfo = CultureInfo.GetCultureInfo("id-ID")

    Private Sub AdminSetupView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        MuatDaftarPeriode()
        MuatDaftarSoal()
        PerbaruiTampilanTab(True)
    End Sub

    Private Sub btnTabPeriode_Click(sender As Object, e As RoutedEventArgs)
        PerbaruiTampilanTab(True)
    End Sub

    Private Sub btnTabSoal_Click(sender As Object, e As RoutedEventArgs)
        PerbaruiTampilanTab(False)
    End Sub

    Private Sub PerbaruiTampilanTab(tampilkanPeriode As Boolean)
        Dim warnaAksen As Brush = TryCast(Application.Current.FindResource("AccentBrush"), Brush)
        Dim warnaSekunder As Brush = TryCast(Application.Current.FindResource("InkSecondaryBrush"), Brush)

        If tampilkanPeriode Then
            panelPeriode.Visibility = Visibility.Visible
            panelSoal.Visibility = Visibility.Collapsed
            garisTabPeriode.BorderBrush = warnaAksen
            btnTabPeriode.Foreground = warnaAksen
            btnTabPeriode.FontWeight = FontWeights.SemiBold
            garisTabSoal.BorderBrush = Brushes.Transparent
            btnTabSoal.Foreground = warnaSekunder
            btnTabSoal.FontWeight = FontWeights.Normal
        Else
            panelPeriode.Visibility = Visibility.Collapsed
            panelSoal.Visibility = Visibility.Visible
            garisTabPeriode.BorderBrush = Brushes.Transparent
            btnTabPeriode.Foreground = warnaSekunder
            btnTabPeriode.FontWeight = FontWeights.Normal
            garisTabSoal.BorderBrush = warnaAksen
            btnTabSoal.Foreground = warnaAksen
            btnTabSoal.FontWeight = FontWeights.SemiBold
        End If
    End Sub

    Private Sub MuatDaftarPeriode()
        semuaPeriode = New List(Of PeriodeItem)()

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim query As String = "SELECT id, nama_periode, tahun_ajaran, semester, tanggal_mulai, tanggal_selesai, status_aktif, " &
                                      "(CURDATE() BETWEEN tanggal_mulai AND tanggal_selesai) AS dalam_rentang " &
                                      "FROM periode_edom " &
                                      "ORDER BY status_aktif DESC, tahun_ajaran DESC, FIELD(semester, 'Genap', 'Ganjil'), id DESC"

                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim tahunAjaran As Integer = Convert.ToInt32(reader("tahun_ajaran"))
                            Dim tglMulai As Date = Convert.ToDateTime(reader("tanggal_mulai")).Date
                            Dim tglSelesai As Date = Convert.ToDateTime(reader("tanggal_selesai")).Date
                            Dim aktif As Boolean = Convert.ToBoolean(reader("status_aktif"))
                            Dim dalamRentang As Boolean = Convert.ToBoolean(reader("dalam_rentang"))

                            Dim teksStatus As String
                            Dim warnaStatus As String

                            If aktif AndAlso dalamRentang Then
                                teksStatus = "Aktif (Jadwal Berjalan)"
                                warnaStatus = "#15803D"
                            ElseIf aktif Then
                                teksStatus = "Aktif (Di Luar Jadwal)"
                                warnaStatus = "#B45309"
                            Else
                                teksStatus = "Ditutup"
                                warnaStatus = "#64748B"
                            End If

                            semuaPeriode.Add(New PeriodeItem With {
                                .Id = Convert.ToInt32(reader("id")),
                                .NamaPeriode = Convert.ToString(reader("nama_periode")),
                                .TahunAjaran = tahunAjaran,
                                .TahunAjaranTeks = $"{tahunAjaran}/{tahunAjaran + 1}",
                                .Semester = Convert.ToString(reader("semester")),
                                .TanggalMulai = tglMulai,
                                .TanggalSelesai = tglSelesai,
                                .TanggalMulaiTeks = tglMulai.ToString("dd MMM yyyy", kulturIndonesia),
                                .TanggalSelesaiTeks = tglSelesai.ToString("dd MMM yyyy", kulturIndonesia),
                                .StatusAktif = aktif,
                                .DalamRentang = dalamRentang,
                                .StatusTeks = teksStatus,
                                .WarnaStatus = warnaStatus
                            })
                        End While
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal memuat daftar periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Using

        PerbaruiRingkasanBannerPeriode()
        TerapkanFilterPeriode()
        PerbaruiTombolSesi()
    End Sub

    Private Sub PerbaruiRingkasanBannerPeriode()
        If semuaPeriode Is Nothing Then Exit Sub

        Dim periodeAktif As PeriodeItem = semuaPeriode.FirstOrDefault(Function(p) p.StatusAktif)
        Dim pengubahWarna As New BrushConverter()

        If periodeAktif Is Nothing Then
            txtNamaPeriodeAktif.Text = "Belum ada periode aktif"
            txtRentangAktif.Text = "-"
            txtStatusBanner.Text = "Semua sesi ditutup"
            Dim kuasTutup As Brush = DirectCast(pengubahWarna.ConvertFromString("#64748B"), Brush)
            dotStatusBanner.Fill = kuasTutup
            txtStatusBanner.Foreground = kuasTutup
        Else
            txtNamaPeriodeAktif.Text = periodeAktif.NamaPeriode
            txtRentangAktif.Text = $"{periodeAktif.TanggalMulaiTeks} - {periodeAktif.TanggalSelesaiTeks}"

            If periodeAktif.DalamRentang Then
                txtStatusBanner.Text = "Sesi Terbuka"
                Dim kuasBuka As Brush = DirectCast(pengubahWarna.ConvertFromString("#15803D"), Brush)
                dotStatusBanner.Fill = kuasBuka
                txtStatusBanner.Foreground = kuasBuka
            Else
                txtStatusBanner.Text = "Aktif di luar rentang tanggal"
                Dim kuasJeda As Brush = DirectCast(pengubahWarna.ConvertFromString("#B45309"), Brush)
                dotStatusBanner.Fill = kuasJeda
                txtStatusBanner.Foreground = kuasJeda
            End If
        End If
    End Sub

    Private Sub TerapkanFilterPeriode()
        If semuaPeriode Is Nothing OrElse dgPeriode Is Nothing Then Exit Sub

        Dim kataKunci As String = txtCariPeriode.Text.Trim().ToLower()
        txtPlaceholderPeriode.Visibility = If(String.IsNullOrEmpty(txtCariPeriode.Text), Visibility.Visible, Visibility.Collapsed)

        Dim hasil As List(Of PeriodeItem) = semuaPeriode.Where(Function(p)
                                                                   If String.IsNullOrEmpty(kataKunci) Then Return True
                                                                   Return p.NamaPeriode.ToLower().Contains(kataKunci) OrElse
                                                                          p.TahunAjaranTeks.ToLower().Contains(kataKunci) OrElse
                                                                          p.TahunAjaran.ToString().Contains(kataKunci) OrElse
                                                                          p.Semester.ToLower().Contains(kataKunci) OrElse
                                                                          p.StatusTeks.ToLower().Contains(kataKunci)
                                                               End Function).ToList()

        dgPeriode.ItemsSource = hasil
        txtRingkasanPeriode.Text = $"Menampilkan {hasil.Count} dari {semuaPeriode.Count} periode"
    End Sub

    Private Sub txtCariPeriode_TextChanged(sender As Object, e As TextChangedEventArgs)
        TerapkanFilterPeriode()
    End Sub

    Private Sub dgPeriode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        PerbaruiTombolSesi()
    End Sub

    Private Sub PerbaruiTombolSesi()
        If btnToggleSesi Is Nothing Then Exit Sub

        Dim terpilih As PeriodeItem = TryCast(dgPeriode.SelectedItem, PeriodeItem)
        If terpilih Is Nothing Then
            btnToggleSesi.Content = "Buka Sesi"
        ElseIf terpilih.StatusAktif Then
            btnToggleSesi.Content = "Tutup Sesi"
        Else
            btnToggleSesi.Content = "Buka Sesi"
        End If
    End Sub

    Private Sub btnTambahPeriode_Click(sender As Object, e As RoutedEventArgs)
        Dim dialog As New PeriodeFormWindow()
        dialog.Owner = Window.GetWindow(Me)

        If dialog.ShowDialog() <> True Then Exit Sub

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim queryCek As String = "SELECT COUNT(*) FROM periode_edom WHERE tahun_ajaran = @ta AND semester = @sem"
                Using cmdCek As New MySqlCommand(queryCek, conn)
                    cmdCek.Parameters.AddWithValue("@ta", dialog.TahunAjaran)
                    cmdCek.Parameters.AddWithValue("@sem", dialog.Semester)

                    If Convert.ToInt32(cmdCek.ExecuteScalar()) > 0 Then
                        MessageBox.Show($"Periode {dialog.NamaPeriode} sudah terdaftar.", "Duplikasi Data", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Exit Sub
                    End If
                End Using

                Dim queryInsert As String = "INSERT INTO periode_edom (nama_periode, tahun_ajaran, semester, tanggal_mulai, tanggal_selesai, status_aktif) " &
                                            "VALUES (@nama, @ta, @sem, @mulai, @selesai, FALSE)"

                Using cmdInsert As New MySqlCommand(queryInsert, conn)
                    cmdInsert.Parameters.AddWithValue("@nama", dialog.NamaPeriode)
                    cmdInsert.Parameters.AddWithValue("@ta", dialog.TahunAjaran)
                    cmdInsert.Parameters.AddWithValue("@sem", dialog.Semester)
                    cmdInsert.Parameters.AddWithValue("@mulai", dialog.TanggalMulai.ToString("yyyy-MM-dd"))
                    cmdInsert.Parameters.AddWithValue("@selesai", dialog.TanggalSelesai.ToString("yyyy-MM-dd"))
                    cmdInsert.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal menambahkan periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
        End Using

        MuatDaftarPeriode()
        SinkronkanHeaderPeriode()
    End Sub

    Private Sub btnEditPeriode_Click(sender As Object, e As RoutedEventArgs)
        EditPeriodeTerpilih()
    End Sub

    Private Sub dgPeriode_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If Not ApakahKlikBaris(e.OriginalSource) Then Exit Sub
        EditPeriodeTerpilih()
    End Sub

    Private Sub EditPeriodeTerpilih()
        Dim terpilih As PeriodeItem = TryCast(dgPeriode.SelectedItem, PeriodeItem)
        If terpilih Is Nothing Then
            MessageBox.Show("Pilih baris periode yang ingin diubah terlebih dahulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim dialog As New PeriodeFormWindow()
        dialog.Owner = Window.GetWindow(Me)
        dialog.SetData(terpilih.TahunAjaran, terpilih.Semester, terpilih.TanggalMulai, terpilih.TanggalSelesai)

        If dialog.ShowDialog() <> True Then Exit Sub

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim queryCek As String = "SELECT COUNT(*) FROM periode_edom WHERE tahun_ajaran = @ta AND semester = @sem AND id <> @id"
                Using cmdCek As New MySqlCommand(queryCek, conn)
                    cmdCek.Parameters.AddWithValue("@ta", dialog.TahunAjaran)
                    cmdCek.Parameters.AddWithValue("@sem", dialog.Semester)
                    cmdCek.Parameters.AddWithValue("@id", terpilih.Id)

                    If Convert.ToInt32(cmdCek.ExecuteScalar()) > 0 Then
                        MessageBox.Show($"Periode {dialog.NamaPeriode} sudah terdaftar.", "Duplikasi Data", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Exit Sub
                    End If
                End Using

                Dim queryUpdate As String = "UPDATE periode_edom " &
                                            "SET nama_periode = @nama, tahun_ajaran = @ta, semester = @sem, tanggal_mulai = @mulai, tanggal_selesai = @selesai " &
                                            "WHERE id = @id"

                Using cmdUpdate As New MySqlCommand(queryUpdate, conn)
                    cmdUpdate.Parameters.AddWithValue("@nama", dialog.NamaPeriode)
                    cmdUpdate.Parameters.AddWithValue("@ta", dialog.TahunAjaran)
                    cmdUpdate.Parameters.AddWithValue("@sem", dialog.Semester)
                    cmdUpdate.Parameters.AddWithValue("@mulai", dialog.TanggalMulai.ToString("yyyy-MM-dd"))
                    cmdUpdate.Parameters.AddWithValue("@selesai", dialog.TanggalSelesai.ToString("yyyy-MM-dd"))
                    cmdUpdate.Parameters.AddWithValue("@id", terpilih.Id)
                    cmdUpdate.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal memperbarui periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
        End Using

        MuatDaftarPeriode()
        SinkronkanHeaderPeriode()
    End Sub

    Private Sub btnToggleSesi_Click(sender As Object, e As RoutedEventArgs)
        Dim terpilih As PeriodeItem = TryCast(dgPeriode.SelectedItem, PeriodeItem)
        If terpilih Is Nothing Then
            MessageBox.Show("Pilih periode yang ingin dibuka atau ditutup terlebih dahulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If terpilih.StatusAktif Then
            Dim konfirmasiTutup As MessageBoxResult = MessageBox.Show(
                $"Tutup sesi untuk periode '{terpilih.NamaPeriode}'?" & Environment.NewLine & "Mahasiswa tidak dapat mengisi kuesioner selama tidak ada periode aktif.",
                "Konfirmasi Tutup Sesi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)

            If konfirmasiTutup <> MessageBoxResult.Yes Then Exit Sub

            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Try
                    Using cmd As New MySqlCommand("UPDATE periode_edom SET status_aktif = FALSE WHERE id = @id", conn)
                        cmd.Parameters.AddWithValue("@id", terpilih.Id)
                        cmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Gagal menutup sesi: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                    Exit Sub
                End Try
            End Using
        Else
            Dim konfirmasiBuka As MessageBoxResult = MessageBox.Show(
                $"Aktifkan sesi untuk periode '{terpilih.NamaPeriode}'?" & Environment.NewLine & "Periode lain yang sedang aktif akan otomatis ditutup.",
                "Konfirmasi Buka Sesi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)

            If konfirmasiBuka <> MessageBoxResult.Yes Then Exit Sub

            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim transaksi As MySqlTransaction = Nothing
                Try
                    transaksi = conn.BeginTransaction()

                    Using cmdReset As New MySqlCommand("UPDATE periode_edom SET status_aktif = FALSE", conn, transaksi)
                        cmdReset.ExecuteNonQuery()
                    End Using

                    Using cmdAktif As New MySqlCommand("UPDATE periode_edom SET status_aktif = TRUE WHERE id = @id", conn, transaksi)
                        cmdAktif.Parameters.AddWithValue("@id", terpilih.Id)
                        cmdAktif.ExecuteNonQuery()
                    End Using

                    transaksi.Commit()

                Catch ex As Exception
                    If transaksi IsNot Nothing Then
                        Try
                            transaksi.Rollback()
                        Catch
                        End Try
                    End If

                    MessageBox.Show("Gagal mengaktifkan periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                    Exit Sub
                End Try
            End Using
        End If

        MuatDaftarPeriode()
        SinkronkanHeaderPeriode()
    End Sub

    Private Sub btnHapusPeriode_Click(sender As Object, e As RoutedEventArgs)
        Dim terpilih As PeriodeItem = TryCast(dgPeriode.SelectedItem, PeriodeItem)
        If terpilih Is Nothing Then
            MessageBox.Show("Pilih baris periode yang ingin dihapus terlebih dahulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If terpilih.StatusAktif Then
            MessageBox.Show("Periode yang sedang aktif tidak dapat dihapus. Tutup sesi terlebih dahulu.", "Penghapusan Ditolak", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim jumlahPengampu As Integer
                Using cmdPengampu As New MySqlCommand("SELECT COUNT(*) FROM pengampu WHERE id_periode = @id", conn)
                    cmdPengampu.Parameters.AddWithValue("@id", terpilih.Id)
                    jumlahPengampu = Convert.ToInt32(cmdPengampu.ExecuteScalar())
                End Using

                Dim jumlahHasil As Integer
                Using cmdHasil As New MySqlCommand("SELECT COUNT(*) FROM hasil_fuzzy_dosen WHERE id_periode = @id", conn)
                    cmdHasil.Parameters.AddWithValue("@id", terpilih.Id)
                    jumlahHasil = Convert.ToInt32(cmdHasil.ExecuteScalar())
                End Using

                If jumlahPengampu > 0 OrElse jumlahHasil > 0 Then
                    MessageBox.Show(
                        $"Periode '{terpilih.NamaPeriode}' tidak dapat dihapus karena terhubung dengan {jumlahPengampu} penugasan pengampu dan {jumlahHasil} data hasil evaluasi.",
                        "Penghapusan Ditolak",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)
                    Exit Sub
                End If

                Dim konfirmasi As MessageBoxResult = MessageBox.Show(
                    $"Hapus permanen periode '{terpilih.NamaPeriode}'?",
                    "Konfirmasi Hapus",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning)

                If konfirmasi <> MessageBoxResult.Yes Then Exit Sub

                Using cmdHapus As New MySqlCommand("DELETE FROM periode_edom WHERE id = @id", conn)
                    cmdHapus.Parameters.AddWithValue("@id", terpilih.Id)
                    cmdHapus.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal menghapus periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
        End Using

        MuatDaftarPeriode()
        SinkronkanHeaderPeriode()
    End Sub

    Private Sub MuatDaftarSoal()
        semuaSoal = New List(Of SoalItem)()

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim query As String = "SELECT id, kategori, pertanyaan " &
                                      "FROM pertanyaan " &
                                      "WHERE aktif = TRUE " &
                                      "ORDER BY FIELD(kategori, 'pedagogik', 'profesional', 'kepribadian', 'sosial'), id ASC"

                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        Dim nomorUrut As Integer = 1
                        While reader.Read()
                            Dim kodeKategori As String = Convert.ToString(reader("kategori"))
                            semuaSoal.Add(New SoalItem With {
                                .Id = Convert.ToInt32(reader("id")),
                                .Nomor = nomorUrut,
                                .Kategori = kodeKategori,
                                .LabelKategori = FormatLabelKategori(kodeKategori),
                                .Pertanyaan = Convert.ToString(reader("pertanyaan"))
                            })
                            nomorUrut += 1
                        End While
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal memuat daftar pertanyaan: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Using

        If txtJumlahSoalRingkas IsNot Nothing Then
            txtJumlahSoalRingkas.Text = $"{semuaSoal.Count} pertanyaan aktif"
        End If

        TerapkanFilterSoal()
    End Sub

    Private Function FormatLabelKategori(kodeKategori As String) As String
        Select Case kodeKategori.Trim().ToLower()
            Case "pedagogik"
                Return "Kompetensi Pedagogik"
            Case "profesional"
                Return "Kompetensi Profesional"
            Case "kepribadian"
                Return "Kompetensi Kepribadian"
            Case "sosial"
                Return "Kompetensi Sosial"
            Case Else
                Return kodeKategori
        End Select
    End Function

    Private Sub TerapkanFilterSoal()
        If semuaSoal Is Nothing OrElse dgSoal Is Nothing Then Exit Sub

        Dim kataKunci As String = txtCariSoal.Text.Trim().ToLower()
        txtPlaceholderSoal.Visibility = If(String.IsNullOrEmpty(txtCariSoal.Text), Visibility.Visible, Visibility.Collapsed)

        Dim filterKategori As String = "semua"
        Dim itemKategori As ComboBoxItem = TryCast(cmbFilterKategori.SelectedItem, ComboBoxItem)
        If itemKategori IsNot Nothing AndAlso itemKategori.Tag IsNot Nothing Then
            filterKategori = itemKategori.Tag.ToString().ToLower()
        End If

        Dim hasil As List(Of SoalItem) = semuaSoal.Where(Function(s)
                                                             Dim cocokKategori As Boolean = (filterKategori = "semua") OrElse
                                                                                            String.Equals(s.Kategori, filterKategori, StringComparison.OrdinalIgnoreCase)

                                                             If Not cocokKategori Then Return False
                                                             If String.IsNullOrEmpty(kataKunci) Then Return True

                                                             Return s.Pertanyaan.ToLower().Contains(kataKunci) OrElse
                                                                    s.LabelKategori.ToLower().Contains(kataKunci)
                                                         End Function).ToList()

        Dim tampilanGrup As ICollectionView = CollectionViewSource.GetDefaultView(hasil)
        If tampilanGrup IsNot Nothing Then
            tampilanGrup.GroupDescriptions.Clear()
            tampilanGrup.GroupDescriptions.Add(New PropertyGroupDescription("LabelKategori"))
        End If

        dgSoal.ItemsSource = tampilanGrup
        txtRingkasanSoal.Text = $"Menampilkan {hasil.Count} dari {semuaSoal.Count} butir pertanyaan aktif"
    End Sub

    Private Sub txtCariSoal_TextChanged(sender As Object, e As TextChangedEventArgs)
        TerapkanFilterSoal()
    End Sub

    Private Sub cmbFilterKategori_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        TerapkanFilterSoal()
    End Sub

    Private Sub btnTambahSoal_Click(sender As Object, e As RoutedEventArgs)
        Dim dialog As New PertanyaanFormWindow()
        dialog.Owner = Window.GetWindow(Me)

        Dim itemFilter As ComboBoxItem = TryCast(cmbFilterKategori.SelectedItem, ComboBoxItem)
        If itemFilter IsNot Nothing AndAlso itemFilter.Tag IsNot Nothing Then
            Dim kodeKategori As String = itemFilter.Tag.ToString()
            If kodeKategori <> "semua" Then
                dialog.PresetKategori(kodeKategori)
            End If
        End If

        If dialog.ShowDialog() <> True Then Exit Sub

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim queryInsert As String = "INSERT INTO pertanyaan (kategori, pertanyaan, aktif) VALUES (@kategori, @pertanyaan, TRUE)"
                Using cmd As New MySqlCommand(queryInsert, conn)
                    cmd.Parameters.AddWithValue("@kategori", dialog.Kategori)
                    cmd.Parameters.AddWithValue("@pertanyaan", dialog.TeksPertanyaan)
                    cmd.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal menambahkan pertanyaan: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
        End Using

        MuatDaftarSoal()
    End Sub

    Private Sub btnEditSoal_Click(sender As Object, e As RoutedEventArgs)
        EditSoalTerpilih()
    End Sub

    Private Sub dgSoal_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If Not ApakahKlikBaris(e.OriginalSource) Then Exit Sub
        EditSoalTerpilih()
    End Sub

    Private Sub EditSoalTerpilih()
        Dim terpilih As SoalItem = TryCast(dgSoal.SelectedItem, SoalItem)
        If terpilih Is Nothing Then
            MessageBox.Show("Pilih butir pertanyaan yang ingin diubah terlebih dahulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim dialog As New PertanyaanFormWindow()
        dialog.Owner = Window.GetWindow(Me)
        dialog.SetData(terpilih.Kategori, terpilih.Pertanyaan)

        If dialog.ShowDialog() <> True Then Exit Sub

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim queryUpdate As String = "UPDATE pertanyaan SET kategori = @kategori, pertanyaan = @pertanyaan WHERE id = @id"
                Using cmd As New MySqlCommand(queryUpdate, conn)
                    cmd.Parameters.AddWithValue("@kategori", dialog.Kategori)
                    cmd.Parameters.AddWithValue("@pertanyaan", dialog.TeksPertanyaan)
                    cmd.Parameters.AddWithValue("@id", terpilih.Id)
                    cmd.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal memperbarui pertanyaan: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
        End Using

        MuatDaftarSoal()
    End Sub

    Private Sub btnHapusSoal_Click(sender As Object, e As RoutedEventArgs)
        Dim terpilih As SoalItem = TryCast(dgSoal.SelectedItem, SoalItem)
        If terpilih Is Nothing Then
            MessageBox.Show("Pilih butir pertanyaan yang ingin dihapus terlebih dahulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Using conn As MySqlConnection = db.GetConnection()
            If conn Is Nothing Then Exit Sub

            Try
                Dim queryCek As String = "SELECT COUNT(*) FROM pertanyaan WHERE kategori = @kategori AND aktif = TRUE"
                Using cmdCek As New MySqlCommand(queryCek, conn)
                    cmdCek.Parameters.AddWithValue("@kategori", terpilih.Kategori)
                    Dim sisaAktif As Integer = Convert.ToInt32(cmdCek.ExecuteScalar())

                    If sisaAktif <= 1 Then
                        MessageBox.Show(
                            $"Penghapusan dibatalkan. Setiap kategori wajib memiliki minimal 1 pertanyaan aktif ({terpilih.LabelKategori}).",
                            "Penghapusan Ditolak",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning)
                        Exit Sub
                    End If
                End Using

                Dim konfirmasi As MessageBoxResult = MessageBox.Show(
                    "Nonaktifkan butir pertanyaan berikut?" & Environment.NewLine & Environment.NewLine & terpilih.Pertanyaan,
                    "Konfirmasi Hapus",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question)

                If konfirmasi <> MessageBoxResult.Yes Then Exit Sub

                Using cmdNonaktif As New MySqlCommand("UPDATE pertanyaan SET aktif = FALSE WHERE id = @id", conn)
                    cmdNonaktif.Parameters.AddWithValue("@id", terpilih.Id)
                    cmdNonaktif.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                MessageBox.Show("Gagal menonaktifkan pertanyaan: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
        End Using

        MuatDaftarSoal()
    End Sub

    Private Sub SinkronkanHeaderPeriode()
        Dim jendelaUtama As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If jendelaUtama IsNot Nothing Then
            jendelaUtama.MuatPeriodeAktif()
        End If
    End Sub

    Private Function ApakahKlikBaris(sumber As Object) As Boolean
        Dim elemen As DependencyObject = TryCast(sumber, DependencyObject)
        While elemen IsNot Nothing AndAlso Not TypeOf elemen Is DataGridRow
            If TypeOf elemen Is Visual OrElse TypeOf elemen Is Visual3D Then
                elemen = VisualTreeHelper.GetParent(elemen)
            Else
                elemen = LogicalTreeHelper.GetParent(elemen)
            End If
        End While

        Return TypeOf elemen Is DataGridRow
    End Function

End Class
