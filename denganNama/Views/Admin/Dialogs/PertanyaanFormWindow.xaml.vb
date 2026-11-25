Imports System.Windows.Controls

Public Class PertanyaanFormWindow

    Public Property Kategori As String

    Public Property TeksPertanyaan As String

    Private Sub PertanyaanFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If cmbKategori.SelectedIndex < 0 AndAlso cmbKategori.Items.Count > 0 Then
            cmbKategori.SelectedIndex = 0
        End If

        txtPertanyaan.Focus()
        txtPertanyaan.CaretIndex = txtPertanyaan.Text.Length
    End Sub

    Public Sub PresetKategori(kategoriKode As String)
        PilihKategori(kategoriKode)
    End Sub

    Public Sub SetData(kategoriAwal As String, pertanyaanAwal As String)
        txtJudulForm.Text = "Edit Pertanyaan EDOM"
        txtSubjudulForm.Text = "Perbarui aspek kompetensi atau redaksi butir pertanyaan."
        PilihKategori(kategoriAwal)
        txtPertanyaan.Text = pertanyaanAwal
    End Sub

    Private Sub PilihKategori(kategoriKode As String)
        If String.IsNullOrWhiteSpace(kategoriKode) Then Exit Sub

        For Each item As ComboBoxItem In cmbKategori.Items
            If String.Equals(Convert.ToString(item.Tag), kategoriKode.Trim(), StringComparison.OrdinalIgnoreCase) Then
                cmbKategori.SelectedItem = item
                Exit For
            End If
        Next
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        Dim itemKategori As ComboBoxItem = TryCast(cmbKategori.SelectedItem, ComboBoxItem)
        If itemKategori Is Nothing Then
            MessageBox.Show("Pilih aspek kompetensi terlebih dahulu.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim teks As String = txtPertanyaan.Text.Trim()
        If String.IsNullOrWhiteSpace(teks) Then
            MessageBox.Show("Redaksi butir pertanyaan wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtPertanyaan.Focus()
            Exit Sub
        End If

        Kategori = Convert.ToString(itemKategori.Tag)
        TeksPertanyaan = teks

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class
