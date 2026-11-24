namespace SampleApplication
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            configurationCheckBox1 = new Coding4Coffee.WinForms.Configuration.Controls.ConfigurationCheckBox();
            configurationComboBox1 = new Coding4Coffee.WinForms.Configuration.Controls.ConfigurationComboBox();
            configurationRadioButtonGroup1 = new Coding4Coffee.WinForms.Configuration.Controls.ConfigurationRadioButtonGroup();
            configurationDateTimePicker1 = new Coding4Coffee.WinForms.Configuration.Controls.ConfigurationDateTimePicker();
            configurationTrackBar1 = new Coding4Coffee.WinForms.Configuration.Controls.ConfigurationTrackBar();
            ((System.ComponentModel.ISupportInitialize)configurationCheckBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)configurationComboBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)configurationRadioButtonGroup1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)configurationDateTimePicker1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)configurationTrackBar1).BeginInit();
            SuspendLayout();
            // 
            // configurationCheckBox1
            // 
            configurationCheckBox1.AutoSize = true;
            configurationCheckBox1.ConfigKey = null;
            configurationCheckBox1.Location = new Point(206, 165);
            configurationCheckBox1.Name = "configurationCheckBox1";
            configurationCheckBox1.Size = new Size(156, 19);
            configurationCheckBox1.TabIndex = 0;
            configurationCheckBox1.Text = "configurationCheckBox1";
            configurationCheckBox1.TextResourceKey = null;
            configurationCheckBox1.UseVisualStyleBackColor = true;
            configurationCheckBox1.ValueMode = Coding4Coffee.WinForms.Configuration.Controls.ConfigurationCheckBoxValueMode.Boolean;
            // 
            // configurationComboBox1
            // 
            configurationComboBox1.ConfigKey = "CultureName";
            configurationComboBox1.DisplayMember = "DisplayName";
            configurationComboBox1.FormattingEnabled = true;
            configurationComboBox1.Location = new Point(250, 277);
            configurationComboBox1.Name = "configurationComboBox1";
            configurationComboBox1.SelectionStorageMode = Coding4Coffee.WinForms.Configuration.Controls.ConfigurationComboBoxValueMode.Value;
            configurationComboBox1.Size = new Size(121, 23);
            configurationComboBox1.TabIndex = 1;
            configurationComboBox1.ValueMember = "Key";
            // 
            // configurationRadioButtonGroup1
            // 
            configurationRadioButtonGroup1.ConfigKey = null;
            configurationRadioButtonGroup1.Location = new Point(541, 161);
            configurationRadioButtonGroup1.Name = "configurationRadioButtonGroup1";
            configurationRadioButtonGroup1.Size = new Size(200, 201);
            configurationRadioButtonGroup1.TabIndex = 2;
            configurationRadioButtonGroup1.TabStop = false;
            configurationRadioButtonGroup1.Text = "configurationRadioButtonGroup1";
            configurationRadioButtonGroup1.TextResourceKey = null;
            // 
            // configurationDateTimePicker1
            // 
            configurationDateTimePicker1.ConfigKey = null;
            configurationDateTimePicker1.Location = new Point(486, 78);
            configurationDateTimePicker1.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            configurationDateTimePicker1.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            configurationDateTimePicker1.Name = "configurationDateTimePicker1";
            configurationDateTimePicker1.Size = new Size(200, 23);
            configurationDateTimePicker1.TabIndex = 3;
            // 
            // configurationTrackBar1
            // 
            configurationTrackBar1.ConfigKey = null;
            configurationTrackBar1.Location = new Point(115, 370);
            configurationTrackBar1.Name = "configurationTrackBar1";
            configurationTrackBar1.Size = new Size(104, 45);
            configurationTrackBar1.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(configurationTrackBar1);
            Controls.Add(configurationDateTimePicker1);
            Controls.Add(configurationRadioButtonGroup1);
            Controls.Add(configurationComboBox1);
            Controls.Add(configurationCheckBox1);
            Name = "Form1";
            Text = "Form1";
            TextResourceKey = "form1.caption";
            ((System.ComponentModel.ISupportInitialize)configurationCheckBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)configurationComboBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)configurationRadioButtonGroup1).EndInit();
            ((System.ComponentModel.ISupportInitialize)configurationDateTimePicker1).EndInit();
            ((System.ComponentModel.ISupportInitialize)configurationTrackBar1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Coding4Coffee.WinForms.Configuration.Controls.ConfigurationCheckBox configurationCheckBox1;
        private Coding4Coffee.WinForms.Configuration.Controls.ConfigurationComboBox configurationComboBox1;
        private Coding4Coffee.WinForms.Configuration.Controls.ConfigurationRadioButtonGroup configurationRadioButtonGroup1;
        private Coding4Coffee.WinForms.Configuration.Controls.ConfigurationDateTimePicker configurationDateTimePicker1;
        private Coding4Coffee.WinForms.Configuration.Controls.ConfigurationTrackBar configurationTrackBar1;
    }
}
