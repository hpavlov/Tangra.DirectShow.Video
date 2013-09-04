namespace ASCOM.DirectShow
{
	partial class frmSetupDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetupDialog));
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label8 = new System.Windows.Forms.Label();
            this.btnCompressorProps = new System.Windows.Forms.Button();
            this.btnInputPros = new System.Windows.Forms.Button();
            this.cbxCaptureDevices = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxSensorType = new System.Windows.Forms.ComboBox();
            this.pnlBWPixels = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxMonochromePixelsFrom = new System.Windows.Forms.ComboBox();
            this.gbxCompression = new System.Windows.Forms.GroupBox();
            this.rbCompressionUnsupported = new System.Windows.Forms.RadioButton();
            this.cbxOtherCodecs = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.rbCompressionHuffyuv211 = new System.Windows.Forms.RadioButton();
            this.rbCompressionXviD = new System.Windows.Forms.RadioButton();
            this.rbCompressionDV = new System.Windows.Forms.RadioButton();
            this.rbCompressionUncompressed = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblVersion = new System.Windows.Forms.Label();
            this.cbxCrossbarInput = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.pnlBWPixels.SuspendLayout();
            this.gbxCompression.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(16, 369);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(120, 24);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(142, 368);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(120, 25);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = ((System.Drawing.Image)(resources.GetObject("picASCOM.Image")));
            this.picASCOM.Location = new System.Drawing.Point(437, 7);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Video Input";
            // 
            // btnCompressorProps
            // 
            this.btnCompressorProps.Location = new System.Drawing.Point(343, 116);
            this.btnCompressorProps.Name = "btnCompressorProps";
            this.btnCompressorProps.Size = new System.Drawing.Size(120, 43);
            this.btnCompressorProps.TabIndex = 23;
            this.btnCompressorProps.Text = "Codec properties";
            this.btnCompressorProps.Click += new System.EventHandler(this.btnCompressorProps_Click);
            // 
            // btnInputPros
            // 
            this.btnInputPros.Location = new System.Drawing.Point(297, 31);
            this.btnInputPros.Name = "btnInputPros";
            this.btnInputPros.Size = new System.Drawing.Size(120, 24);
            this.btnInputPros.TabIndex = 22;
            this.btnInputPros.Text = "Show property pages";
            this.btnInputPros.Click += new System.EventHandler(this.btnInputPros_Click);
            // 
            // cbxCaptureDevices
            // 
            this.cbxCaptureDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCaptureDevices.Location = new System.Drawing.Point(15, 34);
            this.cbxCaptureDevices.Name = "cbxCaptureDevices";
            this.cbxCaptureDevices.Size = new System.Drawing.Size(276, 21);
            this.cbxCaptureDevices.TabIndex = 20;
            this.cbxCaptureDevices.SelectedIndexChanged += new System.EventHandler(this.cbxCaptureDevices_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Simulated Sensor Type";
            // 
            // cbxSensorType
            // 
            this.cbxSensorType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSensorType.Location = new System.Drawing.Point(16, 126);
            this.cbxSensorType.Name = "cbxSensorType";
            this.cbxSensorType.Size = new System.Drawing.Size(275, 21);
            this.cbxSensorType.TabIndex = 25;
            this.cbxSensorType.SelectedIndexChanged += new System.EventHandler(this.cbSensorType_SelectedIndexChanged);
            // 
            // pnlBWPixels
            // 
            this.pnlBWPixels.Controls.Add(this.label3);
            this.pnlBWPixels.Controls.Add(this.cbxMonochromePixelsFrom);
            this.pnlBWPixels.Location = new System.Drawing.Point(291, 110);
            this.pnlBWPixels.Name = "pnlBWPixels";
            this.pnlBWPixels.Size = new System.Drawing.Size(193, 48);
            this.pnlBWPixels.TabIndex = 27;
            this.pnlBWPixels.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Monochrome Pixels From ...";
            // 
            // cbxMonochromePixelsFrom
            // 
            this.cbxMonochromePixelsFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMonochromePixelsFrom.Location = new System.Drawing.Point(6, 16);
            this.cbxMonochromePixelsFrom.Name = "cbxMonochromePixelsFrom";
            this.cbxMonochromePixelsFrom.Size = new System.Drawing.Size(182, 21);
            this.cbxMonochromePixelsFrom.TabIndex = 27;
            // 
            // gbxCompression
            // 
            this.gbxCompression.Controls.Add(this.rbCompressionUnsupported);
            this.gbxCompression.Controls.Add(this.cbxOtherCodecs);
            this.gbxCompression.Controls.Add(this.label1);
            this.gbxCompression.Controls.Add(this.label4);
            this.gbxCompression.Controls.Add(this.rbCompressionHuffyuv211);
            this.gbxCompression.Controls.Add(this.rbCompressionXviD);
            this.gbxCompression.Controls.Add(this.rbCompressionDV);
            this.gbxCompression.Controls.Add(this.rbCompressionUncompressed);
            this.gbxCompression.Controls.Add(this.btnCompressorProps);
            this.gbxCompression.Location = new System.Drawing.Point(16, 160);
            this.gbxCompression.Name = "gbxCompression";
            this.gbxCompression.Size = new System.Drawing.Size(469, 193);
            this.gbxCompression.TabIndex = 28;
            this.gbxCompression.TabStop = false;
            this.gbxCompression.Text = "Video Compression";
            // 
            // rbCompressionUnsupported
            // 
            this.rbCompressionUnsupported.AutoSize = true;
            this.rbCompressionUnsupported.Location = new System.Drawing.Point(17, 162);
            this.rbCompressionUnsupported.Name = "rbCompressionUnsupported";
            this.rbCompressionUnsupported.Size = new System.Drawing.Size(14, 13);
            this.rbCompressionUnsupported.TabIndex = 32;
            this.rbCompressionUnsupported.UseVisualStyleBackColor = true;
            this.rbCompressionUnsupported.CheckedChanged += new System.EventHandler(this.rbCompressionUnsupported_CheckedChanged);
            // 
            // cbxOtherCodecs
            // 
            this.cbxOtherCodecs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxOtherCodecs.Enabled = false;
            this.cbxOtherCodecs.Location = new System.Drawing.Point(37, 159);
            this.cbxOtherCodecs.Name = "cbxOtherCodecs";
            this.cbxOtherCodecs.Size = new System.Drawing.Size(238, 21);
            this.cbxOtherCodecs.TabIndex = 31;
            this.cbxOtherCodecs.SelectedIndexChanged += new System.EventHandler(this.cbxOtherCodecs_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(436, 39);
            this.label1.TabIndex = 24;
            this.label1.Text = "The following codecs are supported by this driver. If any of the items appear dis" +
    "abled this means that the corresponding codecs have not been installed on this s" +
    "ystem.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(305, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "The following codecs may work but there is no support for them";
            // 
            // rbCompressionHuffyuv211
            // 
            this.rbCompressionHuffyuv211.AutoSize = true;
            this.rbCompressionHuffyuv211.Location = new System.Drawing.Point(142, 97);
            this.rbCompressionHuffyuv211.Name = "rbCompressionHuffyuv211";
            this.rbCompressionHuffyuv211.Size = new System.Drawing.Size(95, 17);
            this.rbCompressionHuffyuv211.TabIndex = 3;
            this.rbCompressionHuffyuv211.Text = "Huffyuv v2.1.1";
            this.rbCompressionHuffyuv211.UseVisualStyleBackColor = true;
            this.rbCompressionHuffyuv211.CheckedChanged += new System.EventHandler(this.SelectedCodecChanged);
            // 
            // rbCompressionXviD
            // 
            this.rbCompressionXviD.AutoSize = true;
            this.rbCompressionXviD.Location = new System.Drawing.Point(142, 74);
            this.rbCompressionXviD.Name = "rbCompressionXviD";
            this.rbCompressionXviD.Size = new System.Drawing.Size(123, 17);
            this.rbCompressionXviD.TabIndex = 2;
            this.rbCompressionXviD.Text = "Xvid MPEG-4 Codec";
            this.rbCompressionXviD.UseVisualStyleBackColor = true;
            this.rbCompressionXviD.CheckedChanged += new System.EventHandler(this.SelectedCodecChanged);
            // 
            // rbCompressionDV
            // 
            this.rbCompressionDV.AutoSize = true;
            this.rbCompressionDV.Location = new System.Drawing.Point(17, 97);
            this.rbCompressionDV.Name = "rbCompressionDV";
            this.rbCompressionDV.Size = new System.Drawing.Size(113, 17);
            this.rbCompressionDV.TabIndex = 1;
            this.rbCompressionDV.Text = "DV Video Encoder";
            this.rbCompressionDV.UseVisualStyleBackColor = true;
            this.rbCompressionDV.CheckedChanged += new System.EventHandler(this.SelectedCodecChanged);
            // 
            // rbCompressionUncompressed
            // 
            this.rbCompressionUncompressed.AutoSize = true;
            this.rbCompressionUncompressed.Checked = true;
            this.rbCompressionUncompressed.Location = new System.Drawing.Point(17, 74);
            this.rbCompressionUncompressed.Name = "rbCompressionUncompressed";
            this.rbCompressionUncompressed.Size = new System.Drawing.Size(96, 17);
            this.rbCompressionUncompressed.TabIndex = 0;
            this.rbCompressionUncompressed.TabStop = true;
            this.rbCompressionUncompressed.Text = "Uncompressed";
            this.rbCompressionUncompressed.UseVisualStyleBackColor = true;
            this.rbCompressionUncompressed.CheckedChanged += new System.EventHandler(this.SelectedCodecChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.WindowText;
            this.panel1.Location = new System.Drawing.Point(16, 359);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(468, 1);
            this.panel1.TabIndex = 29;
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(337, 368);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(148, 23);
            this.lblVersion.TabIndex = 30;
            this.lblVersion.Text = "v1.0.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cbxCrossbarInput
            // 
            this.cbxCrossbarInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCrossbarInput.Enabled = false;
            this.cbxCrossbarInput.Items.AddRange(new object[] {
            "Automatic"});
            this.cbxCrossbarInput.Location = new System.Drawing.Point(16, 79);
            this.cbxCrossbarInput.Name = "cbxCrossbarInput";
            this.cbxCrossbarInput.Size = new System.Drawing.Size(276, 21);
            this.cbxCrossbarInput.TabIndex = 32;
            this.cbxCrossbarInput.SelectedIndexChanged += new System.EventHandler(this.cbxCrossbarInput_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 13);
            this.label5.TabIndex = 31;
            this.label5.Text = "Crossbar Input Source";
            // 
            // frmSetupDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 408);
            this.Controls.Add(this.cbxCrossbarInput);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbxCompression);
            this.Controls.Add(this.pnlBWPixels);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbxSensorType);
            this.Controls.Add(this.btnInputPros);
            this.Controls.Add(this.cbxCaptureDevices);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSetupDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Video Capture Setup";
            this.Load += new System.EventHandler(this.frmSetupDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.pnlBWPixels.ResumeLayout(false);
            this.pnlBWPixels.PerformLayout();
            this.gbxCompression.ResumeLayout(false);
            this.gbxCompression.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.PictureBox picASCOM;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnCompressorProps;
		private System.Windows.Forms.Button btnInputPros;
		private System.Windows.Forms.ComboBox cbxCaptureDevices;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbxSensorType;
		private System.Windows.Forms.Panel pnlBWPixels;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbxMonochromePixelsFrom;
		private System.Windows.Forms.GroupBox gbxCompression;
		private System.Windows.Forms.RadioButton rbCompressionHuffyuv211;
		private System.Windows.Forms.RadioButton rbCompressionXviD;
		private System.Windows.Forms.RadioButton rbCompressionDV;
		private System.Windows.Forms.RadioButton rbCompressionUncompressed;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cbxOtherCodecs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rbCompressionUnsupported;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.ComboBox cbxCrossbarInput;
        private System.Windows.Forms.Label label5;
	}
}