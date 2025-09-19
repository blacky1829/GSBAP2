Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initialisation éventuelle si besoin
    End Sub

    ' Fonction pour vérifier que tous les champs obligatoires sont remplis
    Private Function ChampsValides() As Boolean
        ' Vérifie les TextBox
        Dim champsTexte As TextBox() = {txtNom, txtPrenom}
        For Each tb As TextBox In champsTexte
            If String.IsNullOrWhiteSpace(tb.Text) Then
                MessageBox.Show("Veuillez remplir le champ : " & tb.Name.Replace("txt", ""), "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                tb.Focus()
                Return False
            End If
        Next

        ' Vérifie la ComboBox
        If cmbDisciplines.SelectedIndex = -1 Then
            MessageBox.Show("Veuillez sélectionner une discipline.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbDisciplines.Focus()
            Return False
        End If

        ' Vérifie les RadioButtons
        Dim radios As RadioButton() = {rOr, rArgent, rBronze, rAbandon}
        Dim unCoche As Boolean = radios.Any(Function(r) r.Checked)
        If Not unCoche Then
            MessageBox.Show("Veuillez sélectionner un score ou Abandon.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Return True ' tout est valide
    End Function

    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        ' Vérifie les champs obligatoires
        If Not ChampsValides() Then
            Exit Sub
        End If

        ' Calcul du score
        Dim score As Integer = 0
        Dim abandon As String = "Non"

        If rOr.Checked Then
            score = 1
        ElseIf rArgent.Checked Then
            score = 2
        ElseIf rBronze.Checked Then
            score = 3
        ElseIf rAbandon.Checked Then
            abandon = "Oui"
        End If

        Dim nom As String = txtNom.Text
        Dim prenom As String = txtPrenom.Text
        Dim selected As String = cmbDisciplines.SelectedItem

        ' Affiche le résultat
        MessageBox.Show(prenom & " " & nom & vbCrLf &
                        "Score : " & score & vbCrLf &
                        "Discipline : " & selected & vbCrLf &
                        "Abandon : " & abandon,
                        "Confirmation")
    End Sub

    ' Les autres événements du formulaire peuvent rester vides
    Private Sub SelectDiscipline_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDisciplines.SelectedIndexChanged
    End Sub

    Private Sub LabelTOOp_Click(sender As Object, e As EventArgs) Handles LabelTOOp.Click
    End Sub

    Private Sub rAbandon_CheckedChanged(sender As Object, e As EventArgs) Handles rAbandon.CheckedChanged
    End Sub

    Private Sub rOr_CheckedChanged(sender As Object, e As EventArgs) Handles rOr.CheckedChanged
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

    Private Sub txtPrenom_TextChanged(sender As Object, e As EventArgs) Handles txtPrenom.TextChanged
    End Sub

    Private Sub txtNom_TextChanged(sender As Object, e As EventArgs) Handles txtNom.TextChanged
    End Sub

End Class

