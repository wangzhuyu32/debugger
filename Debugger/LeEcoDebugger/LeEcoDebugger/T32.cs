using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace LeEcoDebugger
{
    public class T32
    {
        public enum CPUType
        {
            ARM = 0,
            HEXAGON = 1,
            XTENSA = 2, //wifi
        }

        private T32API t32;
        private T32API.Channel chan;
        public CPUType cpuType;
        public T32_Info_type T32_type;
        private int portNum;                         //t32 port number
        private string t32ConfigFile = string.Empty; //t32 config file path
        private string csRegRestoreMenuFile = string.Empty; // T32 crashscope menu file path
        private readonly UInt64 T32MinVersionFor64bit = 540696447824; // 2014.4.5.2896
        private readonly string T32_DEFAULT_FOLDER = @"C:\T32";
        private readonly string T32SYS = "T32SYS";
        private readonly string T32VER = "T32VER";
        private readonly string t32ExeARM = "t32marm.exe";
        private readonly string t32ExeARM64 = "t32marm64.exe";
        private readonly string t32ExeQ6 = "t32mqdsp6.exe";
        private readonly string t32ExeXtensa = "t32mxtensa.exe";
        private readonly string t32ExeUbi = "t32mubi32.exe";
        private readonly int MIN_PORT = 41234;
        private readonly int MAX_PORT = 49999;
        private readonly int SUCCESS = 0;
        private readonly int FAIL = 1;
        private readonly int COMMUNICATION_ERROR = -1;
        private bool is_multipd = false;
        private string pd_cmd = string.Empty;
        private string cmdLog = string.Empty; //log all the commands we sent to T32
        private string dblog = string.Empty;
        private Dictionary<string, RelocableType> T32_reloc;
        
        //private string msgLog = string.Empty; //log all messages received after executing the commands
        private string t32ExePath = string.Empty; //the path of T32 simulator exe

        private string t32Folder = string.Empty;  //the folder of T32
        public T32()
        {
            t32 = new T32API();
        }
        public T32(T32_Info_type info)
        {
            t32 = new T32API();
            T32_type = info;
            this.cpuType = getCPUType(info.subSystem);
        }
        public T32(T32_Info_type info, Dictionary<string, RelocableType> reloc)
        {
            t32 = new T32API();
            T32_type = info;
            T32_reloc = reloc;
            this.cpuType = getCPUType(info.subSystem);
        }
        public void loadRamDump()
        {
            int rc = SUCCESS;
            try
            {
                //generate port
                rc = genPortNum();
                if (rc != SUCCESS)
                    return;
                //find exe Path
                rc = findT32Path();
                if (rc != SUCCESS)
                    return;
                //check T32 version
                rc = checkT32Version();
                //if (rc != SUCCESS)
                   // return;
                //config
                rc = createT32ConfigFile();
                if (rc != SUCCESS)
                    return;
                //launch T32
                rc = launchT32SimExe();
                if (rc != SUCCESS)
                    return;
                //setup T32
                rc = setupT32Connection();
                if (rc != SUCCESS)
                    return;
                //setup title
                rc = setTitle();
                if (rc != SUCCESS)
                    return;
                //setup window
                rc = setupAreaWindow();
                if (rc != SUCCESS)
                    return;
                rc = setupSys();
                if (rc != SUCCESS)
                    return;
                rc = loadDumpFiles();
                if (rc != SUCCESS)
                    return;
                rc = loadElf();
                if (rc != SUCCESS)
                    return;
                rc = loadOS();
                if (rc != SUCCESS)
                    return;
                rc = configMMU();
                if (rc != SUCCESS)
                    return;
                rc = misc();
                //
                printMsg("RAM DUMP loading is complete!");
                
                //close the connection
                t32.Exit();
                MessageBox.Show("Dump loading complete!");
            }
            catch (Exception e)
            { }
       
        }

        private int genPortNum()
        {
            List<int> usedPorts = getUsedPorts();
            if (usedPorts == null)
                return FAIL;
            for (portNum = MIN_PORT; portNum < MAX_PORT; portNum++)
            {
                if (!usedPorts.Contains(portNum))
                    break;
            }
            if (portNum > MAX_PORT)
                return FAIL;
            else
                return SUCCESS;

        }

        private List<int> getUsedPorts()
        {
            List<int> usedPorts = null;
            IPGlobalProperties pro;
            IPEndPoint[] udpEndPoints;
            try
            {
                pro = IPGlobalProperties.GetIPGlobalProperties();
                udpEndPoints = pro.GetActiveUdpListeners();
                usedPorts = udpEndPoints.Select(p => p.Port).ToList<int>();
            }
            catch (Exception e)
            {
            }

            return usedPorts;
        }
        private CPUType getCPUType(string name)
        {
            string cpu = name.ToUpper();
            if (cpu.IndexOf("Modem", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return CPUType.HEXAGON;
            else
            {
                return CPUType.ARM;
            }
        }
        private int findT32Path()
        {
            //hardcode cpu type
            string t32ExeName = string.Empty;
            switch (cpuType)
            {
                case CPUType.ARM:
                    t32ExeName = t32ExeARM64;
                    break;
                case CPUType.HEXAGON:
                    t32ExeName = t32ExeQ6;
                    break;
                default:
                    t32ExeName = "NA";
                    break;
            }

            t32Folder = T32_DEFAULT_FOLDER;
            t32ExePath = Path.Combine(t32Folder, t32ExeName);
            if (Util.fileExists(t32ExePath))
                return SUCCESS;
            return FAIL;
        }

        private int checkT32Version()
        {
            string versionstr = string.Empty;
            UInt64 ver = getT32Version(t32ExePath, out versionstr);
            if (ver < T32MinVersionFor64bit)
            {
                dblog = string.Format("version is not support ARM8");
                return FAIL;
            }
            return SUCCESS;
        }

        private UInt64 getT32Version(string file, out string versionstring)
        {
            versionstring = string.Empty;
            UInt64 version = 0;
            UInt64 year = 0, month = 0, day = 0, build = 0;
            try
            {
                versionstring = FileVersionInfo.GetVersionInfo(file).FileVersion;
                Match m = Regex.Match(versionstring, @"(\d+).(\d+).(\d+).(\d+)");
                if (m.Success)
                {
                    bool ok = UInt64.TryParse(m.Groups[1].Value.ToString(), out year);
                    if (ok) ok = UInt64.TryParse(m.Groups[2].Value.ToString(), out month);
                    if (ok) ok = UInt64.TryParse(m.Groups[3].Value.ToString(), out day);
                    if (ok) ok = UInt64.TryParse(m.Groups[4].Value.ToString(), out build);
                    if (ok) return ((year & 0xFFF) << 28) + ((month & 0xF) << 24) + ((day & 0xFF) << 16) + (build & 0xFFFF);  // format : 2014.4.5.2894
                }
            }
            catch (Exception e) { }
            return version;
        }

        private int createT32ConfigFile()
        {
            string tempFolderPath = Util.GetTempFolder();
            if (string.IsNullOrEmpty(tempFolderPath))
                return FAIL;
            t32ConfigFile = Path.Combine(tempFolderPath, "Le_t32_config_" + portNum + ".t32");
            try
            {
                using (StreamWriter sw = new StreamWriter(t32ConfigFile, false))
                {
                    sw.WriteLine(@";Environment Variables");
                    sw.WriteLine(@"OS=");
                    sw.WriteLine(@"ID=T32");
                    sw.WriteLine(@"TMP=C:\TEMP");
                    sw.WriteLine(@"SYS=" + t32Folder);
                    sw.WriteLine(@"HELP=" + Path.Combine(t32Folder, "pdf"));
                    sw.WriteLine("");
                    sw.WriteLine(@";T32 API Access");
                    sw.WriteLine(@"RCL=NETASSIST");
                    sw.WriteLine(@"PORT=" + portNum);
                    sw.WriteLine(@"PACKLEN=1024");
                    sw.WriteLine("");
                    sw.WriteLine(@";Connection to Host");
                    sw.WriteLine(@"PBI=SIM");
                    sw.WriteLine("");
                    sw.WriteLine(@";Screen Settings:");
                    sw.WriteLine(@"SCREEN=");
                    sw.WriteLine(@"FONT=SMALL");
                    sw.WriteLine(@"HEADER=Trace32 SIM");
                    sw.WriteLine("");
                    sw.WriteLine(@";Printer Settings:");
                    sw.WriteLine(@"PRINTER=WINDOWS");
                }
                return SUCCESS;
            }
            catch (Exception e)
            {
                dblog += string.Format(e.ToString()) + Environment.NewLine;
                return FAIL;
            }
        }

        private int launchT32SimExe()
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.CreateNoWindow = false;
            startinfo.UseShellExecute = false;
            startinfo.RedirectStandardOutput = true;
            startinfo.FileName = t32ExePath;
            startinfo.WindowStyle = ProcessWindowStyle.Hidden;
            startinfo.Arguments = "-c" + t32ConfigFile;

            try
            {
                Process process = Process.Start(startinfo);
                Thread.Sleep(1000);
                process.WaitForInputIdle();
                process.Refresh();
                return SUCCESS;
            }

            catch (Exception e)
            {
                dblog += string.Format(e.ToString()) + Environment.NewLine;
                return FAIL;
            }

        }

        private int setupT32Connection()
        {
            int rc = SUCCESS;
            //create channel
            chan = new T32API.Channel();
            t32.GetChannelDefaults(chan);
            t32.SetChannel(chan);

            t32.Config("NODE=", "localhost");
            t32.Config("PACKLEN=", "1024");
            t32.Config("PORT=", portNum.ToString());
            //init
            rc = t32.Init();
            rc = t32.Attach((int)T32API.Device.ICD);
            if (rc != SUCCESS)
                MessageBox.Show("Failed to attach to T32 Simulator");

            return rc;
        }

        private int setTitle()
        {
            string strCMD = string.Format("TITLE \"{1} {0} {2}\"", cpuType.ToString(), T32_type.targetName, T32_type.dumpPath);
            return runCommand(strCMD);
        }

        private int runCommand(string cmd)
        {
            int rc;
            //send empty command
            rc = t32.Nop();
            if (rc != SUCCESS)
            {
                rc = reConnect(5);
                if (rc != SUCCESS)
                    return rc;
            }
            // t32 command
            rc = t32.Cmd(cmd);
            cmdLog += cmd + "\n";
            if (rc == COMMUNICATION_ERROR)
            {
                rc = reConnect(200);
            }
            return rc;
        }

        private int reConnect(int retry)
        {
            int i = 0;
            int rc = COMMUNICATION_ERROR;
            if (!isPortActive(portNum))
            {
                return rc;
            }
            while (i < retry)
            {
                t32.SetChannel(chan);
                rc = t32.Attach((int)T32API.Device.ICD);
                if (rc == SUCCESS)
                    break;
                i++;
            }

            return rc;
        }

        private bool isPortActive(int portN)
        {
            var usePorts = getUsedPorts();
            return (usePorts != null && usePorts.Contains(portN));
        }

        private int setupAreaWindow()
        {
            int rc;
            rc = runCommand("AREA.RESET");
            rc = runCommand("AREA.CREATE");
            rc = runCommand("WINPOS 0. 63% 69% 41% 0. 0. W001");
            rc = runCommand("AREA.CLEAR");
            rc = runCommand("AREA.VIEW");
            rc = runCommand("AREA.SELECT");
            return rc;
        }

        private int setupSys()
        {
            string cpuName = T32_type.CPU_name;
            if (string.IsNullOrEmpty(cpuName))
            {
                printMsg("Failed to config CPU");
                return FAIL;
            }
            int rc = runCommand("sys.CPU " + cpuName);
            if (rc != SUCCESS)
            {
                printMsg("CPU type is not support! It is related to T32 version");
                return rc;
            }

            rc = runCommand("SYS.U");
            return rc;
        }

        private void printMsg(string msg)
        {
            string str = string.Format("PRINT \"{0}\"", msg);
            runCommand(str);
        }

        private int loadDumpFiles()
        {
            int rc = SUCCESS;
            if (string.IsNullOrEmpty(T32_type.loadBinaryCommand))
            {
                MessageBox.Show("check if load.cmm is in dump file!");
                return FAIL;
            }
            printMsg("loading Binary ...");
            rc = runCommand(T32_type.loadBinaryCommand);
            
            return rc;
        }

        private int loadElf()
        {
            int rc = FAIL;
            printMsg("loading ELF ...");
            //string strCmd = string.Format("D.LOAD.ELF \"{0}\" 0x{1}++0x{2} /NOCODE /NOCLEAR", T32_type.elfPath, "D0000000", "6A00000");
            string strCmd = string.Format("D.LOAD.ELF \"{0}\"  /NOCODE /NOCLEAR", T32_type.elfPath);
            rc = runCommand(strCmd);
            return rc;
        }

        private int loadOS()
        {

            int rc = SUCCESS;
            if (T32_type.subSystem.Equals("modem")) {
                string Task = Path.GetFullPath(Path.Combine(Application.StartupPath, "config\\qurt_model.t32"));
                string menu = Path.GetFullPath(Path.Combine(Application.StartupPath, "config\\qurt_model.men"));
                if (!string.IsNullOrEmpty(Task) && !string.IsNullOrEmpty(menu))
                {
                    rc = runCommand(string.Format("TASK.CONFIG  \"{0}\"", Task));
                    rc = runCommand(string.Format("MENU.REPROGRAM \"{0}\"", menu));

                }
                else
                {
                    printMsg("Failed to load OS awareness File");
                    
                }
            }
           
                
            return rc;
        }

        private int configMMU()
        {
            string strCmd = string.Empty;
            int rc = SUCCESS;
            int addr = 0, size, accessClass;
            if (cpuType == CPUType.HEXAGON)
            {
                //8976 is using v1 pt
                rc = t32.GetSymbol("QURTK_pagetables", out addr, out size, out accessClass);
                if (rc == SUCCESS && addr != 0)
                {
                    //ceate cmm script and run it
                    uint vPTaddr = (uint)addr;
                    //get virstar
                    rc = t32.GetSymbol("start", out addr, out size, out accessClass);
                    uint vStart = (uint)addr;
                    if (rc == FAIL)
                        vStart = 0xC0000000;
                    uint Pstart = 0x86800000;
                    uint Psize = 0x6A00000;
                    //update if relocable has update
                    if (T32_reloc.ContainsKey("modem"))
                    {
                        Pstart = T32_reloc["modem"].startAddr;
                        Psize = T32_reloc["modem"].size;
                    }
                    //restor TCM
                    //DATA.COPY(qurt_tcm_dump - &VIRT_START + &MPSS_SW_start)++v.value(qurt_tcm_dump_size) d.l(QURTK_l2tcm_base)
                    strCmd = string.Format("DATA.COPY (qurt_tcm_dump-0x{0}+0x{1})++v.value(qurt_tcm_dump_size) d.l(QURTK_l2tcm_base)", vStart.ToString("X"), Pstart.ToString("X"));
                    rc = runCommand(strCmd);
                    //MMU.FORMAT QURT d.l(&ABS_QURTK_pagetables) &VIRT_START++&MPSS_SW_size &MPSS_SW_start
                    uint PPTaddr = Pstart + vPTaddr - vStart;
                    uint temp1 = (uint)PPTaddr;
                    byte[] Buffer = new byte[4];
                    rc = t32.ReadMemory((int)PPTaddr, (short)T32API.MemoryClass.D, out Buffer, 4);
                    uint rt_pt = Util.ExtractUInt32(Buffer, 0);
                    if (T32_type.targetName.Contains("8996"))
                        strCmd = string.Format("MMU.FORMAT {0} 0x{1:X} 0x{2:X}++0x{3:X} 0x{4:X}", "QURT", rt_pt, vStart, Psize, Pstart);
                    else
                        strCmd = string.Format("MMU.FORMAT {0} 0x{1:X}", "QURT", rt_pt);
                    rc = runCommand("MMU.off");
                    rc = runCommand("MMU.reset");
                    rc = runCommand(strCmd);
                    rc = runCommand("MMU.ON");
                    //if it is V1
                    rc = runCommand("MMU.SCAN ALL");
                    return rc;
                }

            }

            return rc;
        }

        private int misc()
        {
            int rc = SUCCESS;
            if (cpuType == CPUType.HEXAGON)
                rc = runCommand("FRAME.CONFIG.EPILOG OFF");
            rc = runCommand("v.v coredump");
            return rc;
        }
    } //class T32
    
}
