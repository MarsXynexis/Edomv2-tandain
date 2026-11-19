Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

Public Class InputKategoriFuzzy

    <JsonPropertyName("tot_ped")>
    Public Property TotPed As Double

    <JsonPropertyName("jml_ped")>
    Public Property JmlPed As Integer

    <JsonPropertyName("tot_prof")>
    Public Property TotProf As Double

    <JsonPropertyName("jml_prof")>
    Public Property JmlProf As Integer

    <JsonPropertyName("tot_kep")>
    Public Property TotKep As Double

    <JsonPropertyName("jml_kep")>
    Public Property JmlKep As Integer

    <JsonPropertyName("tot_sos")>
    Public Property TotSos As Double

    <JsonPropertyName("jml_sos")>
    Public Property JmlSos As Integer

End Class

Public Class HasilEvaluasiFuzzy

    <JsonPropertyName("pedagogik")>
    Public Property Pedagogik As Double

    <JsonPropertyName("profesional")>
    Public Property Profesional As Double

    <JsonPropertyName("kepribadian")>
    Public Property Kepribadian As Double

    <JsonPropertyName("sosial")>
    Public Property Sosial As Double

    <JsonPropertyName("skor_akhir")>
    Public Property SkorAkhir As Double

    <JsonPropertyName("predikat")>
    Public Property Predikat As String = "-"

    <JsonPropertyName("rekomendasi")>
    Public Property Rekomendasi As String = ""

End Class

Public Module FuzzyHelper

    Public Function CariPathScript() As String
        Dim direktoriSaatIni As New DirectoryInfo(Application.StartupPath)

        For langkah As Integer = 0 To 5
            If direktoriSaatIni Is Nothing Then Exit For

            Dim kandidat As String = Path.Combine(direktoriSaatIni.FullName, "fuzzy_sugeno.py")

            If File.Exists(kandidat) Then
                Return kandidat
            End If

            direktoriSaatIni = direktoriSaatIni.Parent
        Next

        Dim lokal As String = Path.Combine(Application.StartupPath, "fuzzy_sugeno.py")

        If File.Exists(lokal) Then
            Return lokal
        End If

        Return ""
    End Function

    Public Function HitungBatchFuzzy(daftarInput As List(Of InputKategoriFuzzy)) As List(Of HasilEvaluasiFuzzy)
        If daftarInput Is Nothing OrElse daftarInput.Count = 0 Then
            Return New List(Of HasilEvaluasiFuzzy)()
        End If

        Dim scriptPath As String = CariPathScript()

        If String.IsNullOrEmpty(scriptPath) Then
            Throw New FileNotFoundException("Berkas fuzzy_sugeno.py tidak ditemukan.")
        End If

        Dim jsonInput As String = JsonSerializer.Serialize(daftarInput)
        Dim jsonOutput As String = JalankanProsesPython(scriptPath, jsonInput)
        Dim hasil As List(Of HasilEvaluasiFuzzy) = JsonSerializer.Deserialize(Of List(Of HasilEvaluasiFuzzy))(jsonOutput)

        If hasil Is Nothing Then
            Return New List(Of HasilEvaluasiFuzzy)()
        End If

        Return hasil
    End Function

    Public Function HitungSatuFuzzy(inputData As InputKategoriFuzzy) As HasilEvaluasiFuzzy
        Dim daftar As New List(Of InputKategoriFuzzy) From {inputData}
        Dim hasil As List(Of HasilEvaluasiFuzzy) = HitungBatchFuzzy(daftar)

        If hasil.Count > 0 Then
            Return hasil(0)
        End If

        Return New HasilEvaluasiFuzzy()
    End Function

    Private Function JalankanProsesPython(scriptPath As String, jsonInput As String) As String
        Dim kandidatPerintah As String() = {"python", "py"}
        Dim pesanTerakhir As String = ""

        For Each perintah As String In kandidatPerintah
            Try
                Dim psi As New ProcessStartInfo()
                psi.FileName = perintah
                psi.Arguments = """" & scriptPath & """ --json"
                psi.UseShellExecute = False
                psi.CreateNoWindow = True
                psi.RedirectStandardInput = True
                psi.RedirectStandardOutput = True
                psi.RedirectStandardError = True
                psi.StandardInputEncoding = Encoding.UTF8
                psi.StandardOutputEncoding = Encoding.UTF8
                psi.StandardErrorEncoding = Encoding.UTF8

                Using proses As Process = Process.Start(psi)
                    If proses Is Nothing Then Continue For

                    proses.StandardInput.Write(jsonInput)
                    proses.StandardInput.Close()

                    Dim outputTeks As String = proses.StandardOutput.ReadToEnd()
                    Dim errorTeks As String = proses.StandardError.ReadToEnd()
                    proses.WaitForExit()

                    If proses.ExitCode = 0 AndAlso Not String.IsNullOrWhiteSpace(outputTeks) Then
                        Return outputTeks.Trim()
                    End If

                    pesanTerakhir = If(Not String.IsNullOrWhiteSpace(errorTeks), errorTeks.Trim(), "Kode keluar: " & proses.ExitCode.ToString())
                End Using
            Catch ex As Win32Exception
                pesanTerakhir = ex.Message
            End Try
        Next

        Throw New InvalidOperationException("Gagal menjalankan Python untuk fuzzy_sugeno.py: " & pesanTerakhir)
    End Function

End Module
