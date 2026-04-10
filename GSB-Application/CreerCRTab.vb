Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Data.Odbc

Public Class CreerCRTab
    Inherits UserControl

    Private ReadOnly _currentUserId As Integer
    Private _editingCrId As Integer? = Nothing

    Private WithEvents CBPraticien As ComboBox
    Private WithEvents CBMotif As ComboBox
    Private WithEvents CLBProduits As CheckedListBox
    Private WithEvents DTPVisite As DateTimePicker
    Private WithEvents RTBBilan As RichTextBox
    Private WithEvents bValider As Button
    Private WithEvents NudConfiance As NumericUpDown

    ' Dictionnaire NomProd -> IDPROD
    Private _produitsDict As New Dictionary(Of String, Integer)

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
        NudConfiance = New NumericUpDown With {.Location = New Point(120, currentTop - 3), .Minimum = 0, .Maximum = 5, .Value = 3}
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
            ' Praticiens
            Dim cmdP As New OdbcCommand("SELECT IDPRAT, (NOMPRAT || ' ' || PRENOMPRAT) AS NOM_COMPLET FROM PRATICIEN", DbManager.Connection)
            Dim dtP As New DataTable()
            dtP.Load(cmdP.ExecuteReader())
            CBPraticien.DisplayMember = "NOM_COMPLET"
            CBPraticien.ValueMember = "IDPRAT"
            CBPraticien.DataSource = dtP

            ' Motifs
            Dim cmdM As New OdbcCommand("SELECT MOTIF, LIBELLE FROM MOTIF", DbManager.Connection)
            Dim dtM As New DataTable()
            dtM.Load(cmdM.ExecuteReader())
            CBMotif.DisplayMember = "LIBELLE"
            CBMotif.ValueMember = "MOTIF"
            CBMotif.DataSource = dtM

            ' Produits avec leur IDPROD
            CLBProduits.Items.Clear()
            _produitsDict.Clear()
            Dim cmdProd As New OdbcCommand("SELECT IDPROD, NOMPROD FROM PRODUIT", DbManager.Connection)
            Using reader = cmdProd.ExecuteReader()
                While reader.Read()
                    Dim idProd As Integer = CInt(reader("IDPROD"))
                    Dim nomProd As String = reader("NOMPROD").ToString()
                    CLBProduits.Items.Add(nomProd)
                    _produitsDict(nomProd) = idProd
                End While
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
            ' Charger le CR
            Dim query As String = "SELECT IDPRAT, LIBELLE, DATEVISITE, COEFCONFIANCE, BILANCR FROM GSBAdmin.CR WHERE IDCR = ?"
            Using cmd As New OdbcCommand(query, DbManager.Connection)
                cmd.Parameters.Add("ID", OdbcType.Int).Value = idCr
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        CBPraticien.SelectedValue = reader("IDPRAT")
                        CBMotif.Text = reader("LIBELLE").ToString()
                        DTPVisite.Value = CDate(reader("DATEVISITE"))
                        NudConfiance.Value = CDec(reader("COEFCONFIANCE"))
                        RTBBilan.Text = If(IsDBNull(reader("BILANCR")), "", reader("BILANCR").ToString())
                    End If
                End Using
            End Using

            ' Décocher tout d'abord
            For i As Integer = 0 To CLBProduits.Items.Count - 1
                CLBProduits.SetItemChecked(i, False)
            Next

            ' Cocher les produits déjà distribués pour ce CR
            Dim queryDist As String = "SELECT P.NOMPROD FROM GSBAdmin.DISTRIBUTION D JOIN GSBAdmin.PRODUIT P ON D.IDPROD = P.IDPROD WHERE D.IDCR = ?"
            Using cmd2 As New OdbcCommand(queryDist, DbManager.Connection)
                cmd2.Parameters.Add("ID", OdbcType.Int).Value = idCr
                Using reader2 = cmd2.ExecuteReader()
                    While reader2.Read()
                        Dim nom As String = reader2("NOMPROD").ToString()
                        Dim idx As Integer = CLBProduits.Items.IndexOf(nom)
                        If idx >= 0 Then
                            CLBProduits.SetItemChecked(idx, True)
                        End If
                    End While
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Erreur complète : " & ex.ToString())
        End Try
    End Sub

    Private Sub bValider_Click(sender As Object, e As EventArgs)
        If CBPraticien.SelectedValue Is Nothing OrElse CBMotif.SelectedValue Is Nothing Then
            MessageBox.Show("Veuillez sélectionner un praticien et un motif.")
            Return
        End If

        Dim isEdit As Boolean = _editingCrId.HasValue
        Dim idCr As Integer

        Try
            If isEdit Then
                idCr = _editingCrId.Value

                ' Mise à jour du CR
                Dim sqlUpdate As String = "UPDATE GSBAdmin.CR SET IDPRAT=?, LIBELLE=?, DATEVISITE=?, COEFCONFIANCE=?, BILANCR=? WHERE IDCR=?"
                Using cmd As New OdbcCommand(sqlUpdate, DbManager.Connection)
                    cmd.Parameters.Add("P", OdbcType.Int).Value = CBPraticien.SelectedValue
                    cmd.Parameters.Add("M", OdbcType.VarChar).Value = CBMotif.Text
                    cmd.Parameters.Add("D", OdbcType.Date).Value = DTPVisite.Value
                    cmd.Parameters.Add("C", OdbcType.Double).Value = CDbl(NudConfiance.Value)
                    cmd.Parameters.Add("B", OdbcType.VarChar).Value = RTBBilan.Text
                    cmd.Parameters.Add("ID", OdbcType.Int).Value = idCr
                    cmd.ExecuteNonQuery()
                End Using

                ' Supprimer les anciennes distributions
                Dim sqlDelDist As String = "DELETE FROM GSBAdmin.DISTRIBUTION WHERE IDCR = ?"
                Using cmd As New OdbcCommand(sqlDelDist, DbManager.Connection)
                    cmd.Parameters.Add("ID", OdbcType.Int).Value = idCr
                    cmd.ExecuteNonQuery()
                End Using

            Else
                idCr = New Random().Next(1000, 99999)

                ' Insertion du CR
                Dim sqlInsert As String = "INSERT INTO GSBAdmin.CR (IDPRAT, LIBELLE, DATEVISITE, COEFCONFIANCE, BILANCR, IDUSER, IDCR, DATECR, VERSIONCR) VALUES (?, ?, ?, ?, ?, ?, ?, CURRENT_DATE, 1)"
                Using cmd As New OdbcCommand(sqlInsert, DbManager.Connection)
                    cmd.Parameters.Add("P", OdbcType.Int).Value = CBPraticien.SelectedValue
                    cmd.Parameters.Add("M", OdbcType.VarChar).Value = CBMotif.Text
                    cmd.Parameters.Add("D", OdbcType.Date).Value = DTPVisite.Value
                    cmd.Parameters.Add("C", OdbcType.Double).Value = CDbl(NudConfiance.Value)
                    cmd.Parameters.Add("B", OdbcType.VarChar).Value = RTBBilan.Text
                    cmd.Parameters.Add("U", OdbcType.Int).Value = _currentUserId
                    cmd.Parameters.Add("NEWID", OdbcType.Int).Value = idCr
                    cmd.ExecuteNonQuery()
                End Using
            End If

            ' Insérer les distributions cochées
            For Each item As Object In CLBProduits.CheckedItems
                Dim nomProd As String = item.ToString()
                If _produitsDict.ContainsKey(nomProd) Then
                    Dim idProd As Integer = _produitsDict(nomProd)
                    Dim sqlDist As String = "INSERT INTO GSBAdmin.DISTRIBUTION (IDPROD, IDCR, VERSIONCR, NOMBREECHANTILLONS) VALUES (?, ?, 1, 0)"
                    Using cmdDist As New OdbcCommand(sqlDist, DbManager.Connection)
                        cmdDist.Parameters.Add("PROD", OdbcType.Int).Value = idProd
                        cmdDist.Parameters.Add("CR", OdbcType.Int).Value = idCr
                        cmdDist.ExecuteNonQuery()
                    End Using
                End If
            Next

            MessageBox.Show(If(isEdit, "Modifié avec succès !", "Enregistré avec succès !"))

            ' Reset
            _editingCrId = Nothing
            bValider.Text = "Enregistrer le Compte-Rendu"
            bValider.BackColor = Color.LightSkyBlue
            RTBBilan.Clear()
            For i As Integer = 0 To CLBProduits.Items.Count - 1
                CLBProduits.SetItemChecked(i, False)
            Next

        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.ToString())
        End Try
    End Sub
End Class