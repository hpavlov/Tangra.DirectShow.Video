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
using TACOS.DirectShowVideoBase.DirectShowVideo;

namespace ASCOM.DirectShow
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(IVideo))]
	[Guid("809B906A-240F-4802-B54F-04C65D1EB3E8")]
	[ProgId("TACOS.DirectShow.Video")]
	public class Video : DirectShowVideoBase, IVideo
	{
		/// <summary>
		/// Category under which the device will be listed by the ASCOM Chooser
		/// </summary>
		private static string DRIVER_DEVICE_TYPE = "Video";

		/// <summary>
		/// ASCOM DeviceID (COM ProgID) for this driver.
		/// The DeviceID is used by ASCOM applications to load the driver at runtime.
		/// </summary>
		private static string DRIVER_ID = "TACOS.DirectShow.Video";

		/// <summary>
		/// Driver description that displays in the ASCOM Chooser.
		/// </summary>
		private static string DRIVER_DESCRIPTION = "TACOS Video Capture";

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

		public Video()
		{
			Properties.Settings.Default.Reload();

			base.Initialize(Properties.Settings.Default);
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
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gainmax is not supported</exception>
		public short GammaMax
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
		public short GammaMin
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
		///	<exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if gamma is not supported</exception>
		public short Gamma
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
	}
}