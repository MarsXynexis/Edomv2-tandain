Imports System.Data

Class AdminKelasView

    Private dtKelas As DataTable

    Private Sub AdminKelasView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadData()
    End Sub

    ' ====== LOAD DATA ======
    Private Sub LoadData()
        ' TODO: ganti dengan query ke database kamu
        ' Contoh: dtKelas = Database.GetTable("SELECT id, prodi, angkatan, rombel, aktif FROM kelas")

        If dtKelas Is Nothing Then
            dtKelas = New DataTable()
            dtKelas.Columns.Add("id", GetType(Integer))
            dtKelas.Columns.Add("prodi", GetType(String))
            dtKelas.Columns.Add("angkatan", GetType(String))
            dtKelas.Columns.Add("rombel", GetType(String))
            dtKelas.Columns.Add("aktif", GetType(String))
        End If

        dgKelas.ItemsSource = dtKelas.DefaultView
    End Sub

    ' ====== SEARCH ======
    Private Sub txtCari_TextChanged(sender As Object, e As TextChangedEventArgs)
        If dtKelas Is Nothing Then Return

        Dim kata As String = txtCari.Text.Trim().Replace("'", "''")

        If kata = "" Then
            dtKelas.DefaultView.RowFilter = ""
        Else
            dtKelas.DefaultView.RowFilter =
                $"prodi LIKE '%{kata}%' OR angkatan LIKE '%{kata}%' OR rombel LIKE '%{kata}%'"
        End If
    End Sub

    ' ====== TAMBAH ======
    Private Sub btnTambah_Click(sender As Object, e As RoutedEventArgs)
        Dim frm As New KelasFormWindow()
        frm.Owner = Window.GetWindow(Me)

        If frm.ShowDialog() = True Then
            ' TODO: simpan ke database, contoh:
            ' Database.Execute("INSERT INTO kelas ...")
            Dim row As DataRow = dtKelas.NewRow()
            row("id") = dtKelas.Rows.Count + 1
            row("prodi") = frm.Prodi
            row("angkatan") = frm.Angkatan
            row("rombel") = frm.Rombel
            row("aktif") = If(frm.Aktif, "Aktif", "Nonaktif")
            dtKelas.Rows.Add(row)
        End If
    End Sub

    ' ====== EDIT ======
    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih data yang ingin diedit.", "Info",
                            MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        Dim frm As New KelasFormWindow()
        frm.Owner = Window.GetWindow(Me)
        frm.SetData(rv("prodi").ToString(), rv("angkatan").ToString(),
                    rv("rombel").ToString(), rv("aktif").ToString() = "Aktif")

        If frm.ShowDialog() = True Then
            ' TODO: UPDATE ke database
            rv("prodi") = frm.Prodi
            rv("angkatan") = frm.Angkatan
            rv("rombel") = frm.Rombel
            rv("aktif") = If(frm.Aktif, "Aktif", "Nonaktif")
        End If
    End Sub

    ' ====== NONAKTIFKAN ======
    Private Sub btnHapus_Click(sender As Object, e As RoutedEventArgs)
        Dim rv As DataRowView = TryCast(dgKelas.SelectedItem, DataRowView)
        If rv Is Nothing Then
            MessageBox.Show("Pilih data yang ingin dinonaktifkan.", "Info",
                            MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        If MessageBox.Show("Nonaktifkan kelas ini?", "Konfirmasi",
                           MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            ' TODO: UPDATE kelas SET aktif = 0 WHERE id = ...
            rv("aktif") = "Nonaktif"
        End If
    End Sub

End Class