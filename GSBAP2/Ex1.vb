Public Class Form1

    ' Déclaration des boutons
    Private btnSimple As New TextBox()
    Private btnRadio1 As New RadioButton()
    Private btnRadio2 As New RadioButton()
    Private btnCheck As New CheckBox()
    Private btnValider As New Button()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' === Configuration de la Form ===
        Me.Text = "Exemple de Boutons"
        Me.Size = New Size(400, 300)

        ' === BOUTON SIMPLE ===
        btnSimple.Text = "Clique-moi"
        btnSimple.Location = New Point(30, 30)
        btnSimple.Size = New Size(120, 30)
        AddHandler btnSimple.Click, AddressOf BtnSimple_Click
        Me.Controls.Add(btnSimple)

        ' === RADIOBUTTON 1 ===
        btnRadio1.Text = "Badminton"
        btnRadio1.Location = New Point(30, 80)
        btnRadio1.AutoSize = True
        Me.Controls.Add(btnRadio1)

        ' === RADIOBUTTON 2 ===
        btnRadio2.Text = "Athlétisme"
        btnRadio2.Location = New Point(30, 110)
        btnRadio2.AutoSize = True
        Me.Controls.Add(btnRadio2)

        ' === CHECKBOX ===
        btnCheck.Text = "J'accepte les conditions"
        btnCheck.Location = New Point(30, 150)
        btnCheck.AutoSize = True
        Me.Controls.Add(btnCheck)

        ' === BOUTON VALIDER ===
        btnValider.Text = "Valider"
        btnValider.Location = New Point(30, 200)
        btnValider.Size = New Size(120, 30)
        AddHandler btnValider.Click, AddressOf BtnValider_Click
        Me.Controls.Add(btnValider)

    End Sub

    ' === Événement : Bouton simple ===
    Private Sub BtnSimple_Click(sender As Object, e As EventArgs)
        MessageBox.Show("Tu as cliqué sur le bouton simple !", "Info")
    End Sub

    ' === Événement : Bouton Valider ===
    Private Sub BtnValider_Click(sender As Object, e As EventArgs)
        Dim discipline As String = ""

        ' Vérifie quelle discipline est sélectionnée
        If btnRadio1.Checked Then
            discipline = "Badminton"
        ElseIf btnRadio2.Checked Then
            discipline = "Athlétisme"
        Else
            MessageBox.Show("Sélectionnez une discipline !", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        ' Vérifie la CheckBox
        If Not btnCheck.Checked Then
            MessageBox.Show("Vous devez accepter les conditions pour continuer.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        ' Affiche le résultat
        MessageBox.Show("Discipline choisie : " & discipline, "Validation")
    End Sub

End Class
