Imports System.Windows.Forms
Imports System.Drawing

Public Class CreerCRTab
    Inherits UserControl

    ' IDENTIFIANT UNIQUE (BC31429 corrigé : on utilise un seul nom)
    Private _currentUserId As Integer
    Private connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"

    ' État interne
    Private quantities As New Dictionary(Of String, Integer)
    Private selectedMedicaments As New HashSet(Of String)
    Private visitRating As Double = 0.0
    Private suppressVersionGuard As Boolean = False
    Private defaultVersion As String = "1"

    ' CONSTRUCTEUR
    Public Sub New(userId As Integer)
        ' Répare les erreurs "Non déclaré" (BC30451)
        InitializeComponent()

        _currentUserId = userId

        ' Initialisation des médicaments par défaut
        Dim meds = {"Paracétamol", "Ibuprofène", "Amoxicilline", "Doliprane", "Spasfon"}
        For Each m In meds
            quantities(m) = 0
        Next
    End Sub

    Private Sub CreerCRTab_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Setup basique de l'interface
        TBversion.Text = defaultVersion
        TBversion.ReadOnly = True
        TBnomPrenom.ReadOnly = True

        ' Remplissage de la liste de médicaments
        Doliprane.Items.Clear()
        For Each m In quantities.Keys
            Doliprane.Items.Add(m)
        Next
    End Sub

    ' GESTION DU BOUTON MODIFIER
    Private Sub bModifier_Click(sender As Object, e As EventArgs) Handles bModifier.Click
        Dim v As Integer
        If Integer.TryParse(TBversion.Text, v) Then
            suppressVersionGuard = True
            v += 1
            TBversion.Text = v.ToString()
            defaultVersion = TBversion.Text
            suppressVersionGuard = False
        End If
    End Sub

    ' GARDE FOU VERSION
    Private Sub TBversion_TextChanged(sender As Object, e As EventArgs) Handles TBversion.TextChanged
        If Not suppressVersionGuard AndAlso TBversion.Text <> defaultVersion Then
            TBversion.Text = defaultVersion
        End If
    End Sub

    ' VALIDATION ET INSERTION DB
    Private Sub bValider_Click(sender As Object, e As EventArgs) Handles bValider.Click
        ' Ici tu peux ajouter ton code Odbc pour sauvegarder avec _currentUserId
        MessageBox.Show("Compte-rendu enregistré pour l'utilisateur " & _currentUserId)
    End Sub
End Class