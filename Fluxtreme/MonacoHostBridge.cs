using System;
using System.Runtime.InteropServices;

namespace Fluxtreme
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class MonacoHostBridge
    {
        internal event Action<string> OnTextChanged;

        public event Action<int, int, int, int, string> OnDisplayErrorMarker;

        public void InvokeDisplayErrorMarker(int fromline, int toline, int fromcol, int tocol, string message)
        {
            OnDisplayErrorMarker?.Invoke(fromline, toline, fromcol, tocol, message);
        }

        public void TextChanged(string text)
        {
            OnTextChanged?.Invoke(text);
        }

        public void Log(string message)
        {
            Console.WriteLine($"Monaco: {message}");
        }
    }
}
