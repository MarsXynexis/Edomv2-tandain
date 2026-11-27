Imports System.Collections.ObjectModel

Public Class MhsMainView
    Public Property DaftarDosenMhs As ObservableCollection(Of DosenMhs)
    Public Property DaftarMatkulMhs As ObservableCollection(Of MatkulMhs)

    Private Sub cmbPeriodeSemester_SelectionChanged(Sender As Object, e As SelectionChangedEventArgs)

    End Sub

    Private Sub btnTampilkan_Click(Sender As Object, e As RoutedEventArgs)

    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        DaftarMatkulMhs = New ObservableCollection(Of MatkulMhs) From {
            New MatkulMhs With {.Nim = "123456", .Nama = "Algoritma Pemrograman", .Jurusan = "Teknik Informatika dan Komputer", .Prodi = "Teknik Informatika, Teknik Multimedia Jaringan, Teknik Multimedia Digital"},
            New MatkulMhs With {.Nim = "123457", .Nama = "Aljabar Linear", .Jurusan = "Teknik Informatika dan Komputer", .Prodi = "Teknik Informatika, Teknik Multimedia Jaringan, Teknik Multimedia Digital"}
        }

        DaftarDosenMhs = New ObservableCollection(Of DosenMhs) From {
            New DosenMhs With {.Nama = "Jacobi, S.Kom., M.Kom", .Kode = "TIK101", .Matkul = "Aljabar Linear"}
        }




        Me.DataContext = Me
    End Sub
End Class
