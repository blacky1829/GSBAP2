Imports System.Data
Imports System.Data.Odbc
Imports System.Windows.Forms

Public NotInheritable Class DbManager
    Private Sub New()
    End Sub

    Private Shared ReadOnly _connString As String = "DSN=ORA14;Uid=GSBAdmin;Pwd=Iroise29;"
    Private Shared _connection As OdbcConnection

    Public Shared ReadOnly Property Connection As OdbcConnection
        Get
            Return _connection
        End Get
    End Property

    Public Shared Sub Initialize()
        If _connection Is Nothing Then
            _connection = New OdbcConnection(_connString)
        End If

        If _connection.State <> ConnectionState.Open Then
            _connection.Open()
        End If

        RemoveHandler Application.ApplicationExit, AddressOf OnApplicationExit
        AddHandler Application.ApplicationExit, AddressOf OnApplicationExit
    End Sub

    Public Shared Sub Close()
        Try
            If _connection IsNot Nothing Then
                If _connection.State <> ConnectionState.Closed Then
                    _connection.Close()
                End If
                _connection.Dispose()
                _connection = Nothing
            End If
        Catch
        End Try
    End Sub

    Private Shared Sub OnApplicationExit(sender As Object, e As EventArgs)
        Close()
    End Sub
End Class
