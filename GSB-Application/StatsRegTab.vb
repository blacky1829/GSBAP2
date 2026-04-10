Imports System
Imports System.Data.Odbc
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class StatsRegTab
    Inherits UserControl

    ' ---------- Champs ----------
    Private ReadOnly _regionId As Integer
    Private _selectedVisitorId As Integer = -1
    ' Use shared connection from DbManager

    ' ---------- Controls ----------
    Private panelFilters As GroupBox
    Private dtStart As DateTimePicker
    Private dtEnd As DateTimePicker
    Private btnApply As Button
    Private btnReset As Button
    Private btnVoir As Button
    Private lstVisitors As ListBox
    Private flowMetrics As FlowLayoutPanel

    ' ---------- Constructor ----------
    Public Sub New(regionOrUserId As Integer)
        Try
            ' Resolve argument: it may be a region id or a user id. Try to detect.
            Dim resolvedId As Integer = regionOrUserId

            ' Ensure central connection is available
            If DbManager.Connection Is Nothing OrElse DbManager.Connection.State <> ConnectionState.Open Then
                Try
                    DbManager.Initialize()
                Catch exInit As Exception
                    Dim temp = System.IO.Path.GetTempPath()
                    Try
                        System.IO.File.AppendAllText(System.IO.Path.Combine(temp, "statsreg_error.log"), DateTime.Now.ToString("s") & " - DbManager.Initialize failed: " & exInit.ToString() & Environment.NewLine)
                    Catch
                    End Try
                    MessageBox.Show("Impossible d'ouvrir la connexion à la base : " & exInit.Message, "Erreur connexion", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If

            ' If a region with this ID exists, use it
            Using cmdCheck As New OdbcCommand("SELECT COUNT(*) FROM gsbAdmin.REGION WHERE ID = ?", DbManager.Connection)
                cmdCheck.Parameters.Add(New OdbcParameter With {.OdbcType = OdbcType.Int, .Value = regionOrUserId})
                Dim cntObj = cmdCheck.ExecuteScalar()
                If cntObj IsNot Nothing AndAlso Convert.ToInt32(cntObj) > 0 Then
                    resolvedId = regionOrUserId
                Else
                    ' Otherwise try to find the region id for this user
                    Using cmd As New OdbcCommand("SELECT R.ID FROM gsbAdmin.REGION R JOIN gsbAdmin.UTILISATEUR U ON U.LIBELLE = R.LIBELLE WHERE U.IDUSER = ?", DbManager.Connection)
                        cmd.Parameters.Add(New OdbcParameter With {.OdbcType = OdbcType.Int, .Value = regionOrUserId})
                        Dim obj = cmd.ExecuteScalar()
                        If obj IsNot Nothing AndAlso Not IsDBNull(obj) Then
                            resolvedId = Convert.ToInt32(obj)
                        End If
                    End Using
                End If
            End Using

            _regionId = resolvedId
            Me.Dock = DockStyle.Fill
            InitializeUI()
            LoadVisitors()
            LoadMetrics()
        Catch ex As Exception
            ' If any unexpected error occurs during construction, log to temp and show a messagebox so tab doesn't silently disappear
            Try
                Dim temp = System.IO.Path.GetTempPath()
                System.IO.File.AppendAllText(System.IO.Path.Combine(temp, "statsreg_error.log"), DateTime.Now.ToString("s") & " - Constructor fatal error: " & ex.ToString() & Environment.NewLine)
            Catch : End Try
            MessageBox.Show("Erreur lors de la construction de l'onglet Stats régionales : " & ex.Message & vbCrLf & "Voir %TEMP%\statsreg_error.log pour plus de détails.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ' create minimal UI so tab exists
            _regionId = regionOrUserId
            Me.Dock = DockStyle.Fill
            InitializeUI()
        End Try
    End Sub

    ' ---------- UI ----------
    Private Sub InitializeUI()
        Me.BackColor = Color.White

        panelFilters = New GroupBox With {
            .Text = "Filtres",
            .Dock = DockStyle.Left,
            .Width = 280,
            .Padding = New Padding(10)
        }

        Dim lblPeriode As New Label With {.Text = "Période", .Location = New Point(10, 25)}
        Dim lblDebut As New Label With {.Text = "Début", .Location = New Point(10, 55)}
        dtStart = New DateTimePicker With {
            .Format = DateTimePickerFormat.Short,
            .Value = New Date(Date.Now.Year, 1, 1),
            .Location = New Point(70, 50)
        }

        Dim lblFin As New Label With {.Text = "Fin", .Location = New Point(10, 85)}
        dtEnd = New DateTimePicker With {
            .Format = DateTimePickerFormat.Short,
            .Value = New Date(Date.Now.Year, 12, 31),
            .Location = New Point(70, 80)
        }

        btnApply = New Button With {
            .Text = "Appliquer",
            .Location = New Point(10, 115),
            .Width = 100
        }
        AddHandler btnApply.Click, Sub() LoadMetrics()

        btnReset = New Button With {
            .Text = "Réinitialiser",
            .Location = New Point(120, 115),
            .Width = 100
        }
        AddHandler btnReset.Click,
            Sub()
                dtStart.Value = New Date(Date.Now.Year, 1, 1)
                dtEnd.Value = New Date(Date.Now.Year, 12, 31)
                _selectedVisitorId = -1
                lstVisitors.ClearSelected()
                LoadMetrics()
            End Sub

        Dim lblVisitors As New Label With {
            .Text = "Visiteurs",
            .Location = New Point(10, 155)
        }

        lstVisitors = New ListBox With {
            .Location = New Point(10, 175),
            .Width = 250,
            .Height = 200,
            .DisplayMember = "Value"
        }

        AddHandler lstVisitors.SelectedIndexChanged,
            Sub()
                If lstVisitors.SelectedIndex >= 0 Then
                    _selectedVisitorId =
                        CType(lstVisitors.SelectedItem, KeyValuePair(Of Integer, String)).Key
                Else
                    _selectedVisitorId = -1
                End If
            End Sub

        btnVoir = New Button With {
            .Text = "Voir",
            .Location = New Point(10, 385),
            .Width = 250
        }
        AddHandler btnVoir.Click, Sub() LoadMetrics()

        panelFilters.Controls.AddRange({
            lblPeriode, lblDebut, dtStart, lblFin, dtEnd,
            btnApply, btnReset,
            lblVisitors, lstVisitors, btnVoir
        })

        Me.Controls.Add(panelFilters)

        flowMetrics = New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(15)
        }

        AddHandler flowMetrics.Resize, AddressOf ResizeMetricCards
        Me.Controls.Add(flowMetrics)
    End Sub

    ' ---------- Load Visitors ----------
    Private Sub LoadVisitors()
        lstVisitors.Items.Clear()

        Try
            ' debug log
            Try
                Dim logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statsreg_debug.log")
                System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - LoadVisitors start regionId=" & _regionId.ToString() & " DBState=" & If(DbManager.Connection Is Nothing, "null", DbManager.Connection.State.ToString()) & Environment.NewLine)
            Catch : End Try
            ' REGION is linked to UTILISATEUR by LIBELLE (string).
            ' Allow filtering by either region ID or region libelle: try both to be safe.
            Dim sql As String =
"SELECT U.IDUSER, U.NOMUSER || ' ' || U.PRENOMUSER AS FULLNAME " &
"FROM gsbAdmin.UTILISATEUR U " &
"JOIN gsbAdmin.VISITEUR V ON V.IDUSER = U.IDUSER " &
"JOIN gsbAdmin.REGION R ON R.LIBELLE = U.LIBELLE " &
"WHERE R.ID = ? OR R.LIBELLE = ? " &
"ORDER BY U.NOMUSER, U.PRENOMUSER"

            Using cmd As New OdbcCommand(sql, DbManager.Connection)
                ' param 1 = region id (int) - will be ignored if libelle match used
                cmd.Parameters.Add(New OdbcParameter With {
                    .OdbcType = OdbcType.Int,
                    .Value = _regionId
                })
                ' param 2 = region libelle (string) - in case caller provided libelle instead of id
                cmd.Parameters.Add(New OdbcParameter With {
                    .OdbcType = OdbcType.VarChar,
                    .Value = _regionId.ToString()
                })

                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        lstVisitors.Items.Add(
                            New KeyValuePair(Of Integer, String)(
                                CInt(rd("IDUSER")),
                                rd("FULLNAME").ToString()
                            )
                        )
                    End While
                End Using
                Try
                    Dim logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statsreg_debug.log")
                    System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - LoadVisitors finished, count=" & lstVisitors.Items.Count.ToString() & Environment.NewLine)
                Catch : End Try
            End Using
        Catch ex As Exception
            Try
                Dim logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statsreg_debug.log")
                System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - LoadVisitors error: " & ex.ToString() & Environment.NewLine)
            Catch : End Try
            MessageBox.Show("Erreur chargement visiteurs : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---------- Load Metrics ----------
    Private Sub LoadMetrics()
        flowMetrics.Controls.Clear()

        Try
            Try
                Dim logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statsreg_debug.log")
                System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - LoadMetrics start regionId=" & _regionId.ToString() & " selectedVisitor=" & _selectedVisitorId.ToString() & Environment.NewLine)
            Catch : End Try
            Dim userFilter As String = If(_selectedVisitorId > 0, " AND C.IDUSER = ?", "")

            ' All metrics filtered to the region via U.LIBELLE = R.LIBELLE and R.ID = ?
            AddMetric(
                "Nombre de visites",
                "SELECT COUNT(*) FROM gsbAdmin.CR C " &
                "JOIN gsbAdmin.UTILISATEUR U ON C.IDUSER = U.IDUSER " &
                "JOIN gsbAdmin.REGION R ON U.LIBELLE = R.LIBELLE " &
                "WHERE R.ID = ? AND C.DATEVISITE BETWEEN ? AND ?" & userFilter)

            AddMetric(
                "Échantillons distribués",
                "SELECT NVL(SUM(D.NOMBREECHANTILLONS),0) " &
                "FROM gsbAdmin.DISTRIBUTION D " &
                "JOIN gsbAdmin.CR C ON C.IDCR = D.IDCR " &
                "JOIN gsbAdmin.UTILISATEUR U ON C.IDUSER = U.IDUSER " &
                "JOIN gsbAdmin.REGION R ON U.LIBELLE = R.LIBELLE " &
                "WHERE R.ID = ? AND C.DATEVISITE BETWEEN ? AND ?" & userFilter)

            AddMetric(
                "Praticiens visités",
                "SELECT COUNT(DISTINCT C.IDPRAT) FROM gsbAdmin.CR C " &
                "JOIN gsbAdmin.UTILISATEUR U ON C.IDUSER = U.IDUSER " &
                "JOIN gsbAdmin.REGION R ON U.LIBELLE = R.LIBELLE " &
                "WHERE R.ID = ? AND C.DATEVISITE BETWEEN ? AND ?" & userFilter)

        Catch ex As Exception
            Try
                Dim logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statsreg_debug.log")
                System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("s") & " - LoadMetrics error: " & ex.ToString() & Environment.NewLine)
            Catch : End Try
            MessageBox.Show("Erreur statistiques : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        ResizeMetricCards(Nothing, EventArgs.Empty)
    End Sub

    ' ---------- Metric helper ----------
    Private Sub AddMetric(title As String, sql As String)
        Using cmd As New OdbcCommand(sql, DbManager.Connection)
            ' If the SQL expects a region id first (R.ID = ?), add it before dates
            If sql.Contains("R.ID = ?") Or sql.Contains("R.ID=?") Then
                cmd.Parameters.Add(New OdbcParameter With {
                    .OdbcType = OdbcType.Int,
                    .Value = _regionId
                })
            ElseIf sql.Contains("R.LIBELLE = ?") Or sql.Contains("R.LIBELLE=?") Then
                cmd.Parameters.Add(New OdbcParameter With {
                    .OdbcType = OdbcType.VarChar,
                    .Value = _regionId.ToString()
                })
            End If

            ' dates (toujours présents)
            cmd.Parameters.Add(New OdbcParameter With {
                .OdbcType = OdbcType.Date,
                .Value = dtStart.Value.Date
            })

            cmd.Parameters.Add(New OdbcParameter With {
                .OdbcType = OdbcType.Date,
                .Value = dtEnd.Value.Date
            })

            ' visitor filter param (optionnel, doit être ajouté last)
            If _selectedVisitorId > 0 Then
                cmd.Parameters.Add(New OdbcParameter With {
                    .OdbcType = OdbcType.Int,
                    .Value = _selectedVisitorId
                })
            End If

            Dim result = cmd.ExecuteScalar()
            Dim value As String = If(result IsNot Nothing AndAlso result IsNot DBNull.Value, result.ToString(), "0")

            flowMetrics.Controls.Add(CreateMetricCard(title, value))
        End Using
    End Sub

    ' ---------- Metric Card ----------
    Private Function CreateMetricCard(title As String, value As String) As Panel
        Dim p As New Panel With {
            .BorderStyle = BorderStyle.FixedSingle,
            .Padding = New Padding(10),
            .Margin = New Padding(5),
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink
        }

        Dim lblTitle As New Label With {
            .Text = title,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .AutoSize = True,
            .Dock = DockStyle.Top
        }

        Dim lblValue As New Label With {
            .Text = value,
            .Font = New Font("Segoe UI", 11),
            .AutoSize = True,
            .Dock = DockStyle.Top
        }
        ' mark for resize wrapping
        lblValue.Tag = "metricValue"

        ' Add in order: title then value (value below title)
        p.Controls.Add(lblValue)
        p.Controls.Add(lblTitle)
        Return p
    End Function

    ' ---------- Resize cards ----------
    Private Sub ResizeMetricCards(sender As Object, e As EventArgs)
        Dim targetWidth As Integer =
            flowMetrics.ClientSize.Width -
            flowMetrics.Padding.Horizontal -
            SystemInformation.VerticalScrollBarWidth - 5

        For Each ctrl As Control In flowMetrics.Controls
            ctrl.Width = targetWidth
            ' adjust inner value labels to wrap within the panel
            For Each child As Control In ctrl.Controls
                If child.Tag IsNot Nothing AndAlso child.Tag.ToString() = "metricValue" Then
                    child.MaximumSize = New Size(Math.Max(100, targetWidth - ctrl.Padding.Horizontal - 20), 0)
                End If
            Next
        Next
    End Sub

End Class
