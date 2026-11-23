Imports System.Data
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class AdminDetailDosenView

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

    Private Class BarisKelasPengampu
        Public Property IdPengampu As Integer
        Public Property MatkulLabel As String
        Public Property KelasLabel As String
        Public Property Sks As Integer
        Public Property Responden As Integer
        Public Property InputFuzzy As InputKategoriFuzzy
    End Class

    Private ReadOnly idDosenTarget As Integer

    Private ReadOnly namaDosenTarget As String

    Private ReadOnly nipDosenTarget As String

    Private ReadOnly idPeriodeAwal As Integer

    Private ReadOnly halamanAsal As String

    Private sedangMuatFilter As Boolean = False

    Public Sub New(idDosen As Integer, namaDosen As String, nipDosen As String, Optional idPeriode As Integer = 0, Optional asalNav As String = "rekap")
        InitializeComponent()
        idDosenTarget = idDosen
        namaDosenTarget = namaDosen
        nipDosenTarget = nipDosen
        idPeriodeAwal = idPeriode
        halamanAsal = asalNav
    End Sub

    Private Sub AdminDetailDosenView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        txtNamaDosen.Text = namaDosenTarget
        txtNipDosen.Text = "NIP " & nipDosenTarget
        MuatPeriode()
    End Sub

    Private Sub MuatPeriode()
        sedangMuatFilter = True
        cmbPeriode.Items.Clear()

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT id, nama_periode, status_aktif FROM periode_edom ORDER BY status_aktif DESC, tahun_ajaran DESC, semester DESC"

                Using cmd As New MySqlCommand(query, conn)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        Dim idxPilih As Integer = -1
                        Dim idx As Integer = 0

                        While rd.Read()
                            Dim idP As Integer = Convert.ToInt32(rd("id"))
                            Dim namaP As String = rd("nama_periode").ToString()
                            Dim aktif As Boolean = Convert.ToBoolean(rd("status_aktif"))

                            cmbPeriode.Items.Add(New PilihanItem(idP, namaP))

                            If idPeriodeAwal > 0 AndAlso idP = idPeriodeAwal Then
                                idxPilih = idx
                            ElseIf idPeriodeAwal <= 0 AndAlso aktif AndAlso idxPilih = -1 Then
                                idxPilih = idx
                            End If

                            idx += 1
                        End While

                        If cmbPeriode.Items.Count > 0 Then
                            cmbPeriode.SelectedIndex = If(idxPilih >= 0, idxPilih, 0)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat daftar periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            sedangMuatFilter = False
        End Try

        MuatDaftarKelas()
        MuatEvaluasi()
    End Sub

    Private Sub MuatDaftarKelas()
        sedangMuatFilter = True
        cmbKelas.Items.Clear()
        cmbKelas.Items.Add(New PilihanItem(0, "Semua Kelas"))

        Dim itemPeriode As PilihanItem = TryCast(cmbPeriode.SelectedItem, PilihanItem)
        If itemPeriode IsNot Nothing Then
            Try
                Using conn As MySqlConnection = db.GetConnection()
                    If conn IsNot Nothing Then
                        Dim query As String = "SELECT DISTINCT k.id, CONCAT(k.angkatan, ' · ', k.rombel) AS nama_kelas, k.angkatan, k.rombel " &
                                              "FROM pengampu p " &
                                              "INNER JOIN kelas k ON p.id_kelas = k.id " &
                                              "WHERE p.id_dosen = @idDosen AND p.id_periode = @idPeriode AND p.aktif = TRUE " &
                                              "ORDER BY k.angkatan DESC, k.rombel ASC"

                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@idDosen", idDosenTarget)
                            cmd.Parameters.AddWithValue("@idPeriode", itemPeriode.Id)

                            Using rd As MySqlDataReader = cmd.ExecuteReader()
                                While rd.Read()
                                    cmbKelas.Items.Add(New PilihanItem(Convert.ToInt32(rd("id")), rd("nama_kelas").ToString()))
                                End While
                            End Using
                        End Using
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Gagal memuat daftar kelas dosen: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If

        cmbKelas.SelectedIndex = 0
        sedangMuatFilter = False
    End Sub

    Private Sub cmbPeriode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatFilter Then Exit Sub
        MuatDaftarKelas()
        MuatEvaluasi()
    End Sub

    Private Sub cmbKelas_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatFilter Then Exit Sub
        MuatEvaluasi()
    End Sub

    Private Sub MuatEvaluasi()
        Dim itemPeriode As PilihanItem = TryCast(cmbPeriode.SelectedItem, PilihanItem)
        Dim itemKelas As PilihanItem = TryCast(cmbKelas.SelectedItem, PilihanItem)

        If itemPeriode Is Nothing Then
            ResetRingkasan("Belum ada periode semester.")
            dgKelasDosen.ItemsSource = Nothing
            txtKosongDetailDosen.Visibility = Visibility.Visible
            Exit Sub
        End If

        Dim idKelasFilter As Integer = If(itemKelas IsNot Nothing, itemKelas.Id, 0)
        Dim daftar As New List(Of BarisKelasPengampu)()

        Dim query As String = "SELECT p.id AS id_pengampu, CONCAT(m.kode_matkul, ' - ', m.nama_matkul) AS matkul_label, " &
                              "m.sks, CONCAT(k.angkatan, ' · ', k.rombel) AS kelas_label, " &
                              "(SELECT COUNT(*) FROM pengisian_kuesioner pk WHERE pk.id_pengampu = p.id) AS jml_resp, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'pedagogik' THEN j.nilai ELSE 0 END), 0) AS tot_ped, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'pedagogik' THEN 1 ELSE 0 END), 0) AS jml_ped, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'profesional' THEN j.nilai ELSE 0 END), 0) AS tot_prof, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'profesional' THEN 1 ELSE 0 END), 0) AS jml_prof, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'kepribadian' THEN j.nilai ELSE 0 END), 0) AS tot_kep, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'kepribadian' THEN 1 ELSE 0 END), 0) AS jml_kep, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'sosial' THEN j.nilai ELSE 0 END), 0) AS tot_sos, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'sosial' THEN 1 ELSE 0 END), 0) AS jml_sos " &
                              "FROM pengampu p " &
                              "INNER JOIN matkul m ON p.id_matkul = m.id " &
                              "INNER JOIN kelas k ON p.id_kelas = k.id " &
                              "LEFT JOIN jawaban j ON j.id_pengampu = p.id " &
                              "LEFT JOIN pertanyaan pt ON j.id_pertanyaan = pt.id " &
                              "WHERE p.id_dosen = @idDosen AND p.id_periode = @idPeriode AND p.aktif = TRUE "

        If idKelasFilter > 0 Then
            query &= "AND p.id_kelas = @idKelas "
        End If

        query &= "GROUP BY p.id, m.kode_matkul, m.nama_matkul, m.sks, k.angkatan, k.rombel ORDER BY k.angkatan DESC, k.rombel ASC, m.nama_matkul ASC"

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idDosen", idDosenTarget)
                    cmd.Parameters.AddWithValue("@idPeriode", itemPeriode.Id)
                    If idKelasFilter > 0 Then
                        cmd.Parameters.AddWithValue("@idKelas", idKelasFilter)
                    End If

                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim b As New BarisKelasPengampu()
                            b.IdPengampu = Convert.ToInt32(rd("id_pengampu"))
                            b.MatkulLabel = rd("matkul_label").ToString()
                            b.KelasLabel = rd("kelas_label").ToString()
                            b.Sks = Convert.ToInt32(rd("sks"))
                            b.Responden = Convert.ToInt32(rd("jml_resp"))

                            Dim inp As New InputKategoriFuzzy()
                            inp.TotPed = Convert.ToDouble(rd("tot_ped"))
                            inp.JmlPed = Convert.ToInt32(rd("jml_ped"))
                            inp.TotProf = Convert.ToDouble(rd("tot_prof"))
                            inp.JmlProf = Convert.ToInt32(rd("jml_prof"))
                            inp.TotKep = Convert.ToDouble(rd("tot_kep"))
                            inp.JmlKep = Convert.ToInt32(rd("jml_kep"))
                            inp.TotSos = Convert.ToDouble(rd("tot_sos"))
                            inp.JmlSos = Convert.ToInt32(rd("jml_sos"))
                            b.InputFuzzy = inp

                            daftar.Add(b)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat detail evaluasi dosen: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End Try

        If daftar.Count = 0 Then
            ResetRingkasan("Dosen ini belum memiliki penugasan kelas pada filter terpilih.")
            dgKelasDosen.ItemsSource = Nothing
            txtKosongDetailDosen.Visibility = Visibility.Visible
            Exit Sub
        End If

        Dim aggInput As New InputKategoriFuzzy()
        Dim batchInput As New List(Of InputKategoriFuzzy)()
        Dim mapIndeks As New Dictionary(Of Integer, Integer)()
        Dim adaResponden As Boolean = False

        For i As Integer = 0 To daftar.Count - 1
            Dim itemB As BarisKelasPengampu = daftar(i)
            Dim punyaNilai As Boolean = (itemB.InputFuzzy.JmlPed > 0 OrElse itemB.InputFuzzy.JmlProf > 0 OrElse itemB.InputFuzzy.JmlKep > 0 OrElse itemB.InputFuzzy.JmlSos > 0)

            If punyaNilai Then
                adaResponden = True
                mapIndeks(i) = batchInput.Count
                batchInput.Add(itemB.InputFuzzy)

                aggInput.TotPed += itemB.InputFuzzy.TotPed
                aggInput.JmlPed += itemB.InputFuzzy.JmlPed
                aggInput.TotProf += itemB.InputFuzzy.TotProf
                aggInput.JmlProf += itemB.InputFuzzy.JmlProf
                aggInput.TotKep += itemB.InputFuzzy.TotKep
                aggInput.JmlKep += itemB.InputFuzzy.JmlKep
                aggInput.TotSos += itemB.InputFuzzy.TotSos
                aggInput.JmlSos += itemB.InputFuzzy.JmlSos
            End If
        Next

        Dim idxAgg As Integer = -1
        If adaResponden Then
            idxAgg = batchInput.Count
            batchInput.Add(aggInput)
        End If

        Dim hasilBatch As New List(Of HasilEvaluasiFuzzy)()
        If batchInput.Count > 0 Then
            Try
                hasilBatch = FuzzyHelper.HitungBatchFuzzy(batchInput)
            Catch ex As Exception
                MessageBox.Show("Gagal menghitung skor Fuzzy Sugeno: " & ex.Message, "Kesalahan Fuzzy Engine", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If

        Dim dt As New DataTable()
        dt.Columns.Add("matkul_label", GetType(String))
        dt.Columns.Add("kelas_label", GetType(String))
        dt.Columns.Add("sks", GetType(Integer))
        dt.Columns.Add("responden_label", GetType(String))
        dt.Columns.Add("ped", GetType(String))
        dt.Columns.Add("prof", GetType(String))
        dt.Columns.Add("kep", GetType(String))
        dt.Columns.Add("sos", GetType(String))
        dt.Columns.Add("skor_akhir", GetType(String))
        dt.Columns.Add("predikat_label", GetType(String))
        dt.Columns.Add("predikat_color", GetType(String))

        For i As Integer = 0 To daftar.Count - 1
            Dim itemB As BarisKelasPengampu = daftar(i)

            If mapIndeks.ContainsKey(i) AndAlso mapIndeks(i) < hasilBatch.Count Then
                Dim h As HasilEvaluasiFuzzy = hasilBatch(mapIndeks(i))
                dt.Rows.Add(
                    itemB.MatkulLabel,
                    itemB.KelasLabel,
                    itemB.Sks,
                    itemB.Responden.ToString() & " mhs",
                    h.Pedagogik.ToString("0.00", CultureInfo.InvariantCulture),
                    h.Profesional.ToString("0.00", CultureInfo.InvariantCulture),
                    h.Kepribadian.ToString("0.00", CultureInfo.InvariantCulture),
                    h.Sosial.ToString("0.00", CultureInfo.InvariantCulture),
                    h.SkorAkhir.ToString("0.00", CultureInfo.InvariantCulture),
                    h.Predikat,
                    WarnaPredikatHex(h.Predikat)
                )
            Else
                dt.Rows.Add(
                    itemB.MatkulLabel,
                    itemB.KelasLabel,
                    itemB.Sks,
                    itemB.Responden.ToString() & " mhs",
                    "-", "-", "-", "-", "-",
                    "Belum dinilai",
                    "#64748B"
                )
            End If
        Next

        dgKelasDosen.ItemsSource = dt.DefaultView
        txtKosongDetailDosen.Visibility = Visibility.Collapsed

        If idxAgg >= 0 AndAlso idxAgg < hasilBatch.Count Then
            TampilkanRingkasan(hasilBatch(idxAgg))
        Else
            ResetRingkasan("Belum ada mahasiswa yang mengisi kuesioner untuk pengampu di periode ini.")
        End If
    End Sub

    Private Sub TampilkanRingkasan(hasil As HasilEvaluasiFuzzy)
        txtSkorAkhir.Text = hasil.SkorAkhir.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorPed.Text = hasil.Pedagogik.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorProf.Text = hasil.Profesional.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorKep.Text = hasil.Kepribadian.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorSos.Text = hasil.Sosial.ToString("0.00", CultureInfo.InvariantCulture)
        txtPredikatKeseluruhan.Text = hasil.Predikat
        txtRekomendasi.Text = If(String.IsNullOrWhiteSpace(hasil.Rekomendasi), "-", hasil.Rekomendasi)

        Dim warna As New BrushConverter()
        Dim brushPred As System.Windows.Media.Brush = CType(warna.ConvertFromString(WarnaPredikatHex(hasil.Predikat)), System.Windows.Media.Brush)
        txtPredikatKeseluruhan.Foreground = brushPred
        indikatorPredikat.Fill = brushPred
    End Sub

    Private Sub ResetRingkasan(catatan As String)
        txtSkorAkhir.Text = "-"
        txtSkorPed.Text = "-"
        txtSkorProf.Text = "-"
        txtSkorKep.Text = "-"
        txtSkorSos.Text = "-"
        txtPredikatKeseluruhan.Text = "Belum dinilai"
        txtRekomendasi.Text = catatan

        Dim brushMuted As New SolidColorBrush(System.Windows.Media.Color.FromRgb(&H64, &H74, &H8B))
        txtPredikatKeseluruhan.Foreground = brushMuted
        indikatorPredikat.Fill = brushMuted
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

    Private Sub btnKembali_Click(sender As Object, e As RoutedEventArgs)
        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then mainWin = TryCast(Application.Current.MainWindow, MainWindow)
        If mainWin Is Nothing Then Exit Sub

        If halamanAsal = "dosen" Then
            mainWin.NavigasiKeHalaman(New AdminUserView("dosen"), "Daftar Dosen", "Akun dosen aktif (klik dua kali baris untuk lihat detail evaluasi)", mainWin.btnAdminDosen)
        Else
            mainWin.NavigasiKeHalaman(New AdminDashboardView(), "Rekapitulasi", "Evaluasi seluruh dosen dan perhitungan Fuzzy Sugeno", mainWin.btnAdminRekap)
        End If
    End Sub

End Class
