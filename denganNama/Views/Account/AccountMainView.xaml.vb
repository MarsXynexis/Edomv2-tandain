Imports MySql.Data.MySqlClient

Public Class AccountMainView

    Private Sub AccountMainView_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        If Not UserSession.IsLoggedIn() Then Exit Sub

        MuatProfilPengguna()
    End Sub

    Private Sub MuatProfilPengguna()
        Dim namaPengguna As String = If(Not String.IsNullOrEmpty(UserSession.Nama), UserSession.Nama, UserSession.Username)

        lblInisial.Text = BuatInisial(namaPengguna)
        lblNama.Text = namaPengguna
        lblNamaLengkap.Text = namaPengguna
        lblUsername.Text = UserSession.Username

        Select Case UserSession.Role
            Case "mahasiswa"
                Dim infoKelas As String = AmbilInfoKelas(UserSession.IdKelas)

                lblJudulId.Text = "NIM"
                lblRole.Text = "Mahasiswa"
                lblJudulUnit.Text = "Kelas"
                lblUnit.Text = infoKelas
                lblSubIdentitas.Text = "Mahasiswa · " & infoKelas

            Case "dosen"
                lblJudulId.Text = "NIP"
                lblRole.Text = "Dosen"
                lblJudulUnit.Text = "Jurusan"
                lblUnit.Text = "Teknik Informatika dan Komputer"
                lblSubIdentitas.Text = "Dosen · Teknik Informatika dan Komputer"

            Case Else
                lblJudulId.Text = "ID Admin"
                lblRole.Text = "Admin Jurusan"
                lblJudulUnit.Text = "Jurusan"
                lblUnit.Text = "Teknik Informatika dan Komputer"
                lblSubIdentitas.Text = "Admin Jurusan · Teknik Informatika dan Komputer"
        End Select
    End Sub

    Private Function AmbilInfoKelas(idKelas As Nullable(Of Integer)) As String
        If Not idKelas.HasValue Then Return "-"

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Return "-"

                Dim query As String = "SELECT CONCAT('TI - ', rombel, ' (Angkatan ', angkatan, ')') FROM kelas WHERE id = @id LIMIT 1"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", idKelas.Value)

                    Dim hasil As Object = cmd.ExecuteScalar()

                    If hasil IsNot Nothing AndAlso Not IsDBNull(hasil) Then
                        Return hasil.ToString()
                    End If
                End Using
            End Using
        Catch ex As Exception
        End Try

        Return "-"
    End Function

    Private Function BuatInisial(nama As String) As String
        If String.IsNullOrWhiteSpace(nama) Then Return "-"

        Dim bagian As String() = nama.Trim().Split({" "c}, StringSplitOptions.RemoveEmptyEntries)

        If bagian.Length = 1 Then
            Return bagian(0).Substring(0, 1).ToUpper()
        End If

        Return (bagian(0).Substring(0, 1) & bagian(1).Substring(0, 1)).ToUpper()
    End Function

    Private Sub btnGantiPassword_Click(sender As Object, e As RoutedEventArgs)
        Dim mainWin As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWin Is Nothing Then
            mainWin = TryCast(Application.Current.MainWindow, MainWindow)
        End If

        If mainWin IsNot Nothing Then
            mainWin.NavigasiKeHalaman(New PasswordChangeView(), "Ganti Kata Sandi", "Perbarui kata sandi untuk akun ini", mainWin.btnMenuAkun)
        End If
    End Sub

End Class
