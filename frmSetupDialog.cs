//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Video Driver - DirectShow
//
// Description:	This is the set up form for the Video Driver DirectShow 
//
// Author:		(HDP) Hristo Pavlov <hristo_dpavlov@yahoo.com>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 13-Mar-2013	HDP	6.0.0	Initial commit
// 21-Mar-2013	HDP	6.0.0.	Implemented monochrome and colour grabbing
// 22-Mar-2013	HDP	6.0.0	Added support for XviD and Huffyuv codecs
// --------------------------------------------------------------------------------
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.DeviceInterface;
using ASCOM.DirectShow.Properties;
using ASCOM.DirectShow.VideoCaptureImpl;
using DirectShowLib;

namespace ASCOM.DirectShow
{
	[ComVisible(false)]
	public partial class frmSetupDialog : Form
	{
		IBaseFilter theDevice = null;
		IBaseFilter theCompressor = null;

		public frmSetupDialog()
		{
			InitializeComponent();

            CrossbarHelper.UpdateNoCrossbarSettings(cbxCrossbarInput);
		}

		private void frmSetupDialog_Load(object sender, EventArgs e)
		{
			LoadSettings();

			btnInputPros.Enabled = true;
			btnCompressorProps.Enabled = true;

            AssemblyFileVersionAttribute attr = (AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0];
            lblVersion.Text = "v" + attr.Version;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
            if (rbCompressionUnsupported.Checked && cbxOtherCodecs.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a video codec to use.", "ASCOM Video Capture");
                cbxOtherCodecs.Focus();
                return;
            }

			if (SaveSettings())
			{
				Properties.Settings.Default.Save();
				DialogResult = DialogResult.OK;

				Close();
			}
		}

		private void LoadSettings()
		{
			cbxCaptureDevices.Items.Clear();
			foreach (DsDevice ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
			{
				cbxCaptureDevices.Items.Add(ds.Name);
			}

			List<SystemCodecEntry> systemCodecs = VideoCodecs.GetSupportedVideoCodecs();
			foreach (SystemCodecEntry codec in systemCodecs)
			{
				RadioButton rbCodec =  gbxCompression
					.Controls
					.Cast<Control>()
				    .SingleOrDefault(x => x is RadioButton && string.Equals(x.Text, codec.DeviceName.ToString())) as RadioButton;

				if (rbCodec != null)
				{
					rbCodec.Enabled = codec.DeviceName != null && codec.IsInstalled;
					rbCodec.Checked = codec.DeviceName == Settings.Default.PreferredCompressorDevice;
					rbCodec.Tag = codec;
				}
			}

            cbxOtherCodecs.Items.Clear();
            foreach (DsDevice ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoCompressorCategory).Where(x => systemCodecs.All(y => y.DeviceName != x.Name)))
            {
                cbxOtherCodecs.Items.Add(ds.Name);
                if (ds.Name == Settings.Default.PreferredCompressorDevice)
                {
                    cbxOtherCodecs.SelectedItem = ds.Name;
                    rbCompressionUnsupported.Checked = true;
                }
            }

			if (cbxCaptureDevices.Items.Count > 0)
			{
				if (cbxCaptureDevices.Items.Contains(Settings.Default.PreferredCaptureDevice))
					cbxCaptureDevices.SelectedIndex = cbxCaptureDevices.Items.IndexOf(Settings.Default.PreferredCaptureDevice);
				else
					cbxCaptureDevices.SelectedIndex = 0;
			}

			cbxSensorType.Items.Clear();
			cbxSensorType.Items.Add(VideoFrameLayout.Monochrome);
			cbxSensorType.Items.Add(VideoFrameLayout.Color);
			cbxSensorType.Items.Add(VideoFrameLayout.BayerRGGB);
			cbxSensorType.SelectedItem = Settings.Default.SimulatedImageLayout;

			cbxMonochromePixelsFrom.Items.Clear();
			cbxMonochromePixelsFrom.Items.Add(MonochromePixelMode.R);
			cbxMonochromePixelsFrom.Items.Add(MonochromePixelMode.G);
			cbxMonochromePixelsFrom.Items.Add(MonochromePixelMode.B);
			cbxMonochromePixelsFrom.Items.Add(MonochromePixelMode.GrayScale);
			cbxMonochromePixelsFrom.SelectedItem = Settings.Default.MonochromePixelsType;
		}

