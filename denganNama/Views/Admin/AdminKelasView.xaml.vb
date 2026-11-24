Imports System.ComponentModel
Imports System.Data
Imports System.Text.RegularExpressions
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Imports MySql.Data.MySqlClient

Public Class AdminKelasView

    Private Class PilihanFilter
        Public Property Nilai As String
        Public Property Teks As String

        Public Sub New(nilaiFilter As String, labelTeks As String)
            Nilai = nilaiFilter
            Teks = labelTeks
        End Sub
    End Class

    Private dtKelas As DataTable

    Private daftarProdiMaster As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)

    Private sedangMuatFilter As Boolean = False

    Private Sub AdminKelasView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadData()
    End Sub

    Private Sub LoadData()
        dtKelas = New DataTable()
        dtKelas.Columns.Add("id", GetType(Integer))
        dtKelas.Columns.Add("prodi", GetType(String))
        dtKelas.Columns.Add("angkatan", GetType(String))
        dtKelas.Columns.Add("angkatan_group", GetType(String))
        dtKelas.Columns.Add("rombel", GetType(String))
        dtKelas.Columns.Add("rombel_label", GetType(String))
        dtKelas.Columns.Add("total_mhs_label", GetType(String))
        dtKelas.Columns.Add("total_pengampu_label", GetType(String))
        dtKelas.Columns.Add("aktif", GetType(Boolean))
        dtKelas.Columns.Add("status_label", GetType(String))
        dtKelas.Columns.Add("status_color", GetType(String))

        Dim daftarAngkatan As New SortedSet(Of Integer)(Comparer(Of Integer).Create(Function(a, b) b.CompareTo(a)))
        daftarProdiMaster = New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT k.id, k.prodi, k.angkatan, k.rombel, k.aktif, " &
                                      "(SELECT COUNT(*) FROM users u WHERE u.id_kelas = k.id AND u.role = 'mahasiswa' AND u.aktif = TRUE) AS total_mhs, " &
                                      "(SELECT COUNT(*) FROM pengampu p WHERE p.id_kelas = k.id AND p.aktif = TRUE) AS total_pengampu " &
                                      "FROM kelas k " &
                                      "ORDER BY k.angkatan DESC, k.prodi ASC, k.aktif DESC, k.rombel ASC"

                Using cmd As New MySqlCommand(query, conn)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim prodiStr As String = rd("prodi").ToString().Trim()
                            Dim aktif As Boolean = Convert.ToBoolean(rd("aktif"))
                            Dim angkatanInt As Integer = Convert.ToInt32(rd("angkatan"))
                            Dim angkatanStr As String = angkatanInt.ToString()
                            Dim rombelStr As String = rd("rombel").ToString().Trim()
                            Dim totalMhs As Integer = Convert.ToInt32(rd("total_mhs"))
                            Dim totalPengampu As Integer = Convert.ToInt32(rd("total_pengampu"))

                            daftarAngkatan.Add(angkatanInt)
                            If Not String.IsNullOrEmpty(prodiStr) Then
                                daftarProdiMaster.Add(prodiStr)
                            End If

                            dtKelas.Rows.Add(
                                Convert.ToInt32(rd("id")),
                                prodiStr,
                                angkatanStr,
                                "Tahun Masuk " & angkatanStr,
                                rombelStr,
                                "Rombel " & rombelStr,
                                totalMhs.ToString() & " Mahasiswa",
                                totalPengampu.ToString() & " Mata Kuliah",
                                aktif,
                                If(aktif, "Aktif", "Nonaktif"),
                                If(aktif, "#15803D", "#B91C1C")
                            )
                        End While
                    End Using
                End Using
            End Using

            IsiPilihanFilter(daftarProdiMaster, daftarAngkatan)

            Dim view As ICollectionView = CollectionViewSource.GetDefaultView(dtKelas.DefaultView)
            view.GroupDescriptions.Clear()
            view.GroupDescriptions.Add(New PropertyGroupDescription("angkatan_group"))
            view.GroupDescriptions.Add(New PropertyGroupDescription("prodi"))
            dgKelas.ItemsSource = view

            TerapkanFilter()
        Catch ex As Exception
            MessageBox.Show("Gagal memuat data kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub IsiPilihanFilter(daftarProdi As SortedSet(Of String), daftarAngkatan As SortedSet(Of Integer))
        Dim prodiLama As String = If(TryCast(cmbFilterProdi.SelectedItem, PilihanFilter) IsNot Nothing, DirectCast(cmbFilterProdi.SelectedItem, PilihanFilter).Nilai, "")
        Dim tahunLama As String = If(TryCast(cmbFilterAngkatan.SelectedItem, PilihanFilter) IsNot Nothing, DirectCast(cmbFilterAngkatan.SelectedItem, PilihanFilter).Nilai, "")

        sedangMuatFilter = True

        cmbFilterProdi.Items.Clear()
        cmbFilterProdi.Items.Add(New PilihanFilter("", "Semua Program Studi"))
        Dim idxProdi As Integer = 0
        Dim idx As Integer = 1
        For Each prodi As String In daftarProdi
            cmbFilterProdi.Items.Add(New PilihanFilter(prodi, prodi))
            If String.Equals(prodi, prodiLama, StringComparison.OrdinalIgnoreCase) Then
                idxProdi = idx
            End If
            idx += 1
        Next
        cmbFilterProdi.SelectedIndex = idxProdi

        cmbFilterAngkatan.Items.Clear()
        cmbFilterAngkatan.Items.Add(New PilihanFilter("", "Semua Tahun Masuk"))
        Dim idxTahun As Integer = 0
        idx = 1
        For Each thn As Integer In daftarAngkatan
            Dim thnStr As String = thn.ToString()
            cmbFilterAngkatan.Items.Add(New PilihanFilter(thnStr, "Tahun Masuk " & thnStr))
            If thnStr = tahunLama Then
                idxTahun = idx
            End If
            idx += 1
        Next
        cmbFilterAngkatan.SelectedIndex = idxTahun

        sedangMuatFilter = False
    End Sub

    Private Sub txtCari_TextChanged(sender As Object, e As TextChangedEventArgs)
        lblPlaceholderCari.Visibility = If(String.IsNullOrEmpty(txtCari.Text), Visibility.Visible, Visibility.Collapsed)
        TerapkanFilter()
    End Sub

    Private Sub cmbFilterProdi_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatFilter Then Exit Sub
        TerapkanFilter()
    End Sub

    Private Sub cmbFilterAngkatan_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatFilter Then Exit Sub
        TerapkanFilter()
    End Sub

    Private Sub cmbFilterStatus_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If sedangMuatFilter Then Exit Sub
        TerapkanFilter()
    End Sub

    Private Sub TerapkanFilter()
        If dtKelas Is Nothing Then Exit Sub

        Dim bagian As New List(Of String)()
        Dim kata As String = EscapeRowFilter(txtCari.Text.Trim())

        If Not String.IsNullOrEmpty(kata) Then
            bagian.Add(String.Format("(prodi LIKE '%{0}%' OR angkatan LIKE '%{0}%' OR rombel LIKE '%{0}%')", kata))
        End If

        Dim itemProdi As PilihanFilter = TryCast(cmbFilterProdi.SelectedItem, PilihanFilter)
        If itemProdi IsNot Nothing AndAlso Not String.IsNullOrEmpty(itemProdi.Nilai) Then
            bagian.Add("prodi = '" & EscapeRowFilter(itemProdi.Nilai) & "'")
        End If

        Dim itemTahun As PilihanFilter = TryCast(cmbFilterAngkatan.SelectedItem, PilihanFilter)
        If itemTahun IsNot Nothing AndAlso Not String.IsNullOrEmpty(itemTahun.Nilai) Then
            bagian.Add("angkatan = '" & EscapeRowFilter(itemTahun.Nilai) & "'")
        End If

        Dim itemStatus As ComboBoxItem = TryCast(cmbFilterStatus.SelectedItem, ComboBoxItem)
        If itemStatus IsNot Nothing Then
            Dim tagStatus As String = If(itemStatus.Tag, "").ToString()
            If Not String.IsNullOrEmpty(tagStatus) Then
                bagian.Add("status_label = '" & EscapeRowFilter(tagStatus) & "'")
            End If
        End If

        dtKelas.DefaultView.RowFilter = String.Join(" AND ", bagian)

        Dim jumlahTampil As Integer = dtKelas.DefaultView.Count
        txtKosongKelas.Visibility = If(jumlahTampil = 0, Visibility.Visible, Visibility.Collapsed)
        txtJumlahKelas.Text = "Menampilkan " & jumlahTampil.ToString() & " dari " & dtKelas.Rows.Count.ToString() & " kelas"
    End Sub

    Private Function EscapeRowFilter(nilai As String) As String
        Return nilai.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
    End Function

    Private Function CocokkanMasterProdi(prodiInput As String) As String
        Dim bersih As String = Regex.Replace(prodiInput.Trim(), "\s+", " ")
        For Each item As String In daftarProdiMaster
            If String.Equals(item, bersih, StringComparison.OrdinalIgnoreCase) Then
                Return item
            End If
        Next

        Return bersih
    End Function

    Private Function ApakahKlikBaris(source As Object) As Boolean
        Dim node As DependencyObject = TryCast(source, DependencyObject)
        While node IsNot Nothing
            If TypeOf node Is DataGridRow Then Return True
            If TypeOf node Is Visual OrElse TypeOf node Is Visual3D Then
                node = VisualTreeHelper.GetParent(node)
            Else
                node = LogicalTreeHelper.GetParent(node)
            End If
        End While

        Return False
    End Function

    Private Sub btnDetail_Click(sender As Object, e As RoutedEventArgs)
        BukaDetailKelasTerpilih()
    End Sub

    Private Sub dgKelas_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If ApakahKlikBaris(e.OriginalSource) Then
            BukaDetailKelasTerpilih()
        End If
    End Sub

    Private Sub BukaDetailKelasTerpilih()
        Dim rv As DataRowView = TryCast(dgKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih baris kelas yang ingin dibuka detailnya terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idKelas As Integer = Convert.ToInt32(rv("id"))
        Dim prodi As String = rv("prodi").ToString()
        Dim angkatan As String = rv("angkatan").ToString()
        Dim rombel As String = rv("rombel").ToString()

        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then mainWin = TryCast(Application.Current.MainWindow, MainWindow)

        If mainWin IsNot Nothing Then
            mainWin.NavigasiKeHalaman(
                New DetailKelasView(idKelas, prodi, angkatan, rombel),
                "Detail Kelas " & angkatan & " · " & rombel,
                "",
                mainWin.btnAdminKelas
            )
        End If
    End Sub

    Private Sub btnTambah_Click(sender As Object, e As RoutedEventArgs)
        Dim frm As New KelasFormWindow()
        frm.Owner = Window.GetWindow(Me)
        frm.SetDaftarProdi(daftarProdiMaster)

        If frm.ShowDialog() = True Then
            Try
                Dim prodiFinal As String = CocokkanMasterProdi(frm.Prodi)

                Using conn As MySqlConnection = db.GetConnection()
                    If conn Is Nothing Then Exit Sub

                    Dim query As String = "INSERT INTO kelas (prodi, angkatan, rombel, aktif) VALUES (@prodi, @angkatan, @rombel, @aktif)"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@prodi", prodiFinal)
                        cmd.Parameters.AddWithValue("@angkatan", Convert.ToInt32(frm.Angkatan))
                        cmd.Parameters.AddWithValue("@rombel", frm.Rombel)
                        cmd.Parameters.AddWithValue("@aktif", frm.Aktif)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                LoadData()
                SinkronkanFilterPascaSimpan(prodiFinal, frm.Angkatan, frm.Aktif)
            Catch ex As MySqlException
                If ex.Number = 1062 Then
                    MessageBox.Show("Kelas dengan prodi, tahun masuk, dan rombel tersebut sudah terdaftar.", "Data Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning)
                Else
                    MessageBox.Show("Gagal menambahkan kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                MessageBox.Show("Gagal menambahkan kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih baris kelas yang ingin diubah terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim idKelas As Integer = Convert.ToInt32(rv("id"))
        Dim frm As New KelasFormWindow()
        frm.Owner = Window.GetWindow(Me)
        frm.SetDaftarProdi(daftarProdiMaster)
        frm.SetData(rv("prodi").ToString(), rv("angkatan").ToString(), rv("rombel").ToString(), Convert.ToBoolean(rv("aktif")))

        If frm.ShowDialog() = True Then
            Try
                Dim prodiFinal As String = CocokkanMasterProdi(frm.Prodi)

                Using conn As MySqlConnection = db.GetConnection()
                    If conn Is Nothing Then Exit Sub

                    Dim query As String = "UPDATE kelas SET prodi = @prodi, angkatan = @angkatan, rombel = @rombel, aktif = @aktif WHERE id = @id"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@prodi", prodiFinal)
                        cmd.Parameters.AddWithValue("@angkatan", Convert.ToInt32(frm.Angkatan))
                        cmd.Parameters.AddWithValue("@rombel", frm.Rombel)
                        cmd.Parameters.AddWithValue("@aktif", frm.Aktif)
                        cmd.Parameters.AddWithValue("@id", idKelas)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                LoadData()
                SinkronkanFilterPascaSimpan(prodiFinal, frm.Angkatan, frm.Aktif)
            Catch ex As MySqlException
                If ex.Number = 1062 Then
                    MessageBox.Show("Kombinasi prodi, tahun masuk, dan rombel sudah dipakai kelas lain.", "Data Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning)
                Else
                    MessageBox.Show("Gagal memperbarui kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                MessageBox.Show("Gagal memperbarui kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If
    End Sub

    Private Sub SinkronkanFilterPascaSimpan(prodiKelas As String, angkatanKelas As String, aktifKelas As Boolean)
        sedangMuatFilter = True

        Dim itemProdi As PilihanFilter = TryCast(cmbFilterProdi.SelectedItem, PilihanFilter)
        If itemProdi IsNot Nothing AndAlso Not String.IsNullOrEmpty(itemProdi.Nilai) AndAlso
           Not String.Equals(itemProdi.Nilai, prodiKelas, StringComparison.OrdinalIgnoreCase) Then
            cmbFilterProdi.SelectedIndex = 0
        End If

        Dim itemTahun As PilihanFilter = TryCast(cmbFilterAngkatan.SelectedItem, PilihanFilter)
        If itemTahun IsNot Nothing AndAlso Not String.IsNullOrEmpty(itemTahun.Nilai) AndAlso
           itemTahun.Nilai <> angkatanKelas Then
            cmbFilterAngkatan.SelectedIndex = 0
        End If

        Dim itemStatus As ComboBoxItem = TryCast(cmbFilterStatus.SelectedItem, ComboBoxItem)
        Dim statusTarget As String = If(aktifKelas, "Aktif", "Nonaktif")
        If itemStatus IsNot Nothing AndAlso Not String.IsNullOrEmpty(If(itemStatus.Tag, "").ToString()) AndAlso
           itemStatus.Tag.ToString() <> statusTarget Then
            cmbFilterStatus.SelectedIndex = 2
        End If

        sedangMuatFilter = False
        TerapkanFilter()
    End Sub

    Private Sub btnHapus_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih baris kelas yang ingin dinonaktifkan terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If Not Convert.ToBoolean(rv("aktif")) Then
            MessageBox.Show("Kelas ini statusnya sudah nonaktif.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information)
            Exit Sub
        End If

        Dim labelKelas As String = rv("angkatan").ToString() & " · " & rv("rombel").ToString()
        Dim konfirmasi As MessageBoxResult = MessageBox.Show("Nonaktifkan kelas " & labelKelas & "?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question)

        If konfirmasi <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "UPDATE kelas SET aktif = FALSE WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", Convert.ToInt32(rv("id")))
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            LoadData()
        Catch ex As Exception
            MessageBox.Show("Gagal menonaktifkan kelas: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

End Class
