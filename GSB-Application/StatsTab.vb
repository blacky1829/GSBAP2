Imports System
Imports System.Data.Odbc
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Public Class StatsTab
    Inherits UserControl

    Private ReadOnly _userId As Integer
    Private ReadOnly connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"

    Private tblMain As TableLayoutPanel
    Private panelFiltres As GroupBox
    Private panelStats As GroupBox
    Private panelInfos As GroupBox
    Private panelPortefeuille As GroupBox
    Private ReadOnly _defaultUserId As Integer
    Private _currentUserId As Integer



    Private dtDebut As DateTimePicker
    Private dtFin As DateTimePicker
    Private cbPraticien As ComboBox
    Private btnApply As Button
    Private btnReset As Button
    Private lbPortefeuille As ListBox

    Private practList As List(Of KeyValuePair(Of Integer, String))

    Public Sub New(userId As Integer)
        _userId = userId
        _defaultUserId = userId ' <-- on stocke l'utilisateur par défaut
        Me.Dock = DockStyle.Fill
        InitializeUI()
        LoadPraticiens()
        ConstruireStatsDynamiques(_defaultUserId) ' stats initiales
    End Sub



    Public Sub LoadStatsForUser(userId As Integer)
        _currentUserId = userId

        LoadPraticiens()
        LoadUserInfo(_currentUserId)
        ConstruireStatsDynamiques()
    End Sub



    Public ReadOnly Property DefaultUserId As Integer
        Get
            Return _defaultUserId
        End Get
    End Property

    Private Sub LoadUserInfo(userId As Integer)
        panelInfos.Controls.Clear()
        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                Dim sql As String = "SELECT NOMUSER, PRENOMUSER, EMAILUSER FROM GSBAdmin.UTILISATEUR WHERE IDUSER=?"
                Using cmd As New OdbcCommand(sql, conn)
                    cmd.Parameters.Add("p1", OdbcType.Int).Value = _currentUserId

                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim yPos As Integer = 20
                            panelInfos.Controls.Add(CaseStat(panelInfos, "Nom :", reader("NOMUSER").ToString(), yPos))
                            yPos += 35
                            panelInfos.Controls.Add(CaseStat(panelInfos, "Prénom :", reader("PRENOMUSER").ToString(), yPos))
                            yPos += 35
                            panelInfos.Controls.Add(CaseStat(panelInfos, "Email :", reader("EMAILUSER").ToString(), yPos))
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            WriteLog("LoadUserInfo: " & ex.Message)
        End Try
    End Sub

    Private Sub LoadPortefeuille(userId As Integer)
        lbPortefeuille.Items.Clear()
        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                Dim sql As String = "SELECT P.IDPRAT, NVL(P.NOMPRAT,''), NVL(P.PRENOMPRAT,''), NVL(P.SPECIALITEPRAT,'') " &
                                "FROM GSBAdmin.PRATICIEN P JOIN GSBAdmin.PORTEFEUILLE PF ON PF.IDPRAT = P.IDPRAT " &
                                "WHERE PF.IDUSER=? ORDER BY P.IDPRAT"
                Using cmd As New OdbcCommand(sql, conn)
                    cmd.Parameters.Add("p1", OdbcType.Int).Value = _currentUserId
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim label = If(String.IsNullOrWhiteSpace(reader("NOMPRAT").ToString()) AndAlso String.IsNullOrWhiteSpace(reader("PRENOMPRAT").ToString()),
                                        $"ID {reader("IDPRAT")} | {reader("SPECIALITEPRAT")}",
                                        $"{reader("NOMPRAT")} {reader("PRENOMPRAT")} | {reader("SPECIALITEPRAT")}")
                            lbPortefeuille.Items.Add(label)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            WriteLog("LoadPortefeuille: " & ex.Message)
        End Try
    End Sub


    Private Sub ConstruireStatsDynamiques(Optional praticienId As Integer = -1)
        panelStats.Controls.Clear()
        Dim statsY As Integer = 30

        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()

                ' 1) Nombre de visites
                Dim sqlVisites As String = "SELECT COUNT(*) FROM GSBAdmin.CR WHERE IDUSER=? AND DATEVISITE BETWEEN ? AND ?"
                If praticienId > 0 Then sqlVisites &= " AND IDPRAT = ?"
                Using cmdVisites As New OdbcCommand(sqlVisites, conn)
                    cmdVisites.Parameters.Add("p1", OdbcType.Int).Value = _currentUserId
                    cmdVisites.Parameters.Add("p2", OdbcType.Date).Value = dtDebut.Value.Date
                    cmdVisites.Parameters.Add("p3", OdbcType.Date).Value = dtFin.Value.Date
                    If praticienId > 0 Then cmdVisites.Parameters.Add("p4", OdbcType.Int).Value = praticienId

                    Dim nbVisitesObj As Object = cmdVisites.ExecuteScalar()
                    Dim nbVisites As String = If(nbVisitesObj IsNot Nothing, nbVisitesObj.ToString(), "0")
                    panelStats.Controls.Add(CaseStat(panelStats, "Nombre de visites :", nbVisites, statsY))
                    statsY += 45
                End Using

                ' 2) Échantillons donnés
                Dim sqlEchant As String =
                "SELECT NVL(SUM(D.NOMBREECHANTILLONS),0) FROM GSBAdmin.DISTRIBUTION D JOIN GSBAdmin.CR C ON D.IDCR=C.IDCR " &
                "WHERE C.IDUSER=? AND C.DATEVISITE BETWEEN ? AND ?"
                If praticienId > 0 Then sqlEchant &= " AND C.IDPRAT = ?"
                Using cmdEchant As New OdbcCommand(sqlEchant, conn)
                    cmdEchant.Parameters.Add("p1", OdbcType.Int).Value = _currentUserId
                    cmdEchant.Parameters.Add("p2", OdbcType.Date).Value = dtDebut.Value.Date
                    cmdEchant.Parameters.Add("p3", OdbcType.Date).Value = dtFin.Value.Date
                    If praticienId > 0 Then cmdEchant.Parameters.Add("p4", OdbcType.Int).Value = praticienId

                    Dim nbEchantillonsObj As Object = cmdEchant.ExecuteScalar()
                    Dim nbEchantillons As String = If(nbEchantillonsObj IsNot Nothing, nbEchantillonsObj.ToString(), "0")
                    panelStats.Controls.Add(CaseStat(panelStats, "Échantillons donnés :", nbEchantillons, statsY))
                    statsY += 45
                End Using

                ' 3) Praticiens visités (distincts)
                Dim sqlPrats As String = "SELECT COUNT(DISTINCT IDPRAT) FROM GSBAdmin.CR WHERE IDUSER=? AND DATEVISITE BETWEEN ? AND ?"
                If praticienId > 0 Then sqlPrats &= " AND IDPRAT = ?"
                Using cmdPrats As New OdbcCommand(sqlPrats, conn)
                    cmdPrats.Parameters.Add("p1", OdbcType.Int).Value = _currentUserId
                    cmdPrats.Parameters.Add("p2", OdbcType.Date).Value = dtDebut.Value.Date
                    cmdPrats.Parameters.Add("p3", OdbcType.Date).Value = dtFin.Value.Date
                    If praticienId > 0 Then cmdPrats.Parameters.Add("p4", OdbcType.Int).Value = praticienId

                    Dim nbPratsObj As Object = cmdPrats.ExecuteScalar()
                    Dim nbPrats As String = If(nbPratsObj IsNot Nothing, nbPratsObj.ToString(), "0")
                    panelStats.Controls.Add(CaseStat(panelStats, "Praticiens visitésss :", nbPrats, statsY))
                    statsY += 45
                End Using

                ' 4) Motif le plus utilisé
                Dim sqlMotif As String =
                "SELECT M.LIBELLE FROM GSBAdmin.MOTIF M JOIN GSBAdmin.CR C ON M.LIBELLE = C.LIBELLE " &
                "WHERE C.IDUSER=? AND C.DATEVISITE BETWEEN ? AND ?"
                If praticienId > 0 Then sqlMotif &= " AND C.IDPRAT = ?"
                sqlMotif &= " GROUP BY M.LIBELLE ORDER BY COUNT(C.IDCR) DESC FETCH FIRST 1 ROWS ONLY"
                Using cmdMotif As New OdbcCommand(sqlMotif, conn)
                    cmdMotif.Parameters.Add("p1", OdbcType.Int).Value = _currentUserId
                    cmdMotif.Parameters.Add("p2", OdbcType.Date).Value = dtDebut.Value.Date
                    cmdMotif.Parameters.Add("p3", OdbcType.Date).Value = dtFin.Value.Date
                    If praticienId > 0 Then cmdMotif.Parameters.Add("p4", OdbcType.Int).Value = praticienId

                    Dim topMotifObj As Object = cmdMotif.ExecuteScalar()
                    Dim motifTexte As String = If(topMotifObj IsNot Nothing, topMotifObj.ToString(), "-")
                    panelStats.Controls.Add(CaseStat(panelStats, "Motif le plus utilisé :", motifTexte, statsY))
                    statsY += 45
                End Using

            End Using
        Catch ex As Exception
            WriteLog("ConstruireStatsDynamiques error: " & ex.Message)
            MessageBox.Show("Erreur ODBC : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function CaseStat(parent As Control, labelText As String, valueText As String, y As Integer, Optional width As Integer = 250) As Panel
        Dim panel As New Panel() With {
        .Width = width,
        .Height = 40,
        .Location = New Point(15, y)
    }

        Dim lblLabel As New Label() With {
        .Text = labelText,
        .AutoSize = True,
        .Location = New Point(0, 10),
        .Font = New Font("Segoe UI", 9, FontStyle.Bold)
    }

        Dim lblValue As New Label() With {
        .Text = valueText,
        .AutoSize = True,
        .Location = New Point(150, 10),
        .Font = New Font("Segoe UI", 9),
        .TextAlign = ContentAlignment.MiddleRight
    }

        panel.Controls.Add(lblLabel)
        panel.Controls.Add(lblValue)
        Return panel
    End Function



    'initialize UI
    Private Sub InitializeUI()
        Me.BackColor = Color.White

        tblMain = New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(10),
            .ColumnCount = 3,
            .RowCount = 2
        }
        tblMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30))
        tblMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45))
        tblMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
        tblMain.RowStyles.Add(New RowStyle(SizeType.Percent, 65))
        tblMain.RowStyles.Add(New RowStyle(SizeType.Percent, 35))

        panelFiltres = New GroupBox() With {.Text = "Filtres :", .Dock = DockStyle.Fill, .Padding = New Padding(8)}
        panelStats = New GroupBox() With {.Text = "Statistiques", .Dock = DockStyle.Fill, .Padding = New Padding(8)}
        panelInfos = New GroupBox() With {.Text = "Informations personnelles", .Dock = DockStyle.Fill, .Padding = New Padding(8)}
        panelPortefeuille = New GroupBox() With {.Text = "Mon portefeuille de praticiens", .Dock = DockStyle.Fill, .Padding = New Padding(8)}

        tblMain.Controls.Add(panelFiltres, 0, 0)
        tblMain.Controls.Add(panelStats, 1, 0)
        tblMain.Controls.Add(panelInfos, 2, 0)
        tblMain.Controls.Add(panelPortefeuille, 0, 1)

        ' Filtres
        Dim lblPraticien As New Label With {.Text = "Praticien :", .Location = New Point(8, 110), .AutoSize = True}
        cbPraticien = New ComboBox With {.Location = New Point(80, 106), .Width = 160, .DropDownStyle = ComboBoxStyle.DropDownList}

        btnApply = New Button With {.Text = "Appliquer", .Width = 100, .Location = New Point(118, 145)}
        AddHandler btnApply.Click,
            Sub()
                ConstruireStatsDynamiques(SelectedPraticienId())
            End Sub

        btnReset = New Button With {.Text = "Réinitialiser", .Width = 100, .Location = New Point(8, 145)}
        AddHandler btnReset.Click,
            Sub()
                dtDebut.Value = New Date(Date.Now.Year, 1, 1)
                dtFin.Value = New Date(Date.Now.Year, 12, 31)
                cbPraticien.SelectedIndex = 0
                ConstruireStatsDynamiques(-1)
            End Sub

        AddHandler cbPraticien.SelectedIndexChanged,
            Sub()
                ConstruireStatsDynamiques(SelectedPraticienId())
            End Sub

        ' Periode
        Dim lblPeriode As New Label With {.Text = "Période sélectionnée :", .Location = New Point(8, 18), .AutoSize = True}
        Dim lblDebut As New Label With {.Text = "Début :", .Location = New Point(8, 48), .AutoSize = True}
        dtDebut = New DateTimePicker() With {.Format = DateTimePickerFormat.Short, .Value = New Date(Date.Now.Year, 1, 1), .Location = New Point(70, 45), .Width = 120}

        Dim lblFin As New Label With {.Text = "Fin :", .Location = New Point(8, 80), .AutoSize = True}
        dtFin = New DateTimePicker() With {.Format = DateTimePickerFormat.Short, .Value = New Date(Date.Now.Year, 12, 31), .Location = New Point(70, 77), .Width = 120}

        panelFiltres.Controls.AddRange(New Control() {
            lblPeriode, lblDebut, dtDebut, lblFin, dtFin,
            lblPraticien, cbPraticien,
            btnReset, btnApply
        })

        lbPortefeuille = New ListBox() With {.Dock = DockStyle.Fill}
        panelPortefeuille.Controls.Add(lbPortefeuille)

        Me.Controls.Add(tblMain)
    End Sub

    ' helper to get selected praticien ID
    Private Function SelectedPraticienId() As Integer
        Dim selected = CType(cbPraticien.SelectedItem, KeyValuePair(Of Integer, String))
        Return selected.Key
    End Function



    'praticiens
    Private Sub LoadPraticiens()
        practList = New List(Of KeyValuePair(Of Integer, String))()
        practList.Add(New KeyValuePair(Of Integer, String)(-1, "(Tous)"))
        lbPortefeuille.Items.Clear()

        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()

                Dim query As String =
                    "SELECT P.IDPRAT, NVL(P.NOMPRAT,'') AS NOMPRAT, NVL(P.PRENOMPRAT,'') AS PRENOMPRAT, NVL(P.SPECIALITEPRAT,'') AS SPECIALITE " &
                    "FROM GSBAdmin.PRATICIEN P " &
                    "JOIN GSBAdmin.PORTEFEUILLE PF ON PF.IDPRAT = P.IDPRAT " &
                    "WHERE PF.IDUSER = ? ORDER BY P.IDPRAT"

                Using cmd As New OdbcCommand(query, conn)
                    cmd.Parameters.Add("IDUSER", OdbcType.Int).Value = _currentUserId

                    Using reader As OdbcDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim id As Integer = CInt(reader("IDPRAT"))
                            Dim nom As String = reader("NOMPRAT").ToString()
                            Dim prenom As String = reader("PRENOMPRAT").ToString()
                            Dim spec As String = reader("SPECIALITE").ToString()

                            Dim label As String =
                                If(nom.Trim() = "" AndAlso prenom.Trim() = "",
                                   $"ID {id} | {spec}",
                                   $"{nom} {prenom} | {spec}")

                            lbPortefeuille.Items.Add(label)
                            practList.Add(New KeyValuePair(Of Integer, String)(id, label))
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            WriteLog("LoadPraticiens: " & ex.Message)
        End Try

        ' bind to ComboBox
        cbPraticien.DataSource = New BindingSource(practList, Nothing)
        cbPraticien.DisplayMember = "Value"
        cbPraticien.ValueMember = "Key"

        cbPraticien.SelectedIndex = 0
    End Sub

    'logging
    Private Sub WriteLog(message As String)
        Try
            Dim logPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt")
            Using sw As New StreamWriter(logPath, True)
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}")
            End Using
        Catch
        End Try
    End Sub

End Class
