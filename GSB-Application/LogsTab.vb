Imports System.Windows.Forms
Imports System.Data.Odbc
Imports System.Data
Imports System.Drawing

Public Class LogsTab
    Inherits UserControl

    Private WithEvents dgvLogs As DataGridView
    Private WithEvents btnClearLogs As Button
    Private WithEvents cmbUsers As ComboBox
    Private WithEvents lblTotalDuration As Label
    Private WithEvents sidePanel As Panel

    Public Sub New()
        Me.Dock = DockStyle.Fill
        InitializeUI()
        LoadUserList() ' Charger les noms dans le filtre
        LoadLogs()     ' Charger tous les logs par défaut
    End Sub

    Private Sub InitializeUI()
        ' 1. Panneau latéral
        sidePanel = New Panel() With {
            .Dock = DockStyle.Left,
            .Width = 180,
            .Padding = New Padding(10),
            .BackColor = Color.FromArgb(245, 245, 245)
        }

        ' 2. Bouton Vider
        btnClearLogs = New Button() With {
            .Text = "Vider les logs",
            .Dock = DockStyle.Top,
            .Height = 40,
            .BackColor = Color.IndianRed,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Margin = New Padding(0, 0, 0, 20)
        }
        btnClearLogs.FlatAppearance.BorderSize = 0

        ' 3. Label Filtre
        Dim lblFilter As New Label() With {
            .Text = "Filtrer par utilisateur :",
            .Dock = DockStyle.Top,
            .Height = 30,
            .TextAlign = ContentAlignment.BottomLeft
        }

        ' 4. ComboBox Utilisateurs
        cmbUsers = New ComboBox() With {
            .Dock = DockStyle.Top,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Margin = New Padding(0, 5, 0, 20)
        }

        ' 5. Label Durée Totale
        lblTotalDuration = New Label() With {
            .Text = "Durée totale : 0h 0m 0s",
            .Dock = DockStyle.Top,
            .Height = 60,
            .ForeColor = Color.DarkBlue,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        ' Ajout au panel (ordre inverse du dock Top pour l'affichage)
        sidePanel.Controls.Add(lblTotalDuration)
        sidePanel.Controls.Add(cmbUsers)
        sidePanel.Controls.Add(lblFilter)
        sidePanel.Controls.Add(btnClearLogs)

        ' 6. GridView
        dgvLogs = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .AllowUserToAddRows = False,
            .ReadOnly = True,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .RowHeadersVisible = False,
            .BackgroundColor = Color.White
        }

        Me.Controls.Add(dgvLogs)
        Me.Controls.Add(sidePanel)
    End Sub

    ' Charge la liste des usernames dans le ComboBox
    Private Sub LoadUserList()
        cmbUsers.Items.Clear()
        cmbUsers.Items.Add("Tous les utilisateurs")

        Dim query As String = "SELECT USERNAME FROM GSBAdmin.UTILISATEUR ORDER BY USERNAME"
        Try
            Using cmd As New OdbcCommand(query, DbManager.Connection)
                Dim reader = cmd.ExecuteReader()
                While reader.Read()
                    cmbUsers.Items.Add(reader("USERNAME").ToString())
                End While
            End Using
            cmbUsers.SelectedIndex = 0
        Catch ex As Exception
            ' Erreur silencieuse ou log
        End Try
    End Sub

    Public Sub LoadLogs()
        Dim selectedUser As String = cmbUsers.SelectedItem?.ToString()
        Dim hasFilter As Boolean = (selectedUser <> "Tous les utilisateurs" And Not String.IsNullOrEmpty(selectedUser))

        ' Requête principale (Tableau)
        Dim query As String = "SELECT u.USERNAME AS ""UTILISATEUR"", " &
                             "TO_CHAR(l.DATEDEBUT, 'DD/MM/YYYY HH24:MI:SS') AS ""DÉBUT"", " &
                             "TO_CHAR(l.DATEFIN, 'DD/MM/YYYY HH24:MI:SS') AS ""FIN"", " &
                             "TRUNC((l.DATEFIN - l.DATEDEBUT) * 24) || 'h ' || " &
                             "TRUNC(MOD((l.DATEFIN - l.DATEDEBUT) * 1440, 60)) || 'm ' || " &
                             "TRUNC(MOD((l.DATEFIN - l.DATEDEBUT) * 86400, 60)) || 's' AS ""DURÉE"" " &
                             "FROM GSBAdmin.SESSION_LOGS l " &
                             "JOIN GSBAdmin.UTILISATEUR u ON l.LOGIN = CAST(u.IDUSER AS VARCHAR2(50)) "

        If hasFilter Then query &= "WHERE u.USERNAME = ? "
        query &= "ORDER BY l.DATEDEBUT DESC"

        ' Requête de calcul de durée totale (Somme)
        Dim sumQuery As String = "SELECT SUM(l.DATEFIN - l.DATEDEBUT) FROM GSBAdmin.SESSION_LOGS l " &
                                "JOIN GSBAdmin.UTILISATEUR u ON l.LOGIN = CAST(u.IDUSER AS VARCHAR2(50)) "
        If hasFilter Then sumQuery &= "WHERE u.USERNAME = ? "

        Try
            If DbManager.Connection.State = ConnectionState.Closed Then DbManager.Connection.Open()

            ' 1. Charger la Grid
            Using cmd As New OdbcCommand(query, DbManager.Connection)
                If hasFilter Then cmd.Parameters.Add("user", OdbcType.VarChar).Value = selectedUser
                Dim dt As New DataTable()
                dt.Load(cmd.ExecuteReader())
                dgvLogs.DataSource = dt
            End Using

            ' 2. Calculer la durée totale
            Using cmdSum As New OdbcCommand(sumQuery, DbManager.Connection)
                If hasFilter Then cmdSum.Parameters.Add("user", OdbcType.VarChar).Value = selectedUser
                Dim totalDays As Object = cmdSum.ExecuteScalar()

                If totalDays IsNot DBNull.Value AndAlso totalDays IsNot Nothing Then
                    Dim d As Double = Convert.ToDouble(totalDays)
                    Dim h As Integer = Int(d * 24)
                    Dim m As Integer = Int((d * 1440) Mod 60)
                    Dim s As Integer = Int((d * 86400) Mod 60)
                    lblTotalDuration.Text = $"Durée totale :{vbCrLf}{h}h {m}m {s}s"
                Else
                    lblTotalDuration.Text = "Durée totale : 0h 0m 0s"
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message)
        End Try
    End Sub

    ' ÉVÉNEMENT : Changer de filtre
    Private Sub cmbUsers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbUsers.SelectedIndexChanged
        LoadLogs()
    End Sub

    Private Sub btnClearLogs_Click(sender As Object, e As EventArgs) Handles btnClearLogs.Click
        If MessageBox.Show("Voulez-vous vider TOUS les logs ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                Using cmd As New OdbcCommand("TRUNCATE TABLE GSBAdmin.SESSION_LOGS", DbManager.Connection)
                    cmd.ExecuteNonQuery()
                End Using
                LoadLogs()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub
End Class