		private bool SaveSettings()
		{
			if (cbxCaptureDevices.SelectedIndex == -1)
				Settings.Default.PreferredCaptureDevice = string.Empty;
			else
				Settings.Default.PreferredCaptureDevice = (string)cbxCaptureDevices.SelectedItem;

            if (rbCompressionUnsupported.Checked && cbxOtherCodecs.SelectedIndex != -1)
                Settings.Default.PreferredCompressorDevice = (string) cbxOtherCodecs.SelectedItem;
            else
            {
                RadioButton rbCodec = gbxCompression
                                    .Controls
                                    .Cast<Control>()
                                    .SingleOrDefault(x => x is RadioButton && ((RadioButton)x).Checked) as RadioButton;

                if (rbCodec != null && rbCodec.Tag is SystemCodecEntry)
                    Settings.Default.PreferredCompressorDevice = ((SystemCodecEntry)rbCodec.Tag).DeviceName;
                else
                    Settings.Default.PreferredCompressorDevice = VideoCodecs.UNCOMPRESSED_VIDEO;                
            }

			Settings.Default.SimulatedImageLayout = (VideoFrameLayout)cbxSensorType.SelectedItem;
			Settings.Default.MonochromePixelsType = (MonochromePixelMode)cbxMonochromePixelsFrom.SelectedItem;

			return true;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BrowseToAscom(object sender, EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("http://ascom-standards.org/");
			}
			catch (System.ComponentModel.Win32Exception noBrowser)
			{
				if (noBrowser.ErrorCode == -2147467259)
					MessageBox.Show(noBrowser.Message);
			}
			catch (System.Exception other)
			{
				MessageBox.Show(other.Message);
			}
		}

		

		/// <summary>
		/// Enumerates all filters of the selected category and returns the IBaseFilter for the 
		/// filter described in friendlyname
		/// </summary>
		/// <param name="category">Category of the filter</param>
		/// <param name="friendlyname">Friendly name of the filter</param>
		/// <returns>IBaseFilter for the device</returns>
		private IBaseFilter CreateFilter(Guid category, string friendlyname)
		{
			object source = null;
			Guid iid = typeof(IBaseFilter).GUID;
			foreach (DsDevice device in DsDevice.GetDevicesOfCat(category))
			{
				if (device.Name.CompareTo(friendlyname) == 0)
				{
					device.Mon.BindToObject(null, null, ref iid, out source);
					break;
				}
			}

			return (IBaseFilter)source;
		}

		private void btnInputPros_Click(object sender, EventArgs e)
		{
			DirectShowCapture.DisplayPropertyPage(theDevice, this.Handle);
		}

		private void cbxCaptureDevices_SelectedIndexChanged(object sender, EventArgs e)
		{
			//Release COM objects
			if (theDevice != null)
			{
				Marshal.ReleaseComObject(theDevice);
				theDevice = null;
			}

			if (cbxCaptureDevices.SelectedIndex != -1)
			{
				//Create the filter for the selected video input device
                string deviceName = cbxCaptureDevices.SelectedItem.ToString();
                theDevice = CreateFilter(FilterCategory.VideoInputDevice, deviceName);

                if (!string.IsNullOrEmpty(deviceName))
                {
                    Cursor = Cursors.WaitCursor;
                    Update();

                    cbxCrossbarInput.Items.Clear();
                    cbxCrossbarInput.SelectedIndexChanged -= new EventHandler(cbxCrossbarInput_SelectedIndexChanged);
                    try
                    {
                        CrossbarHelper.LoadCrossbarSources(deviceName, cbxCrossbarInput);
                    }
                    finally
                    {
                        cbxCrossbarInput.SelectedIndexChanged += new EventHandler(cbxCrossbarInput_SelectedIndexChanged);
                        Cursor = Cursors.Default;
                    }
                }
			}
		}

		private void SelectedCodecChanged(object sender, EventArgs e)
		{
		    RadioButton rbSender = sender as RadioButton;
            if (rbSender != null && rbSender.Checked)
			{
				SystemCodecEntry selectedCodec = ((SystemCodecEntry)(sender as RadioButton).Tag);
				if (selectedCodec != null && selectedCodec.Device != null)
				{
					theCompressor = CreateFilter(FilterCategory.VideoCompressorCategory, selectedCodec.Device.Name);
				}
			}
		}

		private void btnCompressorProps_Click(object sender, EventArgs e)
		{
			if (theCompressor != null)
				DirectShowCapture.DisplayPropertyPage(theCompressor, this.Handle);
		}

		private void cbSensorType_SelectedIndexChanged(object sender, EventArgs e)
		{
			pnlBWPixels.Visible = cbxSensorType.SelectedIndex == 0;
		}

        private void cbxOtherCodecs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxOtherCodecs.SelectedIndex != -1)
            {
                theCompressor = CreateFilter(FilterCategory.VideoCompressorCategory, (string)cbxOtherCodecs.SelectedItem);

                rbCompressionUncompressed.Checked = false;
                rbCompressionDV.Checked = false;
                rbCompressionXviD.Checked = false;
                rbCompressionHuffyuv211.Checked = false;
                rbCompressionUnsupported.Checked = true;
            }
        }

        private void cbxCrossbarInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = cbxCrossbarInput.SelectedItem as CrossbarHelper.CrossbarPinEntry;

            if (selectedItem != null)
            {
                Settings.Default.CrossbarInputPin = selectedItem.PinIndex;
                Settings.Default.Save();
            }
        }

        private void rbCompressionUnsupported_CheckedChanged(object sender, EventArgs e)
        {
            cbxOtherCodecs.Enabled = rbCompressionUnsupported.Checked;
        }
	}
}