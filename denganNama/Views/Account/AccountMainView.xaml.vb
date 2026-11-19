Public Class AccountMainView

    Private Sub btnGantiPassword_Click(sender As Object, e As RoutedEventArgs)
        Dim mainWin As MainWindow = TryCast(Application.Current.MainWindow, MainWindow)
        If mainWin IsNot Nothing Then
            mainWin.txtJudulHalaman.Text = "Form Ubah Kata Sandi"
            mainWin.MainFrame.Navigate(New PasswordChangeView())
        End If
    End Sub

End Class