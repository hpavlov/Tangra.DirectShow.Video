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
// 21-Mar-2013	HDP	6.0.0	Implemented monochrome and colour grabbing
// 19-Sep-2013  HDP 6.1.0   Added the PreviewBitmap property
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
		private Bitmap previewBitmap;

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

			rv.previewBitmap = cameraFrame.PreviewBitmap;
			rv.frameNumber = cameraFrame.FrameNumber;
			rv.exposureStartTime = null;
			rv.exposureDuration = null;
			rv.imageInfo = null;

			return rv;
		}

		public object ImageArray
		{
			get
			{
				return pixels;
			}
		}

		public object ImageArrayVariant
		{
			get
			{
				return pixelsVariant;
			}
		}

		public Bitmap PreviewBitmap
		{
			get
			{
				return previewBitmap;
			}
		}

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

		public string ImageInfo
		{
			get { return imageInfo; }
		}

	}
}

