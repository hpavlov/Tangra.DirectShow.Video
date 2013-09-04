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
using System.Linq;
using System.Text;
using ASCOM.DeviceInterface;

namespace ASCOM.DirectShow.VideoCaptureImpl
{
	internal class VideoCameraFrame
	{
		public object Pixels;
		public long FrameNumber;

		public VideoFrameLayout ImageLayout;
	}
}
