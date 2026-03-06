Imports System
Imports System.Data.Odbc
Imports System.Drawing
Imports System.Windows.Forms

Public Class StatsRegTab
    Inherits UserControl

    ' ---------- Champs ----------
    Private ReadOnly _regionId As Integer
    Private _selectedVisitorId As Integer = -1
    Private ReadOnly connString As String = "DSN=ORA14;Uid=GSBApp;Pwd=Iroise29;"

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
    Public Sub New(regionId As Integer)
        _regionId = regionId
        Me.Dock = DockStyle.Fill
        InitializeUI()
        LoadVisitors()
        LoadMetrics()
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
            Using conn As New OdbcConnection(connString)
                conn.Open()

                Dim sql As String =
    "SELECT U.IDUSER, U.NOMUSER || ' ' || U.PRENOMUSER AS FULLNAME " &
    "FROM gsbAdmin.UTILISATEUR U " &
    "JOIN gsbAdmin.VISITEUR V ON V.IDUSER = U.IDUSER " &
    "JOIN gsbAdmin.REGION R ON R.LIBELLE = U.LIBELLE " &
    "WHERE R.ID = ? " &
    "ORDER BY U.NOMUSER, U.PRENOMUSER"


                Using cmd As New OdbcCommand(sql, conn)
                    cmd.Parameters.Add(New OdbcParameter With {
    .OdbcType = OdbcType.Int,
    .Value = _regionId
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
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur chargement visiteurs : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---------- Load Metrics ----------
    Private Sub LoadMetrics()
        flowMetrics.Controls.Clear()

        Try
            Using conn As New OdbcConnection(connString)
                conn.Open()

                Dim userFilter As String = If(_selectedVisitorId > 0, " AND C.IDUSER = ? ", "")

                AddMetric(conn,
                    "Nombre de visites",
                    "SELECT COUNT(*) FROM gsbAdmin.CR C " &
                    "WHERE C.DATEVISITE BETWEEN ? AND ?" & userFilter)

                AddMetric(conn,
                    "Échantillons distribués",
                    "SELECT NVL(SUM(D.NOMBREECHANTILLONS),0) " &
                    "FROM gsbAdmin.DISTRIBUTION D " &
                    "JOIN gsbAdmin.CR C ON C.IDCR = D.IDCR " &
                    "WHERE C.DATEVISITE BETWEEN ? AND ?" & userFilter)

                AddMetric(conn,
                    "Praticiens visités",
                    "SELECT COUNT(DISTINCT C.IDPRAT) FROM gsbAdmin.CR C " &
                    "WHERE C.DATEVISITE BETWEEN ? AND ?" & userFilter)

            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur statistiques : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        ResizeMetricCards(Nothing, EventArgs.Empty)
    End Sub

    ' ---------- Metric helper ----------
    Private Sub AddMetric(conn As OdbcConnection, title As String, sql As String)
        Using cmd As New OdbcCommand(sql, conn)
            cmd.Parameters.Add(New OdbcParameter With {
    .OdbcType = OdbcType.Date,
    .Value = dtStart.Value.Date
})

            cmd.Parameters.Add(New OdbcParameter With {
    .OdbcType = OdbcType.Date,
    .Value = dtEnd.Value.Date
})

            If _selectedVisitorId > 0 Then
                cmd.Parameters.Add(New OdbcParameter With {
        .OdbcType = OdbcType.Int,
        .Value = _selectedVisitorId
    })
            End If


            Dim result = cmd.ExecuteScalar()
            Dim value As String =
                If(result IsNot Nothing AndAlso result IsNot DBNull.Value,
                   result.ToString(), "0")

            flowMetrics.Controls.Add(CreateMetricCard(title, value))
        End Using
    End Sub

    ' ---------- Metric Card ----------
    Private Function CreateMetricCard(title As String, value As String) As Panel
        Dim p As New Panel With {
            .BorderStyle = BorderStyle.FixedSingle,
            .Padding = New Padding(10),
            .Margin = New Padding(5),
            .Height = 60
        }

        Dim lblTitle As New Label With {
            .Text = title,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Dock = DockStyle.Top
        }

        Dim lblValue As New Label With {
            .Text = value,
            .Font = New Font("Segoe UI", 11),
            .Dock = DockStyle.Fill
        }

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
        Next
    End Sub

End Class
