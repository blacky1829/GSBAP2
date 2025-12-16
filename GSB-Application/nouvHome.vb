Imports System.Windows.Forms

Public Class nouvHome

    ' Propriétés pour recevoir les infos depuis LoginForm
    Public Property UserFullName As String
    Public Property UserRole As String
    Public Property UserId As Integer
    Public Property LoginRef As LoginForm

    Private Sub HomeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Bienvenue, " & UserFullName

        TabControl1.Dock = DockStyle.Fill
        Me.Controls.Add(TabControl1)

        ' Définir la liste des onglets et les rôles autorisés + usine de création de UserControl
        Dim tabs As New Dictionary(Of String, (roles As String, factory As Func(Of UserControl))) From {
            {"Créer un C.R.", ("Visiteur,Delegue", Function() CreateControlSafe(Function() New CreerCRTab(UserId)))},
            {"Mes statistiques", ("Visiteur,Delegue", Function() CreateControlSafe(Function() New StatsTab(UserId)))},
            {"Mes C.R.", ("Visiteur,Delegue", Function() CreateControlSafe(Function() New MesCRTab(UserId)))},
            {"Stats. régionales", ("Delegue", Function() CreateControlSafe(Function() New StatsRegTab(UserId)))},
            {"Stats. de secteur", ("Responsable", Function() CreateControlSafe(Function() New StatsSecteurTab()))}
        }

        ' Parcourir et ajouter les onglets autorisés
        For Each kvp In tabs
            Dim tabName = kvp.Key
            Dim allowedRoles = kvp.Value.roles.Split(","c).Select(Function(r) r.Trim()).ToArray()
            If allowedRoles.Contains(UserRole) Then
                Dim tp As New TabPage(tabName) With {.Padding = New Padding(3)}
                Dim ctrl As UserControl = kvp.Value.factory.Invoke()
                If ctrl IsNot Nothing Then
                    ctrl.Dock = DockStyle.Fill
                    tp.Controls.Add(ctrl)
                Else
                    ' Afficher message d'erreur minimal dans l'onglet si la création échoue
                    Dim lblErr As New Label() With {
                        .Text = "Impossible de charger le contenu de l'onglet.",
                        .Dock = DockStyle.Fill,
                        .TextAlign = ContentAlignment.MiddleCenter,
                        .ForeColor = Color.Red
                    }
                    tp.Controls.Add(lblErr)
                End If
                TabControl1.TabPages.Add(tp)
            End If
        Next

        ' Bouton Déconnexion
        Dim btnLogout As New Button() With {
            .Text = "Déconnexion",
            .Dock = DockStyle.Bottom
        }
        AddHandler btnLogout.Click, AddressOf btnLogout_Click
        Me.Controls.Add(btnLogout)
    End Sub

    ' Crée un UserControl via la fonction fournie et attrape les exceptions.
    Private Function CreateControlSafe(factory As Func(Of UserControl)) As UserControl
        Try
            Dim ctrl As UserControl = factory.Invoke()
            ' Si le UserControl expose des propriétés utiles, on peut les initialiser ici
            ' Exemple : If TypeOf ctrl Is IHomeAware Then DirectCast(ctrl, IHomeAware).Initialize(UserId, Me)
            Return ctrl
        Catch ex As Exception
            Debug.WriteLine("Erreur création contrôle onglet : " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        Me.Close()
        If LoginRef IsNot Nothing Then
            LoginRef.Show()
        End If
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(
    sender As Object,
    e As EventArgs
) Handles TabControl1.SelectedIndexChanged

        Dim tc As TabControl = CType(sender, TabControl)
        Dim tab As TabPage = tc.SelectedTab

        For Each ctrl As Control In tab.Controls
            If TypeOf ctrl Is StatsTab Then
                Dim st As StatsTab = CType(ctrl, StatsTab)

                ' 🔁 reset systématique sur l'utilisateur connecté
                st.LoadStatsForUser(UserId)

                Exit Sub
            End If
        Next
    End Sub


End Class
