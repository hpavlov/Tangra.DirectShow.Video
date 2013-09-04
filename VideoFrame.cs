//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Video Driver - DirectShow
//
// Description:	This file implements the IVideoFrame interface for the Video Capture Driver
//
// Implements:	ASCOM Video Frame interface version: 1
//
// Author:		(HDP) Hristo Pavlov <hristo_dpavlov@yahoo.com>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 15-Mar-2013	HDP	6.0.0	Initial commit
// 21-Mar-2013	HDP	6.0.0.	Implemented monochrome and colour grabbing
// --------------------------------------------------------------------------------
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using ASCOM.DeviceInterface;
using ASCOM.DirectShow.VideoCaptureImpl;

namespace ASCOM.DirectShow
{
	public enum VideoFrameLayout
	{
		Monochrome,
		Color,
		BayerRGGB
	}

	public enum MonochromePixelMode
	{
		R,
		G,
		B,
		GrayScale
	}

	public class VideoFrame : IVideoFrame
	{
		private long? frameNumber;
		private string imageInfo;
		private double? exposureDuration;
		private string exposureStartTime;
		private object pixels;
		private object pixelsVariant;

		private static int s_Counter = 0;

		internal static VideoFrame FakeFrame(int width, int height)
		{
			var rv = new VideoFrame();
			s_Counter++;
			rv.frameNumber = s_Counter;

			rv.pixels = new int[0, 0];
			return rv;
		}

		internal static VideoFrame CreateFrameVariant(int width, int height, VideoCameraFrame cameraFrame)
		{
			return InternalCreateFrame(width, height, cameraFrame, true);
		}

		internal static VideoFrame CreateFrame(int width, int height, VideoCameraFrame cameraFrame)
		{
			return InternalCreateFrame(width, height, cameraFrame, false);
		}

		private static VideoFrame InternalCreateFrame(int width, int height, VideoCameraFrame cameraFrame, bool variant)
		{
			var rv = new VideoFrame();

			if (cameraFrame.ImageLayout == VideoFrameLayout.Monochrome)
			{
				if (variant)
				{
					rv.pixelsVariant = new object[height, width];
					rv.pixels = null;
				}
				else
				{
					rv.pixels = new int[height, width];
					rv.pixelsVariant = null;
				}

				if (variant)
					Array.Copy((int[,])cameraFrame.Pixels, (object[,])rv.pixelsVariant, ((int[,])cameraFrame.Pixels).Length);
				else
					rv.pixels = (int[,])cameraFrame.Pixels;
			}
			else if (cameraFrame.ImageLayout == VideoFrameLayout.Color)
			{
				if (variant)
				{
					rv.pixelsVariant = new object[height, width, 3];
					rv.pixels = null;
				}
				else
				{
					rv.pixels = new int[height, width, 3];
					rv.pixelsVariant = null;
				}

				if (variant)
					Array.Copy((int[, ,])cameraFrame.Pixels, (object[, ,])rv.pixelsVariant, ((int[, ,])cameraFrame.Pixels).Length);
				else
					rv.pixels = (int[, ,])cameraFrame.Pixels;
			}
			else if (cameraFrame.ImageLayout == VideoFrameLayout.BayerRGGB)
			{
				throw new NotSupportedException();
			}
			else
				throw new NotSupportedException();

			rv.frameNumber = cameraFrame.FrameNumber;
			rv.exposureStartTime = null;
			rv.exposureDuration = null;
			rv.imageInfo = null;

			return rv;
		}

		/// <summary>
		/// Returns a safearray of int of size <see cref="P:ASCOM.DeviceInterface.IVideo.Width"/> * <see cref="P:ASCOM.DeviceInterface.IVideo.Height"/> containing the pixel values from the video frame. 
		/// </summary>
		/// <remarks>
		/// The application must inspect the Safearray parameters to determine the dimensions. 
		/// <para>The value will be only populated when the video frame has been obtained from the <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrame"/> property. When the video frame
		/// is obtained from the <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrameImageArrayVariant"/> property a NULL value must be returned. Do not throw an exception in this case.</para>
		/// <para>For color or multispectral cameras, will produce an array of  <see cref="P:ASCOM.DeviceInterface.IVideo.Width"/> * <see cref="P:ASCOM.DeviceInterface.IVideo.Height"/> *
		/// NumPlanes.  If the application cannot handle multispectral images, it should use just the first plane.</para>
		/// <para>The pixels in the array start from the top left part of the image and are listed by horizontal lines/rows. The second pixels in the array is the second pixels from the first horizontal row
		/// and the second last pixel in the array is the second last pixels from the last horizontal row.</para>
		/// </remarks>
		/// <value>The image array.</value>
		public object ImageArray
		{
			get
			{
				return pixels;
			}
		}

