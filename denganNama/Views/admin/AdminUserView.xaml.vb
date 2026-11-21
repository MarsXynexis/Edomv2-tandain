
Imports MySql.Data.MySqlClient
Imports System.Data

Public Class AdminUserView

    Private dtUser As DataTable

    Public Sub New()

        InitializeComponent()

        ' Role guard
        If Not String.Equals(UserSession.Role, "admin",
                             StringComparison.OrdinalIgnoreCase) Then

            MessageBox.Show(
                "Anda tidak memiliki akses ke halaman Admin.",
                "Akses Ditolak",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            )

            IsEnabled = False
            Return

        End If

        LoadData()

    End Sub

    ' ==========================================
    ' 1. LOAD DATA PENGGUNA
    ' ==========================================

    Private Sub LoadData()

        Try

            Using conn As MySqlConnection = GetConnection()

                If conn Is Nothing Then Return

                Dim query As String =
                    "SELECT id, username, nama, role, aktif " &
                    "FROM users ORDER BY id ASC"

                Using cmd As New MySqlCommand(query, conn)

                    Using da As New MySqlDataAdapter(cmd)

                        dtUser = New DataTable()
                        da.Fill(dtUser)

                        dgUser.ItemsSource = dtUser.DefaultView

                    End Using

                End Using

            End Using

            FilterData()

        Catch ex As Exception

            MessageBox.Show(
                "Gagal mengambil data pengguna: " & ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            )

        End Try

    End Sub

    ' ==========================================
    ' 2. PENCARIAN DAN FILTER ROLE
    ' ==========================================

    Private Sub FilterData()

        If dtUser Is Nothing Then Return

        Dim keyword As String = txtCari.Text.Trim()

        ' Escape karakter khusus untuk RowFilter
        keyword = keyword.Replace("'", "''")
        keyword = keyword.Replace("[", "[[]")
        keyword = keyword.Replace("%", "[%]")
        keyword = keyword.Replace("*", "[*]")

        Dim filter As String = ""

        If keyword <> "" Then

            filter =
                "(username LIKE '%" & keyword &
                "%' OR nama LIKE '%" & keyword & "%')"

        End If

        Dim selectedRole As ComboBoxItem =
            TryCast(cmbFilterRole.SelectedItem, ComboBoxItem)

        If selectedRole IsNot Nothing Then

            Dim role As String =
                selectedRole.Content.ToString()

            If role <> "Semua Role" Then

                If filter <> "" Then
                    filter &= " AND "
                End If

                filter &= "role = '" & role & "'"

            End If

        End If

        dtUser.DefaultView.RowFilter = filter

    End Sub

    Private Sub txtCari_TextChanged(
        sender As Object,
        e As TextChangedEventArgs)

        FilterData()

    End Sub

    Private Sub cmbFilterRole_SelectionChanged(
        sender As Object,
        e As SelectionChangedEventArgs)

        FilterData()

    End Sub

    ' ==========================================
    ' 3. TAMBAH PENGGUNA
    ' ==========================================

    Private Sub btnTambah_Click(
        sender As Object,
        e As RoutedEventArgs)

        Dim form As New UserFormWindow()

        form.Owner = Window.GetWindow(Me)

        If form.ShowDialog() = True Then

            SimpanPengguna(form)

        End If

    End Sub

    Private Sub SimpanPengguna(form As UserFormWindow)

        Try

            Using conn As MySqlConnection = GetConnection()

                If conn Is Nothing Then Return

                Dim query As String =
                    "INSERT INTO users " &
                    "(username, password, nama, role, id_kelas, aktif) " &
                    "VALUES (@username, SHA2(@password, 256), " &
                    "@nama, @role, NULL, 1)"

                Using cmd As New MySqlCommand(query, conn)

                    cmd.Parameters.AddWithValue(
                        "@username", form.UsernameValue)

                    cmd.Parameters.AddWithValue(
                        "@password", form.PasswordValue)

                    cmd.Parameters.AddWithValue(
                        "@nama", form.NamaValue)

                    cmd.Parameters.AddWithValue(
                        "@role", form.RoleValue)

                    cmd.ExecuteNonQuery()

                End Using

            End Using

            MessageBox.Show(
                "Pengguna berhasil ditambahkan.",
                "Berhasil",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            )

            LoadData()

        Catch ex As MySqlException

            If ex.Number = 1062 Then

                MessageBox.Show(
                    "Username sudah digunakan. Gunakan username lain.",
                    "Username Duplikat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                )

            Else

                MessageBox.Show(
                    "Gagal menambahkan pengguna: " & ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                )

            End If

        Catch ex As Exception

            MessageBox.Show(
                "Terjadi kesalahan: " & ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            )

        End Try

    End Sub

    ' ==========================================
    ' 4. EDIT PENGGUNA
    ' ==========================================

    Private Sub btnEdit_Click(
        sender As Object,
        e As RoutedEventArgs)

        If dgUser.SelectedItem Is Nothing Then

            MessageBox.Show("Pilih pengguna yang ingin diedit.")
            Return

        End If

        Dim selected As DataRowView =
            TryCast(dgUser.SelectedItem, DataRowView)

        If selected Is Nothing Then Return

        Dim userId As Integer = CInt(selected("id"))

        Dim form As New UserFormWindow()

        form.Owner = Window.GetWindow(Me)

        form.SetData(
            selected("username").ToString(),
            selected("nama").ToString(),
            selected("role").ToString()
        )

        If form.ShowDialog() = True Then

            UpdatePengguna(userId, form)

        End If

    End Sub

    Private Sub UpdatePengguna(
        userId As Integer,
        form As UserFormWindow)

        Try

            Using conn As MySqlConnection = GetConnection()

                If conn Is Nothing Then Return

                Dim query As String =
                    "UPDATE users SET nama = @nama, role = @role " &
                    "WHERE id = @id"

                ' Jika password diisi, password juga diperbarui
                If Not String.IsNullOrWhiteSpace(form.PasswordValue) Then

                    query =
                        "UPDATE users SET nama = @nama, role = @role, " &
                        "password = SHA2(@password, 256) WHERE id = @id"

                End If

                Using cmd As New MySqlCommand(query, conn)

                    cmd.Parameters.AddWithValue(
                        "@nama", form.NamaValue)

                    cmd.Parameters.AddWithValue(
                        "@role", form.RoleValue)

                    cmd.Parameters.AddWithValue(
                        "@id", userId)

                    If Not String.IsNullOrWhiteSpace(form.PasswordValue) Then

                        cmd.Parameters.AddWithValue(
                            "@password", form.PasswordValue)

                    End If

                    cmd.ExecuteNonQuery()

                End Using

            End Using

            MessageBox.Show(
                "Data pengguna berhasil diperbarui.",
                "Berhasil",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            )

            LoadData()

        Catch ex As Exception

            MessageBox.Show(
                "Gagal memperbarui pengguna: " & ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            )

        End Try

    End Sub

    ' ==========================================
    ' 5. NONAKTIFKAN PENGGUNA (SOFT DELETE)
    ' ==========================================

    Private Sub btnHapus_Click(
        sender As Object,
        e As RoutedEventArgs)

        If dgUser.SelectedItem Is Nothing Then

            MessageBox.Show("Pilih pengguna yang ingin dinonaktifkan.")
            Return

        End If

        Dim selected As DataRowView =
            TryCast(dgUser.SelectedItem, DataRowView)

        If selected Is Nothing Then Return

        Dim userId As Integer = CInt(selected("id"))

        Dim username As String =
            selected("username").ToString()

        ' Mencegah admin menonaktifkan akunnya sendiri
        If userId = UserSession.Id Then

            MessageBox.Show(
                "Anda tidak dapat menonaktifkan akun sendiri.",
                "Peringatan",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            )

            Return

        End If

        Dim konfirmasi As MessageBoxResult =
            MessageBox.Show(
                "Apakah Anda yakin ingin menonaktifkan pengguna " &
                username & "?",
                "Konfirmasi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            )

        If konfirmasi <> MessageBoxResult.Yes Then Return

        Try

            Using conn As MySqlConnection = GetConnection()

                If conn Is Nothing Then Return

                Dim query As String =
                    "UPDATE users SET aktif = 0 WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)

                    cmd.Parameters.AddWithValue("@id", userId)

                    cmd.ExecuteNonQuery()

                End Using

            End Using

            MessageBox.Show(
                "Pengguna berhasil dinonaktifkan.",
                "Berhasil",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            )

            LoadData()

        Catch ex As Exception

            MessageBox.Show(
                "Gagal menonaktifkan pengguna: " & ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            )

        End Try

    End Sub

End Class
