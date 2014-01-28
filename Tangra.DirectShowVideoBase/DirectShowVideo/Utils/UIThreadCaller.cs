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
		public delegate void CallInUIThreadCallback(IWin32Window applicationWindow, params object[] additionalParams);

		public static void Invoke(CallInUIThreadCallback action, params object[] additionalParams)
		{
			Form appFormWithMessageLoop = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x != null && x.Owner == null);
			if (appFormWithMessageLoop == null) appFormWithMessageLoop = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x != null);

			if (appFormWithMessageLoop != null && (Application.MessageLoop || hasOwnMessageLoop))
			{
				if (appFormWithMessageLoop.InvokeRequired)
				{
					if (hasOwnMessageLoop)
						PostToSyncContext(appFormWithMessageLoop, action, additionalParams);
					else
						appFormWithMessageLoop.Invoke(action, appFormWithMessageLoop, additionalParams);
				}
				else
				{
					action.Invoke(appFormWithMessageLoop, additionalParams);
				}
			}
			else if ((!Application.MessageLoop || appFormWithMessageLoop == null) && syncContext == null)
			{
				if (syncContext == null)
				{
					ThreadPool.QueueUserWorkItem(RunAppThread);
					while (syncContext == null) Thread.Sleep(10);
				}

				if (syncContext != null)
				{
					PostToSyncContext(appFormWithMessageLoop, action, additionalParams);
				}
				else
				{
					action.Invoke(appFormWithMessageLoop != null && !appFormWithMessageLoop.InvokeRequired ? appFormWithMessageLoop : null, additionalParams);
				}
			}
			else
			{
				if (syncContext == null)
				{
					syncContext = new WindowsFormsSynchronizationContext();
				}

				if (syncContext != null)
				{
					PostToSyncContext(appFormWithMessageLoop, action, additionalParams);
				}
				else
				{
					action.Invoke(appFormWithMessageLoop != null && !appFormWithMessageLoop.InvokeRequired ? appFormWithMessageLoop : null, additionalParams);
				}
			}
		}

		private static void PostToSyncContext( Form appFormWithMessageLoop, CallInUIThreadCallback action, params object[] additionalParams)
		{
			bool callFinished = false;
			syncContext.Post(new SendOrPostCallback(delegate(object state)
			{
				action.Invoke(appFormWithMessageLoop != null && !appFormWithMessageLoop.InvokeRequired ? appFormWithMessageLoop : null, additionalParams);
				callFinished = true;
			}), null);
			while (!callFinished) Thread.Sleep(10);			
		}

		private static WindowsFormsSynchronizationContext syncContext;
		private static bool hasOwnMessageLoop = false;

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

			hasOwnMessageLoop = true;
		}
	}
}
