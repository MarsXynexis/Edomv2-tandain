Public Class LoginWindow

    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrEmpty(txtUsername.Text) OrElse String.IsNullOrEmpty(txtPassword.Password) Then
            MessageBox.Show("Mohon isi username dan password!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim main As New MainWindow()
        main.Show()
        Me.Close()
    End Sub

End Class