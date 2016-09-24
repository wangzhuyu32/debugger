using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
namespace LeEcoDebugger
{
    public partial class Form1 : Form
    {
        static public List<CrashFile> CrashFiles = null;
        CrashFile cf = null;
        private uint ociMem = 0x6680000;
        private uint ocOffset = 0x3f000;
        public Form1()
        {
            InitializeComponent();
            CrashFiles = new List<CrashFile>();
            targetLabel.Text = "";
        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            //clear up
            targetLabel.Text = "";
            if (CrashFiles != null && CrashFiles.Count > 0)
                CrashFiles.Clear();
            //end clear
            char[] slashesToTrim = new char[] { '\\', '/' };
            this.txtDump.Text = this.txtDump.Text.Trim().TrimEnd(slashesToTrim);
            this.txtElf.Text = this.txtElf.Text.Trim().TrimEnd(slashesToTrim);
            T32_Info_type info = new T32_Info_type();
            info.subSystem = "modem";
            info.CPU_name = "HexagonV55";
            ProcessSubD(this.txtDump.Text, this.txtElf.Text, ref info);
            Cursor.Current = Cursors.Default;
            this.Refresh();
            Thread simulatorThread = new Thread(
            new ThreadStart(delegate ()
            {
                T32 tModem;
                if (cf == null)
                {
                    MessageBox.Show("OCIMEM not found");
                    tModem = new LeEcoDebugger.T32(info);
                }
                else
                { 
                    tModem = new LeEcoDebugger.T32(info, cf.reloc);
                }
                tModem.loadRamDump();
            }
            ));
            simulatorThread.Start();
            Cursor.Current = Cursors.WaitCursor;
            simulatorThread.Join();
            this.Close();
        }

        private void btnDumpFolder_Click(object sender, EventArgs e)
        {
            OnPickFolderPath(sender, e, txtDump);
        }

        private void OnPickFolderPath(object sender, EventArgs e, TextBox t)
        {
            UI_ext diaglog = new UI_ext() { InitialFolder = t.Text };
            if (diaglog.ShowDialog(this) != System.Windows.Forms.DialogResult.Cancel)
            {
                t.Text = diaglog.Folder;
            }
        }

