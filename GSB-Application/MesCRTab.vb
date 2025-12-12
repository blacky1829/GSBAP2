Imports System.Windows.Forms
Imports System.Data.Odbc

Public Class MesCRTab
    Inherits UserControl

    Private _userId As Integer
    Private dgv As DataGridView

    Public Sub New(userId As Integer)
        _userId = userId
        Me.Dock = DockStyle.Fill
        InitializeUI()
        LoadCRs()
    End Sub

    Private Sub InitializeUI()
        dgv = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }
        Me.Controls.Add(dgv)
    End Sub

    Private Sub LoadCRs()
        Dim connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"
        Dim query As String = "SELECT IDCR, VERSIONCR, LIBELLE, IDPRAT, DATECR, DATEVISITE, COEFCONFIANCE, BILANCR " &
                              "FROM GSBAdmin.CR WHERE IDUSER = ? ORDER BY DATEVISITE DESC"

        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                Using cmd As New OdbcCommand(query, conn)
                    cmd.Parameters.Add("IDUSER", OdbcType.Int).Value = _userId
                    Using reader As OdbcDataReader = cmd.ExecuteReader()
                        Dim dt As New DataTable()
                        dt.Load(reader)
                        dgv.DataSource = dt
                    End Using
                End Using
            End Using
        Catch ex As OdbcException
            MessageBox.Show("Erreur ODBC : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
