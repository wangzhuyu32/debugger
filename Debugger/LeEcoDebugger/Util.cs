using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
namespace LeEcoDebugger
{
    class Util
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        /// <summary>Determines if a file exists and is accessible, with a given Timeout</summary>
        /// <param name="path">Path of file to check</param>
        /// <param name="timeout">(optional) Timeout in milliseconds, default is 10 seconds</param>

        public enum Endian { BigE, LittleE};
        public static bool fileExists(string path, int timeout = 10000)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            bool found = ShortTask.Execute<bool>(timeout, false, () =>
            {
                bool exists = false;
                if (path.IndexOfAny(new char[] { '?', '*' }) >= 0)
                {
                    string folderPath = Path.GetDirectoryName(path);
                    string filePath = Path.GetFileName(path);
                    string[] files = null;
                    try
                    {
                        if (Util.directoryExists(folderPath))
                            files = Directory.GetFiles(folderPath, filePath, SearchOption.TopDirectoryOnly);
                    }
                    catch (Exception ex) { }
                    exists = (files != null && files.Length != 0);
                }
                else
                    exists = File.Exists(path);
                return exists;
            });

            return found;             
        }

        public static bool directoryExists(string path, int timeout = 10000)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            bool found = ShortTask.Execute<bool>(timeout, false, () =>
            {
                return Directory.Exists(path);
            });
            return found;
                
        }

        public static string GetTempFolder()
        {
            return Environment.GetEnvironmentVariable("Temp");
        }

        public static string ExtractString(byte[] buffer, uint index, uint maxSize = 0)
        {
            if (buffer == null) return string.Empty;
            if (maxSize == 0) maxSize = (uint)buffer.Length;
            List<byte> list = new List<byte>();
            uint i = index;
            while (i < buffer.Length && buffer[i] != 0 && i - index < maxSize)
            {
                list.Add(buffer[i++]);
                
            }

            return Encoding.UTF8.GetString(list.ToArray());
        }

        public static UInt32 ExtractUInt32(byte[] buffer, uint index, Endian endian = Endian.LittleE)
        {
            if (buffer == null) return 0;
            if (buffer.Length - index < 4)
                return 0;
            if (endian == Endian.LittleE)
            {
                return (uint)(buffer[index + 3] << 24) + (uint)(buffer[index + 2] << 16) + (uint)(buffer[index + 1] << 8) + buffer[index]; 
            }
            else
            {
                return (uint)(buffer[index] << 24) + (uint)(buffer[index + 1] << 16) + (uint)(buffer[index + 2] << 8) + buffer[index + 3];
            }
        }
    }
}
