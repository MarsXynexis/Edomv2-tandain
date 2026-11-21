Public Class KelasFormWindow

    Public Property Prodi As String

    Public Property Angkatan As String

    Public Property Rombel As String

    Public Property Aktif As Boolean = True

    Private Sub KelasFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If String.IsNullOrWhiteSpace(txtProdi.Text) Then
            txtProdi.Text = "Sarjana Terapan Teknik Informatika"
            txtProdi.SelectAll()
        End If

        txtProdi.Focus()
    End Sub

    Public Sub SetData(prodiAwal As String, angkatanAwal As String, rombelAwal As String, aktifAwal As Boolean)
        txtJudulForm.Text = "Edit Kelas"
        txtProdi.Text = prodiAwal
        txtAngkatan.Text = angkatanAwal
        txtRombel.Text = rombelAwal
        chkAktif.IsChecked = aktifAwal
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtProdi.Text) OrElse
           String.IsNullOrWhiteSpace(txtAngkatan.Text) OrElse
           String.IsNullOrWhiteSpace(txtRombel.Text) Then
            MessageBox.Show("Program studi, angkatan, dan rombel wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim tahun As Integer
        If Not Integer.TryParse(txtAngkatan.Text.Trim(), tahun) OrElse txtAngkatan.Text.Trim().Length <> 4 Then
            MessageBox.Show("Angkatan harus berupa 4 digit angka tahun, misalnya 2025.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtAngkatan.Focus()
            Exit Sub
        End If

        Prodi = txtProdi.Text.Trim()
        Angkatan = tahun.ToString()
        Rombel = txtRombel.Text.Trim().ToUpper()
        Aktif = (chkAktif.IsChecked = True)

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class
