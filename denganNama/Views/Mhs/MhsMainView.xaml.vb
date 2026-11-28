Imports System.Collections.ObjectModel
Imports System.Windows.Navigation
Imports MySql.Data.MySqlClient

Public Class MhsMainView

    Private cmdPeriodeSemester As Object

    Private Class PilihanPeriode
        Public Property Id As Integer
        Public Property Teks As String

        Public Sub New(idItem As Integer, teksItem As String)
            Id = idItem
            Teks = teksItem
        End Sub

        Public Overrides Function ToString() As String
            Return Teks
        End Function
    End Class

    Private sedangMuat As Boolean = False
    Public Property MainFrame As Object

    Private Sub MuatDaftarPeriode()
        sedangMuat = True
        cmbPeriodeSemester.Items.Clear()

        Try
            Using conn As MySqlConnection = db.GetConnection()
                If conn Is Nothing Then Exit Sub

                Dim query As String = "SELECT id, nama_periode, status_aktif FROM periode_edom ORDER BY status_aktif DESC, tahun_ajaran DESC, semester DESC"
                Using cmd As New MySqlCommand(query, conn)
                    Using rd As MySqlDataReader = cmd.ExecuteReader()
                        Dim idxAktif As Integer = -1
                        Dim idx As Integer = 0

                        While rd.Read()
                            Dim Aktif As Boolean = Convert.ToBoolean(rd("status_aktif"))
                            cmbPeriodeSemester.Items.Add(New PilihanPeriode(Convert.ToInt32(rd("id")), rd("nama_periode").ToString()))
                            If Aktif AndAlso idxAktif = -1 Then idxAktif = idx
                            idx += 1
                        End While

                        If cmbPeriodeSemester.Items.Count > 0 Then
                            cmbPeriodeSemester.SelectedIndex = If(idxAktif >= 0, idxAktif, 0)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Gagal memuat daftar periode: " & ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            sedangMuat = False
        End Try
    End Sub

    Private Sub MhsMainView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        MuatDaftarPeriode()
    End Sub


    Private Sub cmbPeriodeSemester_SelectionChanged(Sender As Object, e As SelectionChangedEventArgs)
        If sedangMuat Then Exit Sub

        Dim itemTerpilih As PilihanPeriode = TryCast(cmbPeriodeSemester.SelectedItem, PilihanPeriode)
        If itemTerpilih IsNot Nothing Then
            Dim idPeriode As Integer = itemTerpilih.Id
        End If
    End Sub

    Private Sub btnTampilkan_Click(Sender As Object, e As RoutedEventArgs)
        Dim navService As NavigationService = NavigationService.GetNavigationService(Me)

        If navService IsNot Nothing Then
            navService.Navigate(New MhsQuestionView())
        Else
            MessageBox.Show("Navigation service not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            MainFrame.Navigate(New MhsQuestionView())
        End If
    End Sub




End Class
