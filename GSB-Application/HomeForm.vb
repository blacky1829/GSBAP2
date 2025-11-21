Imports System.Data.Odbc

Public Class HomeForm

    ' Propriétés pour recevoir les infos depuis LoginForm
    Public Property UserFullName As String
    Public Property UserRole As String
    Public Property UserId As String

    Private Sub HomeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Affichage du nom
        Me.Text = "Bienvenue, " & UserFullName


        ' Création du TabControl
        TabControl1.Dock = DockStyle.Fill
        Me.Controls.Add(TabControl1)

        ' Dictionnaire des onglets et des rôles autorisés
        Dim allTabs As New Dictionary(Of String, String) From {
            {"Créer un C.R.", "Visiteur,Délégué"},
            {"Mes statistiques", "Visiteur,Délégué"},
            {"Mes C.R.", "Visiteur,Délégué"},
            {"Stats. régionales", "Délégué"},
            {"Stats. de secteur", "Responsable"}
        }

        ' Boucle pour ajouter dynamiquement les onglets selon le rôle
        For Each tabName In allTabs.Keys
            Dim roles = allTabs(tabName).Split(","c)
            If roles.Contains(UserRole) Then
                Dim tp As New TabPage(tabName)
                TabControl1.TabPages.Add(tp)

                ' Si c'est l'onglet Mes C.R., on ajoute un DataGridView
                If tabName = "Mes C.R." Then
                    Dim dgv As New DataGridView()
                    dgv.Dock = DockStyle.Fill
                    dgv.Name = "dgvMesCR"
                    tp.Controls.Add(dgv)

                    ' Appel méthode pour remplir le DataGridView
                    RemplirMesCR(dgv)
                Else
                    ' Autres onglets : label indicatif
                    Dim lbl As New Label()
                    lbl.Text = "Contenu de " & tabName
                    lbl.Dock = DockStyle.Fill
                    lbl.TextAlign = ContentAlignment.MiddleCenter
                    tp.Controls.Add(lbl)
                End If
            End If
        Next

        ' Bouton Déconnexion
        Dim btnLogout As New Button()
        btnLogout.Text = "Déconnexion"
        btnLogout.Dock = DockStyle.Bottom
        AddHandler btnLogout.Click, AddressOf btnLogout_Click
        Me.Controls.Add(btnLogout)
    End Sub

    Private Sub RemplirMesCR(dgv As DataGridView)
        If String.IsNullOrEmpty(UserId) Then
            MessageBox.Show("Utilisateur non défini.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"
        Dim query As String = "SELECT IDCR, VERSIONCR, LIBELLE, IDPRAT, DATECR, DATEVISITE, COEFCONFIANCE, BILANCR " &
                              "FROM GSBAdmin.CR WHERE IDUSER = ? ORDER BY DATEVISITE DESC"

        Try
            Using myConnection As New OdbcConnection(connString)
                myConnection.Open()
                Using cmd As New OdbcCommand(query, myConnection)
                    cmd.Parameters.Add("IDUSER", OdbcType.Int).Value = CInt(UserId)
                    Using reader As OdbcDataReader = cmd.ExecuteReader()
                        If reader.HasRows Then
                            Dim dt As New DataTable()
                            dt.Load(reader)
                            dgv.DataSource = dt
                        Else
                            MessageBox.Show("Pas d’informations à afficher pour le moment.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As OdbcException
            MessageBox.Show("Erreur ODBC : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Property LoginRef As LoginForm

    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        Me.Close() ' ferme proprement HomeForm
        If LoginRef IsNot Nothing Then
            LoginRef.Show()
        End If
    End Sub


    Private Sub lblWelcome_Click(sender As Object, e As EventArgs)

    End Sub
End Class
