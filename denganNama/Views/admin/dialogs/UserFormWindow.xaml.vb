
Public Class UserFormWindow

    Public Property UsernameValue As String
    Public Property NamaValue As String
    Public Property RoleValue As String
    Public Property PasswordValue As String

    Public Property IsEditMode As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

    ' Mengisi data ketika form dibuka untuk edit
    Public Sub SetData(username As String,
                       nama As String,
                       role As String)

        IsEditMode = True

        txtJudulForm.Text = "Edit Pengguna"

        txtUsername.Text = username
        txtUsername.IsEnabled = False

        txtNama.Text = nama

        For Each item As ComboBoxItem In cmbRole.Items
            If item.Content.ToString().ToLower() = role.ToLower() Then
                cmbRole.SelectedItem = item
                Exit For
            End If
        Next

        txtPassword.Password = ""

    End Sub

    ' Tombol simpan
    Private Sub btnSimpan_Click(sender As Object,
                                e As RoutedEventArgs)

        If String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("Username harus diisi.")
            txtUsername.Focus()
            Return
        End If

        If String.IsNullOrWhiteSpace(txtNama.Text) Then
            MessageBox.Show("Nama lengkap harus diisi.")
            txtNama.Focus()
            Return
        End If

        If cmbRole.SelectedItem Is Nothing Then
            MessageBox.Show("Silakan pilih role pengguna.")
            cmbRole.Focus()
            Return
        End If

        If Not IsEditMode AndAlso
           String.IsNullOrWhiteSpace(txtPassword.Password) Then

            MessageBox.Show("Password harus diisi.")
            txtPassword.Focus()
            Return

        End If

        UsernameValue = txtUsername.Text.Trim()
        NamaValue = txtNama.Text.Trim()

        Dim selectedRole As ComboBoxItem =
            DirectCast(cmbRole.SelectedItem, ComboBoxItem)

        RoleValue = selectedRole.Content.ToString()

        PasswordValue = txtPassword.Password

        Me.DialogResult = True

    End Sub

    ' Tombol batal
    Private Sub btnBatal_Click(sender As Object,
                               e As RoutedEventArgs)

        Me.DialogResult = False

    End Sub

End Class
