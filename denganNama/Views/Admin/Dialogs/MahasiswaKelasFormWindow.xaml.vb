Public Class MahasiswaKelasFormWindow

    Public Property NimValue As String

    Public Property NamaValue As String

    Public Property PasswordValue As String

    Public Property IsEditMode As Boolean = False

    Private Sub MahasiswaKelasFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If txtNim.IsEnabled Then
            txtNim.Focus()
        Else
            txtNama.Focus()
        End If
    End Sub

    Public Sub SetEditData(nimAwal As String, namaAwal As String)
        IsEditMode = True
        txtJudulForm.Text = "Edit Data Mahasiswa"
        txtSubJudulForm.Text = "Perbarui nama lengkap mahasiswa."
        txtNim.Text = nimAwal
        txtNim.IsEnabled = False
        txtNama.Text = namaAwal
        panelPassword.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtNim.Text) OrElse String.IsNullOrWhiteSpace(txtNama.Text) Then
            MessageBox.Show("NIM dan nama lengkap wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim passInput As String = txtPassword.Password.Trim()
        If Not IsEditMode AndAlso Not String.IsNullOrEmpty(passInput) AndAlso passInput.Length < 6 Then
            MessageBox.Show("Kata sandi awal minimal 6 karakter, atau kosongkan untuk memakai default mhs.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPassword.Focus()
            Exit Sub
        End If

        NimValue = txtNim.Text.Trim()
        NamaValue = txtNama.Text.Trim()
        PasswordValue = If(String.IsNullOrEmpty(passInput), "mhs", passInput)

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class
