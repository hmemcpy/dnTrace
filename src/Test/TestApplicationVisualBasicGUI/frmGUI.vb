
Imports System.Threading

Public Class frmGUI
    Private Sub btnTrackKillersIP_Click(sender As Object, e As EventArgs) Handles btnTrackKillersIP.Click
        TextBox1.Clear()
        ThreadPool.QueueUserWorkItem(sub(state)
                                     Ping()
                                     End Sub)
    End Sub

    Private Sub Ping()
        Dim p = New Process()

        p.StartInfo.FileName = "cmd"
        p.StartInfo.Arguments = "/c ping 127.0.0.1"
        p.StartInfo.RedirectStandardOutput = True
        p.StartInfo.UseShellExecute = False
        p.StartInfo.CreateNoWindow = True
        AddHandler p.OutputDataReceived, AddressOf OutputHandler
        p.Start()

        p.BeginOutputReadLine()
        p.WaitForExit()
        p.Close()
    End Sub

    Private Sub OutputHandler(sender As Object, e As DataReceivedEventArgs)
        TextBox1.Invoke(New MethodInvoker(sub()
                                              TextBox1.AppendText(e.Data + Environment.NewLine)
                                              End Sub))
    End Sub
End Class

