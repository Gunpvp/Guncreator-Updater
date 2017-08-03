Imports System.ComponentModel
Imports System.Net
Imports System.IO

Public Class Form1

    Private WithEvents httpclient_version As WebClient
    Private WithEvents httpclient_update As WebClient

    Dim installedVersion As String
    Dim updateVersion As String
    Dim isNewer As Short
    Dim isInstalled As Boolean
    Dim isAborted As Boolean

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        download.Enabled = False
        Label2.Hide()
        CheckBox1.Hide()
        ProgressBar1.Value = 0
        ProgressBar1.Maximum = 100

        httpclient_version = New WebClient
        httpclient_update = New WebClient
        checkForUpdate()



    End Sub

    Private Sub cancel_Click(sender As Object, e As EventArgs) Handles cancel.Click
        If download.Enabled = False And reload.Enabled = False And Not ProgressBar1.Value.Equals(ProgressBar1.Maximum) Then
            Dim msgCancel = MsgBox("Do you want to abort the download and close the updater?", MsgBoxStyle.OkCancel, Me.Text)
            If msgCancel = MsgBoxResult.Ok Then
                httpclient_update.CancelAsync()
                isAborted = True
                Me.Close()
            Else
                isAborted = False
            End If
        Else
            Me.Close()
        End If
    End Sub

    Private Sub httpclient_version_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles httpclient_version.DownloadFileCompleted
        installedVersion = My.Settings.version
        updateVersion = File.ReadAllText("./version.txt")
        isNewer = installedVersion.CompareTo(updateVersion)

        Label3.Text = "Installed version: " & installedVersion

        If File.Exists("./guncreator.jar") Then
            isInstalled = True
            startGunCreator.Enabled = True

            If isNewer = -1 Then
                ProgressBar1.Value = 0
                Label1.Text = "Found ver. " & updateVersion & "   -   Do you want to download it?"
                download.Enabled = True
            Else
                Label1.Text = "Latest version is allready installed."
                CheckBox1.Show()
            End If
        Else
            isInstalled = False
            startGunCreator.Enabled = False

            Label1.Text = "GunCreator was not found. Do you want to download it?"
            Label3.Text = "Installed version: -"
            download.Enabled = True
        End If

        reload.Enabled = True

    End Sub

    Private Sub httpclient_version_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs) Handles httpclient_version.DownloadProgressChanged
        ProgressBar1.Value = e.ProgressPercentage

    End Sub

    Private Sub httpclient_update_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles httpclient_update.DownloadFileCompleted
        Try
            File.Delete("./guncreator.jar")
            File.Copy("./update.tmp", "./guncreator.jar")
            File.Delete("./update.tmp")
        Catch ex As Exception
            MsgBox("Error!" & vbCrLf & ex.Message, MsgBoxStyle.Critical)
        End Try

        If isInstalled = False Or CheckBox1.Checked = True Then
            Label1.Text = "Finished download."
        Else
            Label1.Text = "Finished update."
        End If

        My.Settings.version = updateVersion
        Label3.Text = "Installed version: " & My.Settings.version

        reload.Enabled = True
        download.Enabled = True
        CheckBox1.Enabled = True
        CheckBox1.Checked = False
        startGunCreator.Enabled = True
        Label2.Hide()
        cancel.Text = "Exit"
        Me.Text = "GunCreator Updater"
    End Sub

    Private Sub httpclient_update_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs) Handles httpclient_update.DownloadProgressChanged
        Dim totalBytes As ULong = e.TotalBytesToReceive / 1024 / 1024
        Dim bytes As ULong = e.BytesReceived / 1024 / 1024

        Label2.Text = bytes.ToString & " MB of " & totalBytes & " MB"
        Me.Text = "GunCreator Updater " & bytes.ToString & " MB of " & totalBytes & " MB"
        ProgressBar1.Value = e.ProgressPercentage
    End Sub

    Private Sub download_Click(sender As Object, e As EventArgs) Handles download.Click
        download.Enabled = False
        CheckBox1.Enabled = False
        reload.Enabled = False
        startGunCreator.Enabled = False
        Label2.Show()
        Try
            httpclient_update.DownloadFileAsync(New Uri("http://www.3ahel.tk/update/guncreator.jar"), "./update.tmp")
        Catch ex As Exception
            MsgBox("Error!" & vbCrLf & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            download.Enabled = True
        Else
            download.Enabled = False
        End If
    End Sub

    Private Sub Form1_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If isAborted = False Then
            Try
                File.Delete("./update.tmp")
                File.Delete("./version.txt")
            Catch ex As Exception
                MsgBox("Error!" & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            End Try
        Else
            Try
                'File.Delete("./update.tmp")
                File.Delete("./version.txt")
            Catch ex As Exception
                MsgBox("Error!" & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub checkForUpdate()
        ProgressBar1.Value = 0
        Try
            httpclient_version.DownloadFileAsync(New Uri("http://www.3ahel.tk/update/version.txt"), "./version.txt")
        Catch ex As Exception
            MsgBox("Error!" & vbCrLf & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub reload_Click(sender As Object, e As EventArgs) Handles reload.Click
        checkForUpdate()
        reload.Enabled = False
    End Sub

    Private Sub startGunCreator_Click(sender As Object, e As EventArgs) Handles startGunCreator.Click
        Dim appPath = Path.GetDirectoryName(Application.ExecutablePath)
        Shell("java -jar " & appPath & "\guncreator.jar")

    End Sub
End Class
