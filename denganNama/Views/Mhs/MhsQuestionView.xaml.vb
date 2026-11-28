Class MhsQuestionView
    Public Property MainFrame As Object

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
