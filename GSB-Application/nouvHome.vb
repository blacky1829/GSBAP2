Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Data.Odbc

Public Class nouvHome

    ' Propriétés pour recevoir les infos depuis LoginForm
    Public Property UserFullName As String
    Public Property UserRole As String
    Public Property UserId As Integer
    Public Property LoginRef As LoginForm

    ' Références pour la communication entre onglets
    Private _instanceCreerCR As CreerCRTab
    Private _instanceMesCR As MesCRTab

    Private Sub HomeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Bienvenue, " & UserFullName

        TabControl1.Dock = DockStyle.Fill
        ' Me.Controls.Add(TabControl1) ' Déjà présent via le designer normalement

        ' Définition des onglets
        Dim tabs As New Dictionary(Of String, (roles As String, factory As Func(Of UserControl))) From {
            {"Créer un C.R.", ("Visiteur,Delegue", Function()
                                                       _instanceCreerCR = New CreerCRTab(UserId)
                                                       Return _instanceCreerCR
                                                   End Function)},
            {"Mes statistiques", ("Visiteur,Delegue", Function() New StatsTab(UserId))},
            {"Mes C.R.", ("Visiteur,Delegue", Function()
                                                  _instanceMesCR = New MesCRTab(UserId)
                                                  ' IMPORTANT : On s'abonne à l'événement de modification ici
                                                  AddHandler _instanceMesCR.OnEditRequested, AddressOf GérerDemandeModification
                                                  Return _instanceMesCR
                                              End Function)},
            {"Stats. régionales", ("Delegue", Function() New StatsRegTab(UserId))},
            {"Stats. de secteur", ("Responsable", Function() New StatsSecteurTab())}
        }

        ' Parcourir et ajouter les onglets autorisés
        For Each kvp In tabs
            Dim allowedRoles = kvp.Value.roles.Split(","c).Select(Function(r) r.Trim()).ToArray()
            If allowedRoles.Contains(UserRole) Then
                Dim tp As New TabPage(kvp.Key) With {.Padding = New Padding(3)}
                Dim ctrl As UserControl = CreateControlSafe(kvp.Value.factory)

                If ctrl IsNot Nothing Then
                    ctrl.Dock = DockStyle.Fill
                    tp.Controls.Add(ctrl)
                    TabControl1.TabPages.Add(tp)
                End If
            End If
        Next

        ' Bouton Déconnexion
        Dim btnLogout As New Button() With {.Text = "Déconnexion", .Dock = DockStyle.Bottom, .Height = 30}
        AddHandler btnLogout.Click, AddressOf btnLogout_Click
        Me.Controls.Add(btnLogout)
    End Sub

    ' La fonction qui fait le lien quand on clique sur "Modifier" dans la liste
    Private Sub GérerDemandeModification(idCr As Integer)
        If _instanceCreerCR IsNot Nothing Then
            ' 1. On remplit les champs dans l'onglet de création
            _instanceCreerCR.PreparerModification(idCr)

            ' 2. On cherche la TabPage qui contient cet onglet pour l'afficher
            For Each tp As TabPage In TabControl1.TabPages
                If tp.Controls.Contains(_instanceCreerCR) Then
                    TabControl1.SelectedTab = tp
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Function CreateControlSafe(factory As Func(Of UserControl)) As UserControl
        Try
            Return factory.Invoke()
        Catch ex As Exception
            Debug.WriteLine("Erreur création contrôle : " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        Me.Close()
        If LoginRef IsNot Nothing Then LoginRef.Show()
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        ' Actualisation automatique quand on arrive sur "Mes C.R." ou "Stats"
        Dim tab As TabPage = TabControl1.SelectedTab
        If tab Is Nothing Then Return

        For Each ctrl As Control In tab.Controls
            If TypeOf ctrl Is StatsTab Then
                DirectCast(ctrl, StatsTab).LoadStatsForUser(UserId)
            ElseIf TypeOf ctrl Is MesCRTab Then
                DirectCast(ctrl, MesCRTab).LoadCRs() ' Rafraîchit la liste
            End If
        Next
    End Sub
End Class