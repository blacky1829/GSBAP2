Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Data.Odbc
Imports System.IO

Public Class nouvHome

    ' Propriétés pour recevoir les infos depuis LoginForm
    Public Property UserFullName As String
    Public Property UserRole As String
    Public Property UserId As Integer

    Public Property dateDebutSession As DateTime
    Public Property LoginRef As LoginForm

    ' Références pour la communication entre onglets
    Private _instanceCreerCR As CreerCRTab
    Private _instanceMesCR As MesCRTab
    Private _instanceLogsTab As LogsTab

    Private Sub HomeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Bienvenue, " & UserFullName

        TabControl1.Dock = DockStyle.Fill
        ' Me.Controls.Add(TabControl1) ' Déjà présent via le designer normalement

        ' Définition des onglets
        Dim tabs As New Dictionary(Of String, (roles As String, factory As Func(Of UserControl))) From {
    {"Créer un C.R.", ("VISITEUR,DELEGUE", Function()
                                               _instanceCreerCR = New CreerCRTab(UserId)
                                               Return _instanceCreerCR
                                           End Function)},
    {"Mes statistiques", ("VISITEUR,DELEGUE", Function() New StatsTab(UserId))},
    {"Mes C.R.", ("VISITEUR,DELEGUE", Function()
                                          _instanceMesCR = New MesCRTab(UserId)
                                          ' IMPORTANT : On s'abonne à l'événement de modification ici
                                          AddHandler _instanceMesCR.OnEditRequested, AddressOf GérerDemandeModification
                                          Return _instanceMesCR
                                      End Function)},
    {"Logs", ("RESPONSABLE", Function()
                                 _instanceLogsTab = New LogsTab()
                                 Return _instanceLogsTab
                             End Function)}
                                      }

        ' Parcourir et ajouter les onglets autorisés
        Try
            Dim logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tab_creation.log")
            File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - HomeForm_Load userRole='" & UserRole & "' userId='" & UserId.ToString() & "'" & Environment.NewLine)
        Catch : End Try

        For Each kvp In tabs
            Dim allowedRoles = kvp.Value.roles.Split(","c).Select(Function(r) r.Trim().ToUpper()).ToArray()
            Dim userRoleUpper = UserRole.Trim().ToUpper()
            Dim allow = allowedRoles.Contains(userRoleUpper)
            Try
                Dim logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tab_creation.log")
                File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - Tab='" & kvp.Key & "' allowedRoles='" & kvp.Value.roles & "' userRole='" & UserRole & "' userRoleUpper='" & userRoleUpper & "' allowed=" & allow.ToString() & Environment.NewLine)
            Catch : End Try

            If allow Then
                Dim tp As New TabPage(kvp.Key) With {.Padding = New Padding(3)}
                Dim ctrl As UserControl = CreateControlSafe(kvp.Value.factory)

                If ctrl IsNot Nothing Then
                    ctrl.Dock = DockStyle.Fill
                    tp.Controls.Add(ctrl)
                    TabControl1.TabPages.Add(tp)
                    Try
                        Dim logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tab_creation.log")
                        File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - TabAdded='" & kvp.Key & "'" & Environment.NewLine)
                    Catch : End Try
                Else
                    ' show detailed error to help debugging when a tab fails to create
                    MessageBox.Show("Échec création onglet: '" & kvp.Key & "'. Voir tab_errors.log pour la pile.
" & "Détails : " & Environment.NewLine & "(voir log)", "Erreur onglet", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            ' Log to file and show message so developer can see why the tab failed to create
            Try
                Dim logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tab_errors.log")
                File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - CreateControlSafe error: " & ex.ToString() & Environment.NewLine)
            Catch
            End Try
            MessageBox.Show("Erreur lors de la création d'un onglet : " & ex.Message & vbCrLf & "Voir tab_errors.log pour plus de détails.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function

    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        Dim dateFinSession As DateTime = DateTime.Now
        Try
            Dim query As String = "INSERT INTO GSBAdmin.SESSION_LOGS (LOGIN, DATEDEBUT, DATEFIN) VALUES (?, ?, ?)"
            Using cmd As New OdbcCommand(query, DbManager.Connection)
                cmd.Parameters.Add("LOGIN", OdbcType.Int).Value = UserId
                cmd.Parameters.Add("DATEDEBUT", OdbcType.DateTime).Value = dateDebutSession
                cmd.Parameters.Add("DATEFIN", OdbcType.DateTime).Value = dateFinSession
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As OdbcException
            Dim msg As String = "Erreur ODBC : " & ex.Message & vbCrLf
            For Each err As OdbcError In ex.Errors
                msg &= "Code : " & err.NativeError & " - " & err.Message & vbCrLf
            Next
            MessageBox.Show(msg)
        End Try
        ' insérer ici commande sql pour les logs
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