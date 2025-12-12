Imports System.Windows.Forms

Public Class StatsRegTab
    Inherits UserControl

    Public Sub New()
        Me.Dock = DockStyle.Fill
        InitializeUI()
    End Sub

    Private Sub InitializeUI()
        Dim lbl As New Label() With {
            .Text = "Méthode : ConstruireOngletStatsReg",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter
        }
        Me.Controls.Add(lbl)
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'StatsRegTab
        '
        Me.Name = "StatsRegTab"
        Me.ResumeLayout(False)

    End Sub

    Private Sub StatsRegTab_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
