
' ===============================
' FICHIER : compterendu.txt
' (Form CompteRenduRO)
' ===============================
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Linq

' ---------- DTO ----------
Public Class CompteRenduData
    Public Property Visiteur As String
    Public Property VersionApp As String
    Public Property DateCompteRendu As Date?
    Public Property DateVisite As Date?
    Public Property Praticien As String
    Public Property NoteGlobale As Double
    Public Property Echantillons As New List(Of Tuple(Of String, Integer))
    Public Property BilanRtf As String ' <- déjà ajouté précédemment
End Class

' ---------- FORM READ-ONLY ----------
Public Class CompteRenduRO
    Inherits Form
    Private ReadOnly _data As CompteRenduData

    ' Contrôles
    Private roTitre As Label
    Private roVisiteur As TextBox
    Private roVersion As TextBox
    Private roPraticien As TextBox
    Private roDates As TextBox
    Private roDatedv As TextBox
    Private roGrid As DataGridView
    Private roStars As StarRatingControl
    Private roLblNote As Label
    Private roBilan As RichTextBox
    Private btnModifier As Button

    ' Marges & espacement pour le layout 2 colonnes
    Private ReadOnly MARGIN_LEFT As Integer = 16
    Private ReadOnly MARGIN_RIGHT As Integer = 16
    Private ReadOnly MARGIN_TOP As Integer = 140
    Private ReadOnly COL_GAP As Integer = 16
    Private ReadOnly BLOCK_HEIGHT As Integer = 260  ' même hauteur pour Grid et Bilan

    ' ===== Constructeurs =====
    Public Sub New(data As CompteRenduData)
        _data = data
        BuildUI()
        AddHandler Me.Load, AddressOf CompteRenduRO_Load
        AddHandler Me.Resize, AddressOf CompteRenduRO_Resize ' responsive
    End Sub

    Public Sub New()
        BuildUI()
        AddHandler Me.Load, AddressOf CompteRenduRO_Load
        AddHandler Me.Resize, AddressOf CompteRenduRO_Resize ' responsive
    End Sub

    ' ===== UI lecture seule =====
    Private Sub BuildUI()
        Me.Text = "Compte-rendu (lecture seule)"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(820, 560)
        Me.MinimizeBox = False
        Me.MaximizeBox = False

        roTitre = New Label() With {
            .Text = "Récapitulatif de la visite",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .AutoSize = True,
            .Location = New Point(16, 16)
        }

        ' Champ de référence pour uniformiser
        roVersion = MakeRoTextBox("Version :", New Point(360, 60), New Size(100, 22))
        Dim refSize As Size = roVersion.Size
        roVisiteur = MakeRoTextBox("Visiteur :", New Point(16, 60), refSize)
        roPraticien = MakeRoTextBox("Praticien :", New Point(16, 100), refSize)
        roDatedv = MakeRoTextBox("Date Visite :", New Point(360, 100), refSize)
        roDates = MakeRoTextBox("Date CR :", New Point(500, 423), refSize)

        ' Grille (lecture seule)
        roGrid = New DataGridView() With {
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }
        roGrid.Columns.Add("Medicament", "Médicament")
        roGrid.Columns.Add("Quantite", "Quantité")

        ' RichTextBox Bilan (lecture seule)
        roBilan = New RichTextBox() With {
            .ReadOnly = True,
            .BorderStyle = BorderStyle.FixedSingle,
            .DetectUrls = True,
            .ScrollBars = RichTextBoxScrollBars.Vertical
        }
        roBilan.BackColor = SystemColors.ControlLight

        ' Étoiles : non interactives
        roStars = New StarRatingControl() With {
            .MaxStars = 5,
            .AllowHalfStars = False,
            .StarColor = Color.Goldenrod,
            .EmptyColor = Color.LightGray,
            .StarSize = 20,
            .Spacing = 6
        }
        roStars.AnimationEnabled = False
        roStars.BounceEnabled = False
        roStars.Enabled = False
        roStars.TabStop = False
        roStars.Cursor = Cursors.Default

        roLblNote = New Label() With {
            .Text = "Note : 0 / 5",
            .AutoSize = True
        }

        btnModifier = New Button() With {
            .Text = "Modifier",
            .AutoSize = True
        }
        AddHandler btnModifier.Click, AddressOf BtnModifier_Click

        ' Ajout des contrôles
        Me.Controls.AddRange(New Control() {
            roTitre, roVisiteur, roVersion, roPraticien, roDates,
            roGrid, roBilan, roStars, roLblNote, btnModifier
        })

        ' Applique le layout 2 colonnes
        ApplyTwoColumnsLayout()
    End Sub

    Private Function MakeRoTextBox(labelText As String, labelLoc As Point, tbSize As Size) As TextBox
        Dim lbl = New Label() With {.Text = labelText, .AutoSize = True, .Location = labelLoc}
        Dim tb = New TextBox() With {
            .Location = New Point(labelLoc.X + 110, labelLoc.Y - 2),
            .Size = tbSize,
            .ReadOnly = True,
            .BackColor = SystemColors.ControlLight,
            .BorderStyle = BorderStyle.FixedSingle
        }
        Me.Controls.Add(lbl)
        Me.Controls.Add(tb)
        Return tb
    End Function

    ' ======== Layout 2 colonnes (Grid à gauche, Bilan à droite) ========
    Private Sub ApplyTwoColumnsLayout()
        ' largeur disponible intérieure
        Dim innerWidth As Integer = Me.ClientSize.Width - MARGIN_LEFT - MARGIN_RIGHT
        If innerWidth < 200 Then innerWidth = 200

        ' chaque colonne = moitié de la largeur (moins l'espace inter-colonnes)
        Dim colWidth As Integer = (innerWidth - COL_GAP) \ 2

        ' Positions
        Dim gridX As Integer = MARGIN_LEFT
        Dim gridY As Integer = MARGIN_TOP

        Dim bilanX As Integer = gridX + colWidth + COL_GAP
        Dim bilanY As Integer = gridY

        ' Appliquer tailles/positions identiques en hauteur
        roGrid.Location = New Point(gridX, gridY)
        roGrid.Size = New Size(colWidth, BLOCK_HEIGHT)

        roBilan.Location = New Point(bilanX, bilanY)
        roBilan.Size = New Size(colWidth, BLOCK_HEIGHT)

        ' Placer étoiles et libellé de note sous les deux colonnes (aligné à gauche)
        Dim belowY As Integer = gridY + BLOCK_HEIGHT + 16
        roStars.Location = New Point(gridX, belowY)
        roLblNote.Location = New Point(roStars.Left + roStars.Width + 12, roStars.Top + 2)

        ' Bouton Modifier à droite du libellé
        btnModifier.Location = New Point(roLblNote.Left + 160, roLblNote.Top - 4)
    End Sub

    Private Sub CompteRenduRO_Resize(sender As Object, e As EventArgs)
        ' recalculer à chaque redimensionnement
        ApplyTwoColumnsLayout()
    End Sub

    ' ===== Remplissage =====
    Private Sub CompteRenduRO_Load(sender As Object, e As EventArgs)
        If _data Is Nothing Then Exit Sub

        roVisiteur.Text = _data.Visiteur
        roVersion.Text = _data.VersionApp
        roPraticien.Text = If(String.IsNullOrWhiteSpace(_data.Praticien), "(non renseigné)", _data.Praticien)
        Dim dCR As String = If(_data.DateCompteRendu.HasValue, _data.DateCompteRendu.Value.ToString("dd/MM/yyyy"), "-")
        Dim dV As String = If(_data.DateVisite.HasValue, _data.DateVisite.Value.ToString("dd/MM/yyyy"), "-")
        roDatedv.Text = dV
        roDates.Text = dCR

        roGrid.Rows.Clear()
        For Each t In _data.Echantillons
            roGrid.Rows.Add(t.Item1, t.Item2)
        Next

        roStars.Value = Math.Max(0, Math.Min(roStars.MaxStars, _data.NoteGlobale))
        roLblNote.Text = $"Note : {roStars.Value} / {roStars.MaxStars}"

        ' Charger le RTF du bilan
        If Not String.IsNullOrWhiteSpace(_data.BilanRtf) Then
            Try
                roBilan.Rtf = _data.BilanRtf
            Catch
                roBilan.Text = _data.BilanRtf
            End Try
        Else
            roBilan.Text = "Aucun bilan transmis."
        End If

        ' S'assurer du layout au chargement
        ApplyTwoColumnsLayout()
    End Sub

    ' ===== Bouton "Modifier" =====
    Private Sub BtnModifier_Click(sender As Object, e As EventArgs)
        Dim isSameDay As Boolean =
            (_data IsNot Nothing AndAlso _data.DateCompteRendu.HasValue AndAlso
             _data.DateCompteRendu.Value.Date = Date.Today)

        Dim newVersion As String = IncrementVersion(_data.VersionApp, isSameDay)
        _data.VersionApp = newVersion
        roVersion.Text = newVersion

        Dim main = TryCast(Me.Owner, DTCcompteRendu)
        If main IsNot Nothing Then
            main.ApplyVersion(newVersion)
            main.Activate()
            Me.Close()
        Else
            Me.Close()
        End If
    End Sub



    Private Function IncrementVersion(current As String, isSameDay As Boolean) As String
        ' isSameDay est volontairement ignoré
        Dim v As Integer
        If Not Integer.TryParse(current, v) Then
            v = 1 ' valeur par défaut si non parsable (ex. "1.2" -> 1)
        End If
        v += 1
        Return v.ToString() ' "1" -> "2"
    End Function


End Class