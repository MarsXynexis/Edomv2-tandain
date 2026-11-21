Imports MySql.Data.MySqlClient

Public Class UserFormWindow

    Private Class PilihanKelas
        Public Property Id As Integer
        Public Property Teks As String

        Public Sub New(idKelas As Integer, labelKelas As String)
            Id = idKelas
            Teks = labelKelas
        End Sub
    End Class

    Public Property UsernameValue As String

    Public Property NamaValue As String

    Public Property RoleValue As String

    Public Property PasswordValue As String

    Public Property IdKelasValue As Nullable(Of Integer)

    Public Property IsEditMode As Boolean = False

    Public Sub New()
        InitializeComponent()
        MuatDaftarKelas()
    End Sub

    Private Sub UserFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If txtUsername.IsEnabled Then
            txtUsername.Focus()
        Else
            txtNama.Focus()
        End If
    End Sub

    Private Sub MuatDaftarKelas()
        cmbKelas.Items.Clear()

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT id, prodi, angkatan, rombel FROM kelas WHERE aktif = TRUE ORDER BY angkatan DESC, rombel ASC"

                Using cmd As New MySqlCommand(query, conn)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim teks As String = rd("angkatan").ToString() & " · " & rd("rombel").ToString() & " (" & rd("prodi").ToString() & ")"
                            cmbKelas.Items.Add(New PilihanKelas(Convert.ToInt32(rd("id")), teks))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Sub PresetRole(roleKode As String)
        Dim target As String = If(roleKode, "").Trim().ToLower()

        For Each item As ComboBoxItem In cmbRole.Items
            If If(item.Tag, "").ToString().ToLower() = target Then
                cmbRole.SelectedItem = item
                Exit For
            End If
        Next
    End Sub

    Public Sub SetData(username As String, nama As String, role As String, idKelas As Nullable(Of Integer))
        IsEditMode = True
        txtJudulForm.Text = "Edit Akun"
        txtUsername.Text = username
        txtUsername.IsEnabled = False
        txtNama.Text = nama
        txtPassword.Password = ""
        txtHintPassword.Text = "Kosongkan bila kata sandi tidak ingin diubah (minimal 6 karakter jika diisi)."

        PresetRole(role)
        AturVisibilitasKelas()

        If idKelas.HasValue Then
            For Each item As PilihanKelas In cmbKelas.Items
                If item.Id = idKelas.Value Then
                    cmbKelas.SelectedItem = item
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub cmbRole_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        AturVisibilitasKelas()
    End Sub

    Private Sub AturVisibilitasKelas()
        If panelKelas Is Nothing OrElse cmbRole Is Nothing Then Exit Sub

        Dim selectedRole As ComboBoxItem = TryCast(cmbRole.SelectedItem, ComboBoxItem)
        Dim kodeRole As String = If(selectedRole IsNot Nothing AndAlso selectedRole.Tag IsNot Nothing, selectedRole.Tag.ToString().ToLower(), "")

        If kodeRole = "mahasiswa" Then
            panelKelas.Visibility = Visibility.Visible
        Else
            panelKelas.Visibility = Visibility.Collapsed
            cmbKelas.SelectedItem = Nothing
        End If
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("NIM, NIP, atau username wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtUsername.Focus()
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(txtNama.Text) Then
            MessageBox.Show("Nama lengkap wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtNama.Focus()
            Exit Sub
        End If

        Dim selectedRole As ComboBoxItem = TryCast(cmbRole.SelectedItem, ComboBoxItem)
        If selectedRole Is Nothing Then
            MessageBox.Show("Pilih peran pengguna terlebih dulu.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            cmbRole.Focus()
            Exit Sub
        End If

        Dim roleKode As String = selectedRole.Tag.ToString().ToLower()
        Dim idKelasDipilih As Nullable(Of Integer) = Nothing

        If roleKode = "mahasiswa" Then
            Dim kelasDipilih As PilihanKelas = TryCast(cmbKelas.SelectedItem, PilihanKelas)
            If kelasDipilih Is Nothing Then
                MessageBox.Show("Pilih kelas rombel untuk akun mahasiswa.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
                cmbKelas.Focus()
                Exit Sub
            End If

            idKelasDipilih = kelasDipilih.Id
        End If

        Dim passInput As String = txtPassword.Password.Trim()

        If Not IsEditMode AndAlso passInput.Length < 6 Then
            MessageBox.Show("Kata sandi akun baru minimal 6 karakter.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPassword.Focus()
            Exit Sub
        End If

        If IsEditMode AndAlso Not String.IsNullOrEmpty(passInput) AndAlso passInput.Length < 6 Then
            MessageBox.Show("Kata sandi baru minimal 6 karakter.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPassword.Focus()
            Exit Sub
        End If

        UsernameValue = txtUsername.Text.Trim()
        NamaValue = txtNama.Text.Trim()
        RoleValue = roleKode
        IdKelasValue = idKelasDipilih
        PasswordValue = passInput

        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

End Class
