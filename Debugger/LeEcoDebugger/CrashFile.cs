using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LeEcoDebugger
{
    public class CrashFile
    {
        private ulong start = 0;
        private ulong size = 0;
        private ulong fileOffset = 0;
        private FileStream stream = null;
        private BinaryReader reader = null;
        public string filename = string.Empty;
        public ulong MemStart { get { return start; } }
        public ulong FileOffset { get { return fileOffset; } }
        public ulong MemSize { get { return size; } }
        public string iName = string.Empty;
        public Dictionary<string, RelocableType> reloc = new Dictionary<string, RelocableType>();
        public CrashFile(string file, ulong memStart, string iName = "")
        {
            mapFile(file, memStart, 0, 0, iName);
        }
        public CrashFile(string file, ulong memStart, ulong offset, ulong length, string iName = "")
        {
            mapFile(file, memStart, offset, length, iName);
        }

        private void mapFile(string file, ulong memStart, ulong offset, ulong length, string iName = "")
        {
            try
            {
                if (Util.fileExists(file))
                {
                    filename = file;
                    start = memStart;
                    fileOffset = offset;
                    size = length;
                    if (length == 0)
                    {
                        FileInfo info = new FileInfo(file);
                        size = (UInt64)info.Length - offset;
                    }
                }
                this.iName = iName;
            }
            catch (Exception e)
            { }
        }

        public bool readBufferExFromPA(UInt64 addr, UInt32 size, out byte[] buffer, List<CrashFile> dumpFile, string imageName)
        {
            buffer = null;
            if (dumpFile == null)
                return false;
            uint current_size = 0;

            MemoryStream ms = new MemoryStream();
            foreach (CrashFile file in dumpFile)
            {
                if (file.iName.Equals(imageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    current_size = (uint)(file.size - (addr - file.MemStart));
                    var buff = file.read(addr, current_size);
                    if (buff == null)
                        buff = new byte[current_size];
                    ms.Write(buff, 0, buff.Length);
                }
               
            }
            if (ms.Length == 0)
                return false;
            if (ms.Length >= size)
            {
                ms.Position = 0;
                buffer = new byte[size];
                ms.Read(buffer, 0, (int)size);
            }
            ms.Dispose();
            return true;

        }

        public byte[] read(UInt64 addr, uint bytes)
        {
            byte[] buffer = null;
            try
            {
                if (stream == null)
                {
                    stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    reader = new BinaryReader(stream);
                }
                reader.BaseStream.Seek((long)(addr - start + fileOffset), 0);
                buffer = reader.ReadBytes((int)bytes);
            }
            catch (Exception e)
            {
                string msg = "debugger fail to access file";
            }

            return buffer;
        }

    }

}
