Imports System
Imports System.Data.Odbc
Imports System.Drawing
Imports System.Windows.Forms

Public Class StatsRegTab
    Inherits UserControl

    ' ---------- Propriétés & Connexion ----------
    Private ReadOnly _regionId As Integer
    Private _selectedVisitorId As Integer = -1
    Private ReadOnly connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"

    ' ---------- Controls ----------
    Private panelFilters As GroupBox
    Private dtStart As DateTimePicker
    Private dtEnd As DateTimePicker
    Private btnApply As Button
    Private btnReset As Button
    Private lstVisitors As ListBox
    Private flowMetrics As FlowLayoutPanel

    ' ---------- Constructor ----------
    Public Sub New(regionId As Integer)
        _regionId = regionId
        Me.Dock = DockStyle.Fill
        InitializeUI()
        LoadVisitors()
        LoadMetrics()
    End Sub

    ' ---------- UI Initialization ----------
    Private Sub InitializeUI()
        Me.BackColor = Color.White

        ' --- Panel Filters ---
        panelFilters = New GroupBox With {
            .Text = "Filtres :",
            .Dock = DockStyle.Top,
            .Height = 250,
            .Padding = New Padding(8)
        }

        Dim lblPeriode As New Label With {.Text = "Période :", .Location = New Point(8, 20), .AutoSize = True}
        Dim lblDebut As New Label With {.Text = "Début :", .Location = New Point(8, 50), .AutoSize = True}
        dtStart = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Value = New Date(Date.Now.Year, 1, 1), .Location = New Point(70, 47), .Width = 120}
        Dim lblFin As New Label With {.Text = "Fin :", .Location = New Point(8, 80), .AutoSize = True}
        dtEnd = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Value = New Date(Date.Now.Year, 12, 31), .Location = New Point(70, 77), .Width = 120}

        btnApply = New Button With {.Text = "Appliquer", .Width = 100, .Location = New Point(8, 110)}
        AddHandler btnApply.Click, Sub() LoadMetrics()

        btnReset = New Button With {.Text = "Réinitialiser", .Width = 100, .Location = New Point(118, 110)}
        AddHandler btnReset.Click,
            Sub()
                dtStart.Value = New Date(Date.Now.Year, 1, 1)
                dtEnd.Value = New Date(Date.Now.Year, 12, 31)
                _selectedVisitorId = -1
                lstVisitors.ClearSelected()
                LoadMetrics()
            End Sub

        Dim lblVisitors As New Label With {.Text = "Visiteurs :", .Location = New Point(8, 145), .AutoSize = True}
        lstVisitors = New ListBox With {.Location = New Point(8, 165), .Width = 200, .Height = 250}
        AddHandler lstVisitors.SelectedIndexChanged,
            Sub()
                If lstVisitors.SelectedIndex >= 0 Then
                    _selectedVisitorId = CType(lstVisitors.SelectedItem, KeyValuePair(Of Integer, String)).Key
                Else
                    _selectedVisitorId = -1
                End If
                LoadMetrics()
            End Sub

        panelFilters.Controls.AddRange({lblPeriode, lblDebut, dtStart, lblFin, dtEnd, btnApply, btnReset, lblVisitors, lstVisitors})
        Me.Controls.Add(panelFilters)

        ' --- FlowLayoutPanel pour les metrics ---
        flowMetrics = New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(8)
        }
        AddHandler flowMetrics.Resize, AddressOf OnFlowMetricsResize
        Me.Controls.Add(flowMetrics)
    End Sub

    ' ---------- Load Visitors ----------
    Private Sub LoadVisitors()
        lstVisitors.Items.Clear()
        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                ' Récupère tous les visiteurs de la région
                Dim sqlVisitors As String =
                    "SELECT U.IDUSER, U.NOMUSER || ' ' || U.PRENOMUSER AS FULLNAME " &
                    "FROM GSBAdmin.UTILISATEUR U " &
                    "JOIN GSBAdmin.VISITEUR V ON U.IDUSER = V.IDUSER " &
                    "WHERE V.IDUSER IN (SELECT IDUSER FROM GSBAdmin.VISITEUR WHERE IDUSER IN " &
                    "(SELECT IDUSER FROM GSBAdmin.VISITEUR WHERE IDUSER IN (SELECT IDUSER FROM GSBAdmin.VISITEUR WHERE 1=1))) " & ' simplifié pour région
                    "AND V.IDUSER IN (SELECT IDUSER FROM GSBAdmin.VISITEUR WHERE IDUSER IN (SELECT IDUSER FROM GSBAdmin.VISITEUR)) " & ' placeholder
                    "ORDER BY U.NOMUSER, U.PRENOMUSER"

                ' --- Pour la vraie région, il faut remplacer par IDREGION = _regionId si la table VISITEUR a le champ IDREGION ---
                Using cmd As New OdbcCommand(sqlVisitors, conn)
                    ' cmd.Parameters.Add("p1", OdbcType.Int).Value = _regionId
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim id As Integer = CInt(reader("IDUSER"))
                            Dim name As String = reader("FULLNAME").ToString()
                            lstVisitors.Items.Add(New KeyValuePair(Of Integer, String)(id, name))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur récupération visiteurs : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---------- Load Metrics ----------
    Private Sub LoadMetrics()
        flowMetrics.Controls.Clear()
        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()
                Dim userFilter As String = ""
                Dim paramValue As Integer = 0
                If _selectedVisitorId > 0 Then
                    userFilter = " AND C.IDUSER = ? "
                    paramValue = _selectedVisitorId
                End If

                ' 1) Nombre de visites
                Dim sqlVisites As String = "SELECT COUNT(*) FROM GSBAdmin.CR C WHERE C.DATEVISITE BETWEEN ? AND ? " & userFilter
                Using cmd As New OdbcCommand(sqlVisites, conn)
                    cmd.Parameters.Add("p1", OdbcType.Date).Value = dtStart.Value.Date
                    cmd.Parameters.Add("p2", OdbcType.Date).Value = dtEnd.Value.Date
                    If _selectedVisitorId > 0 Then cmd.Parameters.Add("p3", OdbcType.Int).Value = paramValue
                    Dim nbVisites = If(cmd.ExecuteScalar() IsNot Nothing, cmd.ExecuteScalar().ToString(), "0")
                    flowMetrics.Controls.Add(CreateMetricCard("Nombre de visites :", nbVisites))
                End Using

                ' 2) Nombre échantillons donnés
                Dim sqlEchant As String = "SELECT NVL(SUM(D.NOMBREECHANTILLONS),0) " &
                                          "FROM GSBAdmin.DISTRIBUTION D " &
                                          "JOIN GSBAdmin.CR C ON D.IDCR = C.IDCR " &
                                          "WHERE C.DATEVISITE BETWEEN ? AND ? " & userFilter
                Using cmd As New OdbcCommand(sqlEchant, conn)
                    cmd.Parameters.Add("p1", OdbcType.Date).Value = dtStart.Value.Date
                    cmd.Parameters.Add("p2", OdbcType.Date).Value = dtEnd.Value.Date
                    If _selectedVisitorId > 0 Then cmd.Parameters.Add("p3", OdbcType.Int).Value = paramValue
                    Dim nbEchant = If(cmd.ExecuteScalar() IsNot Nothing, cmd.ExecuteScalar().ToString(), "0")
                    flowMetrics.Controls.Add(CreateMetricCard("Nombre échantillons donnés :", nbEchant))
                End Using

                ' 3) Praticiens visités
                Dim sqlPrats As String = "SELECT COUNT(DISTINCT C.IDPRAT) FROM GSBAdmin.CR C WHERE C.DATEVISITE BETWEEN ? AND ? " & userFilter
                Using cmd As New OdbcCommand(sqlPrats, conn)
                    cmd.Parameters.Add("p1", OdbcType.Date).Value = dtStart.Value.Date
                    cmd.Parameters.Add("p2", OdbcType.Date).Value = dtEnd.Value.Date
                    If _selectedVisitorId > 0 Then cmd.Parameters.Add("p3", OdbcType.Int).Value = paramValue
                    Dim nbPrats = If(cmd.ExecuteScalar() IsNot Nothing, cmd.ExecuteScalar().ToString(), "0")
                    flowMetrics.Controls.Add(CreateMetricCard("Praticiens visités :", nbPrats))
                End Using

                ' 4) Motif le plus utilisé
                Dim sqlMotif As String = "SELECT M.LIBELLE FROM GSBAdmin.MOTIF M " &
                                         "JOIN GSBAdmin.CR C ON M.LIBELLE = C.LIBELLE " &
                                         "WHERE C.DATEVISITE BETWEEN ? AND ? " & userFilter &
                                         "GROUP BY M.LIBELLE ORDER BY COUNT(C.IDCR) DESC FETCH FIRST 1 ROWS ONLY"
                Using cmd As New OdbcCommand(sqlMotif, conn)
                    cmd.Parameters.Add("p1", OdbcType.Date).Value = dtStart.Value.Date
                    cmd.Parameters.Add("p2", OdbcType.Date).Value = dtEnd.Value.Date
                    If _selectedVisitorId > 0 Then cmd.Parameters.Add("p3", OdbcType.Int).Value = paramValue
                    Dim topMotif = If(cmd.ExecuteScalar() IsNot Nothing, cmd.ExecuteScalar().ToString(), "-")
                    flowMetrics.Controls.Add(CreateMetricCard("Motif le plus utilisé :", topMotif))
                End Using

            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur récupération statistiques : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        OnFlowMetricsResize(Nothing, EventArgs.Empty)
    End Sub

    ' ---------- Helper : create a metric card ----------
    Private Function CreateMetricCard(labelText As String, valueText As String) As Panel
        Dim pnl As New Panel With {
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BorderStyle = BorderStyle.FixedSingle,
            .Padding = New Padding(8),
            .Margin = New Padding(6),
            .Dock = DockStyle.Top
        }

        Dim lblLabel As New Label With {
            .Text = labelText,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .AutoSize = True,
            .Dock = DockStyle.Top
        }

        Dim lblValue As New Label With {
            .Text = valueText,
            .Font = New Font("Segoe UI", 9, FontStyle.Regular),
            .AutoSize = True,
            .Dock = DockStyle.Top,
            .Tag = "metricValue"
        }

        pnl.Controls.Add(lblValue)
        pnl.Controls.Add(lblLabel)
        Return pnl
    End Function

    Private Sub OnFlowMetricsResize(sender As Object, e As EventArgs)
        Try
            Dim targetWidth As Integer = Math.Max(200, flowMetrics.ClientSize.Width - flowMetrics.Padding.Horizontal - 20)
            For Each ctrl As Control In flowMetrics.Controls
                ctrl.Width = targetWidth
                For Each child As Control In ctrl.Controls
                    If child.Tag IsNot Nothing AndAlso child.Tag.ToString() = "metricValue" Then
                        child.MaximumSize = New Size(Math.Max(100, targetWidth - ctrl.Padding.Horizontal - 10), 0)
                    End If
                Next
            Next
        Catch
        End Try
    End Sub

End Class
