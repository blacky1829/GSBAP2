Public Class Form1





    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load


    End Sub


    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles rAbandon.CheckedChanged, LAfficheScore.Click
        LAfficheScore.Text = "0"
    End Sub

    Private Sub RadioButton4_CheckedChanged(sender As Object, e As EventArgs) Handles rOr.CheckedChanged, LAfficheScore.Click
        LAfficheScore.Text = "1"

    End Sub
    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Dim score As Integer = 0
        Dim abandon As String = "Non"
        Dim nom As String
        Dim prenom As String

        nom = txtNom.Text
        prenom = txtPrenom.Text
        ' Vérifie quel bouton radio est coché
        If rOr.Checked Then
            score = 1
        ElseIf rArgent.Checked Then
            score = 2
        ElseIf rBronze.Checked Then
            score = 3
        ElseIf rAbandon.Checked Then
            abandon = "Oui"
        End If

        ' Affiche le résultat dans le label
        Dim selected As String = cmbDisciplines.SelectedItem
        MessageBox.Show(prenom & " " & nom & vbCrLf & "Score : " & score & vbCrLf & "Discipline : " & selected, "Confirmation")
    End Sub





    Private Sub rArgent_CheckedChanged(sender As Object, e As EventArgs) Handles rArgent.CheckedChanged, LAfficheScore.Click
        LAfficheScore.Text = "2"

    End Sub

    Private Sub rBronze_CheckedChanged(sender As Object, e As EventArgs) Handles rBronze.CheckedChanged, LAfficheScore.Click
        LAfficheScore.Text = "3"
    End Sub



End Class