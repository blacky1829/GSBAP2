Public Class connexion
    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim username As String = txtUser.Text.Trim()
        Dim password As String = txtPass.Text.Trim()

        ' --- Authentification simple ---
        If username = "admin" And password = "1234" Then
            ' Masquer la fenêtre actuelle
            Me.Hide()

            ' Ouvrir la fenêtre principale
            Dim main As New menu()
            main.lblWelcome.Text = "Bienvenue, " & username
            main.Show()
        Else
            MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles txtPass.TextChanged

    End Sub

    Private Sub connexion_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
