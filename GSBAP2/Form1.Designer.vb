<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.LabelName = New System.Windows.Forms.Label()
        Me.txtPrenom = New System.Windows.Forms.TextBox()
        Me.txtNom = New System.Windows.Forms.TextBox()
        Me.LabelTOOp = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.DateTimePicker1 = New System.Windows.Forms.DateTimePicker()
        Me.cmbDisciplines = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.rAbandon = New System.Windows.Forms.RadioButton()
        Me.btnValider = New System.Windows.Forms.Button()
        Me.rArgent = New System.Windows.Forms.RadioButton()
        Me.rBronze = New System.Windows.Forms.RadioButton()
        Me.rOr = New System.Windows.Forms.RadioButton()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.LabelName)
        Me.GroupBox1.Controls.Add(Me.txtPrenom)
        Me.GroupBox1.Controls.Add(Me.txtNom)
        Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.ForeColor = System.Drawing.Color.Green
        Me.GroupBox1.Location = New System.Drawing.Point(12, 46)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(400, 142)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Etat civil"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label1.Location = New System.Drawing.Point(6, 92)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(64, 17)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Prénoms"
        '
        'LabelName
        '
        Me.LabelName.AutoSize = True
        Me.LabelName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.LabelName.Location = New System.Drawing.Point(6, 41)
        Me.LabelName.Name = "LabelName"
        Me.LabelName.Size = New System.Drawing.Size(37, 17)
        Me.LabelName.TabIndex = 2
        Me.LabelName.Text = "Nom"
        '
        'txtPrenom
        '
        Me.txtPrenom.Location = New System.Drawing.Point(91, 86)
        Me.txtPrenom.Name = "txtPrenom"
        Me.txtPrenom.Size = New System.Drawing.Size(290, 23)
        Me.txtPrenom.TabIndex = 1
        '
        'txtNom
        '
        Me.txtNom.Location = New System.Drawing.Point(91, 35)
        Me.txtNom.Name = "txtNom"
        Me.txtNom.Size = New System.Drawing.Size(290, 23)
        Me.txtNom.TabIndex = 0
        '
        'LabelTOOp
        '
        Me.LabelTOOp.AutoSize = True
        Me.LabelTOOp.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelTOOp.ForeColor = System.Drawing.Color.Green
        Me.LabelTOOp.Location = New System.Drawing.Point(169, 9)
        Me.LabelTOOp.Name = "LabelTOOp"
        Me.LabelTOOp.Size = New System.Drawing.Size(86, 24)
        Me.LabelTOOp.TabIndex = 0
        Me.LabelTOOp.Text = "JO 2020"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.DateTimePicker1)
        Me.GroupBox2.Controls.Add(Me.cmbDisciplines)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox2.ForeColor = System.Drawing.Color.Green
        Me.GroupBox2.Location = New System.Drawing.Point(12, 183)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(400, 142)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Naissance"
        '
        'DateTimePicker1
        '
        Me.DateTimePicker1.Location = New System.Drawing.Point(91, 36)
        Me.DateTimePicker1.Name = "DateTimePicker1"
        Me.DateTimePicker1.Size = New System.Drawing.Size(290, 23)
        Me.DateTimePicker1.TabIndex = 5
        '
        'cmbDisciplines
        '
        Me.cmbDisciplines.FormattingEnabled = True
        Me.cmbDisciplines.Items.AddRange(New Object() {"Badminton", "Basketball", "Natation"})
        Me.cmbDisciplines.Location = New System.Drawing.Point(91, 85)
        Me.cmbDisciplines.Name = "cmbDisciplines"
        Me.cmbDisciplines.Size = New System.Drawing.Size(290, 24)
        Me.cmbDisciplines.Sorted = True
        Me.cmbDisciplines.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.ForeColor = System.Drawing.SystemColors.ButtonShadow
        Me.Label2.Location = New System.Drawing.Point(6, 92)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(68, 17)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Discipline"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label3.Location = New System.Drawing.Point(6, 41)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(38, 17)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Date"
        '
        'rAbandon
        '
        Me.rAbandon.AutoSize = True
        Me.rAbandon.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.rAbandon.Location = New System.Drawing.Point(32, 369)
        Me.rAbandon.Name = "rAbandon"
        Me.rAbandon.Size = New System.Drawing.Size(68, 17)
        Me.rAbandon.TabIndex = 5
        Me.rAbandon.TabStop = True
        Me.rAbandon.Text = "Abandon"
        Me.rAbandon.UseVisualStyleBackColor = True
        '
        'btnValider
        '
        Me.btnValider.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnValider.FlatAppearance.BorderColor = System.Drawing.Color.Green
        Me.btnValider.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.btnValider.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnValider.ForeColor = System.Drawing.Color.Black
        Me.btnValider.Location = New System.Drawing.Point(318, 415)
        Me.btnValider.Name = "btnValider"
        Me.btnValider.Size = New System.Drawing.Size(75, 23)
        Me.btnValider.TabIndex = 6
        Me.btnValider.Text = "Valider"
        Me.btnValider.UseVisualStyleBackColor = False
        '
        'rArgent
        '
        Me.rArgent.AutoSize = True
        Me.rArgent.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.rArgent.Location = New System.Drawing.Point(153, 392)
        Me.rArgent.Name = "rArgent"
        Me.rArgent.Size = New System.Drawing.Size(56, 17)
        Me.rArgent.TabIndex = 7
        Me.rArgent.TabStop = True
        Me.rArgent.Text = "Argent"
        Me.rArgent.UseVisualStyleBackColor = True
        '
        'rBronze
        '
        Me.rBronze.AutoSize = True
        Me.rBronze.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.rBronze.Location = New System.Drawing.Point(151, 415)
        Me.rBronze.Name = "rBronze"
        Me.rBronze.Size = New System.Drawing.Size(58, 17)
        Me.rBronze.TabIndex = 8
        Me.rBronze.TabStop = True
        Me.rBronze.Text = "Bronze"
        Me.rBronze.UseVisualStyleBackColor = True
        '
        'rOr
        '
        Me.rOr.AutoSize = True
        Me.rOr.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.rOr.Location = New System.Drawing.Point(173, 369)
        Me.rOr.Name = "rOr"
        Me.rOr.Size = New System.Drawing.Size(36, 17)
        Me.rOr.TabIndex = 9
        Me.rOr.TabStop = True
        Me.rOr.Text = "Or"
        Me.rOr.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(142, 353)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(54, 13)
        Me.Label5.TabIndex = 12
        Me.Label5.Text = "Médaille"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(424, 450)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.rOr)
        Me.Controls.Add(Me.rBronze)
        Me.Controls.Add(Me.rArgent)
        Me.Controls.Add(Me.btnValider)
        Me.Controls.Add(Me.rAbandon)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.LabelTOOp)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents LabelTOOp As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents LabelName As Label
    Friend WithEvents txtPrenom As TextBox
    Friend WithEvents txtNom As TextBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents cmbDisciplines As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents DateTimePicker1 As DateTimePicker
    Friend WithEvents rAbandon As RadioButton
    Friend WithEvents btnValider As Button
    Friend WithEvents rArgent As RadioButton
    Friend WithEvents rBronze As RadioButton
    Friend WithEvents rOr As RadioButton
    Friend WithEvents Label5 As Label
End Class