        private void btnElfFolder_Click(object sender, EventArgs e)
        {
            OnPickFolderPath(sender, e, txtElf);
        }
        //update t_type
        private void ProcessSubD(string dumpPath, string elfPath, ref T32_Info_type t_type)
        {
            t_type.elfPath = elfPath;
            t_type.dumpPath = dumpPath;
            Dictionary<string, string> elfName = new Dictionary<string, string>();

            var elfN = Directory.GetFiles(elfPath, "*.elf", SearchOption.AllDirectories);
            foreach (var elf in elfN)
            {
                //modem
                if (elf.Contains("orig_MODEM_PROC"))
                {
                    t_type.elfPath = Path.Combine(elfPath, elf);
                    break;
                }
            }
            List<string> items = new List<string>();
            string[] supportedExtensions = new string[] { ".bin", ".cmm" };
            var files = Directory.GetFiles(dumpPath, "*.*", SearchOption.AllDirectories);
            foreach (var fl in files)
            {
                if (supportedExtensions.Contains(Path.GetExtension(fl).ToLower()))
                {
                    items.Add(fl);
                }
            }
            //parse OCIMEM
            //PorcessOCIMEM(dumpPath, )
            string cmm = Path.Combine(dumpPath, "load.cmm");
            if (!items.Contains(cmm))
            {
                MessageBox.Show("load.cmm not found! Pls check");
                return;
            }
            //parse load.cmm
            FileStream stream = null;
            StreamReader reader = null;
            try
            {
                if (Util.fileExists(cmm))
                {
                    FileInfo f = new FileInfo(cmm);
                    stream = File.Open(cmm, FileMode.Open, FileAccess.Read, FileShare.Read);
                    reader = new StreamReader(stream);
                    string filename = string.Empty;
                    uint offset = 0;
                    uint skipOffset = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();
                        string[] parts = line.Split(' ');
                        //parse target
                        if (parts.Length == 4 && string.IsNullOrEmpty(targetLabel.Text))
                        {
                            if (parts[3].Contains("8976"))
                            {
                                targetLabel.Text = string.Format("Current Target - MSM8976");
                                t_type.targetName = "MSM8976";
                                t_type.needBinary = "DDRCS1";
                                offset = 0x59FFFFF;
                                skipOffset = 0x0;
                            }
                            else if (parts[3].Contains("8996"))
                            {
                                t_type.targetName = "MSM8996";
                                targetLabel.Text = string.Format("Current Target - MSM8996");
                                t_type.needBinary = "DDRCS0";

                            }
                        }
                        if (parts.Length > 2 && parts[1].Contains("OCIMEM") && parts[2].Substring(0, 2) == "0x")
                        {
                            filename = Path.Combine(dumpPath, parts[1]);
                            UInt64 memAddr = UInt64.Parse(parts[2].Substring(2), System.Globalization.NumberStyles.HexNumber);
                            if (Util.fileExists(filename))
                            {
                                //CrashFile OC_CF = new CrashFile(filename, memAddr, "OCIMEM");
                                cf = new CrashFile(filename, memAddr, "OCIMEM");
                                CrashFiles.Add(cf);
                                if (cf.MemStart == 0x8600000)
                                    ocOffset = 0;
                                PorcessOCIMEM(dumpPath, ref cf);

                            }
                        }
                        if (parts.Length >= 3 && parts[0].Equals("d.load.binary", StringComparison.InvariantCultureIgnoreCase) && parts[1].Contains(t_type.needBinary) && parts[2].Substring(0, 2) == "0x")
                        {
                            //common
                            filename = Path.Combine(dumpPath, parts[1]);
                            UInt64 memAddr = UInt64.Parse(parts[2].Substring(2), System.Globalization.NumberStyles.HexNumber);
                            if (Util.fileExists(filename))
                                CrashFiles.Add(new CrashFile(filename, memAddr));
                            //--- just for parse modem and need to optimize later
                            //verify binary is present or not 
                            if (!Util.fileExists(dumpPath + "\\" + parts[1].ToString()))
                                return;
                            //8976
                            
                            if (cf.reloc.ContainsKey("modem"))
                            {
                                parts[2] = "0x" + (cf.reloc["modem"].startAddr).ToString("X");
                                offset = cf.reloc["modem"].size - 1;
                                skipOffset = cf.reloc["modem"].startAddr - (uint)memAddr;
                            }
                            else
                                parts[2] = (0x86800000).ToString();
                             
                            t_type.loadBinaryCommand = parts[0] + " " + Path.Combine(dumpPath, parts[1]) + " " + parts[2] + "++0x" + offset.ToString("X") +  " " + "/SKIP " + "0x" + skipOffset.ToString("X");
                            //find
                            break;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();
            }

            switch (t_type.subSystem)
            {
                case "modem":
                    //need to have DDRCS0.BIN | load.cmm
                    if (string.IsNullOrEmpty(t_type.loadBinaryCommand))
                    {
                        return;
                    }
                    // verify elf
                    if (string.IsNullOrEmpty(elfPath))
                        return;
                    if (string.IsNullOrEmpty(t_type.elfPath))
                        return;
                    break;
                default:
                    break;
            }
            //verify dump and elf
        }
        private void PorcessOCIMEM(string dumpPath, ref CrashFile cf)
        {
            uint ocAddr = (uint)cf.MemStart + ocOffset + 0x94C;
            string ocPath = Path.Combine(dumpPath, "OCIMEM.BIN");
            const uint entry_num = 10;
            const uint entry_size = 20;
            if (Util.fileExists(ocPath))
            {
                byte[] buffer;
                cf.readBufferExFromPA((ulong)ocAddr, entry_num * entry_size, out buffer, CrashFiles, "OCIMEM");
                if (buffer == null)
                    return;
                for (uint i = 0; i < entry_num; i++)
                {
                    string imageName = Util.ExtractString(buffer, i * entry_size, 8);
                    if (string.IsNullOrEmpty(imageName))
                    {
                        break;
                    }
                    uint imageStart = Util.ExtractUInt32(buffer, 8 + i * entry_size);
                    uint imageSize = Util.ExtractUInt32(buffer, 16 + i * entry_size);
                    cf.reloc.Add(imageName, new RelocableType(imageStart, imageSize));
                }
            }
        }
    
    }   

  }
