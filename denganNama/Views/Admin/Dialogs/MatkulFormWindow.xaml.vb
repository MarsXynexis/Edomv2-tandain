Public Class MatkulFormWindow

    Public Property KodeValue As String

    Public Property NamaValue As String

    Public Property SksValue As Integer

    Private Sub MatkulFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        txtKode.Focus()
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtKode.Text) OrElse String.IsNullOrWhiteSpace(txtNama.Text) Then
            MessageBox.Show("Kode dan nama mata kuliah wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim sks As Integer = 0
        If Not Integer.TryParse(txtSks.Text.Trim(), sks) OrElse sks < 1 OrElse sks > 6 Then
            MessageBox.Show("Bobot SKS harus angka antara 1 sampai 6.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtSks.Focus()
            Exit Sub
        End If

        KodeValue = txtKode.Text.Trim().ToUpper()
        NamaValue = txtNama.Text.Trim()
        SksValue = sks

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class
