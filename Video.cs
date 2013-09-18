//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Video Driver - DirectShow Capture
//
// Description:	This file implements the IVideo COM interface for the Video Capture Driver
//
// Implements:	ASCOM Video interface version: 1
//
// Author:		(HDP) Hristo Pavlov <hristo_dpavlov@yahoo.com>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 15-Mar-2013	HDP	6.0.0	Initial commit
// 21-Mar-2013	HDP	6.0.0.	Implemented monochrome and colour grabbing
// 22-Mar-2013	HDP	6.0.0	Added support for XviD and Huffyuv codecs
// 19-Sep-2013  HDP 6.1.0   Renamed ConfigureImage to ConfigureDeviceProperties and CanConfigureImage to CanConfigureDeviceProperties
// --------------------------------------------------------------------------------
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.DeviceInterface;
using ASCOM.DirectShow;
using ASCOM.DirectShow.Properties;
using Microsoft.Win32;
using ASCOM.DirectShow.VideoCaptureImpl;

namespace ASCOM.DirectShow
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(IVideo))]
	[Guid("809B906A-240F-4802-B54F-04C65D1EB3E8")]
	[ProgId("ASCOM.DirectShow.Video")]
	public class Video : IVideo
	{
		/// <summary>
		/// Category under which the device will be listed by the ASCOM Chooser
		/// </summary>
		private static string DRIVER_DEVICE_TYPE = "Video";

		/// <summary>
		/// ASCOM DeviceID (COM ProgID) for this driver.
		/// The DeviceID is used by ASCOM applications to load the driver at runtime.
		/// </summary>
		private static string DRIVER_ID = "ASCOM.DirectShow.Video";

		/// <summary>
		/// Driver description that displays in the ASCOM Chooser.
		/// </summary>
		private static string DRIVER_DESCRIPTION = "Video Capture";

		#region ASCOM Registration
		//
		// Register or unregister driver for ASCOM. This is harmless if already
		// registered or unregistered. 
		//
		/// <summary>
		/// Register or unregister the driver with the ASCOM Platform.
		/// This is harmless if the driver is already registered/unregistered.
		/// </summary>
		/// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
		private static void RegUnregASCOM(bool bRegister)
		{
			using (var P = new ASCOM.Utilities.Profile())
			{
				P.DeviceType = DRIVER_DEVICE_TYPE;
				if (bRegister)
				{
					P.Register(DRIVER_ID, DRIVER_DESCRIPTION);
				}
				else
				{
					P.Unregister(DRIVER_ID);
				}
			}
		}

		/// <summary>
		/// This function registers the driver with the ASCOM Chooser and
		/// is called automatically whenever this class is registered for COM Interop.
		/// </summary>
		/// <param name="t">Type of the class being registered, not used.</param>
		/// <remarks>
		/// This method typically runs in two distinct situations:
		/// <list type="numbered">
		/// <item>
		/// In Visual Studio, when the project is successfully built.
		/// For this to work correctly, the option <c>Register for COM Interop</c>
		/// must be enabled in the project settings.
		/// </item>
		/// <item>During setup, when the installer registers the assembly for COM Interop.</item>
		/// </list>
		/// This technique should mean that it is never necessary to manually register a driver with ASCOM.
		/// </remarks>
		[ComRegisterFunction]
		public static void RegisterASCOM(Type t)
		{
			RegUnregASCOM(true);
		}

		/// <summary>
		/// This function unregisters the driver from the ASCOM Chooser and
		/// is called automatically whenever this class is unregistered from COM Interop.
		/// </summary>
		/// <param name="t">Type of the class being registered, not used.</param>
		/// <remarks>
		/// This method typically runs in two distinct situations:
		/// <list type="numbered">
		/// <item>
		/// In Visual Studio, when the project is cleaned or prior to rebuilding.
		/// For this to work correctly, the option <c>Register for COM Interop</c>
		/// must be enabled in the project settings.
		/// </item>
		/// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
		/// </list>
		/// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
		/// </remarks>
		[ComUnregisterFunction]
		public static void UnregisterASCOM(Type t)
		{
			RegUnregASCOM(false);
		}
		#endregion

		private VideoCaptureImpl.VideoCapture camera;

		public Video()
		{
			Properties.Settings.Default.Reload();

			camera = new VideoCaptureImpl.VideoCapture();
		}

		/// <summary>
		/// Set True to connect to the device. Set False to disconnect from the device.
		/// You can also read the property to check whether it is connected.
		/// </summary>
		/// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		/// <remarks><p style="color:red"><b>Must be implemented</b></p>Do not use a NotConnectedException here, that exception is for use in other methods that require a connection in order to succeed.</remarks>
		public bool Connected
		{
			get { return camera.IsConnected; }
			set
			{
				if (value != camera.IsConnected)
				{
					if (value)
					{
						if (camera.LocateCaptureDevice())
							camera.EnsureConnected();						
					}
					else
						camera.EnsureDisconnected();
				}
			}
		}

		/// <exception cref="T:ASCOM.NotConnectedException">If the device is not connected and this information is only available when connected.</exception>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public string Description
		{
			get { return DRIVER_DESCRIPTION; }
		}

		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public string DriverInfo
		{
			get
			{
				return string.Format(
                    @"DirectShow Video Capture Driver v{0}", DriverVersion);
			}
		}

		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public string DriverVersion
		{
			get
			{
                return ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0]).Version;
			}
		}

		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public short InterfaceVersion
		{
			get { return 1; }
		}

		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public string Name
		{
			get { return DRIVER_DESCRIPTION; }
		}

		public string VideoCaptureDeviceName
		{
			get
			{
				return camera.DeviceName;
			}
		}

		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public void SetupDialog()
		{
			using (frmSetupDialog setupDlg = new frmSetupDialog())
			{
				Form ownerForm = Application.OpenForms
					.Cast<Form>()
					.FirstOrDefault(x => x != null && x.GetType().FullName == "ASCOM.Utilities.ChooserForm");

				if (ownerForm == null)
					ownerForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x != null && x.Owner == null);

				setupDlg.StartPosition = FormStartPosition.CenterParent;

				if (setupDlg.ShowDialog(ownerForm) == DialogResult.OK)
				{
					Properties.Settings.Default.Save();

					camera.ReloadSettings();

					return;
				}
				Properties.Settings.Default.Reload();
			}
		}

		private void AssertConnected()
		{
			if (!camera.IsConnected)
			    throw new ASCOM.NotConnectedException();
		}


		/// <exception cref="T:ASCOM.MethodNotImplementedException">Throws this exception if no actions are suported.</exception>
		/// <exception cref="T:ASCOM.ActionNotImplementedException">It is intended that the SupportedActions method will inform clients 
		/// of driver capabilities, but the driver must still throw an ASCOM.ActionNotImplemented exception if it is asked to 
		/// perform an action that it does not support.</exception>
		/// <exception cref="T:ASCOM.NotConnectedException">If the driver is not connected.</exception>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		[DebuggerStepThrough]
		public string Action(string ActionName, string ActionParameters)
		{
			throw new MethodNotImplementedException();
		}

		///	<exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public System.Collections.ArrayList SupportedActions
		{
			get
			{
				return new ArrayList();
			}
		}

		public void Dispose()
		{
			if (camera != null && camera.IsConnected)
			    camera.EnsureDisconnected();

			camera = null;
		}

		private double GetCameraExposureFromFrameRate()
		{
			return 1000.0 / camera.FrameRate;
		}

		public double ExposureMax
		{
			get { return GetCameraExposureFromFrameRate(); }
		}

		public double ExposureMin
		{
			get { return GetCameraExposureFromFrameRate(); }
		}

		public VideoCameraFrameRate FrameRate
		{
			get
			{
				if (Math.Abs(camera.FrameRate - 29.97) < 0.5)
					return VideoCameraFrameRate.NTSC;
				else if (Math.Abs(camera.FrameRate - 25) < 0.5)
					return VideoCameraFrameRate.PAL;
				else
					return VideoCameraFrameRate.Variable;
			}
		}

		/// <exception cref="T:ASCOM.NotConnectedException">Must throw exception if data unavailable.</exception>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw exception if camera supports only one integration rate (exposure) that cannot be changed.</exception>		
		public System.Collections.ArrayList SupportedIntegrationRates
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("SupportedIntegrationRates", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.InvalidValueException">Must throw an exception if not valid.</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if the camera supports only one integration rate (exposure) that cannot be changed.</exception>
		public int IntegrationRate
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("IntegrationRate", false);
			}

			[DebuggerStepThrough]
			set
			{
				throw new PropertyNotImplementedException("IntegrationRate", true);
			}
		}

		/// <exception cref="T:ASCOM.NotConnectedException">Must throw exception if data unavailable.</exception>
		/// <exception cref="T:ASCOM.InvalidOperationException">If called before any video frame has been taken</exception>	
		public IVideoFrame LastVideoFrame
		{
			get
			{
				AssertConnected();

				VideoCameraFrame cameraFrame;

				if (camera.GetCurrentFrame(out cameraFrame))
				{
					VideoFrame rv = VideoFrame.CreateFrame(camera.ImageWidth, camera.ImageHeight, cameraFrame);
					return rv;
				}
				else
					throw new ASCOM.InvalidOperationException("No video frames are available.");
			}
		}

		public IVideoFrame LastVideoFrameImageArrayVariant
		{
			get
			{
				AssertConnected();

				VideoCameraFrame cameraFrame;

				if (camera.GetCurrentFrame(out cameraFrame))
				{
					VideoFrame rv = VideoFrame.CreateFrameVariant(camera.ImageWidth, camera.ImageHeight, cameraFrame);
					return rv;
				}
				else
					throw new ASCOM.InvalidOperationException("No video frames are available.");
			}
		}


		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public string SensorName
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("SensorName", false);
			}
		}

		public SensorType SensorType
		{
			get
			{
				return VideoCapture.SimulatedSensorType;
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		public int CameraXSize
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("CameraXSize", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public int CameraYSize
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("CameraYSize", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if data unavailable.</exception>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public double PixelSizeX
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("PixelSizeX", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		public int Width
		{
			get
			{
				AssertConnected();

				return camera.ImageWidth;
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		public int Height
		{
			get
			{
				AssertConnected();

				return camera.ImageHeight;
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if data unavailable.</exception>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public double PixelSizeY
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("PixelSizeY", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if data unavailable.</exception>
		public int BitDepth
		{
			get
			{
				AssertConnected();

				return camera.BitDepth;
			}
		}

		public string VideoCodec
		{
			get
			{
				return camera.GetUsedAviFourCC();
			}
		}

		public string VideoFileFormat
		{
			get { return "AVI"; }
		}

		public int VideoFramesBufferSize
		{
			get
			{
				return 1;
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if not connected.</exception>
		///	<exception cref="T:ASCOM.InvalidOperationException">Must throw exception if the current camera state doesn't allow to begin recording a file.</exception>
		///	<exception cref="T:ASCOM.DriverException">Must throw exception if there is any other problem as a result of which the recording cannot begin.</exception>
		public string StartRecordingVideoFile(string PreferredFileName)
		{
			AssertConnected();

			try
			{
				VideoCameraState currentState = camera.GetCurrentCameraState();

				if (currentState == VideoCameraState.videoCameraRecording)
					throw new InvalidOperationException("The camera is already recording.");
				else if (currentState != VideoCameraState.videoCameraRunning)
					throw new InvalidOperationException("The current state of the video camera doesn't allow a recording operation to begin right now.");

				string directory = Path.GetDirectoryName(PreferredFileName);
				string fileName = Path.GetFileName(PreferredFileName);

				if (!Directory.Exists(directory))
					Directory.CreateDirectory(fileName);

				if (File.Exists(PreferredFileName))
					throw new DriverException(string.Format("File '{0}' already exists. Video can be recorded only in a non existing file.", PreferredFileName));

				return camera.StartRecordingVideoFile(PreferredFileName);
			}
			catch (Exception ex)
			{
				throw new DriverException("Error starting the recording: " + ex.Message, ex);
			}
		}


		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if not connected.</exception>
		///	<exception cref="T:ASCOM.InvalidOperationException">Must throw exception if the current camera state doesn't allow to stop recording the file or no file is currently being recorded.</exception>
		///	<exception cref="T:ASCOM.DriverException">Must throw exception if there is any other problem as result of which the recording cannot stop.</exception>
		public void StopRecordingVideoFile()
		{
			AssertConnected();

			try
			{
				VideoCameraState currentState = camera.GetCurrentCameraState();

				if (currentState != VideoCameraState.videoCameraRecording)
					throw new InvalidOperationException("The camera is currently not recording.");

				camera.StopRecordingVideoFile();

			}
			catch (Exception ex)
			{
				throw new DriverException("Error stopping the recording: " + ex.Message, ex);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must return an exception if the camera status is unavailable.</exception>
		public VideoCameraState CameraState
		{
			get
			{
				AssertConnected();

				return camera.GetCurrentCameraState();
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmax is not supported</exception>
		public short GainMax
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("GainMax", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public short GainMin
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("GainMin", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.InvalidValueException">Must throw an exception if not valid.</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gain is not supported</exception>
		public short Gain
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("Gain", false);
			}

			[DebuggerStepThrough]
			set
			{
				throw new PropertyNotImplementedException("Gain", true);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if Gains is not supported</exception>
		public System.Collections.ArrayList Gains
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("Gains", false);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.InvalidValueException">Must throw an exception if not valid.</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gamma is not supported</exception>
		public int Gamma
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("Gamma", false);
			}

			[DebuggerStepThrough]
			set
			{
				throw new PropertyNotImplementedException("Gamma", true);
			}
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public System.Collections.ArrayList Gammas
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("Gammas", false);
			}
		}

		public bool CanConfigureDeviceProperties
		{
			get { return true; }
		}

		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the camera is not connected.</exception>
		///	<exception cref="T:ASCOM.MethodNotImplementedException">Must throw an exception if ConfigureImage is not supported.</exception>
		[DebuggerStepThrough]
		public void ConfigureDeviceProperties()
		{
			AssertConnected();

			camera.ShowDeviceProperties();
		}
	}
}