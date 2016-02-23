<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGUI
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnTrackKillersIP = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.SuspendLayout
        '
        'btnTrackKillersIP
        '
        Me.btnTrackKillersIP.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.btnTrackKillersIP.Font = New System.Drawing.Font("Comic Sans MS", 27.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.btnTrackKillersIP.Location = New System.Drawing.Point(13, 29)
        Me.btnTrackKillersIP.Name = "btnTrackKillersIP"
        Me.btnTrackKillersIP.Size = New System.Drawing.Size(354, 185)
        Me.btnTrackKillersIP.TabIndex = 0
        Me.btnTrackKillersIP.Text = "Track killer's IP"
        Me.btnTrackKillersIP.UseVisualStyleBackColor = true
        '
        'TextBox1
        '
        Me.TextBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.TextBox1.ForeColor = System.Drawing.Color.Lime
        Me.TextBox1.Location = New System.Drawing.Point(13, 221)
        Me.TextBox1.Multiline = true
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextBox1.Size = New System.Drawing.Size(354, 224)
        Me.TextBox1.TabIndex = 1
        '
        'frmGUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(379, 457)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.btnTrackKillersIP)
        Me.Name = "frmGUI"
        Me.Text = "TestApplication"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents btnTrackKillersIP As Button
    Friend WithEvents TextBox1 As TextBox
End Class
