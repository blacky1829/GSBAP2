Public Class Form1



    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load


    End Sub

    Private Sub SelectDiscipline_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDisciplines.SelectedIndexChanged

    End Sub

    Private Sub LabelTOOp_Click(sender As Object, e As EventArgs) Handles LabelTOOp.Click

    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles rAbandon.CheckedChanged

    End Sub

    Private Sub RadioButton4_CheckedChanged(sender As Object, e As EventArgs) Handles rOr.CheckedChanged

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
            abandon = "Ouip"
        End If

        ' Affiche le résultat dans le label
        Dim selected As String = cmbDisciplines.SelectedItem
        MessageBox.Show(prenom & " " & nom & vbCrLf & "Score : " & score & vbCrLf & "Discipline : " & selected, "Confirmation")
    End Sub

    Private Sub LabelName_Click(sender As Object, e As EventArgs) Handles LabelName.Click

    End Sub

    Private Sub GroupBox1_Enter(sender As Object, e As EventArgs) Handles GroupBox1.Enter

    End Sub

    Private Sub GroupBox2_Enter(sender As Object, e As EventArgs) Handles GroupBox2.Enter

    End Sub

    Private Sub DateTimePicker1_ValueChanged(sender As Object, e As EventArgs) Handles DateTimePicker1.ValueChanged

    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click

    End Sub

    Private Sub rArgent_CheckedChanged(sender As Object, e As EventArgs) Handles rArgent.CheckedChanged

    End Sub

    Private Sub rBronze_CheckedChanged(sender As Object, e As EventArgs) Handles rBronze.CheckedChanged

    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub lblResultat_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub txtPrenom_TextChanged(sender As Object, e As EventArgs) Handles txtPrenom.TextChanged

    End Sub
End Class
