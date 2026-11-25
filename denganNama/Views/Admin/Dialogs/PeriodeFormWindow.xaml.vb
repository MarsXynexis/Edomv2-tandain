Imports System.Windows.Controls

Public Class PeriodeFormWindow

    Private modeEdit As Boolean = False

    Public Property TahunAjaran As Integer

    Public Property Semester As String

    Public Property NamaPeriode As String

    Public Property TanggalMulai As Date

    Public Property TanggalSelesai As Date

    Private Sub PeriodeFormWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not modeEdit Then
            txtTahunMulai.Text = Date.Now.Year.ToString()
            cmbSemester.SelectedIndex = 0
            dpMulai.SelectedDate = Date.Today
            dpSelesai.SelectedDate = Date.Today.AddMonths(1)
        End If

        PerbaruiPratinjau()
        txtTahunMulai.Focus()
        txtTahunMulai.SelectAll()
    End Sub

    Public Sub SetData(tahunAjaranAwal As Integer, semesterAwal As String, mulaiAwal As Date, selesaiAwal As Date)
        modeEdit = True
        txtJudulForm.Text = "Edit Periode EDOM"
        txtSubjudulForm.Text = "Perbarui tahun ajaran, semester, dan rentang jadwal pengisian."

        txtTahunMulai.Text = tahunAjaranAwal.ToString()

        For Each item As ComboBoxItem In cmbSemester.Items
            If String.Equals(Convert.ToString(item.Tag), semesterAwal, StringComparison.OrdinalIgnoreCase) Then
                cmbSemester.SelectedItem = item
                Exit For
            End If
        Next

        dpMulai.SelectedDate = mulaiAwal.Date
        dpSelesai.SelectedDate = selesaiAwal.Date
        PerbaruiPratinjau()
    End Sub

    Private Sub txtTahunMulai_TextChanged(sender As Object, e As TextChangedEventArgs)
        PerbaruiPratinjau()
    End Sub

    Private Sub cmbSemester_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        PerbaruiPratinjau()
    End Sub

    Private Sub PerbaruiPratinjau()
        If txtPratinjauPeriode Is Nothing OrElse txtTahunMulai Is Nothing OrElse cmbSemester Is Nothing Then Exit Sub

        Dim tahunAwal As Integer
        Dim itemSemester As ComboBoxItem = TryCast(cmbSemester.SelectedItem, ComboBoxItem)

        If Integer.TryParse(txtTahunMulai.Text.Trim(), tahunAwal) AndAlso
           tahunAwal >= 2000 AndAlso tahunAwal <= 2100 AndAlso
           itemSemester IsNot Nothing Then
            Dim labelSemester As String = Convert.ToString(itemSemester.Tag)
            txtPratinjauPeriode.Text = $"Periode: {tahunAwal}/{tahunAwal + 1} {labelSemester}"
        Else
            txtPratinjauPeriode.Text = "Periode: -"
        End If
    End Sub

    Private Sub btnBatal_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As RoutedEventArgs)
        Dim tahunAwal As Integer
        If Not Integer.TryParse(txtTahunMulai.Text.Trim(), tahunAwal) OrElse tahunAwal < 2000 OrElse tahunAwal > 2100 Then
            MessageBox.Show("Tahun awal ajaran harus berupa angka antara 2000 hingga 2100.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            txtTahunMulai.Focus()
            Exit Sub
        End If

        Dim itemSemester As ComboBoxItem = TryCast(cmbSemester.SelectedItem, ComboBoxItem)
        If itemSemester Is Nothing Then
            MessageBox.Show("Pilih semester terlebih dahulu.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        If Not dpMulai.SelectedDate.HasValue OrElse Not dpSelesai.SelectedDate.HasValue Then
            MessageBox.Show("Tanggal mulai dan tanggal selesai wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Dim tglMulai As Date = dpMulai.SelectedDate.Value.Date
        Dim tglSelesai As Date = dpSelesai.SelectedDate.Value.Date

        If tglSelesai < tglMulai Then
            MessageBox.Show("Tanggal selesai tidak boleh lebih awal dari tanggal mulai.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning)
            dpSelesai.Focus()
            Exit Sub
        End If

        TahunAjaran = tahunAwal
        Semester = Convert.ToString(itemSemester.Tag)
        NamaPeriode = $"{tahunAwal}/{tahunAwal + 1} {Semester}"
        TanggalMulai = tglMulai
        TanggalSelesai = tglSelesai

        Me.DialogResult = True
        Me.Close()
    End Sub

End Class
