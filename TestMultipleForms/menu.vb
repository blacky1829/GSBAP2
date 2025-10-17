Public Class menu
    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        ' Fermer la fenêtre principale et revenir à la connexion
        Me.Hide()
        Dim login As New connexion()
        login.Show()
    End Sub

    Private Sub menu_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
