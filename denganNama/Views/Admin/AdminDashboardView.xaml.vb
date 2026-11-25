Imports System.Data
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class AdminDashboardView

    Private Class PilihanPeriode
        Public Property Id As Integer
        Public Property Teks As String

        Public Sub New(idPeriode As Integer, namaPeriode As String)
            Id = idPeriode
            Teks = namaPeriode
        End Sub

        Public Overrides Function ToString() As String
            Return Teks
        End Function
    End Class

    Private Class RekapDosenItem
        Public Property IdDosen As Integer
        Public Property Nip As String
        Public Property NamaDosen As String
        Public Property JumlahKelas As Integer
        Public Property JumlahResponden As Integer
        Public Property InputFuzzy As InputKategoriFuzzy
        Public Property HasilFuzzy As HasilEvaluasiFuzzy
    End Class

    Private ReadOnly cacheRekap As New List(Of RekapDosenItem)()

    Private dtRekap As DataTable

    Private sedangMuatPeriode As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub AdminDashboardView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        MuatStatistikUmum()
        MuatDaftarPeriode()
    End Sub

    Private Sub MuatStatistikUmum()
        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                txtTotalMahasiswa.Text = HitungSkalar(conn, "SELECT COUNT(*) FROM users WHERE role = 'mahasiswa' AND aktif = TRUE").ToString()
                txtTotalDosen.Text = HitungSkalar(conn, "SELECT COUNT(*) FROM users WHERE role = 'dosen' AND aktif = TRUE").ToString()
                txtTotalKelas.Text = HitungSkalar(conn, "SELECT COUNT(*) FROM kelas WHERE aktif = TRUE").ToString()
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Function HitungSkalar(conn As MySqlConnection, query As String) As Integer
        Using cmd As New MySqlCommand(query, conn)
            Dim hasil As Object = cmd.ExecuteScalar()
            If hasil IsNot Nothing AndAlso Not IsDBNull(hasil) Then
                Return Convert.ToInt32(hasil)
            End If
        End Using

        Return 0
    End Function

    Private Sub MuatDaftarPeriode()
        sedangMuatPeriode = True
        cmbPeriode.Items.Clear()

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT id, nama_periode, status_aktif FROM periode_edom ORDER BY status_aktif DESC, tahun_ajaran DESC, semester DESC"

                Using cmd As New MySqlCommand(query, conn)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        Dim idxAktif As Integer = -1
                        Dim idx As Integer = 0

                        While rd.Read()
                            Dim aktif As Boolean = Convert.ToBoolean(rd("status_aktif"))
                            cmbPeriode.Items.Add(New PilihanPeriode(Convert.ToInt32(rd("id")), rd("nama_periode").ToString()))
                            If aktif AndAlso idxAktif = -1 Then
                                idxAktif = idx
                            End If
                            idx += 1
                        End While

                        If cmbPeriode.Items.Count > 0 Then
                            cmbPeriode.SelectedIndex = If(idxAktif >= 0, idxAktif, 0)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat daftar periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            sedangMuatPeriode = False
        End Try

        MuatRekapitulasi(False)
    End Sub

    Private Sub MuatRekapitulasi(simpanKeDatabase As Boolean)
        cacheRekap.Clear()

        Dim itemPeriode As PilihanPeriode = TryCast(cmbPeriode.SelectedItem, PilihanPeriode)
        If itemPeriode Is Nothing Then
            txtTotalPengisian.Text = "0"
            dgRekapDosen.ItemsSource = Nothing
            txtKosongRekap.Visibility = Visibility.Visible
            txtInfoRekap.Text = "Belum ada periode terdaftar"
            Exit Sub
        End If

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim qResp As String = "SELECT COUNT(*) FROM pengisian_kuesioner pk INNER JOIN pengampu p ON p.id = pk.id_pengampu WHERE p.id_periode = @idPeriode"
                Using cmdResp As New MySqlCommand(qResp, conn)
                    cmdResp.Parameters.AddWithValue("@idPeriode", itemPeriode.Id)
                    txtTotalPengisian.Text = Convert.ToInt32(cmdResp.ExecuteScalar()).ToString()
                End Using

                Dim query As String = "SELECT u.id AS id_dosen, u.username AS nip, u.nama AS nama_dosen, " &
                                      "COUNT(DISTINCT p.id) AS jml_kelas, " &
                                      "COALESCE(SUM(sub_pk.jml_resp), 0) AS jml_responden, " &
                                      "COALESCE(SUM(sub_j.tot_ped), 0) AS tot_ped, " &
                                      "COALESCE(SUM(sub_j.jml_ped), 0) AS jml_ped, " &
                                      "COALESCE(SUM(sub_j.tot_prof), 0) AS tot_prof, " &
                                      "COALESCE(SUM(sub_j.jml_prof), 0) AS jml_prof, " &
                                      "COALESCE(SUM(sub_j.tot_kep), 0) AS tot_kep, " &
                                      "COALESCE(SUM(sub_j.jml_kep), 0) AS jml_kep, " &
                                      "COALESCE(SUM(sub_j.tot_sos), 0) AS tot_sos, " &
                                      "COALESCE(SUM(sub_j.jml_sos), 0) AS jml_sos, " &
                                      "hf.skor_pedagogik AS db_ped, hf.skor_profesional AS db_prof, " &
                                      "hf.skor_kepribadian AS db_kep, hf.skor_sosial AS db_sos, " &
                                      "hf.skor_akhir_fuzzy AS db_akhir, hf.predikat AS db_predikat, hf.rekomendasi AS db_rekom " &
                                      "FROM users u " &
                                      "LEFT JOIN pengampu p ON p.id_dosen = u.id AND p.id_periode = @idPeriode AND p.aktif = TRUE " &
                                      "LEFT JOIN (SELECT id_pengampu, COUNT(*) AS jml_resp FROM pengisian_kuesioner GROUP BY id_pengampu) sub_pk ON sub_pk.id_pengampu = p.id " &
                                      "LEFT JOIN ( " &
                                      "    SELECT j.id_pengampu, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'pedagogik' THEN j.nilai ELSE 0 END) AS tot_ped, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'pedagogik' THEN 1 ELSE 0 END) AS jml_ped, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'profesional' THEN j.nilai ELSE 0 END) AS tot_prof, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'profesional' THEN 1 ELSE 0 END) AS jml_prof, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'kepribadian' THEN j.nilai ELSE 0 END) AS tot_kep, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'kepribadian' THEN 1 ELSE 0 END) AS jml_kep, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'sosial' THEN j.nilai ELSE 0 END) AS tot_sos, " &
                                      "           SUM(CASE WHEN LOWER(pt.kategori) = 'sosial' THEN 1 ELSE 0 END) AS jml_sos " &
                                      "    FROM jawaban j INNER JOIN pertanyaan pt ON j.id_pertanyaan = pt.id " &
                                      "    GROUP BY j.id_pengampu " &
                                      ") sub_j ON sub_j.id_pengampu = p.id " &
                                      "LEFT JOIN hasil_fuzzy_dosen hf ON hf.id_dosen = u.id AND hf.id_periode = @idPeriode " &
                                      "WHERE u.role = 'dosen' AND u.aktif = TRUE " &
                                      "GROUP BY u.id, u.username, u.nama, hf.skor_pedagogik, hf.skor_profesional, hf.skor_kepribadian, hf.skor_sosial, hf.skor_akhir_fuzzy, hf.predikat, hf.rekomendasi " &
                                      "ORDER BY u.nama ASC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idPeriode", itemPeriode.Id)

                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim itemR As New RekapDosenItem()
                            itemR.IdDosen = Convert.ToInt32(rd("id_dosen"))
                            itemR.Nip = rd("nip").ToString()
                            itemR.NamaDosen = rd("nama_dosen").ToString()
                            itemR.JumlahKelas = Convert.ToInt32(rd("jml_kelas"))
                            itemR.JumlahResponden = Convert.ToInt32(rd("jml_responden"))

                            Dim inp As New InputKategoriFuzzy()
                            inp.TotPed = Convert.ToDouble(rd("tot_ped"))
                            inp.JmlPed = Convert.ToInt32(rd("jml_ped"))
                            inp.TotProf = Convert.ToDouble(rd("tot_prof"))
                            inp.JmlProf = Convert.ToInt32(rd("jml_prof"))
                            inp.TotKep = Convert.ToDouble(rd("tot_kep"))
                            inp.JmlKep = Convert.ToInt32(rd("jml_kep"))
                            inp.TotSos = Convert.ToDouble(rd("tot_sos"))
                            inp.JmlSos = Convert.ToInt32(rd("jml_sos"))
                            itemR.InputFuzzy = inp

                            If Not IsDBNull(rd("db_akhir")) Then
                                Dim hasilDb As New HasilEvaluasiFuzzy()
                                hasilDb.Pedagogik = Convert.ToDouble(rd("db_ped"))
                                hasilDb.Profesional = Convert.ToDouble(rd("db_prof"))
                                hasilDb.Kepribadian = Convert.ToDouble(rd("db_kep"))
                                hasilDb.Sosial = Convert.ToDouble(rd("db_sos"))
                                hasilDb.SkorAkhir = Convert.ToDouble(rd("db_akhir"))
                                hasilDb.Predikat = rd("db_predikat").ToString()
                                hasilDb.Rekomendasi = If(IsDBNull(rd("db_rekom")), "", rd("db_rekom").ToString())
                                itemR.HasilFuzzy = hasilDb
                            End If

                            cacheRekap.Add(itemR)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat rekapitulasi dosen: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End Try

        Dim batchInput As New List(Of InputKategoriFuzzy)()
        Dim mapIndeks As New Dictionary(Of Integer, Integer)()

        For i As Integer = 0 To cacheRekap.Count - 1
            Dim inp As InputKategoriFuzzy = cacheRekap(i).InputFuzzy
            Dim punyaJawaban As Boolean = (inp.JmlPed > 0 OrElse inp.JmlProf > 0 OrElse inp.JmlKep > 0 OrElse inp.JmlSos > 0)

            If punyaJawaban Then
                mapIndeks(i) = batchInput.Count
                batchInput.Add(inp)
            End If
        Next

        If batchInput.Count > 0 Then
            Try
                Dim hasilBatch As List(Of HasilEvaluasiFuzzy) = FuzzyHelper.HitungBatchFuzzy(batchInput)

                For Each kvp In mapIndeks
                    If kvp.Value < hasilBatch.Count Then
                        cacheRekap(kvp.Key).HasilFuzzy = hasilBatch(kvp.Value)
                    End If
                Next
            Catch ex As Exception
                If simpanKeDatabase Then
                    MessageBox.Show("Gagal menghitung Fuzzy Sugeno: " & ex.Message, "Kesalahan Fuzzy Engine", MessageBoxButton.OK, MessageBoxImage.Error)
                    Exit Sub
                End If
            End Try
        End If

        If simpanKeDatabase Then
            SimpanHasilFuzzy(itemPeriode.Id)
        End If

        BangunTabelRekap()
    End Sub

    Private Sub SimpanHasilFuzzy(idPeriode As Integer)
        Dim tersimpan As Integer = 0
        Dim queryUpsert As String = "INSERT INTO hasil_fuzzy_dosen (id_dosen, id_periode, skor_pedagogik, skor_profesional, skor_kepribadian, skor_sosial, skor_akhir_fuzzy, predikat, rekomendasi) " &
                                    "VALUES (@idDosen, @idPeriode, @ped, @prof, @kep, @sos, @akhir, @predikat, @rekomendasi) " &
                                    "ON DUPLICATE KEY UPDATE " &
                                    "skor_pedagogik = VALUES(skor_pedagogik), " &
                                    "skor_profesional = VALUES(skor_profesional), " &
                                    "skor_kepribadian = VALUES(skor_kepribadian), " &
                                    "skor_sosial = VALUES(skor_sosial), " &
                                    "skor_akhir_fuzzy = VALUES(skor_akhir_fuzzy), " &
                                    "predikat = VALUES(predikat), " &
                                    "rekomendasi = VALUES(rekomendasi)"

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Using trans As MySqlTransaction = conn.BeginTransaction()
                    Try
                        For Each itemR As RekapDosenItem In cacheRekap
                            If itemR.HasilFuzzy Is Nothing Then Continue For

                            Using cmd As New MySqlCommand(queryUpsert, conn, trans)
                                cmd.Parameters.AddWithValue("@idDosen", itemR.IdDosen)
                                cmd.Parameters.AddWithValue("@idPeriode", idPeriode)
                                cmd.Parameters.AddWithValue("@ped", itemR.HasilFuzzy.Pedagogik)
                                cmd.Parameters.AddWithValue("@prof", itemR.HasilFuzzy.Profesional)
                                cmd.Parameters.AddWithValue("@kep", itemR.HasilFuzzy.Kepribadian)
                                cmd.Parameters.AddWithValue("@sos", itemR.HasilFuzzy.Sosial)
                                cmd.Parameters.AddWithValue("@akhir", itemR.HasilFuzzy.SkorAkhir)
                                cmd.Parameters.AddWithValue("@predikat", itemR.HasilFuzzy.Predikat)
                                cmd.Parameters.AddWithValue("@rekomendasi", itemR.HasilFuzzy.Rekomendasi)
                                cmd.ExecuteNonQuery()
                                tersimpan += 1
                            End Using
                        Next

                        trans.Commit()
                    Catch
                        trans.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            MessageBox.Show("Perhitungan Fuzzy Sugeno berhasil disimpan untuk " & tersimpan.ToString() & " dosen.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MessageBox.Show("Gagal menyimpan rekap ke database: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BangunTabelRekap()
        dtRekap = New DataTable()
        dtRekap.Columns.Add("id_dosen", GetType(Integer))
        dtRekap.Columns.Add("nip", GetType(String))
        dtRekap.Columns.Add("nama_dosen", GetType(String))
        dtRekap.Columns.Add("kelas_label", GetType(String))
        dtRekap.Columns.Add("responden_label", GetType(String))
        dtRekap.Columns.Add("ped", GetType(String))
        dtRekap.Columns.Add("prof", GetType(String))
        dtRekap.Columns.Add("kep", GetType(String))
        dtRekap.Columns.Add("sos", GetType(String))
        dtRekap.Columns.Add("skor_akhir", GetType(String))
        dtRekap.Columns.Add("predikat", GetType(String))
        dtRekap.Columns.Add("predikat_color", GetType(String))

        For Each r As RekapDosenItem In cacheRekap
            If r.HasilFuzzy IsNot Nothing Then
                dtRekap.Rows.Add(
                    r.IdDosen,
                    r.Nip,
                    r.NamaDosen,
                    r.JumlahKelas.ToString() & " Kelas",
                    r.JumlahResponden.ToString() & " Mahasiswa",
                    r.HasilFuzzy.Pedagogik.ToString("0.00", CultureInfo.InvariantCulture),
                    r.HasilFuzzy.Profesional.ToString("0.00", CultureInfo.InvariantCulture),
                    r.HasilFuzzy.Kepribadian.ToString("0.00", CultureInfo.InvariantCulture),
                    r.HasilFuzzy.Sosial.ToString("0.00", CultureInfo.InvariantCulture),
                    r.HasilFuzzy.SkorAkhir.ToString("0.00", CultureInfo.InvariantCulture),
                    r.HasilFuzzy.Predikat,
                    WarnaPredikatHex(r.HasilFuzzy.Predikat)
                )
            Else
                dtRekap.Rows.Add(
                    r.IdDosen,
                    r.Nip,
                    r.NamaDosen,
                    r.JumlahKelas.ToString() & " Kelas",
                    r.JumlahResponden.ToString() & " Mahasiswa",
                    "-", "-", "-", "-", "-",
                    "Belum Dinilai",
                    "#64748B"
                )
            End If
        Next

        dgRekapDosen.ItemsSource = dtRekap.DefaultView
        TerapkanFilter()
    End Sub

    Private Function WarnaPredikatHex(predikat As String) As String
        Select Case predikat
            Case "Apresiasi"
                Return "#15803D"
            Case "Pertahankan"
                Return "#0F766E"
            Case "Evaluasi", "Perlu Pembinaan"
                Return "#B91C1C"
            Case Else
                Return "#64748B"
        End Select
    End Function

    Private Sub TerapkanFilter()
        If dtRekap Is Nothing Then Exit Sub

        Dim keyword As String = EscapeRowFilter(txtCari.Text.Trim())
        Dim bagian As New List(Of String)()

        If Not String.IsNullOrEmpty(keyword) Then
            bagian.Add(String.Format("(nip LIKE '%{0}%' OR nama_dosen LIKE '%{0}%')", keyword))
        End If

        Dim itemPred As ComboBoxItem = TryCast(cmbFilterPredikat.SelectedItem, ComboBoxItem)
        If itemPred IsNot Nothing Then
            Dim tagPred As String = If(itemPred.Tag, "").ToString()
            If Not String.IsNullOrEmpty(tagPred) Then
                bagian.Add("predikat = '" & EscapeRowFilter(tagPred) & "'")
            End If
        End If

        dtRekap.DefaultView.RowFilter = String.Join(" AND ", bagian)

        Dim tampil As Integer = dtRekap.DefaultView.Count
        txtKosongRekap.Visibility = If(tampil = 0, Visibility.Visible, Visibility.Collapsed)
        txtInfoRekap.Text = "Menampilkan " & tampil.ToString() & " dari " & dtRekap.Rows.Count.ToString() & " dosen"
    End Sub

    Private Function EscapeRowFilter(nilai As String) As String
        Return nilai.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
    End Function

    Private Sub txtCari_TextChanged(sender As Object, e As TextChangedEventArgs)
        lblPlaceholderCari.Visibility = If(String.IsNullOrEmpty(txtCari.Text), Visibility.Visible, Visibility.Collapsed)
        TerapkanFilter()
    End Sub

    Private Sub cmbPeriode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatPeriode Then Exit Sub
        MuatRekapitulasi(False)
    End Sub

    Private Sub cmbFilterPredikat_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        TerapkanFilter()
    End Sub

    Private Sub btnHitungFuzzy_Click(sender As Object, e As RoutedEventArgs)
        MuatRekapitulasi(True)
    End Sub

    Private Sub btnDetailDosen_Click(sender As Object, e As RoutedEventArgs)
        BukaDetailDosenTerpilih()
    End Sub

    Private Sub dgRekapDosen_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If dgRekapDosen.SelectedItem IsNot Nothing Then
            BukaDetailDosenTerpilih()
        End If
    End Sub

    Private Sub BukaDetailDosenTerpilih()
        Dim rv As DataRowView = TryCast(dgRekapDosen.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih baris dosen yang ingin dilihat rinciannya terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idDosen As Integer = Convert.ToInt32(rv("id_dosen"))
        Dim namaDosen As String = rv("nama_dosen").ToString()
        Dim nipDosen As String = rv("nip").ToString()
        Dim itemPeriode As PilihanPeriode = TryCast(cmbPeriode.SelectedItem, PilihanPeriode)
        Dim idPeriode As Integer = If(itemPeriode IsNot Nothing, itemPeriode.Id, 0)

        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then mainWin = TryCast(Application.Current.MainWindow, MainWindow)

        If mainWin IsNot Nothing Then
            mainWin.NavigasiKeHalaman(
                New AdminDetailDosenView(idDosen, namaDosen, nipDosen, idPeriode, "rekap"),
                "Detail Evaluasi Dosen",
                namaDosen & " · Rincian per mata kuliah dan kelas",
                mainWin.btnAdminRekap
            )
        End If
    End Sub

End Class
