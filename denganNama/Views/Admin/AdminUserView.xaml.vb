Imports System.Data
Imports MySql.Data.MySqlClient

Public Class AdminUserView

    Private dtUser As DataTable

    Private ReadOnly roleAwalFilter As String = "dosen"

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(filterRoleAwal As String)
        InitializeComponent()
        If Not String.IsNullOrWhiteSpace(filterRoleAwal) Then
            roleAwalFilter = filterRoleAwal.Trim().ToLower()
        End If
    End Sub

    Private Sub AdminUserView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not String.Equals(UserSession.Role, "admin", StringComparison.OrdinalIgnoreCase) Then
            MessageBox.Show("Akses halaman ini hanya untuk Admin Jurusan.", "Akses Ditolak", MessageBoxButton.OK, MessageBoxImage.Warning)
            IsEnabled = False
            Exit Sub
        End If

        TerapkanFilterAwal()
        LoadData()
    End Sub

    Private Sub TerapkanFilterAwal()
        For Each item As ComboBoxItem In cmbFilterRole.Items
            Dim tagRole As String = If(item.Tag, "").ToString().ToLower()
            If tagRole = roleAwalFilter Then
                cmbFilterRole.SelectedItem = item
                Exit For
            End If
        Next
    End Sub

    Private Sub LoadData()
        dtUser = New DataTable()
        dtUser.Columns.Add("id", GetType(Integer))
        dtUser.Columns.Add("username", GetType(String))
        dtUser.Columns.Add("nama", GetType(String))
        dtUser.Columns.Add("role", GetType(String))
        dtUser.Columns.Add("role_label", GetType(String))
        dtUser.Columns.Add("id_kelas", GetType(Object))
        dtUser.Columns.Add("pengampu_label", GetType(String))
        dtUser.Columns.Add("aktif", GetType(Boolean))
        dtUser.Columns.Add("status_label", GetType(String))
        dtUser.Columns.Add("status_color", GetType(String))

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT u.id, u.username, u.nama, u.role, u.id_kelas, u.aktif, " &
                                      "(SELECT COUNT(*) FROM pengampu p WHERE p.id_dosen = u.id AND p.aktif = TRUE) AS jml_pengampu " &
                                      "FROM users u " &
                                      "WHERE u.role IN ('dosen', 'admin') " &
                                      "ORDER BY u.aktif DESC, u.role DESC, u.nama ASC"

                Using cmd As New MySqlCommand(query, conn)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            Dim roleKode As String = rd("role").ToString().ToLower()
                            Dim aktif As Boolean = Convert.ToBoolean(rd("aktif"))
                            Dim jmlPengampu As Integer = Convert.ToInt32(rd("jml_pengampu"))
                            Dim idKelasObj As Object = If(IsDBNull(rd("id_kelas")), DBNull.Value, CObj(Convert.ToInt32(rd("id_kelas"))))
                            Dim pengampuTeks As String = "-"

                            If roleKode = "dosen" Then
                                pengampuTeks = If(jmlPengampu > 0, jmlPengampu.ToString() & " kelas · matkul", "Belum ditugaskan")
                            End If

                            dtUser.Rows.Add(
                                Convert.ToInt32(rd("id")),
                                rd("username").ToString(),
                                rd("nama").ToString(),
                                roleKode,
                                FormatLabelPeran(roleKode),
                                idKelasObj,
                                pengampuTeks,
                                aktif,
                                If(aktif, "Aktif", "Nonaktif"),
                                If(aktif, "#15803D", "#B91C1C")
                            )
                        End While
                    End Using
                End Using
            End Using

            dgUser.ItemsSource = dtUser.DefaultView
            FilterData()
        Catch ex As Exception
            MessageBox.Show("Gagal memuat data dosen: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Function FormatLabelPeran(roleKode As String) As String
        Select Case roleKode
            Case "dosen"
                Return "Dosen"
            Case "admin"
                Return "Admin Jurusan"
            Case Else
                Return roleKode
        End Select
    End Function

    Private Sub FilterData()
        If dtUser Is Nothing Then Exit Sub

        Dim keyword As String = EscapeRowFilter(txtCari.Text.Trim())
        Dim bagianFilter As New List(Of String)

        If Not String.IsNullOrEmpty(keyword) Then
            bagianFilter.Add(String.Format("(username LIKE '%{0}%' OR nama LIKE '%{0}%')", keyword))
        End If

        Dim itemPeran As ComboBoxItem = TryCast(cmbFilterRole.SelectedItem, ComboBoxItem)
        If itemPeran IsNot Nothing Then
            Dim tagRole As String = If(itemPeran.Tag, "").ToString().ToLower()
            If tagRole = "dosen" OrElse tagRole = "admin" Then
                bagianFilter.Add("role = '" & EscapeRowFilter(tagRole) & "'")
            End If
        End If

        dtUser.DefaultView.RowFilter = String.Join(" AND ", bagianFilter)

        Dim jumlahTampil As Integer = dtUser.DefaultView.Count
        txtKosongUser.Visibility = If(jumlahTampil = 0, Visibility.Visible, Visibility.Collapsed)
        txtJumlahUser.Text = "Menampilkan " & jumlahTampil.ToString() & " dari " & dtUser.Rows.Count.ToString() & " akun"
    End Sub

    Private Function EscapeRowFilter(nilai As String) As String
        Return nilai.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
    End Function

    Private Sub txtCari_TextChanged(sender As Object, e As TextChangedEventArgs)
        lblPlaceholderCari.Visibility = If(String.IsNullOrEmpty(txtCari.Text), Visibility.Visible, Visibility.Collapsed)
        FilterData()
    End Sub

    Private Sub cmbFilterRole_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        FilterData()
    End Sub

    Private Sub btnDetailDosen_Click(sender As Object, e As RoutedEventArgs)
        BukaDetailDosenTerpilih()
    End Sub

    Private Sub dgUser_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If dgUser.SelectedItem IsNot Nothing Then
            BukaDetailDosenTerpilih()
        End If
    End Sub

    Private Sub BukaDetailDosenTerpilih()
        Dim selected As DataRowView = TryCast(dgUser.SelectedItem, DataRowView)
        If selected Is Nothing Then
            MessageBox.Show("Pilih baris dosen terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If selected("role").ToString().ToLower() <> "dosen" Then
            MessageBox.Show("Detail evaluasi hanya tersedia untuk akun dengan peran Dosen.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information)
            Exit Sub
        End If

        Dim idDosen As Integer = Convert.ToInt32(selected("id"))
        Dim namaDosen As String = selected("nama").ToString()
        Dim nipDosen As String = selected("username").ToString()

        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then mainWin = TryCast(Application.Current.MainWindow, MainWindow)

        If mainWin IsNot Nothing Then
            mainWin.NavigasiKeHalaman(
                New AdminDetailDosenView(idDosen, namaDosen, nipDosen, 0, "dosen"),
                "Detail Evaluasi Dosen",
                namaDosen & " · Rincian per mata kuliah dan kelas",
                mainWin.btnAdminDosen
            )
        End If
    End Sub

    Private Sub btnTambah_Click(sender As Object, e As RoutedEventArgs)
        Dim frm As New UserFormWindow()
        frm.Owner = Window.GetWindow(Me)

        Dim itemAktif As ComboBoxItem = TryCast(cmbFilterRole.SelectedItem, ComboBoxItem)
        Dim roleDefault As String = If(itemAktif IsNot Nothing AndAlso If(itemAktif.Tag, "").ToString() = "admin", "admin", "dosen")
        frm.PresetRole(roleDefault)

        If frm.ShowDialog() = True Then
            SimpanPengguna(frm)
        End If
    End Sub

    Private Sub SimpanPengguna(frm As UserFormWindow)
        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "INSERT INTO users (username, password, nama, role, id_kelas, aktif) " &
                                      "VALUES (@username, @password, @nama, @role, @id_kelas, TRUE)"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@username", frm.UsernameValue)
                    cmd.Parameters.AddWithValue("@password", PasswordHelper.Sha256Hex(frm.PasswordValue))
                    cmd.Parameters.AddWithValue("@nama", frm.NamaValue)
                    cmd.Parameters.AddWithValue("@role", frm.RoleValue)
                    cmd.Parameters.AddWithValue("@id_kelas", If(frm.IdKelasValue.HasValue, CObj(frm.IdKelasValue.Value), DBNull.Value))
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            LoadData()
        Catch ex As MySqlException
            If ex.Number = 1062 Then
                MessageBox.Show("NIP atau username tersebut sudah terdaftar.", "Data Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning)
            Else
                MessageBox.Show("Gagal menambahkan akun: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("Gagal menambahkan akun: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs)
        Dim selected As DataRowView = TryCast(dgUser.SelectedItem, DataRowView)
        If selected Is Nothing Then
            MessageBox.Show("Pilih baris akun yang ingin diubah terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim userId As Integer = Convert.ToInt32(selected("id"))
        Dim idKelasAwal As Nullable(Of Integer) = Nothing

        If Not IsDBNull(selected("id_kelas")) Then
            idKelasAwal = Convert.ToInt32(selected("id_kelas"))
        End If

        Dim frm As New UserFormWindow()
        frm.Owner = Window.GetWindow(Me)
        frm.SetData(selected("username").ToString(), selected("nama").ToString(), selected("role").ToString(), idKelasAwal)

        If frm.ShowDialog() = True Then
            UpdatePengguna(userId, frm)
        End If
    End Sub

    Private Sub UpdatePengguna(userId As Integer, frm As UserFormWindow)
        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "UPDATE users SET nama = @nama, role = @role, id_kelas = @id_kelas WHERE id = @id"

                If Not String.IsNullOrWhiteSpace(frm.PasswordValue) Then
                    query = "UPDATE users SET nama = @nama, role = @role, id_kelas = @id_kelas, password = @password WHERE id = @id"
                End If

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@nama", frm.NamaValue)
                    cmd.Parameters.AddWithValue("@role", frm.RoleValue)
                    cmd.Parameters.AddWithValue("@id_kelas", If(frm.IdKelasValue.HasValue, CObj(frm.IdKelasValue.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@id", userId)

                    If Not String.IsNullOrWhiteSpace(frm.PasswordValue) Then
                        cmd.Parameters.AddWithValue("@password", PasswordHelper.Sha256Hex(frm.PasswordValue))
                    End If

                    cmd.ExecuteNonQuery()
                End Using
            End Using

            LoadData()
        Catch ex As Exception
            MessageBox.Show("Gagal memperbarui akun: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnResetPassword_Click(sender As Object, e As RoutedEventArgs)
        Dim selected As DataRowView = TryCast(dgUser.SelectedItem, DataRowView)
        If selected Is Nothing Then
            MessageBox.Show("Pilih akun yang ingin direset kata sandinya terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim userId As Integer = Convert.ToInt32(selected("id"))
        Dim namaAkun As String = selected("nama").ToString()
        Dim usernameAkun As String = selected("username").ToString()

        Dim konfirmasi As MessageBoxResult = MessageBox.Show(
            "Reset kata sandi " & namaAkun & " (" & usernameAkun & ") ke default 123456?",
            "Reset Kata Sandi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        )

        If konfirmasi <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "UPDATE users SET password = @password WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@password", PasswordHelper.Sha256Hex("123456"))
                    cmd.Parameters.AddWithValue("@id", userId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Kata sandi " & namaAkun & " berhasil direset menjadi 123456.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MessageBox.Show("Gagal mereset kata sandi: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub btnHapus_Click(sender As Object, e As RoutedEventArgs)
        Dim selected As DataRowView = TryCast(dgUser.SelectedItem, DataRowView)
        If selected Is Nothing Then
            MessageBox.Show("Pilih baris akun yang ingin dinonaktifkan terlebih dulu.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim userId As Integer = Convert.ToInt32(selected("id"))
        Dim username As String = selected("username").ToString()

        If userId = UserSession.Id Then
            MessageBox.Show("Akun yang sedang dipakai masuk tidak dapat dinonaktifkan.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If Not Convert.ToBoolean(selected("aktif")) Then
            MessageBox.Show("Akun ini statusnya sudah nonaktif.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information)
            Exit Sub
        End If

        Dim konfirmasi As MessageBoxResult = MessageBox.Show("Nonaktifkan akun " & username & "?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question)
        If konfirmasi <> MessageBoxResult.Yes Then Exit Sub

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "UPDATE users SET aktif = FALSE WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", userId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            LoadData()
        Catch ex As Exception
            MessageBox.Show("Gagal menonaktifkan akun: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

End Class
