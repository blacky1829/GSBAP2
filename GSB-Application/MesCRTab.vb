Imports System.Windows.Forms
Imports System.Data.Odbc

Public Class MesCRTab
    Inherits UserControl

    Private _userId As Integer
    Private WithEvents dgv As DataGridView
    ' Use shared DbManager.Connection

    ' ÉVÉNEMENT : Permet au MainForm de savoir qu'on a cliqué sur Modifier
    Public Event OnEditRequested(idCr As Integer)

    Public Sub New(userId As Integer)
        _userId = userId
        Me.Dock = DockStyle.Fill
        InitializeUI()
        LoadCRs()
    End Sub

    Private Sub InitializeUI()
        dgv = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .AllowUserToAddRows = False,
            .ReadOnly = True,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .RowHeadersVisible = False
        }
        ' Colonne Bouton Modifier
        Dim btnCol As New DataGridViewButtonColumn()
        btnCol.Name = "btnEdit"
        btnCol.HeaderText = "Action"
        btnCol.Text = "Modifier"
        btnCol.UseColumnTextForButtonValue = True
        dgv.Columns.Add(btnCol)

        Me.Controls.Add(dgv)
    End Sub

    Public Sub LoadCRs()
        ' JOIN pour afficher Nom Praticien et Libellé Motif proprement
        Dim query As String = "SELECT c.IDCR, c.DATEVISITE, p.NOMPRAT || ' ' || p.PRENOMPRAT AS PRATICIEN, " &
                             "c.LIBELLE AS MOTIF, c.COEFCONFIANCE " &
                             "FROM GSBAdmin.CR c " &
                             "JOIN GSBAdmin.PRATICIEN p ON c.IDPRAT = p.IDPRAT " &
                             "WHERE c.IDUSER = ? ORDER BY c.DATEVISITE DESC"
        Try
            Using cmd As New OdbcCommand(query, DbManager.Connection)
                cmd.Parameters.Add("IDUSER", OdbcType.Int).Value = _userId
                Dim dt As New DataTable()
                dt.Load(cmd.ExecuteReader())
                dgv.DataSource = dt
            End Using
            ' Cacher l'ID technique
            If dgv.Columns.Contains("IDCR") Then dgv.Columns("IDCR").Visible = False
        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message)
        End Try
    End Sub


    Private Sub dgv_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgv.CellContentClick
        If e.RowIndex >= 0 AndAlso dgv.Columns(e.ColumnIndex).Name = "btnEdit" Then
            Dim idCr As Integer = CInt(dgv.Rows(e.RowIndex).Cells("IDCR").Value)
            RaiseEvent OnEditRequested(idCr) ' On envoie l'ID au MainForm
        End If
    End Sub
End Class