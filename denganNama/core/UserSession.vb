Public Module UserSession

    Public Property Id As Integer = 0

    Public Property Username As String = ""

    Public Property Nama As String = ""

    Public Property Role As String = ""

    Public Property IdKelas As Nullable(Of Integer) = Nothing

    Public Function IsLoggedIn() As Boolean
        Return Id > 0
    End Function

    Public Sub Clear()
        Id = 0
        Username = ""
        Nama = ""
        Role = ""
        IdKelas = Nothing
    End Sub

End Module
