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

		/// <summary>
		/// Returns a description of the device, such as manufacturer and modelnumber. Any ASCII characters may be used. 
		/// </summary>
		/// <value>The description.</value>
		/// <exception cref="T:ASCOM.NotConnectedException">If the device is not connected and this information is only available when connected.</exception>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public string Description
		{
			get { return DRIVER_DESCRIPTION; }
		}

		/// <summary>
		/// Descriptive and version information about this ASCOM driver.
		/// </summary>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		/// <remarks>
		///	<p style="color:red"><b>Must be implemented</b></p> This string may contain line endings and may be hundreds to thousands of characters long.
		/// It is intended to display detailed information on the ASCOM driver, including version and copyright data.
		/// See the <see cref="P:ASCOM.DeviceInterface.IVideo.Description"/> property for information on the device itself.
		/// To get the driver version in a parseable string, use the <see cref="P:ASCOM.DeviceInterface.IVideo.DriverVersion"/> property.
		/// </remarks>
		public string DriverInfo
		{
			get
			{
				return string.Format(
                    @"DirectShow Video Capture Driver v{0}", DriverVersion);
			}
		}

		/// <summary>
		/// A string containing only the major and minor version of the driver.
		/// </summary>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		/// <remarks><p style="color:red"><b>Must be implemented</b></p> This must be in the form "n.n".
		/// It should not to be confused with the <see cref="P:ASCOM.DeviceInterface.IVideo.InterfaceVersion"/> property, which is the version of this specification supported by the 
		/// driver.
		/// </remarks>
		public string DriverVersion
		{
			get
			{
                return ((AssemblyFileVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0]).Version;
			}
		}

		/// <summary>
		/// The interface version number that this device supports. Should return 1 for this interface version.
		/// </summary>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		/// <remarks><p style="color:red"><b>Must be implemented</b></p>
		/// </remarks>
		public short InterfaceVersion
		{
			get { return 1; }
		}

		/// <summary>
		/// The short name of the driver, for display purposes.
		/// </summary>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		public string Name
		{
			get { return DRIVER_DESCRIPTION; }
		}

		/// <summary>
		/// The name of the video capture device when such a device is used. For analogue video this is usually the video capture card or dongle attached to the computer. 
		/// </summary>
		public string VideoCaptureDeviceName
		{
			get
			{
				return camera.DeviceName;
			}
		}

		/// <summary>
		/// Launches a configuration dialog box for the driver.  The call will not return
		/// until the user clicks OK or cancel manually.
		/// </summary>
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

		/// <summary>
		/// Invokes the specified device-specific action.
		/// </summary>
		/// <param name="ActionName">
		/// A well known name agreed by interested parties that represents the action to be carried out. 
		/// </param>
		/// <param name="ActionParameters">List of required parameters or an <see cref="T:System.String">Empty String</see> if none are required.
		/// </param>
		///	<returns>A string response. The meaning of returned strings is set by the driver author.</returns>
		/// <exception cref="T:ASCOM.MethodNotImplementedException">Throws this exception if no actions are suported.</exception>
		/// <exception cref="T:ASCOM.ActionNotImplementedException">It is intended that the SupportedActions method will inform clients 
		/// of driver capabilities, but the driver must still throw an ASCOM.ActionNotImplemented exception if it is asked to 
		/// perform an action that it does not support.</exception>
		/// <exception cref="T:ASCOM.NotConnectedException">If the driver is not connected.</exception>
		/// <exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		/// <example>Suppose filter wheels start to appear with automatic wheel changers; new actions could 
		/// be “FilterWheel:QueryWheels” and “FilterWheel:SelectWheel”. The former returning a 
		/// formatted list of wheel names and the second taking a wheel name and making the change, returning appropriate 
		/// values to indicate success or failure.
		/// </example>
		/// <remarks><p style="color:red"><b>Can throw a not implemented exception</b></p> 
		/// This method is intended for use in all current and future device types and to avoid name clashes, management of action names 
		/// is important from day 1. A two-part naming convention will be adopted - <b>DeviceType:UniqueActionName</b> where:
		/// <list type="bullet">
		///		<item><description>DeviceType is the same value as would be used by <see cref="P:ASCOM.Utilities.Chooser.DeviceType"/> e.g. Telescope, Camera, Switch etc.</description></item>
		///		<item><description>UniqueActionName is a single word, or multiple words joined by underscore characters, that sensibly describes the action to be performed.</description></item>
		///	</list>
		/// <para>
		/// It is recommended that UniqueActionNames should be a maximum of 16 characters for legibility.
		/// Should the same function and UniqueActionName be supported by more than one type of device, the reserved DeviceType of 
		/// “General” will be used. Action names will be case insensitive, so FilterWheel:SelectWheel, filterwheel:selectwheel 
		/// and FILTERWHEEL:SELECTWHEEL will all refer to the same action.</para>
		///	<para>The names of all supported actions must bre returned in the <see cref="P:ASCOM.DeviceInterface.IVideo.SupportedActions"/> property.</para>
		/// </remarks>
		[DebuggerStepThrough]
		public string Action(string ActionName, string ActionParameters)
		{
			throw new MethodNotImplementedException();
		}

		/// <summary>
		/// Returns the list of action names supported by this driver.
		/// </summary>
		///	<value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
		///	<exception cref="T:ASCOM.DriverException">Must throw an exception if the call was not successful</exception>
		/// <remarks><p style="color:red"><b>Must be implemented</b></p> This method must return an empty arraylist if no actions are supported. Please do not throw a 
		/// <see cref="T:ASCOM.PropertyNotImplementedException"/>.
		/// <para>This is an aid to client authors and testers who would otherwise have to repeatedly poll the driver to determine its capabilities. 
		/// Returned action names may be in mixed case to enhance presentation but  will be recognised case insensitively in 
		/// the <see cref="M:ASCOM.DeviceInterface.IVideo.Action(System.String,System.String)">Action</see> method.</para>
		/// <para>An array list collection has been selected as the vehicle for  action names in order to make it easier for clients to
		/// determine whether a particular action is supported. This is easily done through the Contains method. Since the
		/// collection is also ennumerable it is easy to use constructs such as For Each ... to operate on members without having to be concerned 
		/// about hom many members are in the collection. </para>
		///	<para>Collections have been used in the Telescope specification for a number of years and are known to be compatible with COM. Within .NET
		/// the ArrayList is the correct implementation to use as the .NET Generic methods are not compatible with COM.</para>
		/// </remarks>
		public System.Collections.ArrayList SupportedActions
		{
			get
			{
				return new ArrayList();
			}
		}

		/// <summary>
		/// Dispose the late-bound interface, if needed. Will release it via COM
		/// if it is a COM object, else if native .NET will just dereference it
		/// for GC.
		/// </summary>
		public void Dispose()
		{
			//if (camera != null && camera.IsConnected)
			//    camera.EnsureDisconnected();

			//camera = null;
		}

		private double GetCameraExposureFromFrameRate()
		{
			return 1000.0 / camera.FrameRate;
		}

		/// <summary>
		/// The maximum supported exposure (integration time) in seconds.
		/// </summary>
		/// <remarks>
		/// This value is for information purposes only. The exposure cannot be set directly in seconds, use <see cref="P:ASCOM.DeviceInterface.IVideo.IntegrationRate"/> method to change the exposure. 
		/// </remarks>
		public double ExposureMax
		{
			get { return GetCameraExposureFromFrameRate(); }
		}

		/// <summary>
		/// The minimum supported exposure (integration time) in seconds.
		/// </summary>
		/// <remarks>
		/// This value is for information purposes only. The exposure cannot be set directly in seconds, use <see cref="P:ASCOM.DeviceInterface.IVideo.IntegrationRate"/> method to change the exposure. 
		/// </remarks>		
		public double ExposureMin
		{
			get { return GetCameraExposureFromFrameRate(); }
		}

		/// <summary>
		/// The frame reate at which the camera is running. 
		/// </summary>
		/// <remarks>
		/// Analogue cameras run in one of the two fixes frame rates - 25fps for PAL video and 29.97fps for NTSC video. Most digital cameras support variable frame rate.
		/// </remarks>
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

		/// <summary>
		/// Returns the list of integration rates supported by the video camera.
		/// </summary>
		/// <remarks>
		/// Digital and integrating analogue video cameras allow the effective exposure of a frame to be changed. If the camera supports setting the exposure directly i.e. 2.153 sec then the driver must only
		/// return a range of useful supported exposures. For many video cameras the supported exposures (integration rates) increase by a factor of 2 from a base exposure e.g. 1, 2, 4, 8, 16 sec or 0.04, 0.08, 0.16, 0.32, 0.64, 1.28, 2.56, 5.12, 10.24 sec.
		/// If the camers supports only one exposure that cannot be changed (such as all non integrating PAL or NTSC video cameras) then this property must throw <see cref="T:ASCOM.PropertyNotImplementedException"/>.
		/// </remarks>
		/// <value>The list of supported integration rates in seconds.</value>
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

		/// <summary>
		///	Index into the <see cref="P:ASCOM.DeviceInterface.IVideo.SupportedIntegrationRates"/> array for the selected camera integration rate
		///	</summary>
		///	<value>Integer index for the current camera integration rate in the <see cref="P:ASCOM.DeviceInterface.IVideo.SupportedIntegrationRates"/> string array.</value>
		///	<returns>Index into the SupportedIntegrationRates array for the selected camera integration rate</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.InvalidValueException">Must throw an exception if not valid.</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if the camera supports only one integration rate (exposure) that cannot be changed.</exception>
		///	<remarks>
		///	<see cref="P:ASCOM.DeviceInterface.IVideo.IntegrationRate"/> can be used to adjust the integration rate (exposure) of the camera, if supported. A 0-based array of strings - <see cref="P:ASCOM.DeviceInterface.IVideo.SupportedIntegrationRates"/>, 
		/// which correspond to different disctere integration rate settings supported by the camera will be returned. <see cref="P:ASCOM.DeviceInterface.IVideo.IntegrationRate"/> must be set to an integer in this range.
		///	<para>The driver must default <see cref="P:ASCOM.DeviceInterface.IVideo.IntegrationRate"/> to a valid value when integration rate is supported by the camera. </para>
		///	</remarks>		
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

		/// <summary>
		/// Returns a <see cref="T:ASCOM.DeviceInterface.IVideoFrame"/> with its <see cref="P:ASCOM.DeviceInterface.IVideoFrame.ImageArray"/> property populated. 
		/// </summary>
		/// <remarks>
		/// The <see cref="P:ASCOM.DeviceInterface.IVideoFrame.ImageArrayVariant"/> property of the video frame will not be populated. Use the <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrameImageArrayVariant"/> property
		/// to obtain a video frame that has the <see cref="P:ASCOM.DeviceInterface.IVideoFrame.ImageArrayVariant"/> populated.
		/// </remarks>
		/// <value>The video frame.</value>
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


		/// <summary>
		/// Sensor name
		/// </summary>
		///	<returns>The name of sensor used within the camera</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<remarks>Returns the name (datasheet part number) of the sensor, e.g. ICX285AL.  The format is to be exactly as shown on 
		///	manufacturer data sheet, subject to the following rules. All letter shall be uppercase.  Spaces shall not be included.
		///	<para>Any extra suffixes that define region codes, package types, temperature range, coatings, grading, color/monochrome, 
		///	etc. shall not be included. For color sensors, if a suffix differentiates different Bayer matrix encodings, it shall be 
		///	included.</para>
		///	<para>Examples:</para>
		///	<list type="bullet">
		///		<item><description>ICX285AL-F shall be reported as ICX285</description></item>
		///		<item><description>KAF-8300-AXC-CD-AA shall be reported as KAF-8300</description></item>
		///	</list>
		///	<para><b>Note:</b></para>
		///	<para>The most common usage of this property is to select approximate color balance parameters to be applied to 
		///	the Bayer matrix of one-shot color sensors.  Application authors should assume that an appropriate IR cutoff filter is 
		///	in place for color sensors.</para>
		///	<para>It is recommended that this function be called only after a <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> is established with 
		///	the camera hardware, to ensure that the driver is aware of the capabilities of the specific camera model.</para>
		///	</remarks>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		public string SensorName
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("SensorName", false);
			}
		}

		/// <summary>
		/// Sensor type, identifies the type of colour sensor
		/// </summary>
		public SensorType SensorType
		{
			get
			{
				return VideoCapture.SimulatedSensorType;
			}
		}

		/// <summary>
		///	Returns the width of the video camera CCD chip in unbinned pixels.
		///	</summary>
		///	<value>The size of the camera X.</value>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		public int CameraXSize
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("CameraXSize", false);
			}
		}

		/// <summary>
		///	Returns the height of the video camera CCD chip in unbinned pixels.
		///	</summary>
		///	<value>The size of the camera Y.</value>
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

		/// <summary>
		///	Returns the width of the CCD chip pixels in microns.
		///	</summary>
		///	<value>The pixel size X.</value>
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

		/// <summary>
		///	Returns the width of the video frame in pixels.
		///	</summary>
		///	<value>The video frame width.</value>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		/// <remarks>
		/// For analogue video cameras working via a frame grabber the dimentions of the video frames may be different than the dimention of the CCD chip
		/// </remarks>
		public int Width
		{
			get
			{
				AssertConnected();

				return camera.ImageWidth;
			}
		}

		/// <summary>
		///	Returns the height of the video frame in pixels.
		///	</summary>
		///	<value>The video frame height.</value>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if the value is not known</exception>
		/// <remarks>
		/// For analogue video cameras working via a frame grabber the dimentions of the video frames may be different than the dimention of the CCD chip
		/// </remarks>
		public int Height
		{
			get
			{
				AssertConnected();

				return camera.ImageHeight;
			}
		}

		/// <summary>
		///	Returns the height of the CCD chip pixels in microns.
		///	</summary>
		///	<value>The pixel size Y.</value>
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

		/// <summary>
		///	Reports the bit depth the camera can produce.
		///	</summary>
		///	<value>The bit depth per pixel.</value>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw exception if data unavailable.</exception>
		public int BitDepth
		{
			get
			{
				AssertConnected();

				return camera.BitDepth;
			}
		}

		/// <summary>
		/// Returns the video codec used to record the video file, e.g. XVID, DVSD, YUY2, HFYU etc. For AVI files this is usually the FourCC identifier of the codec. If no codec is used an empty string must be returned.
		/// </summary>
		public string VideoCodec
		{
			get
			{
				return camera.GetUsedAviFourCC();
			}
		}

		/// <summary>
		/// Returns the file format of the recorded video file, e.g. AVI, MPEG, ADV etc.
		/// </summary>
		public string VideoFileFormat
		{
			get { return "AVI"; }
		}

		/// <summary>
		///	The size of the video frame buffer. 
		///	</summary>
		///	<value>The size of the video frame buffer. </value>
		///	<remarks><p style="color:red"><b>Must be implemented</b></p> When retrieving video frames using the <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrame" /> and 
		/// <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrameImageArrayVariant" /> properties the driver may use a buffer to queue the frames waiting to be read by 
		/// the client. This property returns the size of the buffer in frames or if no buffering is supported then the value of less than 2 should be returned. The size 
		/// of the buffer can be controlled by the end user from the driver setup dialog. 
		///	</remarks>
		public int VideoFramesBufferSize
		{
			get
			{
				return 1;
			}
		}

		/// <summary>
		/// Starts recording a new video file.
		/// </summary>
		/// <param name="PreferredFileName">The file name requested by the client. Some systems may not allow the file name to be controlled directly and they should ignore this parameter.</param>
		/// <returns>The actual file name that is being recorded.</returns>
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


		/// <summary>
		/// Stops the recording of a video file.
		/// </summary>
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

		/// <summary>
		///	Returns the current camera operational state
		///	</summary>
		///	<remarks>
		///	Returns one of the following status information:
		///	<list type="bullet">
		///		<listheader><description>Value  State           Meaning</description></listheader>
		///		<item><description>0      CameraIdle      At idle state, camera is available for commands</description></item>
		///		<item><description>1      CameraBusy	  The camera is waiting for operation to complete. The camera is not responding to commands right now</description></item>
		///		<item><description>2      CameraRunning	  The camera is running and video frames are available for viewing and recording</description></item>
		///		<item><description>3      CameraRecording The camera is running and recording a video</description></item>
		///		<item><description>4      CameraError     Camera error condition serious enough to prevent further operations (connection fail, etc.).</description></item>
		///	</list>
		///	</remarks>
		///	<value>The state of the camera.</value>
		///	<exception cref="T:ASCOM.NotConnectedException">Must return an exception if the camera status is unavailable.</exception>
		public VideoCameraState CameraState
		{
			get
			{
				AssertConnected();

				return camera.GetCurrentCameraState();
			}
		}

		/// <summary>
		///	Maximum value of <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/>
		///	</summary>
		///	<value>Short integer representing the maximum gain value supported by the camera.</value>
		///	<returns>The maximum gain value that this camera supports</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmax is not supported</exception>
		///	<remarks>When specifying the gain setting with an integer value, <see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> is used in conjunction with <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/> to 
		///	specify the range of valid settings.
		///	<para><see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> shall be greater than <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/>. If either is available, then both must be available.</para>
		///	<para>Please see <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> for more information.</para>
		///	<para>It is recommended that this function be called only after a <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> is established with the camera hardware, to ensure 
		///	that the driver is aware of the capabilities of the specific camera model.</para>
		///	</remarks>
		public short GainMax
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("GainMax", false);
			}
		}

		/// <summary>
		///	Minimum value of <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/>
		///	</summary>
		///	<returns>The minimum gain value that this camera supports</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		///	<remarks>When specifying the gain setting with an integer value, <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/> is used in conjunction with <see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> to 
		///	specify the range of valid settings.
		///	<para><see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> shall be greater than <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/>. If either is available, then both must be available.</para>
		///	<para>Please see <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> for more information.</para>
		///	<para>It is recommended that this function be called only after a <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> is established with the camera hardware, to ensure 
		///	that the driver is aware of the capabilities of the specific camera model.</para>
		///	</remarks>
		public short GainMin
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("GainMin", false);
			}
		}

		/// <summary>
		///	Index into the <see cref="P:ASCOM.DeviceInterface.IVideo.Gains"/> array for the selected camera gain
		///	</summary>
		///	<value>Short integer index for the current camera gain in the <see cref="P:ASCOM.DeviceInterface.IVideo.Gains"/> string array.</value>
		///	<returns>Index into the Gains array for the selected camera gain</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.InvalidValueException">Must throw an exception if not valid.</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gain is not supported</exception>
		///	<remarks>
		///	<see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> can be used to adjust the gain setting of the camera, if supported. There are two typical usage scenarios:
		///	<ul>
		///		<li>Discrete gain video cameras will return a 0-based array of strings - <see cref="P:ASCOM.DeviceInterface.IVideo.Gains"/>, which correspond to different disctere gain settings supported by the camera. <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> must be set to an integer in this range. <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/> and <see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> must thrown an exception if 
		///	this mode is used.</li>
		///		<li>Adjustable gain video cameras - <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/> and <see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> return integers, which specify the valid range for <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/> and <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/>.</li>
		///	</ul>
		///	<para>The driver must default <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> to a valid value. </para>
		///	</remarks>
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

		/// <summary>
		/// Gains supported by the camera
		///	</summary>
		///	<returns>An ArrayList of gain names or values</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if Gains is not supported</exception>
		///	<remarks><see cref="P:ASCOM.DeviceInterface.IVideo.Gains"/> provides a 0-based array of available gain settings.  This is often used to specify ISO settings for DSLR cameras.  
		///	Typically the application software will display the available gain settings in a drop list. The application will then supply 
		///	the selected index to the driver via the <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> property. 
		///	<para>The <see cref="P:ASCOM.DeviceInterface.IVideo.Gain"/> setting may alternatively be specified using integer values; if this mode is used then <see cref="P:ASCOM.DeviceInterface.IVideo.Gains"/> is invalid 
		///	and must throw an exception. Please see <see cref="P:ASCOM.DeviceInterface.IVideo.GainMax"/> and <see cref="P:ASCOM.DeviceInterface.IVideo.GainMin"/> for more information.</para>
		///	<para>It is recommended that this function be called only after a <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> is established with the camera hardware, 
		///	to ensure that the driver is aware of the capabilities of the specific camera model.</para>
		///	</remarks>
		public System.Collections.ArrayList Gains
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("Gains", false);
			}
		}

		/// <summary>
		///	Index into the <see cref="P:ASCOM.DeviceInterface.IVideo.Gammas"/> array for the selected camera gamma
		///	</summary>
		///	<value>Integer index for the current camera gamma in the <see cref="P:ASCOM.DeviceInterface.IVideo.Gammas"/> string array.</value>
		///	<returns>Index into the Gammas array for the selected camera gamma</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.InvalidValueException">Must throw an exception if not valid.</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gamma is not supported</exception>
		///	<remarks>
		///	<see cref="P:ASCOM.DeviceInterface.IVideo.Gamma"/> can be used to adjust the gamma setting of the camera, if supported. A 0-based array of strings - <see cref="P:ASCOM.DeviceInterface.IVideo.Gammas"/>, 
		/// which correspond to different disctere gamma settings supported by the camera will be returned. <see cref="P:ASCOM.DeviceInterface.IVideo.Gamma"/> must be set to an integer in this range.
		///	<para>The driver must default <see cref="P:ASCOM.DeviceInterface.IVideo.Gamma"/> to a valid value. </para>
		///	</remarks>
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

		/// <summary>
		/// Gamma values supported by the camera
		///	</summary>
		///	<returns>An ArrayList of gamma names or values</returns>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the information is not available. (Some drivers may require an 
		///	active <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> in order to retrieve necessary information from the camera.)</exception>
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmin is not supported</exception>
		///	<remarks><see cref="P:ASCOM.DeviceInterface.IVideo.Gammas"/> provides a 0-based array of available gamma settings.
		///	Typically the application software will display the available gamma settings in a drop list. The application will then supply 
		///	the selected index to the driver via the <see cref="P:ASCOM.DeviceInterface.IVideo.Gamma"/> property. 
		///	<para>It is recommended that this function be called only after a <see cref="P:ASCOM.DeviceInterface.IVideo.Connected">connection</see> is established with the camera hardware, 
		///	to ensure that the driver is aware of the capabilities of the specific camera model.</para>
		///	</remarks>
		public System.Collections.ArrayList Gammas
		{
			[DebuggerStepThrough]
			get
			{
				throw new PropertyNotImplementedException("Gammas", false);
			}
		}

		/// <summary>
		/// Returns True if the camera supports custom image configuration via the <see cref="M:ASCOM.DeviceInterface.IVideo.ConfigureImage"/> method.
		/// </summary>
		/// <remarks><p style="color:red"><b>Must be implemented</b></p> 
		/// </remarks>
		public bool CanConfigureImage
		{
			get { return true; }
		}

		/// <summary>
		/// Displays an image configuration dialog that allows configuration of specialized image settings such as White or Colour Balance for example. 
		/// </summary>
		///	<exception cref="T:ASCOM.NotConnectedException">Must throw an exception if the camera is not connected.</exception>
		///	<exception cref="T:ASCOM.MethodNotImplementedException">Must throw an exception if ConfigureImage is not supported.</exception>
		/// <remarks>
		/// <para>This dialog is not intended to be used in unattended mode but can give great control over the image quality for some drivers and devices. The dialog may also allow 
		/// chaning settings such as Gamma and Gain that can be also controlled directly via the <see cref="T:ASCOM.DeviceInterface.IVideo"/> interface. If a client software 
		/// displays the current Gamma and Gain it should update the values after this method has been called as those values for Gamma and Gain may have changed.</para>
		/// <para>To support automated and unattended control over the specializded image settings available on this dialog the driver must also alow their control via <see cref="P:ASCOM.DeviceInterface.IVideo.SupportedActions"/></para>
		/// </remarks>
		[DebuggerStepThrough]
		public void ConfigureImage()
		{
			AssertConnected();

			camera.ShowDeviceProperties();
		}
	}
}