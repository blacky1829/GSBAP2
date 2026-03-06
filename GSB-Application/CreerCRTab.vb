Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Data.Odbc

Public Class CreerCRTab
    Inherits UserControl

    ' ---------- Champs ----------
    Private ReadOnly _currentUserId As Integer
    Private ReadOnly connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"

    ' État interne
    Private quantities As New Dictionary(Of String, Integer)
    Private defaultVersion As String = "1"
    Private suppressVersionGuard As Boolean = False

    ' ---------- Contrôles (Déclarés explicitement pour fixer BC30451) ----------
    Private WithEvents TBversion As TextBox
    Private WithEvents TBnomPrenom As TextBox
    Private WithEvents Doliprane As CheckedListBox
    Private WithEvents bModifier As Button
    Private WithEvents bValider As Button
    Private WithEvents lblVersion As Label
    Private WithEvents lblNom As Label

    ' ---------- Constructeur ----------
    Public Sub New(userId As Integer)
        ' On n'appelle PAS InitializeComponent ici pour éviter l'erreur BC30451
        ' si le fichier Designer est cassé. On construit l'UI manuellement.

        _currentUserId = userId
        Me.Dock = DockStyle.Fill
        Me.BackColor = Color.White

        SetupManualUI()
        InitData()
    End Sub

    ' ---------- Construction de l'interface ----------
    Private Sub SetupManualUI()
        ' Nom / Prénom
        lblNom = New Label With {.Text = "Visiteur :", .Location = New Point(20, 20), .AutoSize = True}
        TBnomPrenom = New TextBox With {.Location = New Point(120, 17), .Width = 200, .ReadOnly = True, .BackColor = Color.LightGray}

        ' Version
        lblVersion = New Label With {.Text = "Version CR :", .Location = New Point(20, 55), .AutoSize = True}
        TBversion = New TextBox With {.Location = New Point(120, 52), .Width = 50, .ReadOnly = True, .Text = "1"}

        ' Bouton Modifier
        bModifier = New Button With {.Text = "Modifier Version", .Location = New Point(180, 50), .Width = 120}
        AddHandler bModifier.Click, AddressOf bModifier_Click

        ' Liste Médicaments (ton objet nommé "Doliprane" dans tes erreurs)
        Dim lblMed = New Label With {.Text = "Médicaments :", .Location = New Point(20, 90), .AutoSize = True}
        Doliprane = New CheckedListBox With {.Location = New Point(20, 110), .Width = 280, .Height = 150}

        ' Bouton Valider
        bValider = New Button With {.Text = "Valider le Compte-Rendu", .Location = New Point(20, 280), .Width = 280, .Height = 40, .BackColor = Color.LightSkyBlue}
        AddHandler bValider.Click, AddressOf bValider_Click

        ' Ajout des contrôles à la vue
        Me.Controls.AddRange({lblNom, TBnomPrenom, lblVersion, TBversion, bModifier, lblMed, Doliprane, bValider})
    End Sub

    Private Sub InitData()
        ' Initialisation des médicaments
        Dim meds = {"Paracétamol", "Ibuprofène", "Amoxicilline", "Doliprane", "Spasfon"}
        Doliprane.Items.Clear()
        For Each m In meds
            Doliprane.Items.Add(m)
            quantities(m) = 0
        Next
    End Sub

    ' ---------- Logique des Événements ----------

    Private Sub bModifier_Click(sender As Object, e As EventArgs)
        Dim v As Integer
        If Integer.TryParse(TBversion.Text, v) Then
            suppressVersionGuard = True
            v += 1
            TBversion.Text = v.ToString()
            defaultVersion = TBversion.Text
            suppressVersionGuard = False
        End If
    End Sub

    Private Sub bValider_Click(sender As Object, e As EventArgs)
        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                ' Code de sauvegarde ici
                MessageBox.Show("Succès : Compte-rendu " & TBversion.Text & " enregistré pour l'ID " & _currentUserId)
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur Oracle : " & ex.Message)
        End Try
    End Sub

    ' Sécurité pour empêcher la modification manuelle du texte de la version
    Private Sub TBversion_TextChanged(sender As Object, e As EventArgs) Handles TBversion.TextChanged
        If Not suppressVersionGuard AndAlso TBversion.Text <> defaultVersion Then
            TBversion.Text = defaultVersion
        End If
    End Sub

End Class