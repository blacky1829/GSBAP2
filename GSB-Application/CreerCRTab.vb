Imports System.Windows.Forms

Public Class CreerCRTab
    Inherits UserControl

    Private _userId As Integer

    Public Sub New(userId As Integer)
        _userId = userId
        Me.Dock = DockStyle.Fill
        InitializeUI()
    End Sub

    Private Sub InitializeUI()
        Dim lbl As New Label() With {
            .Text = "Onglet : Créer un C.R.",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter
        }
        Me.Controls.Add(lbl)
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'CreateCRTab
        '
        Me.Name = "CreateCRTab"
        Me.ResumeLayout(False)

    End Sub

    Private Sub CreateCRTab_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
