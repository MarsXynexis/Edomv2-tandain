Imports System.Data
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class DosenMainView

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

    Private Class RigaEvaluasi
        Public Property MataKuliah As String
        Public Property Kelas As String
        Public Property Sks As Integer
        Public Property InputFuzzy As InputKategoriFuzzy
    End Class

    Private sedangMuatFilter As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub DosenMainView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If UserSession.Role <> "dosen" OrElse UserSession.Id <= 0 Then
            ResetRingkasan("Sesi dosen tidak valid. Silakan masuk kembali.")
            dgRincianEvaluasi.ItemsSource = Nothing
            Exit Sub
        End If

        MuatDaftarPeriode()
    End Sub

    Private Sub MuatDaftarPeriode()
        sedangMuatFilter = True
        cmbPeriodeSemester.Items.Clear()

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
                            cmbPeriodeSemester.Items.Add(New PilihanItem(Convert.ToInt32(rd("id")), rd("nama_periode").ToString()))
                            If aktif AndAlso idxAktif = -1 Then idxAktif = idx
                            idx += 1
                        End While
                        If cmbPeriodeSemester.Items.Count > 0 Then cmbPeriodeSemester.SelectedIndex = If(idxAktif >= 0, idxAktif, 0)
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

        Dim itemPeriode As PilihanItem = TryCast(cmbPeriodeSemester.SelectedItem, PilihanItem)
        If itemPeriode IsNot Nothing Then
            Try
                Using conn As MySqlConnection = db.GetConnection()
                    If conn IsNot Nothing Then
                        Dim query As String = "SELECT DISTINCT k.id, CONCAT(k.angkatan, ' · ', k.rombel) AS nama_kelas " &
                                              "FROM pengampu p INNER JOIN kelas k ON p.id_kelas = k.id " &
                                              "WHERE p.id_dosen = @idDosen AND p.id_periode = @idPeriode AND p.aktif = TRUE " &
                                              "ORDER BY k.angkatan DESC, k.rombel ASC"
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@idDosen", UserSession.Id)
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
                MessageBox.Show("Gagal memuat daftar kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If

        If cmbKelas.Items.Count > 0 Then cmbKelas.SelectedIndex = 0
        sedangMuatFilter = False
    End Sub

    Private Sub cmbPeriodeSemester_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatFilter Then Exit Sub
        MuatDaftarKelas()
    End Sub

    Private Sub btnTampilkan_Click(sender As Object, e As RoutedEventArgs)
        MuatEvaluasi()
    End Sub

    Private Sub MuatEvaluasi()
        Dim itemPeriode As PilihanItem = TryCast(cmbPeriodeSemester.SelectedItem, PilihanItem)
        Dim itemKelas As PilihanItem = TryCast(cmbKelas.SelectedItem, PilihanItem)
        If itemPeriode Is Nothing Then
            ResetRingkasan("Belum ada periode semester.")
            dgRincianEvaluasi.ItemsSource = Nothing
            Exit Sub
        End If

        Dim daftar As New List(Of RigaEvaluasi)()
        Dim idKelasFilter As Integer = If(itemKelas IsNot Nothing, itemKelas.Id, 0)
        Dim query As String = "SELECT p.id AS id_pengampu, CONCAT(m.kode_matkul, ' - ', m.nama_matkul) AS matkul_label, " &
                              "m.sks, CONCAT(k.angkatan, ' · ', k.rombel) AS kelas_label, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'pedagogik' THEN j.nilai ELSE 0 END), 0) AS tot_ped, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'pedagogik' THEN 1 ELSE 0 END), 0) AS jml_ped, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'profesional' THEN j.nilai ELSE 0 END), 0) AS tot_prof, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'profesional' THEN 1 ELSE 0 END), 0) AS jml_prof, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'kepribadian' THEN j.nilai ELSE 0 END), 0) AS tot_kep, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'kepribadian' THEN 1 ELSE 0 END), 0) AS jml_kep, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'sosial' THEN j.nilai ELSE 0 END), 0) AS tot_sos, " &
                              "COALESCE(SUM(CASE WHEN LOWER(pt.kategori) = 'sosial' THEN 1 ELSE 0 END), 0) AS jml_sos " &
                              "FROM pengampu p INNER JOIN matkul m ON p.id_matkul = m.id " &
                              "INNER JOIN kelas k ON p.id_kelas = k.id " &
                              "LEFT JOIN jawaban j ON j.id_pengampu = p.id " &
                              "LEFT JOIN pertanyaan pt ON j.id_pertanyaan = pt.id " &
                              "WHERE p.id_dosen = @idDosen AND p.id_periode = @idPeriode AND p.aktif = TRUE "
        If idKelasFilter > 0 Then query &= "AND p.id_kelas = @idKelas "
        query &= "GROUP BY p.id, m.kode_matkul, m.nama_matkul, m.sks, k.angkatan, k.rombel " &
                 "ORDER BY k.angkatan DESC, k.rombel ASC, m.nama_matkul ASC"

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idDosen", UserSession.Id)
                    cmd.Parameters.AddWithValue("@idPeriode", itemPeriode.Id)
                    If idKelasFilter > 0 Then cmd.Parameters.AddWithValue("@idKelas", idKelasFilter)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim item As New RigaEvaluasi With {
                                .MataKuliah = rd("matkul_label").ToString(),
                                .Kelas = rd("kelas_label").ToString(),
                                .Sks = Convert.ToInt32(rd("sks")),
                                .InputFuzzy = New InputKategoriFuzzy() With {
                                    .TotPed = Convert.ToDouble(rd("tot_ped")), .JmlPed = Convert.ToInt32(rd("jml_ped")),
                                    .TotProf = Convert.ToDouble(rd("tot_prof")), .JmlProf = Convert.ToInt32(rd("jml_prof")),
                                    .TotKep = Convert.ToDouble(rd("tot_kep")), .JmlKep = Convert.ToInt32(rd("jml_kep")),
                                    .TotSos = Convert.ToDouble(rd("tot_sos")), .JmlSos = Convert.ToInt32(rd("jml_sos"))
                                }
                            }
                            daftar.Add(item)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat evaluasi dosen: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            Exit Sub
        End Try

        If daftar.Count = 0 Then
            ResetRingkasan("Belum ada penugasan kelas pada periode/filter yang dipilih.")
            dgRincianEvaluasi.ItemsSource = Nothing
            Exit Sub
        End If

        Dim agregat As New InputKategoriFuzzy()
        Dim inputBatch As New List(Of InputKategoriFuzzy)()
        Dim indeksBatch As New Dictionary(Of Integer, Integer)()
        For i As Integer = 0 To daftar.Count - 1
            Dim input As InputKategoriFuzzy = daftar(i).InputFuzzy
            If input.JmlPed + input.JmlProf + input.JmlKep + input.JmlSos = 0 Then Continue For
            indeksBatch(i) = inputBatch.Count
            inputBatch.Add(input)
            agregat.TotPed += input.TotPed : agregat.JmlPed += input.JmlPed
            agregat.TotProf += input.TotProf : agregat.JmlProf += input.JmlProf
            agregat.TotKep += input.TotKep : agregat.JmlKep += input.JmlKep
            agregat.TotSos += input.TotSos : agregat.JmlSos += input.JmlSos
        Next

        Dim indeksAgregat As Integer = -1
        If inputBatch.Count > 0 Then
            indeksAgregat = inputBatch.Count
            inputBatch.Add(agregat)
        End If

        Dim hasilBatch As New List(Of HasilEvaluasiFuzzy)()
        If inputBatch.Count > 0 Then
            Try
                hasilBatch = FuzzyHelper.HitungBatchFuzzy(inputBatch)
            Catch ex As Exception
                MessageBox.Show("Gagal menghitung evaluasi Fuzzy Sugeno: " & ex.Message, "Kesalahan Fuzzy Engine", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If

        Dim dt As New DataTable()
        dt.Columns.Add("No", GetType(Integer))
        dt.Columns.Add("MataKuliah", GetType(String))
        dt.Columns.Add("Kelas", GetType(String))
        dt.Columns.Add("SKS", GetType(Integer))
        dt.Columns.Add("Pedagogik", GetType(String))
        dt.Columns.Add("Profesional", GetType(String))
        dt.Columns.Add("Kepribadian", GetType(String))
        dt.Columns.Add("Sosial", GetType(String))
        dt.Columns.Add("SkorAkhir", GetType(String))
        dt.Columns.Add("Predikat", GetType(String))
        dt.Columns.Add("Rekomendasi", GetType(String))
        For i As Integer = 0 To daftar.Count - 1
            Dim item As RigaEvaluasi = daftar(i)
            If indeksBatch.ContainsKey(i) AndAlso indeksBatch(i) < hasilBatch.Count Then
                Dim hasil As HasilEvaluasiFuzzy = hasilBatch(indeksBatch(i))
                dt.Rows.Add(i + 1, item.MataKuliah, item.Kelas, item.Sks,
                            hasil.Pedagogik.ToString("0.00", CultureInfo.InvariantCulture),
                            hasil.Profesional.ToString("0.00", CultureInfo.InvariantCulture),
                            hasil.Kepribadian.ToString("0.00", CultureInfo.InvariantCulture),
                            hasil.Sosial.ToString("0.00", CultureInfo.InvariantCulture),
                            hasil.SkorAkhir.ToString("0.00", CultureInfo.InvariantCulture), hasil.Predikat, hasil.Rekomendasi)
            Else
                dt.Rows.Add(i + 1, item.MataKuliah, item.Kelas, item.Sks, "-", "-", "-", "-", "-", "Belum dinilai", "Belum ada respons mahasiswa.")
            End If
        Next

        dgRincianEvaluasi.ItemsSource = dt.DefaultView
        If indeksAgregat >= 0 AndAlso indeksAgregat < hasilBatch.Count Then
            TampilkanRingkasan(hasilBatch(indeksAgregat))
        Else
            ResetRingkasan("Belum ada mahasiswa yang mengisi kuesioner untuk kelas pada periode ini.")
        End If
    End Sub

    Private Sub TampilkanRingkasan(hasil As HasilEvaluasiFuzzy)
        txtSkorPedagogik.Text = hasil.Pedagogik.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorProfesional.Text = hasil.Profesional.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorKepribadian.Text = hasil.Kepribadian.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorSosial.Text = hasil.Sosial.ToString("0.00", CultureInfo.InvariantCulture)
        txtSkorAkhir.Text = hasil.SkorAkhir.ToString("0.00", CultureInfo.InvariantCulture)
        txtPredikat.Text = hasil.Predikat
        txtRekomendasi.Text = If(String.IsNullOrWhiteSpace(hasil.Rekomendasi), "-", hasil.Rekomendasi)
        txtPredikat.Foreground = CType(New BrushConverter().ConvertFromString(WarnaPredikatHex(hasil.Predikat)), System.Windows.Media.Brush)
    End Sub

    Private Sub ResetRingkasan(pesan As String)
        txtSkorPedagogik.Text = "0.00"
        txtSkorProfesional.Text = "0.00"
        txtSkorKepribadian.Text = "0.00"
        txtSkorSosial.Text = "0.00"
        txtSkorAkhir.Text = "00.00"
        txtPredikat.Text = "-"
        txtPredikat.Foreground = CType(New BrushConverter().ConvertFromString("#64748B"), System.Windows.Media.Brush)
        txtRekomendasi.Text = pesan
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
End Class
