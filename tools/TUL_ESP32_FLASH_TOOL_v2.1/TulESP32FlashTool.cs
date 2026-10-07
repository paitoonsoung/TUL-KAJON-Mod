using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TulESP32FlashTool
{
    static class Program
    {
        [STAThread] static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    sealed class MainForm : Form
    {
        ComboBox port=new ComboBox(), baud=new ComboBox(), size=new ComboBox();
        TextBox addr=new TextBox(), backup=new TextBox(), bin=new TextBox(), verify=new TextBox(), log=new TextBox();
        Label chip=new Label(), flash=new Label(), mac=new Label(), status=new Label();
        ProgressBar bar=new ProgressBar(); CheckBox safe=new CheckBox();
        string esptool="", detectedChip="", detectedMac=""; bool busy;

        public MainForm()
        {
            Text="TUL ESP32 FLASH TOOL v2.1.1"; StartPosition=FormStartPosition.CenterScreen;
            ClientSize=new Size(900,700); Font=new Font("Segoe UI",9);
            Label("TUL ESP32 FLASH TOOL v2.1.1",18,14,400,true);
            Label("COM Port",18,50); port.Location=new Point(90,46); port.Size=new Size(110,24); port.DropDownStyle=ComboBoxStyle.DropDownList; Controls.Add(port);
            Button("Refresh",210,45,75,()=>RefreshPorts());
            Label("Baud",300,50); baud.Location=new Point(345,46); baud.Size=new Size(100,24); baud.DropDownStyle=ComboBoxStyle.DropDownList; baud.Items.AddRange(new object[]{"115200","460800","921600"}); baud.SelectedIndex=0; Controls.Add(baud);
            Button("CHIP INFO",460,45,110,()=>Run(new[]{"chip-id"},"Reading chip..."));
            Button("FLASH ID",580,45,110,()=>Run(new[]{"flash-id"},"Reading flash..."));

            GroupBox d=new GroupBox{Text="Device",Location=new Point(18,80),Size=new Size(864,70)};
            chip=Info(d,"Chip: -",15,22,260); flash=Info(d,"Flash: -",285,22,220); mac=Info(d,"MAC: -",515,22,320); Controls.Add(d);

            GroupBox r=new GroupBox{Text="BACKUP / READ FLASH",Location=new Point(18,160),Size=new Size(864,125)};
            Label(r,"Address",15,28); addr.Text="0x00000000"; addr.Location=new Point(78,24); addr.Size=new Size(125,24); r.Controls.Add(addr);
            Label(r,"Size",220,28); size.Location=new Point(260,24); size.Size=new Size(170,24); size.DropDownStyle=ComboBoxStyle.DropDownList; size.Items.AddRange(new object[]{"64 KB","1 MB","4 MB","8 MB","16 MB","ALL (full flash)"}); size.SelectedIndex=5; r.Controls.Add(size);
            Label(r,"Save",15,64); backup.Text="Original_Flash.bin"; backup.Location=new Point(78,60); backup.Size=new Size(590,24); r.Controls.Add(backup);
            Button(r,"Browse...",680,58,100,()=>SaveBackup()); Button(r,"READ / BACKUP",320,92,180,()=>ReadFlash()); Controls.Add(r);

            GroupBox w=new GroupBox{Text="WRITE / RESTORE",Location=new Point(18,295),Size=new Size(864,145)};
            Label(w,"BIN",15,28); bin.Location=new Point(78,24); bin.Size=new Size(590,24); w.Controls.Add(bin);
            Button(w,"Browse...",680,22,100,()=>OpenBin());
            safe.Text="AUTO BACKUP before WRITE / ERASE"; safe.Checked=true; safe.Location=new Point(78,55); safe.AutoSize=true; w.Controls.Add(safe);
            Label(w,"Address",15,88); TextBox wa=new TextBox{Text="0x00000000",Location=new Point(78,84),Size=new Size(125,24)}; w.Controls.Add(wa);
            Button(w,"WRITE / RESTORE",230,81,180,()=>WriteFlash(wa.Text)); Button(w,"ERASE FULL FLASH",425,81,160,()=>EraseFlash());
            Label(w,"Full dump = 0x00000000; normal app BIN commonly = 0x00010000.",590,88,245); Controls.Add(w);

            GroupBox v=new GroupBox{Text="VERIFY",Location=new Point(18,450),Size=new Size(864,62)};
            verify.Location=new Point(15,24); verify.Size=new Size(590,24); v.Controls.Add(verify);
            Button(v,"Browse...",620,22,100,()=>OpenVerify()); Button(v,"VERIFY BIN",730,22,105,()=>VerifyFlash()); Controls.Add(v);

            status.Text="Ready."; status.Location=new Point(18,520); status.Size=new Size(864,22); Controls.Add(status);
            bar.Location=new Point(18,548); bar.Size=new Size(864,18); Controls.Add(bar);
            log.Multiline=true; log.ReadOnly=true; log.ScrollBars=ScrollBars.Vertical; log.Font=new Font(FontFamily.GenericMonospace,8.5f); log.Location=new Point(18,575); log.Size=new Size(864,105); Controls.Add(log);
            RefreshPorts(); esptool=FindEsptool();
            if(esptool=="") Log("esptool.exe not found. Install Espressif ESP32 board support in Arduino IDE.");
        }

        void Label(string t,int x,int y,int w=0,bool bold=false){var l=new Label{Text=t,Location=new Point(x,y),AutoSize=w==0,Width=w,Font=bold?new Font(Font,FontStyle.Bold):Font};Controls.Add(l);}
        void Label(Control p,string t,int x,int y,int w=0){var l=new Label{Text=t,Location=new Point(x,y),AutoSize=w==0,Width=w};p.Controls.Add(l);}
        Label Info(Control p,string t,int x,int y,int w){var l=new Label{Text=t,Location=new Point(x,y),Width=w,AutoEllipsis=true};p.Controls.Add(l);return l;}
        void Button(string t,int x,int y,int w,Action a){var b=new Button{Text=t,Location=new Point(x,y),Size=new Size(w,28)};b.Click+=(s,e)=>a();Controls.Add(b);}
        void Button(Control p,string t,int x,int y,int w,Action a){var b=new Button{Text=t,Location=new Point(x,y),Size=new Size(w,28)};b.Click+=(s,e)=>a();p.Controls.Add(b);}
        void RefreshPorts(){string old=port.Text;port.Items.Clear();port.Items.AddRange(SerialPort.GetPortNames());if(port.Items.Count>0)port.SelectedItem=port.Items.Contains(old)?old:port.Items[0];}
        string FindEsptool(){
            string local=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"esptool.exe"); if(File.Exists(local))return local;
            string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Arduino15","packages","esp32","tools","esptool_py");
            if(Directory.Exists(root)){var v=Directory.GetDirectories(root);Array.Sort(v,StringComparer.OrdinalIgnoreCase);for(int i=v.Length-1;i>=0;i--){string p=Path.Combine(v[i],"esptool.exe");if(File.Exists(p))return p;}}
            return "";
        }
        bool Ready(){if(port.SelectedItem==null){MessageBox.Show("Select a COM port.");return false;}if(esptool==""||!File.Exists(esptool)){MessageBox.Show("esptool.exe not found. Install ESP32 board support in Arduino IDE.");return false;}return true;}
        void SaveBackup(){using(var d=new SaveFileDialog{Filter="Binary files|*.bin",DefaultExt="bin",FileName=backup.Text})if(d.ShowDialog()==DialogResult.OK)backup.Text=d.FileName;}
        void OpenBin(){using(var d=new OpenFileDialog{Filter="Binary files|*.bin|All files|*.*"})if(d.ShowDialog()==DialogResult.OK){bin.Text=d.FileName;long n=new FileInfo(bin.Text).Length;if(n==0x400000||n==0x800000||n==0x1000000)MessageBox.Show("Full-flash-sized BIN detected. Restore at 0x00000000.");}}
        void OpenVerify(){using(var d=new OpenFileDialog{Filter="Binary files|*.bin|All files|*.*"})if(d.ShowDialog()==DialogResult.OK)verify.Text=d.FileName;}
        string SafetyName(){string dir=Path.GetDirectoryName(Path.GetFullPath(string.IsNullOrWhiteSpace(bin.Text)?backup.Text:bin.Text));if(string.IsNullOrEmpty(dir))dir=AppDomain.CurrentDomain.BaseDirectory;string c=Regex.Replace(string.IsNullOrEmpty(detectedChip)?"ESP32":detectedChip,"[^A-Za-z0-9_-]","_");string m=string.IsNullOrEmpty(detectedMac)?"UNKNOWN":detectedMac.Replace(':','-');return Path.Combine(dir,"SAFETY_BACKUP_"+c+"_"+m+"_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".bin");}
        string Norm(string s){s=s.Trim();long n=s.StartsWith("0x",StringComparison.OrdinalIgnoreCase)?long.Parse(s.Substring(2),NumberStyles.HexNumber,CultureInfo.InvariantCulture):long.Parse(s,CultureInfo.InvariantCulture);return "0x"+n.ToString("X",CultureInfo.InvariantCulture);}
        async void ReadFlash(){if(!Ready())return;if(string.IsNullOrWhiteSpace(backup.Text)){SaveBackup();if(string.IsNullOrWhiteSpace(backup.Text))return;}string[] sizes={"0x10000","0x100000","0x400000","0x800000","0x1000000","ALL"};await Run(new[]{"read-flash",Norm(addr.Text),sizes[size.SelectedIndex],Path.GetFullPath(backup.Text)},"Reading flash...");}
        async void WriteFlash(string a){
            if(!Ready()||!File.Exists(bin.Text)){MessageBox.Show("Select a valid BIN file.");return;}string address;try{address=Norm(a);}catch{MessageBox.Show("Invalid flash address.");return;}
            long n=new FileInfo(bin.Text).Length;bool full=n==0x400000||n==0x800000||n==0x1000000;if(full&&address!="0x0"){MessageBox.Show("Full-flash BIN must be written at 0x00000000.");return;}
            if(MessageBox.Show("WRITE will change flash contents. Continue?","Warning",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
            if(safe.Checked){string b=SafetyName();if(!await Run(new[]{"read-flash","0x0","ALL",b},"Safety backup before WRITE...")){MessageBox.Show("Safety backup failed. WRITE cancelled.");return;}Log("Safety backup: "+b);}
            await Run(new[]{"write-flash",address,Path.GetFullPath(bin.Text)},"Writing flash...");
        }
        async void EraseFlash(){
            if(!Ready())return;if(MessageBox.Show("ERASE FULL FLASH deletes the entire SPI flash. Continue?","Warning",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
            if(safe.Checked){string b=SafetyName();if(!await Run(new[]{"read-flash","0x0","ALL",b},"Safety backup before ERASE...")){MessageBox.Show("Safety backup failed. ERASE cancelled.");return;}}
            await Run(new[]{"erase-flash"},"Erasing flash...");
        }
        async void VerifyFlash(){if(!Ready()||!File.Exists(verify.Text)){MessageBox.Show("Select a BIN to verify.");return;}try{await Run(new[]{"verify-flash",Norm(addr.Text),Path.GetFullPath(verify.Text)},"Verifying...");}catch(Exception e){MessageBox.Show(e.Message);}}
        async Task<bool> Run(string[] args,string text){
            if(busy)return false;busy=true;status.Text=text;bar.Style=ProgressBarStyle.Marquee;Toggle(false);int code=-1;
            try{code=await Task.Run(()=>Exec(args));}catch(Exception e){Log("ERROR: "+e);}finally{busy=false;bar.Style=ProgressBarStyle.Blocks;Toggle(true);}
            status.Text=code==0?"Completed successfully.":"FAILED — esptool exit code "+code;return code==0;
        }
        int Exec(string[] args){
            var a=new StringBuilder("--port \"").Append(port.Text).Append("\" --baud ").Append(baud.Text).Append(" ");
            foreach(var x in args)a.Append(Q(x)).Append(' ');
            var psi=new ProcessStartInfo{FileName=esptool,Arguments=a.ToString(),UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8};
            using(var p=new Process{StartInfo=psi}){p.OutputDataReceived+=(s,e)=>{if(!string.IsNullOrEmpty(e.Data))Parse(e.Data);};p.ErrorDataReceived+=(s,e)=>{if(!string.IsNullOrEmpty(e.Data))Parse(e.Data);};p.Start();p.BeginOutputReadLine();p.BeginErrorReadLine();p.WaitForExit();return p.ExitCode;}
        }
        static string Q(string s){return "\""+s.Replace("\\","\\\\").Replace("\"","\\\"")+"\"";}
        void Parse(string s){Log(s);var m=Regex.Match(s,@"(\d+)%");if(m.Success)SetBar(int.Parse(m.Groups[1].Value));m=Regex.Match(s,@"Chip type:\s*(.+)",RegexOptions.IgnoreCase);if(m.Success){detectedChip=m.Groups[1].Value.Trim();Set(chip,"Chip: "+detectedChip);}m=Regex.Match(s,@"Detected flash size:\s*(.+)",RegexOptions.IgnoreCase);if(m.Success)Set(flash,"Flash: "+m.Groups[1].Value.Trim());m=Regex.Match(s,@"MAC:\s*([0-9A-Fa-f:]{17})");if(m.Success){detectedMac=m.Groups[1].Value;Set(mac,"MAC: "+detectedMac);}}
        void Log(string s){if(InvokeRequired){BeginInvoke(new Action<string>(Log),s);return;}log.AppendText(s+Environment.NewLine);}
        void SetBar(int n){if(InvokeRequired){BeginInvoke(new Action<int>(SetBar),n);return;}bar.Value=Math.Max(0,Math.Min(100,n));}
        void Set(Control c,string s){if(InvokeRequired){BeginInvoke(new Action<Control,string>(Set),c,s);return;}c.Text=s;}
        void Toggle(bool e){if(InvokeRequired){BeginInvoke(new Action<bool>(Toggle),e);return;}foreach(Control c in Controls)c.Enabled=e;}
    }
}