		/// <summary>
		/// Returns a safearray of Variant of size <see cref="P:ASCOM.DeviceInterface.IVideo.Width"/> * <see cref="P:ASCOM.DeviceInterface.IVideo.Height"/> containing the pixel values from the last video frame. 
		/// </summary>
		/// <remarks>
		/// The application must inspect the Safearray parameters to determine the dimensions. Note: This property should only be used from scripts due to the extremely high memory utilization on
		/// large image arrays (26 bytes per pixel). Pixels values should be in Short, int, or Double format.
		/// <para>The value will be only populated when the video frame has been obtained from the <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrameImageArrayVariant"/> property. When the video frame
		/// is obtained from the <see cref="P:ASCOM.DeviceInterface.IVideo.LastVideoFrame"/> property a NULL value must be returned. Do not throw an exception in this case.</para>
		/// <para>For color or multispectral cameras, will produce an array of <see cref="P:ASCOM.DeviceInterface.IVideo.Width"/> * <see cref="P:ASCOM.DeviceInterface.IVideo.Height"/> *
		/// NumPlanes.  If the application cannot handle multispectral images, it should use just the first plane.</para>
		/// <para>The pixels in the array start from the top left part of the image and are listed by horizontal lines/rows. The second pixels in the array is the second pixels from the first horizontal row
		/// and the second last pixel in the array is the second last pixels from the last horizontal row.</para>
		/// </remarks>
		/// <value>The image array variant.</value>
		public object ImageArrayVariant
		{
			get
			{
				return pixelsVariant;
			}
		}

		/// <summary>
		/// Returns the frame number.
		/// </summary>
		/// <remarks>
		/// The frame number of the first exposed frame may not be zero and is dependent on the device and/or the driver. The frame number increases with each acquired frame not with each requested frame by the client.
		/// </remarks>
		/// <value>The frame number of the current video frame.</value>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if not supported</exception>
		public long FrameNumber
		{
			get
			{
				if (frameNumber.HasValue)
					return frameNumber.Value;

				return -1;
			}
		}

		/// <summary>
		/// Returns the actual exposure duration in seconds (i.e. shutter open time).
		/// </summary>
		/// <remarks>
		/// This may differ from the exposure time corresponding to the requested frame exposure due to shutter latency, camera timing precision, etc.
		/// </remarks>
		/// <value>The duration of the frame exposure.</value>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if not supported</exception>
		public double ExposureDuration
		{
			[DebuggerStepThrough]
			get
			{
				if (exposureDuration.HasValue)
					return exposureDuration.Value;

				throw new ASCOM.PropertyNotImplementedException("Current camera doesn't support frame timing.");
			}
		}

		/// <summary>
		/// Returns the actual exposure start time in the FITS-standard CCYY-MM-DDThh:mm:ss[.sss...] format, if supported.
		/// </summary>
		/// <value>The frame exposure start time.</value>
		/// <exception cref="T:ASCOM.PropertyNotImplementedException">Must throw an exception if not supported</exception>
		public string ExposureStartTime
		{
			[DebuggerStepThrough]
			get
			{
				if (exposureStartTime != null)
					return exposureStartTime;

				throw new ASCOM.PropertyNotImplementedException("Current camera doesn't support frame timing.");
			}
		}

		/// <summary>
		/// Returns additional information associated with the video frame.
		/// </summary>
		/// <remarks><p style="color:red"><b>Must be implemented</b></p> This property must return an empty string if no additonal video frame information is supported. Please do not throw a 
		/// <see cref="T:ASCOM.PropertyNotImplementedException"/>.
		/// </remarks>
		/// <value>A string in a well known format agreed by interested parties that represents any additional information associated with the video frame.</value>
		public string ImageInfo
		{
			get { return imageInfo; }
		}

	}
}

