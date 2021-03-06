﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Duality.Launcher
{
	internal static class NativeMethods
	{
		[DllImport("kernel32.dll")]
		public static extern Int32 AllocConsole();
		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int X, int Y);
		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out Point lpPoint);
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern MessageBoxResult MessageBox(IntPtr hWnd, String text, String caption, int options);

		/// <summary>
		/// Flags that define appearance and behaviour of a standard message box displayed by a call to the MessageBox function.
		/// </summary>
		[Flags]
		public enum MessageBoxOptions : uint
		{
			Ok = 0x000000,
			OkCancel = 0x000001,
			AbortRetryIgnore = 0x000002,
			YesNoCancel = 0x000003,
			YesNo = 0x000004,
			RetryCancel = 0x000005,
			CancelTryContinue = 0x000006,

			IconHand = 0x000010,
			IconQuestion = 0x000020,
			IconExclamation = 0x000030,
			IconAsterisk = 0x000040,
			UserIcon = 0x000080,

			IconWarning = IconExclamation,
			IconError = IconHand,
			IconInformation = IconAsterisk,
			IconStop = IconHand,

			DefButton1 = 0x000000,
			DefButton2 = 0x000100,
			DefButton3 = 0x000200,
			DefButton4 = 0x000300,

			ApplicationModal = 0x000000,
			SystemModal = 0x001000,
			TaskModal = 0x002000,

			Help = 0x004000, //Help Button
			NoFocus = 0x008000,

			SetForeground = 0x010000,
			DefaultDesktopOnly = 0x020000,
			Topmost = 0x040000,
			Right = 0x080000,
			RTLReading = 0x100000,
		}

		/// <summary>
		/// Represents possible values returned by the MessageBox function.
		/// </summary>
		public enum MessageBoxResult : uint
		{
			Ok = 1,
			Cancel,
			Abort,
			Retry,
			Ignore,
			Yes,
			No,
			Close,
			Help,
			TryAgain,
			Continue,
			Timeout = 32000
		}
		public static Point GetCursorPos()
		{
			Point result;
			if (!GetCursorPos(out result)) return Point.Empty;
			return result;
		}
		public static bool GetCursorPos(out int x, out int y)
		{
			Point result;
			bool returnVal = GetCursorPos(out result);
			x = result.X;
			y = result.Y;
			return returnVal;
		}
	}
}
