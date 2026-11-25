Imports System.Windows.Input

Public Class KelasFormWindow

    Public Property Prodi As String

    Public Property Angkatan As String

    Public Property Rombel As String

    Public Property Aktif As Boolean = True

    Private Sub KelasFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        PerbaruiPlaceholderProdi()
        txtProdi.Focus()
    End Sub

    Public Sub SetDaftarProdi(daftar As IEnumerable(Of String))
        lstProdiMaster.Items.Clear()

        If daftar IsNot Nothing Then
            For Each item As String In daftar
                If Not String.IsNullOrWhiteSpace(item) Then
                    lstProdiMaster.Items.Add(item.Trim())
                End If
            Next
        End If

        btnPilihProdi.Visibility = If(lstProdiMaster.Items.Count > 0, Visibility.Visible, Visibility.Collapsed)
    End Sub

    Public Sub SetData(prodiAwal As String, angkatanAwal As String, rombelAwal As String, aktifAwal As Boolean)
        txtJudulForm.Text = "Edit Kelas"
        txtProdi.Text = prodiAwal
        txtAngkatan.Text = angkatanAwal
        txtRombel.Text = rombelAwal
        chkAktif.IsChecked = aktifAwal
        PerbaruiPlaceholderProdi()
    End Sub

    Private Sub txtProdi_TextChanged(sender As Object, e As TextChangedEventArgs)
        PerbaruiPlaceholderProdi()
    End Sub

    Private Sub PerbaruiPlaceholderProdi()
        If lblPlaceholderProdi Is Nothing OrElse txtProdi Is Nothing Then Exit Sub
        lblPlaceholderProdi.Visibility = If(String.IsNullOrEmpty(txtProdi.Text), Visibility.Visible, Visibility.Collapsed)
    End Sub

    Private Sub lstProdiMaster_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If lstProdiMaster.SelectedItem Is Nothing Then Exit Sub

        txtProdi.Text = lstProdiMaster.SelectedItem.ToString()
        lstProdiMaster.SelectedIndex = -1
        btnPilihProdi.IsChecked = False
        txtProdi.Focus()
        txtProdi.CaretIndex = txtProdi.Text.Length
    End Sub

    Private Sub btnPilihProdi_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If btnPilihProdi.IsChecked = True Then
            btnPilihProdi.IsChecked = False
            e.Handled = True
        End If
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtProdi.Text) OrElse
           String.IsNullOrWhiteSpace(txtAngkatan.Text) OrElse
           String.IsNullOrWhiteSpace(txtRombel.Text) Then
            MessageBox.Show("Program studi, angkatan, dan rombel wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim tahun As Integer
        If Not Integer.TryParse(txtAngkatan.Text.Trim(), tahun) OrElse txtAngkatan.Text.Trim().Length <> 4 Then
            MessageBox.Show("Angkatan harus berupa 4 digit angka tahun, misalnya 2025.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtAngkatan.Focus()
            Exit Sub
        End If

        Prodi = txtProdi.Text.Trim()
        Angkatan = tahun.ToString()
        Rombel = txtRombel.Text.Trim().ToUpper()
        Aktif = (chkAktif.IsChecked = True)

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class
