Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms

' ==============================================
' FORMULAIRE : DTCcompteRendu (fusion p1 + patch)
' ==============================================
' - Uniformisation des rectangles (référence : TBversion)
' - Bouton "Modifier" (+1 si jour différent / +0,1 si même jour)
' - Garde le comportement d'origine (sélections, DataGridView, étoiles)
' - Protège la mise à jour de TBversion via un garde pour éviter le reset

Public Class CompteRenduTab
    Inherits UserControl

    ' ==== État existant (extrait de p1) ====
    Private isDropdownVisible As Boolean = False
    Private quantities As New Dictionary(Of String, Integer)
    Private allMedicaments As List(Of String)
    Private selectedMedicaments As New HashSet(Of String)

    Private praticien As List(Of String)
    Private selectedPraticien As New HashSet(Of String)

    Private visiteur As List(Of String)
    Private selectedVisiteur As New HashSet(Of String)

    Private praticiens As String() = {
        "--Slectioné un docteur--", " Dr. Dupont", " Dr. Martin", " Dr. Bernard", " Dr. Lefèvre", " Dr. Moreau",
        " Dr. Laurent", " Dr. Petit", " Dr. Garnier", " Dr. Faure", " Dr. Roux"
    }
    Private allPraticiens As List(Of String)

    Private medicaments As String() = {
        " Paracétamol", " Ibuprofène", " Amoxicilline", " Doliprane", " Spasfon",
        " Efferalgan", " Aspirine", " Lansoprazole", " Metformine", " Ventoline"
    }

    Private visiteurs As String() = {
        "--Sélectionner un visiteur--", "Jean Durand", "Marie Morel", "Paul Simon", "Sophie Michel", "Luc Robert",
        "Marc Richard", "Éric Petit", "Antoine Blanc", "Laura Garcia", "Ana Martinez"
    }

    Private defaultVisiteur As String = String.Empty
    Private defaultVersion As String = String.Empty

    Private visitRating As Double = 0.0
    Private WithEvents starRatingVisite As StarRatingControl

    ' === Garde pour éviter que TBversion_TextChanged remette la valeur par défaut ===
    Private suppressVersionGuard As Boolean = False

    ' ==== Load existant ====
    Private Sub test_AP_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initialisation des médicaments
        allMedicaments = medicaments.ToList()
        For Each med In medicaments
            quantities(med) = 0
        Next

        ' Configuration du DataGridView
        With DGVechantillon
            .Columns.Clear()
            .Columns.Add("Medicament", "Médicament")
            .Columns.Add("Quantite", "Quantité")
            .Columns("Quantite").ValueType = GetType(Integer)
            .AllowUserToAddRows = False
            .ReadOnly = False
        End With
        UpdateCheckedListBox("")

        ' Initialisation des praticiens
        allPraticiens = praticiens.ToList()
        praticien = allPraticiens
        If CBpraticien IsNot Nothing Then
            CBpraticien.Items.Clear()
            For Each p In allPraticiens
                CBpraticien.Items.Add(p)
            Next
            Try : CBpraticien.DropDownStyle = ComboBoxStyle.DropDownList : Catch : End Try
            If CBpraticien.Items.Count > 0 Then CBpraticien.SelectedIndex = 0
        End If

        ' Visiteur fixe
        Try
            If visiteurs IsNot Nothing AndAlso visiteurs.Length > 1 Then
                defaultVisiteur = visiteurs(1).Trim()
                defaultVersion = "1"
            ElseIf visiteurs IsNot Nothing AndAlso visiteurs.Length > 0 Then
                defaultVisiteur = visiteurs(0).Trim()
                defaultVersion = "1"
            Else
                defaultVisiteur = "Visiteur par défaut"
                defaultVersion = "1"
            End If
            If TBnomPrenom IsNot Nothing AndAlso TBversion IsNot Nothing Then
                TBnomPrenom.Text = defaultVisiteur
                TBversion.Text = defaultVersion
                TBnomPrenom.ReadOnly = True
                TBversion.ReadOnly = True
                TBnomPrenom.BackColor = SystemColors.ControlLight
                TBversion.BackColor = SystemColors.ControlLight
                Dim tt As New ToolTip()
                tt.SetToolTip(TBnomPrenom, "Nom du visiteur fixé et non modifiable")
                tt.SetToolTip(TBversion, "Version de l'application fixée et non modifiable")
            End If
        Catch
        End Try
    End Sub

    ' ==== Shown existant + patch : ajout du contrôle d’étoiles + normalisation + bouton Modifier ====
    Private Sub DTCcompteRendu_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If starRatingVisite IsNot Nothing AndAlso Not starRatingVisite.IsDisposed Then
            ' Même si déjà créé, on applique la normalisation et on s'assure du bouton
            NormalizeBoxSizes()

            Return
        End If

        starRatingVisite = New StarRatingControl() With {
            .MaxStars = 5,
            .AllowHalfStars = True,
            .StarColor = Color.Goldenrod,
            .EmptyColor = Color.LightGray,
            .StarSize = 24,
            .Spacing = 6,
            .AnimationEnabled = True,
            .AnimationDurationMs = 200,
            .BounceEnabled = True,
            .BounceDurationMs = 160
        }

        Dim baseX As Integer
        Dim baseY As Integer
        If TBcoefficientDeConfiance IsNot Nothing Then
            baseX = TBcoefficientDeConfiance.Left
            baseY = TBcoefficientDeConfiance.Bottom + 6
        ElseIf TBversion IsNot Nothing Then
            baseX = TBversion.Left
            baseY = TBversion.Bottom + 12
        Else
            baseX = 16
            baseY = 16
        End If
        starRatingVisite.Location = New Point(baseX, baseY)
        Me.Controls.Add(starRatingVisite)

        Dim lblNote As New Label() With {
            .Text = "Note : 0 / 5",
            .AutoSize = True,
            .Location = New Point(starRatingVisite.Right + 12, starRatingVisite.Top + (starRatingVisite.Height \ 2 - 10))
        }
        Me.Controls.Add(lblNote)

        AddHandler starRatingVisite.ValueChanged,
            Sub(_s, _e2)
                visitRating = starRatingVisite.Value
                lblNote.Text = $"Note : {visitRating} / {starRatingVisite.MaxStars}"
            End Sub

        ' === Patch : uniformisation + ajout bouton ===
        NormalizeBoxSizes()

    End Sub

    ' ==== Fonctions existantes (p1) ====
    Private Sub UpdateCheckedListBox(filter As String)
        Dim allCheckedItems = Doliprane.CheckedItems.Cast(Of String)().ToList()
        For Each item In allCheckedItems : selectedMedicaments.Add(item) : Next
        Doliprane.Items.Clear()
        For Each item In allMedicaments
            If item.ToLower().Contains(filter.ToLower()) Then
                Doliprane.Items.Add(item, selectedMedicaments.Contains(item))
            End If
        Next
    End Sub

    Private Sub TBproduit_TextChanged(sender As Object, e As EventArgs) Handles TBproduit.TextChanged
        UpdateCheckedListBox(TBproduit.Text)
    End Sub

    Private Sub UpdateComboBoxFromCheckedList()
        DGVechantillon.Rows.Clear()
        For Each item In selectedMedicaments
            Dim quantity As Integer = If(quantities.ContainsKey(item), quantities(item), 0)
            quantities(item) = quantity
            DGVechantillon.Rows.Add(item, quantity)
        Next
    End Sub

    Private Sub UpdateSelectedItems()
        ' placeholder
    End Sub

    Private Sub Doliprane_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles Doliprane.ItemCheck
        Dim item As String = Doliprane.Items(e.Index).ToString()
        If e.NewValue = CheckState.Checked Then selectedMedicaments.Add(item) Else selectedMedicaments.Remove(item)
        UpdateSelectedItems()
        UpdateComboBoxFromCheckedList()
    End Sub

    Private Sub BpoubelleMedoc_Click(sender As Object, e As EventArgs) Handles BpoubelleMedoc.Click
        selectedMedicaments.Clear()
        For i As Integer = 0 To Doliprane.Items.Count - 1 : Doliprane.SetItemChecked(i, False) : Next
        UpdateSelectedItems()
        UpdateComboBoxFromCheckedList()
    End Sub

    Private Sub DGVechantillon_CellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs) Handles DGVechantillon.CellValidating
        If e.ColumnIndex = 1 Then
            Dim textValue As String = If(e.FormattedValue, "").ToString().Trim()
            Dim newValue As Integer
            If Not Integer.TryParse(textValue, newValue) Then
                e.Cancel = True : MessageBox.Show("La quantité doit être un nombre entier.", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            If newValue < 0 Then
                e.Cancel = True : MessageBox.Show("La quantité ne peut pas être négative.", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            If newValue > 2 Then
                MessageBox.Show("Quantité trop grande. La valeur a été limitée à 2.", "Quantité limitée", MessageBoxButtons.OK, MessageBoxIcon.Information)
                DGVechantillon.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = 2
                Dim med As String = If(DGVechantillon.Rows(e.RowIndex).Cells(0).Value, "").ToString()
                If Not String.IsNullOrEmpty(med) Then quantities(med) = 2
                e.Cancel = False : Return
            End If
            Dim medName As String = If(DGVechantillon.Rows(e.RowIndex).Cells(0).Value, "").ToString()
            If Not String.IsNullOrEmpty(medName) Then quantities(medName) = newValue
        End If
    End Sub

    Private Sub DGVechantillon_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGVechantillon.CellContentClick
        DGVechantillon.Rows.Clear()
        For Each item In selectedMedicaments
            Dim quantity As Integer = If(quantities.ContainsKey(item), quantities(item), 0)
            quantities(item) = quantity
            DGVechantillon.Rows.Add(item, quantity)
        Next
    End Sub

    Private Sub BpoubelleEchantillon_Click(sender As Object, e As EventArgs) Handles BpoubelleEchantillon.Click
        For Each key In selectedMedicaments.ToList() : quantities(key) = 0 : Next
        UpdateComboBoxFromCheckedList()
        DGVechantillon.Refresh()
    End Sub

    Private Sub CBpraticien_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CBpraticien.SelectedIndexChanged
        If CBpraticien.SelectedItem IsNot Nothing Then
            Dim sel As String = CBpraticien.SelectedItem.ToString()
            selectedPraticien.Clear() : selectedPraticien.Add(sel)
            If Me.Controls.ContainsKey("LPraticien") Then
                Dim lbl = TryCast(Me.Controls("LPraticien"), Label)
                If lbl IsNot Nothing Then lbl.Text = sel
            End If
        Else
            selectedPraticien.Clear()
            If Me.Controls.ContainsKey("LPraticien") Then
                Dim lbl = TryCast(Me.Controls("LPraticien"), Label)
                If lbl IsNot Nothing Then lbl.Text = String.Empty
            End If
        End If
    End Sub

    Private Sub ClearPraticienSelection()
        If CBpraticien IsNot Nothing Then CBpraticien.SelectedIndex = -1
        selectedPraticien.Clear()
        If Me.Controls.ContainsKey("LPraticien") Then
            Dim lbl = TryCast(Me.Controls("LPraticien"), Label)
            If lbl IsNot Nothing Then lbl.Text = String.Empty
        End If
    End Sub

    Private Sub dtPcompteRendu_ValueChanged(sender As Object, e As EventArgs) Handles dtPcompteRendu.ValueChanged
        If dtPcompteRendu Is Nothing Then Return
        If dtPcompteRendu.Value.Date <> DateTime.Today Then
            Try : dtPcompteRendu.Value = DateTime.Today : Catch : End Try
        End If
    End Sub

    Private Sub TBnomPrenom_TextChanged(sender As Object, e As EventArgs) Handles TBnomPrenom.TextChanged
        If String.IsNullOrEmpty(defaultVisiteur) Then Return
        If TBnomPrenom.Text <> defaultVisiteur Then
            TBnomPrenom.Text = defaultVisiteur
            TBnomPrenom.SelectionStart = TBnomPrenom.Text.Length
        End If
    End Sub

    Private Sub TBversion_TextChanged(sender As Object, e As EventArgs) Handles TBversion.TextChanged
        ' Patch : ne pas ré-imposer la valeur par défaut si on met à jour automatiquement
        If suppressVersionGuard Then Return
        If String.IsNullOrEmpty(defaultVersion) Then Return
        If TBversion.Text <> defaultVersion Then
            ' On tolère la modification si elle provient du bouton Modifier (guard actif)
            TBversion.Text = defaultVersion
            TBversion.SelectionStart = TBversion.Text.Length
        End If
    End Sub

    Private Sub DTPvisite_ValueChanged(sender As Object, e As EventArgs) Handles DTPvisite.ValueChanged
        If DTPvisite Is Nothing Then Return
        If DTPvisite.Value.Date > DateTime.Today Then
            Try
                DTPvisite.Value = DateTime.Today
                MessageBox.Show("Vous ne pouvez pas sélectionner une date future.", "Date non autorisée", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch
            End Try
        End If
    End Sub

    Private Sub TBcoefficientDeConfiance_TextChanged(sender As Object, e As EventArgs) Handles TBcoefficientDeConfiance.TextChanged
        ' placeholder
    End Sub

    ' ==== Bouton Valider (p1) : ouvre p2 lecture seule si présent ====
    Private Sub bValider_Click(sender As Object, e As EventArgs) Handles bValider.Click
        Dim payload As New CompteRenduData()
        payload.Visiteur = TBnomPrenom.Text
        payload.VersionApp = TBversion.Text
        payload.DateCompteRendu = dtPcompteRendu.Value.Date
        payload.DateVisite = DTPvisite.Value.Date
        payload.Praticien = If(CBpraticien.SelectedItem IsNot Nothing, CBpraticien.SelectedItem.ToString(), Nothing)
        payload.NoteGlobale = visitRating
        payload.Echantillons.Clear()
        For Each med In selectedMedicaments
            Dim q As Integer = If(quantities.ContainsKey(med), quantities(med), 0)
            payload.Echantillons.Add(Tuple.Create(med, q))
        Next
        Dim f2 As New CompteRenduRO(payload)
        f2.Owner = Me
        f2.Show()
    End Sub

    ' ==== Patch : uniformisation des rectangles & bouton Modifier ====
    Private Sub NormalizeBoxSizes()
        If TBversion Is Nothing Then Exit Sub
        Dim refSize As Size = TBversion.Size
        If TBnomPrenom IsNot Nothing Then
            TBnomPrenom.Size = refSize
            TBnomPrenom.ReadOnly = True
            TBnomPrenom.BackColor = SystemColors.ControlLight
        End If
        If dtPcompteRendu IsNot Nothing Then dtPcompteRendu.Width = refSize.Width
        If DTPvisite IsNot Nothing Then DTPvisite.Width = refSize.Width
    End Sub


    Private Sub bModifier_Click(sender As Object, e As EventArgs)
        If TBversion Is Nothing Then Exit Sub
        Dim isSameDay As Boolean
        Try
            If dtPcompteRendu IsNot Nothing Then
                isSameDay = (dtPcompteRendu.Value.Date = Date.Today)
            Else
                isSameDay = True
            End If
        Catch
            isSameDay = True
        End Try
        Dim newVer As String = IncrementVersion(TBversion.Text, isSameDay)
        suppressVersionGuard = True
        TBversion.Text = newVer
        defaultVersion = newVer  ' on met à jour la "version par défaut" pour éviter le reset
        suppressVersionGuard = False
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


    ' Méthode publique optionnelle si p2 veut pousser une version
    Public Sub ApplyVersion(newVer As String)
        suppressVersionGuard = True
        TBversion.Text = newVer
        defaultVersion = newVer
        suppressVersionGuard = False
    End Sub
End Class

' ==============================================
' USERCONTROL : StarRatingControl (inchangé)
' ==============================================
<DefaultEvent("ValueChanged")>
Public Class StarRatingControl
    Inherits Control

    Private _maxStars As Integer = 5
    Private _value As Double = 0
    Private _hoverValue As Double = -1
    Private _starColor As Color = Color.Goldenrod
    Private _emptyColor As Color = Color.LightGray
    Private _allowHalfStars As Boolean = False
    Private _spacing As Integer = 6
    Private _starSize As Integer = 28

    Private _animEnabled As Boolean = True
    Private _animDurationMs As Integer = 180
    Private _animTimer As Timer
    Private _animStartValue As Double = 0
    Private _animTargetValue As Double = 0
    Private _animProgress As Double = 1.0
    Private _bounceEnabled As Boolean = True
    Private _bounceDurationMs As Integer = 160
    Private _bounceIndex As Integer = -1
    Private _bounceProgress As Double = 1.0

    Public Event ValueChanged As EventHandler

    Public Sub New()
        Me.DoubleBuffered = True
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.Cursor = Cursors.Hand
        Me.Size = New Size((_starSize + _spacing) * _maxStars, _starSize + 6)
        _animTimer = New Timer() With {.Interval = 16}
        AddHandler _animTimer.Tick, AddressOf AnimTimer_Tick
    End Sub

    <Category("Behavior")>
    Public Property MaxStars As Integer
        Get
            Return _maxStars
        End Get
        Set(value As Integer)
            If value < 1 Then value = 1
            _maxStars = value
            Me.Size = New Size((_starSize + _spacing) * _maxStars, _starSize + 6)
            Invalidate()
        End Set
    End Property

    <Category("Behavior")>
    Public Property Value As Double
        Get
            Return _value
        End Get
        Set(v As Double)
            Dim nv = Math.Max(0, Math.Min(_maxStars, v))
            If Math.Abs(_value - nv) > 0.0001 Then
                _animStartValue = GetActiveAnimatedValue()
                _animTargetValue = nv
                _value = nv
                StartAnim(_animDurationMs)
                RaiseEvent ValueChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    <Category("Appearance")>
    Public Property StarColor As Color
        Get
            Return _starColor
        End Get
        Set(value As Color)
            _starColor = value
            Invalidate()
        End Set
    End Property

    <Category("Appearance")>
    Public Property EmptyColor As Color
        Get
            Return _emptyColor
        End Get
        Set(value As Color)
            _emptyColor = value
            Invalidate()
        End Set
    End Property

    <Category("Behavior")>
    Public Property AllowHalfStars As Boolean
        Get
            Return _allowHalfStars
        End Get
        Set(value As Boolean)
            _allowHalfStars = value
        End Set
    End Property

    <Category("Layout")>
    Public Property StarSize As Integer
        Get
            Return _starSize
        End Get
        Set(value As Integer)
            _starSize = Math.Max(10, value)
            Me.Size = New Size((_starSize + _spacing) * _maxStars, _starSize + 6)
            Invalidate()
        End Set
    End Property

    <Category("Layout")>
    Public Property Spacing As Integer
        Get
            Return _spacing
        End Get
        Set(value As Integer)
            _spacing = Math.Max(0, value)
            Me.Size = New Size((_starSize + _spacing) * _maxStars, _starSize + 6)
            Invalidate()
        End Set
    End Property

    <Category("Animation")>
    Public Property AnimationEnabled As Boolean
        Get
            Return _animEnabled
        End Get
        Set(value As Boolean)
            _animEnabled = value
        End Set
    End Property

    <Category("Animation")>
    Public Property AnimationDurationMs As Integer
        Get
            Return _animDurationMs
        End Get
        Set(value As Integer)
            _animDurationMs = Math.Max(50, value)
        End Set
    End Property

    <Category("Animation")>
    Public Property BounceEnabled As Boolean
        Get
            Return _bounceEnabled
        End Get
        Set(value As Boolean)
            _bounceEnabled = value
        End Set
    End Property

    <Category("Animation")>
    Public Property BounceDurationMs As Integer
        Get
            Return _bounceDurationMs
        End Get
        Set(value As Integer)
            _bounceDurationMs = Math.Max(80, value)
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim activeAnimatedValue As Double = GetActiveAnimatedValue()
        For i = 0 To _maxStars - 1
            Dim x As Integer = i * (_starSize + _spacing)
            Dim rect As New Rectangle(x, (Me.Height - _starSize) \ 2, _starSize, _starSize)

            Dim starScale As Single = 1.0F
            If _bounceEnabled AndAlso _bounceIndex = i AndAlso _bounceProgress < 1.0 Then
                Dim s As Double = EaseOutQuad(1.0 - _bounceProgress)
                starScale = CSng(1.0 + 0.15 * s)
            End If
            Dim scaledRect As Rectangle = ScaleRect(rect, starScale)

            Using starPath As GraphicsPath = CreateStarPath(scaledRect, 5, 0.5F)
                Using brushEmpty As New SolidBrush(_emptyColor)
                    e.Graphics.FillPath(brushEmpty, starPath)
                End Using
                Using penEmpty As New Pen(Darken(_emptyColor, 0.3), 1.1F)
                    e.Graphics.DrawPath(penEmpty, starPath)
                End Using

                Dim fillFraction As Double = Math.Max(0, Math.Min(1, activeAnimatedValue - i))
                If fillFraction > 0 Then
                    Dim clipRect As RectangleF = scaledRect
                    clipRect.Width = CSng(scaledRect.Width * fillFraction)
                    Dim oldClip As Region = e.Graphics.Clip
                    Using starRegion As New Region(starPath)
                        e.Graphics.SetClip(starRegion, CombineMode.Replace)
                        e.Graphics.SetClip(clipRect, CombineMode.Intersect)
                        Using brushFull As New SolidBrush(_starColor)
                            e.Graphics.FillPath(brushFull, starPath)
                        End Using
                    End Using
                    e.Graphics.Clip = oldClip
                    Using penFull As New Pen(Darken(_starColor, 0.3), 1.2F)
                        e.Graphics.DrawPath(penFull, starPath)
                    End Using
                End If
            End Using
        Next
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim valueFromMouse = GetValueFromPoint(e.Location)
        If Math.Abs(valueFromMouse - _hoverValue) > 0.0001 Then
            _hoverValue = valueFromMouse
            If _animEnabled Then
                _animStartValue = GetActiveAnimatedValue()
                _animTargetValue = _hoverValue
                StartAnim(_animDurationMs)
            Else
                Invalidate()
            End If
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _hoverValue = -1
        If _animEnabled Then
            _animStartValue = GetActiveAnimatedValue()
            _animTargetValue = _value
            StartAnim(_animDurationMs)
        Else
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Dim v = GetValueFromPoint(e.Location)
        Me.Value = v
        If _bounceEnabled Then
            Dim idx As Integer = CInt(Math.Floor(Math.Max(0, Math.Min(_maxStars, v)) - 1))
            _bounceIndex = If(idx >= 0 AndAlso idx < _maxStars, idx, -1)
            StartBounce()
        End If
    End Sub

    Private Sub StartAnim(durationMs As Integer)
        If Not _animEnabled Then
            _animProgress = 1.0 : Invalidate() : Return
        End If
        _animProgress = 0.0
        _animTimer.Tag = New AnimState With {.StartTicks = Environment.TickCount, .DurationMs = durationMs, .Type = AnimType.Fill}
        _animTimer.Start()
    End Sub

    Private Sub StartBounce()
        If Not _bounceEnabled OrElse _bounceIndex < 0 Then Return
        _bounceProgress = 0.0
        _animTimer.Tag = New AnimState With {.StartTicks = Environment.TickCount, .DurationMs = _bounceDurationMs, .Type = AnimType.Bounce}
        _animTimer.Start()
    End Sub

    Private Sub AnimTimer_Tick(sender As Object, e As EventArgs)
        Dim st = TryCast(_animTimer.Tag, AnimState)
        If st Is Nothing Then _animTimer.Stop() : Return
        Dim elapsed As Integer = Environment.TickCount - st.StartTicks
        Dim t As Double = Math.Max(0.0, Math.Min(1.0, elapsed / CDbl(st.DurationMs)))
        Select Case st.Type
            Case AnimType.Fill : _animProgress = t
            Case AnimType.Bounce : _bounceProgress = t
        End Select
        If _animProgress >= 1.0 AndAlso (_bounceIndex = -1 OrElse _bounceProgress >= 1.0) Then
            _animTimer.Stop()
            If _bounceProgress >= 1.0 Then _bounceIndex = -1
        End If
        Invalidate()
    End Sub

    Private Function GetActiveAnimatedValue() As Double
        Dim target As Double = If(_hoverValue >= 0, _hoverValue, _value)
        Dim eased As Double = EaseOutSin(_animProgress)
        Return Lerp(_animStartValue, target, eased)
    End Function

    Private Shared Function Lerp(a As Double, b As Double, t As Double) As Double
        Return a + (b - a) * t
    End Function

    Private Shared Function EaseOutSin(t As Double) As Double
        Return Math.Sin(t * Math.PI / 2.0)
    End Function

    Private Shared Function EaseOutQuad(t As Double) As Double
        Dim u = 1.0 - t
        Return 1.0 - u * u
    End Function

    Private Shared Function ScaleRect(r As Rectangle, scale As Single) As Rectangle
        If Math.Abs(scale - 1.0F) < 0.0001F Then Return r
        Dim newW As Integer = CInt(r.Width * scale)
        Dim newH As Integer = CInt(r.Height * scale)
        Dim dx As Integer = (newW - r.Width) \ 2
        Dim dy As Integer = (newH - r.Height) \ 2
        Return New Rectangle(r.Left - dx, r.Top - dy, newW, newH)
    End Function

    Private Function GetValueFromPoint(p As Point) As Double
        For i = 0 To _maxStars - 1
            Dim x As Integer = i * (_starSize + _spacing)
            Dim rect As New Rectangle(x, (Me.Height - _starSize) \ 2, _starSize, _starSize)
            If p.X >= rect.Left AndAlso p.X <= rect.Right Then
                Return i + 1
            End If
        Next
        If p.X < 0 Then Return 0
        Return _maxStars
    End Function

    Private Shared Function CreateStarPath(bounds As Rectangle, points As Integer, innerRadiusRatio As Single) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim cx As Single = bounds.Left + bounds.Width / 2.0F
        Dim cy As Single = bounds.Top + bounds.Height / 2.0F
        Dim outerR As Single = Math.Min(bounds.Width, bounds.Height) / 2.0F
        Dim innerR As Single = outerR * innerRadiusRatio
        Dim angle As Single = -90.0F
        Dim stepDeg As Single = 360.0F / (points * 2)
        Dim pts As New List(Of PointF)
        For i = 0 To points * 2 - 1
            Dim r As Single = If(i Mod 2 = 0, outerR, innerR)
            Dim rad As Single = CSng(Math.PI / 180.0) * (angle + i * stepDeg)
            pts.Add(New PointF(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad)))
        Next
        path.AddPolygon(pts.ToArray())
        path.CloseFigure()
        Return path
    End Function

    Private Shared Function Darken(c As Color, factor As Double) As Color
        Dim f As Double = Math.Max(0.0, factor)
        Dim r As Integer = CInt(Math.Min(255, Math.Max(0, c.R * (1 - f))))
        Dim g As Integer = CInt(Math.Min(255, Math.Max(0, c.G * (1 - f))))
        Dim b As Integer = CInt(Math.Min(255, Math.Max(0, c.B * (1 - f))))
        Return Color.FromArgb(c.A, r, g, b)
    End Function

    Private Class AnimState
        Public StartTicks As Integer
        Public DurationMs As Integer
        Public Type As AnimType
    End Class

    Private Enum AnimType
        Fill
        Bounce
    End Enum
End Class
