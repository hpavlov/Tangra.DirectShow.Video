//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Video Driver - VideoCameraFrame
//
// Description:	This is the implementation of the internal VideoCameraFrame class
//
// Author:		(HDP) Hristo Pavlov <hristo_dpavlov@yahoo.com>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 13-Mar-2013	HDP	6.0.0	Initial commit
// 21-Mar-2013	HDP	6.0.0.	Implemented monochrome and colour grabbing
// --------------------------------------------------------------------------------
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tangra.DirectShowVideoBase.DirectShowVideo;

namespace Tangra.DirectShowVideoBase.DirectShowVideo.VideoCaptureImpl
{

	public enum VideoFrameLayout
	{
		Monochrome,
		Color,
		BayerRGGB
	}

	internal class VideoCameraFrame
	{
		public object Pixels;
		public long FrameNumber;
		public byte[] PreviewBitmapBytes;

		public VideoFrameLayout ImageLayout;
	}
}
