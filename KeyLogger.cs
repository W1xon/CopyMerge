﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CopyMerge
{
    internal class KeyLogger
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static StringBuilder keyBuffer = new StringBuilder();
        private static StringBuilder clipboard = new StringBuilder();
        private static string clipboardBuffer;
        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(Int32 i);
        public static async Task KeyChecker()
        {
            while (true)
            {
                await Task.Delay(1);
                await semaphore.WaitAsync();
                try
                {
                    for (int i = 0; i < 68; i++)
                    {
                        if (GetAsyncKeyState(i) != 0)
                        {
                            if (((Keys)i) == Keys.Control || ((Keys)i) == Keys.ControlKey || ((Keys)i) == Keys.LControlKey || ((Keys)i) == Keys.RControlKey)
                                keyBuffer.Append("CTRL ");
                            else if (((Keys)i) == Keys.Shift || ((Keys)i) == Keys.ShiftKey || ((Keys)i) == Keys.LShiftKey || ((Keys)i) == Keys.RShiftKey)
                                keyBuffer.Append("SHIFT ");
                            else if (((Keys)i) == Keys.C)
                                keyBuffer.Append("C ");
                            else if (((Keys)i) == Keys.V)
                                keyBuffer.Append("V ");
                            else if((Keys)i == Keys.LButton && GetAsyncKeyState(Convert.ToInt32(Keys.ControlKey)) == 0)
                            {
                                if (clipboardBuffer != Clipboard.GetText())
                                {
                                    clipboard.Clear();
                                    clipboard.Append(Clipboard.GetText());
                                    clipboardBuffer = Clipboard.GetText();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        public static async Task KeyBufferClear()
        {
            while (true)
            {
                await Task.Delay(10);
                await semaphore.WaitAsync();
                try
                {
                    if (keyBuffer.Length > 0 && GetAsyncKeyState(Convert.ToInt32(Keys.ControlKey)) == 0)
                    {
                        CopyHandler();
                        keyBuffer.Clear();
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
        private static void CopyHandler()
        {
            string separator = SetSeparator();
            string[] key = keyBuffer.ToString().Split(' ');
            if (key.Contains("CTRL") && key.Contains("C"))
            {
                if (key.Contains("SHIFT"))
                {
                    if (!string.IsNullOrWhiteSpace(Clipboard.GetText()))
                    {
                        clipboard.Append(separator + Clipboard.GetText());
                        Clipboard.SetText(clipboard.ToString());
                        clipboardBuffer = Clipboard.GetText();
                    }
                    return;
                }
                clipboard.Clear();
                clipboard.Append(Clipboard.GetText());
                clipboardBuffer = Clipboard.GetText();

            }
        }
        private static string SetSeparator()
        {
            switch (Properties.Settings.Default.Separator)
            {
                case "Enter":
                    return "\t\n";

                case "Space":
                    return " ";

                case ", ":
                    return ", ";

                case ". ":
                    return ". ";
            }
            return "";
        }
    }
}
