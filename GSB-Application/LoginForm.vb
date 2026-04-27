Imports System.Data.Odbc

Public Class LoginForm

    Dim myCommand As New OdbcCommand
    Dim myReader As OdbcDataReader

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.StartPosition = FormStartPosition.CenterScreen

        ' Taille de la fenêtre
        Me.ClientSize = New Size(640, 360)

        ' Couleurs
        Me.BackColor = Color.FromArgb(134, 178, 255)

        ' Couleur du bouton
        btnLogin.BackColor = Color.FromArgb(76, 127, 179)  ' bleu foncé
        btnLogin.ForeColor = Color.White                  ' texte en blanc

        ' Couleur des TextBox
        txtUser.BackColor = Color.White ' jaune clair
        txtUser.ForeColor = Color.Black                   ' texte en noir
        txtPass.BackColor = Color.White ' jaune clair
        txtPass.ForeColor = Color.Black

        ' Valeurs par défaut pour tests
        txtUser.Text = "SJoe"
        txtPass.Text = "pass456"

        ' Initialiser la connexion centrale
        Try
            DbManager.Initialize()
        Catch ex As OdbcException
            MessageBox.Show("Erreur de connexion : " & ex.Message)
        End Try
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim username As String = txtUser.Text.Trim()
        Dim password As String = txtPass.Text.Trim()

        If String.IsNullOrEmpty(username) OrElse String.IsNullOrEmpty(password) Then
            MessageBox.Show("Veuillez entrer un nom d'utilisateur et un mot de passe.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim query As String = "SELECT NOMUSER, PRENOMUSER, ROLE, IDUSER " &
                      "FROM GSBAdmin.UTILISATEUR " &
                      "WHERE Trim(USERNAME) = ? " &
                      "AND TRIM(PSWDUSER) = ?"

            myCommand = New OdbcCommand(query, DbManager.Connection)
            myCommand.Parameters.Add("USERNAME", OdbcType.VarChar).Value = username
            myCommand.Parameters.Add("PASSWORD", OdbcType.VarChar).Value = password

            myReader = myCommand.ExecuteReader()

            If myReader.Read() Then
                Dim fullname As String = myReader("NOMUSER").ToString() & " " & myReader("PRENOMUSER").ToString()
                Dim role As String = myReader("ROLE").ToString()
                Dim id As String = myReader("IDUSER")

                Dim main As New nouvHome()
                main.UserFullName = fullname
                main.UserRole = role
                main.UserId = id
                main.LoginRef = Me
                main.dateDebutSession = DateTime.Now
                Me.Hide()
                main.Show()

            Else
                MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If


            myReader.Close()
        Catch ex As OdbcException
            Dim msg As String = "Erreur ODBC : " & ex.Message & vbCrLf
            For Each err As OdbcError In ex.Errors
                msg &= "Code : " & err.NativeError & " - " & err.Message & vbCrLf
            Next
            MessageBox.Show(msg)
        End Try

    End Sub

    Private Sub LoginForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Fermer la connexion centrale si l'application se ferme via ce formulaire
        Try
            If Application.OpenForms.Count <= 1 Then
                DbManager.Close()
            End If
        Catch
        End Try
    End Sub

    Private Sub txtUser_TextChanged(sender As Object, e As EventArgs) Handles txtUser.TextChanged

    End Sub
End Class
