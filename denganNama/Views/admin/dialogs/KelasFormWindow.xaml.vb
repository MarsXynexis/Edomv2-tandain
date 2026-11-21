Class KelasFormWindow

    ' Property untuk mengambil hasil input
    Public Property Prodi As String
    Public Property Angkatan As String
    Public Property Rombel As String
    Public Property Aktif As Boolean = True

    ' Dipanggil saat mode Edit
    Public Sub SetData(prodi As String, angkatan As String, rombel As String, aktif As Boolean)
        txtJudulForm.Text = "Edit Kelas"
        txtProdi.Text = prodi
        txtAngkatan.Text = angkatan
        txtRombel.Text = rombel
        chkAktif.IsChecked = aktif
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtProdi.Text) OrElse
           String.IsNullOrWhiteSpace(txtAngkatan.Text) OrElse
           String.IsNullOrWhiteSpace(txtRombel.Text) Then
            MessageBox.Show("Semua field wajib diisi.", "Validasi",
                            MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim thn As Integer
        If Not Integer.TryParse(txtAngkatan.Text, thn) OrElse txtAngkatan.Text.Length <> 4 Then
            MessageBox.Show("Angkatan harus berupa tahun 4 digit, contoh: 2025.", "Validasi",
                            MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Prodi = txtProdi.Text.Trim()
        Angkatan = txtAngkatan.Text.Trim()
        Rombel = txtRombel.Text.Trim().ToUpper()
        Aktif = chkAktif.IsChecked = True

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class