using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Tangra.DirectShowVideoBase.DirectShowVideo.Utils
{
	public class UIThreadCaller
	{
		public delegate void CallInUIThreadCallback(IWin32Window applicationWindow);

		public static void CallInUIThread(CallInUIThreadCallback action)
		{
			Form appFormWithMessageLoop = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x != null && x.Owner == null);

			if (appFormWithMessageLoop == null)
				appFormWithMessageLoop = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x != null);

			if (appFormWithMessageLoop != null)
			{
				if (appFormWithMessageLoop.InvokeRequired)
					appFormWithMessageLoop.Invoke(action, appFormWithMessageLoop);
				else
					action.Invoke(appFormWithMessageLoop);
			}
			else if (!Application.MessageLoop)
			{
				if (syncContext == null)
				{
					ThreadPool.QueueUserWorkItem(RunAppThread);
					while (syncContext == null)
						Thread.Sleep(10);
				}

				if (syncContext != null)
					syncContext.Post(new SendOrPostCallback(delegate(object state) { action.Invoke(null); }), null);
				else
					action.Invoke(null);
			}
			else
			{
				if (syncContext == null)
					syncContext = new WindowsFormsSynchronizationContext();

				if (syncContext != null)
					syncContext.Post(new SendOrPostCallback(delegate(object state) { action.Invoke(null); }), null);
				else
					action.Invoke(null);
			}
		}

		private static WindowsFormsSynchronizationContext syncContext;

		private static void RunAppThread(object state)
		{
			var ownMessageLoopMainForm = new Form();
			ownMessageLoopMainForm.ShowInTaskbar = false;
			ownMessageLoopMainForm.Width = 0;
			ownMessageLoopMainForm.Height = 0;
			ownMessageLoopMainForm.Load += ownerForm_Load;

			Application.Run(ownMessageLoopMainForm);

			if (syncContext != null)
			{
				syncContext.Dispose();
				syncContext = null;
			}
		}

		static void ownerForm_Load(object sender, EventArgs e)
		{
			Form form = (Form)sender;
			form.Left = -5000;
			form.Top = -5000;
			form.Hide();

			syncContext = new WindowsFormsSynchronizationContext();
		}
	}
}
