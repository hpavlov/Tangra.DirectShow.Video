//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Video Driver - DirectShow
//
// Description:	This is the implementation of the internal DirectShow class. This is the actual
//              implementaion of the DirectShow Capture driver
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ASCOM.DeviceInterface;
using ASCOM.DirectShow.Properties;
using DirectShowLib;
using Koyash.VideoUtilities;

namespace ASCOM.DirectShow.VideoCaptureImpl
{
	internal class VideoCapture
	{
		private DsDevice videoInputDevice;
		private SystemCodecEntry videoCompressor;

		private DirectShowCapture dsCapture = new DirectShowCapture();

		private float frameRate;
		private int imageWidth;
		private int imageHeight;

		private Koyash.VideoUtilities.ICameraImage cameraImageHelper = new Koyash.VideoUtilities.CameraImage();

		private VideoCameraState cameraState = VideoCameraState.videoCameraIdle;

		public bool IsConnected
		{
			get { return dsCapture.IsRunning; }
		}

		public string DeviceName
		{
			get
			{
				if (videoInputDevice != null)
					return videoInputDevice.Name;
				else
					return string.Empty;
			}
		}

		public int ImageWidth
		{
			get { return imageWidth; }
		}

		public int ImageHeight
		{
			get { return imageHeight; }
		}

		public float FrameRate
		{
			get { return frameRate; }
		}

		public int BitDepth
		{
			get { return 8; }
		}

		public bool LocateCaptureDevice()
		{
			FindInputAndCompressorToUse(out videoInputDevice, out videoCompressor);

			return videoInputDevice != null;
		}

		public void EnsureConnected()
		{
			if (!IsConnected)
			{
				dsCapture.CloseResources();

				// TODO: Set a preferred frameRate and image size stored in the configuration

				dsCapture.SetupPreviewOnlyGraph(videoInputDevice, ref frameRate, ref imageWidth, ref imageHeight);

				dsCapture.Start();

				cameraState = dsCapture.IsRunning ? VideoCameraState.videoCameraRunning : VideoCameraState.videoCameraIdle;
			}
		}

		public void EnsureDisconnected()
		{
			dsCapture.Pause();

			dsCapture.CloseResources();

			cameraState = VideoCameraState.videoCameraIdle;
		}

		public void ReloadSettings()
		{
			if (IsConnected && (videoInputDevice != null || videoCompressor != null))
			{
				DsDevice inputDevice;
				SystemCodecEntry compressor;

				FindInputAndCompressorToUse(out inputDevice, out compressor);

				bool reconnect = false;

				if (inputDevice != null && 
					videoInputDevice != null && 
					videoInputDevice.DevicePath != inputDevice.DevicePath)
				{
					// We have a new video device. Will need to reconnect to it
					reconnect = true;
				}

				if (compressor != null &&
					videoCompressor != null &&
					videoCompressor.Codec != compressor.Codec)
				{
					// We have a new compressor device. Will need to reconnect
					reconnect = true;
				}

				if (reconnect)
				{
					EnsureDisconnected();

					videoInputDevice = inputDevice;
					videoCompressor = compressor;

					EnsureConnected();
				}
			}
		}

		private void FindInputAndCompressorToUse(out DsDevice inputDevice, out SystemCodecEntry compressor)
		{
			inputDevice = null;
			compressor = null;

			List<DsDevice> allInputDevices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));

			if (!string.IsNullOrEmpty(Settings.Default.PreferredCaptureDevice))
				inputDevice = allInputDevices.FirstOrDefault(x => x.Name == Settings.Default.PreferredCaptureDevice);
			else if (allInputDevices.Count > 0)
				inputDevice = allInputDevices[0];

			compressor = VideoCodecs.GetSupportedVideoCodec(Settings.Default.PreferredCompressorDevice);
		}

		public static SensorType SimulatedSensorType
		{
			get
			{
				switch (Settings.Default.SimulatedImageLayout)
				{
					case VideoFrameLayout.Monochrome:
						return SensorType.Monochrome;

					case VideoFrameLayout.Color:
						return SensorType.Color;

					case VideoFrameLayout.BayerRGGB:
						return SensorType.RGGB;
				}
				
				throw new ArgumentOutOfRangeException();
			}
		}

		public bool GetCurrentFrame(out VideoCameraFrame cameraFrame)
		{
			long frameId;
			Bitmap bmp = dsCapture.GetNextFrame(out frameId);

			if (bmp != null)
			{
				using (bmp)
				{
					object pixels = cameraImageHelper.GetImageArray(bmp, SimulatedSensorType, (LumaConversionMode)Settings.Default.MonochromePixelsType);

					cameraFrame = new VideoCameraFrame()
					{
						FrameNumber = frameId,
						Pixels = pixels,
						ImageLayout = Settings.Default.SimulatedImageLayout
					};					
				}

				return true;
			}

			cameraFrame = null;
			return false;
		}

		public VideoCameraState GetCurrentCameraState()
		{
			return cameraState;
		}

		public string StartRecordingVideoFile(string preferredFileName)
		{
			if (dsCapture.IsRunning)
				dsCapture.CloseResources();

			dsCapture.SetupFileRecorderGraph(videoInputDevice, videoCompressor, ref frameRate, ref imageWidth, ref imageHeight, preferredFileName);

			dsCapture.Start();

			cameraState = dsCapture.IsRunning ? VideoCameraState.videoCameraRecording : VideoCameraState.videoCameraIdle;

			return preferredFileName;
		}

		public void StopRecordingVideoFile()
		{
			EnsureDisconnected();

			EnsureConnected();
		}

		public string GetUsedAviFourCC()
		{
			return videoCompressor.FourCC;
		}

		public void ShowDeviceProperties()
		{
			if (IsConnected && videoInputDevice != null)
			{
				dsCapture.ShowDeviceProperties();
			}
		}
	}
}
