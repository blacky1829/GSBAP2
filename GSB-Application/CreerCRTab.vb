Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Data.Odbc

Public Class CreerCRTab
    Inherits UserControl

    Private ReadOnly _currentUserId As Integer
    Private ReadOnly connString As String = "DSN=ORA14;Uid=GSBAdmin;Pwd=Iroise29;"
    Private _editingCrId As Integer? = Nothing

    Private WithEvents TBversion As TextBox
    Private WithEvents CBPraticien As ComboBox
    Private WithEvents CBMotif As ComboBox
    Private WithEvents CLBProduits As CheckedListBox
    Private WithEvents DTPVisite As DateTimePicker
    Private WithEvents RTBBilan As RichTextBox
    Private WithEvents bValider As Button
    Private WithEvents NudConfiance As NumericUpDown

    Public Sub New(userId As Integer)
        _currentUserId = userId
        Me.Dock = DockStyle.Fill
        Me.BackColor = Color.White
        InitializeUI()
        LoadData()
    End Sub

    Private Sub InitializeUI()
        Dim currentTop As Integer = 20

        Me.Controls.Add(New Label With {.Text = "Praticien :", .Location = New Point(20, currentTop), .AutoSize = True})
        CBPraticien = New ComboBox With {.Location = New Point(120, currentTop - 3), .Width = 200, .DropDownStyle = ComboBoxStyle.DropDownList}
        Me.Controls.Add(CBPraticien)
        currentTop += 40

        Me.Controls.Add(New Label With {.Text = "Motif :", .Location = New Point(20, currentTop), .AutoSize = True})
        CBMotif = New ComboBox With {.Location = New Point(120, currentTop - 3), .Width = 200, .DropDownStyle = ComboBoxStyle.DropDownList}
        Me.Controls.Add(CBMotif)
        currentTop += 40

        Me.Controls.Add(New Label With {.Text = "Date Visite :", .Location = New Point(20, currentTop), .AutoSize = True})
        DTPVisite = New DateTimePicker With {.Location = New Point(120, currentTop - 3), .Width = 200}
        Me.Controls.Add(DTPVisite)
        currentTop += 40

        Me.Controls.Add(New Label With {.Text = "Confiance (1-5) :", .Location = New Point(20, currentTop), .AutoSize = True})
        NudConfiance = New NumericUpDown With {.Location = New Point(120, currentTop - 3), .Minimum = 1, .Maximum = 5, .Value = 3}
        Me.Controls.Add(NudConfiance)
        currentTop += 40

        Me.Controls.Add(New Label With {.Text = "Produits présentés :", .Location = New Point(20, currentTop), .AutoSize = True})
        CLBProduits = New CheckedListBox With {.Location = New Point(20, currentTop + 20), .Width = 300, .Height = 100}
        Me.Controls.Add(CLBProduits)
        currentTop += 130

        Me.Controls.Add(New Label With {.Text = "Bilan :", .Location = New Point(20, currentTop), .AutoSize = True})
        RTBBilan = New RichTextBox With {.Location = New Point(20, currentTop + 20), .Width = 300, .Height = 80}
        Me.Controls.Add(RTBBilan)
        currentTop += 110

        bValider = New Button With {
            .Text = "Enregistrer le Compte-Rendu",
            .Location = New Point(20, currentTop),
            .Width = 300, .Height = 40,
            .BackColor = Color.LightSkyBlue,
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler bValider.Click, AddressOf bValider_Click
        Me.Controls.Add(bValider)
    End Sub

    Private Sub LoadData()
        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()

                Dim cmdP As New OdbcCommand("SELECT IDPRAT, (NOMPRAT || ' ' || PRENOMPRAT) AS NOM_COMPLET FROM PRATICIEN", conn)
                Dim dtP As New DataTable()
                dtP.Load(cmdP.ExecuteReader())
                CBPraticien.DisplayMember = "NOM_COMPLET"
                CBPraticien.ValueMember = "IDPRAT"
                CBPraticien.DataSource = dtP

                Dim cmdM As New OdbcCommand("SELECT MOTIF, LIBELLE FROM MOTIF", conn)
                Dim dtM As New DataTable()
                dtM.Load(cmdM.ExecuteReader())
                CBMotif.DisplayMember = "LIBELLE"
                CBMotif.ValueMember = "MOTIF"
                CBMotif.DataSource = dtM

                CLBProduits.Items.Clear()
                Dim cmdProd As New OdbcCommand("SELECT NOMPROD FROM PRODUIT", conn)
                Using reader = cmdProd.ExecuteReader()
                    While reader.Read()
                        CLBProduits.Items.Add(reader("NOMPROD").ToString())
                    End While
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur lors du chargement : " & ex.Message)
        End Try
    End Sub

    Public Sub PreparerModification(idCr As Integer)
        _editingCrId = idCr
        bValider.Text = "Mettre à jour le Compte-Rendu"
        bValider.BackColor = Color.Orange

        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                Dim query As String = "SELECT IDPRAT, LIBELLE, DATEVISITE, COEFCONFIANCE, BILANCR FROM GSBAdmin.CR WHERE IDCR = ?"
                Using cmd As New OdbcCommand(query, conn)
                    cmd.Parameters.Add("ID", OdbcType.Int).Value = idCr
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            CBPraticien.SelectedValue = reader("IDPRAT")
                            CBMotif.Text = reader("LIBELLE").ToString()
                            DTPVisite.Value = CDate(reader("DATEVISITE"))
                            NudConfiance.Value = CDec(reader("COEFCONFIANCE"))
                            RTBBilan.Text = reader("BILANCR").ToString()
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message)
        End Try
    End Sub

    Private Sub bValider_Click(sender As Object, e As EventArgs)
        If CBPraticien.SelectedValue Is Nothing OrElse CBMotif.SelectedValue Is Nothing Then
            MessageBox.Show("Veuillez sélectionner un praticien et un motif.")
            Return
        End If

        Dim sql As String
        Dim isEdit As Boolean = _editingCrId.HasValue

        If isEdit Then
            sql = "UPDATE GSBAdmin.CR SET IDPRAT=?, LIBELLE=?, DATEVISITE=?, COEFCONFIANCE=?, BILANCR=? WHERE IDCR=?"
        Else
            sql = "INSERT INTO GSBAdmin.CR (IDPRAT, LIBELLE, DATEVISITE, COEFCONFIANCE, BILANCR, IDUSER, IDCR, DATECR, VERSIONCR) VALUES (?, ?, ?, ?, ?, ?, ?, CURRENT_DATE, 1)"
        End If

        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                Using cmd As New OdbcCommand(sql, conn)
                    cmd.Parameters.Add("P", OdbcType.Int).Value = CBPraticien.SelectedValue
                    cmd.Parameters.Add("M", OdbcType.VarChar).Value = CBMotif.Text
                    cmd.Parameters.Add("D", OdbcType.Date).Value = DTPVisite.Value
                    cmd.Parameters.Add("C", OdbcType.Double).Value = CDbl(NudConfiance.Value)
                    cmd.Parameters.Add("B", OdbcType.VarChar).Value = RTBBilan.Text

                    If isEdit Then
                        cmd.Parameters.Add("ID", OdbcType.Int).Value = _editingCrId.Value
                    Else
                        cmd.Parameters.Add("U", OdbcType.Int).Value = _currentUserId
                        cmd.Parameters.Add("NEWID", OdbcType.Int).Value = New Random().Next(1000, 99999)
                    End If

                    cmd.ExecuteNonQuery()
                    MessageBox.Show(If(isEdit, "Modifié avec succès !", "Enregistré avec succès !"))

                    _editingCrId = Nothing
                    bValider.Text = "Enregistrer le Compte-Rendu"
                    bValider.BackColor = Color.LightSkyBlue
                    RTBBilan.Clear()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message)
        End Try
    End Sub
End Class