// WallpaperManager.cs — Build:
// csc /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Management.dll /out:WallpaperManager.exe src\WallpaperManager.cs
using System;using System.Collections.Generic;using System.Diagnostics;using System.Drawing;using System.IO;using System.Linq;using System.Runtime.InteropServices;using System.Text;using System.Text.RegularExpressions;using System.Threading;using System.Windows.Forms;using Microsoft.Win32;

static class P{[STAThread]static void Main(){bool ok;var mu=new Mutex(true,"AAAA_WM",out ok);if(!ok){MessageBox.Show("Already running.","System Lock");return;}Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Mgr());mu.ReleaseMutex();}}

static class W32{
[DllImport("user32")]public static extern IntPtr FindWindow(string c,string n);
[DllImport("user32")]public static extern IntPtr FindWindowEx(IntPtr p,IntPtr c,string cn,string w);
[DllImport("user32")]public static extern bool ShowWindow(IntPtr h,int c);
[DllImport("user32")]public static extern bool EnumWindows(EnumCb l,IntPtr p);public delegate bool EnumCb(IntPtr h,IntPtr l);
[DllImport("user32")]public static extern IntPtr SendMessageTimeout(IntPtr h,uint m,IntPtr w,IntPtr l,uint f,uint t,out IntPtr r);
[DllImport("user32",SetLastError=true)]public static extern IntPtr SetParent(IntPtr c,IntPtr p);
[DllImport("user32")]public static extern bool MoveWindow(IntPtr h,int x,int y,int w,int h2,bool r);
[DllImport("user32")]public static extern int GetWindowLong(IntPtr h,int i);
[DllImport("user32")]public static extern int SetWindowLong(IntPtr h,int i,int v);
[DllImport("user32")]public static extern bool SetWindowPos(IntPtr h,IntPtr i,int x,int y,int w,int h2,uint f);
[DllImport("user32")]public static extern uint GetWindowThreadProcessId(IntPtr h,out uint p);
[DllImport("user32")]public static extern bool IsWindowVisible(IntPtr h);
[DllImport("user32")]public static extern IntPtr GetForegroundWindow();
[DllImport("user32")]public static extern bool GetWindowRect(IntPtr h,out RECT r);
[StructLayout(LayoutKind.Sequential)]public struct RECT{public int L,T,R,B;}

public static IntPtr WorkerW(){IntPtr p=FindWindow("Progman",null),r=IntPtr.Zero,w=IntPtr.Zero;SendMessageTimeout(p,0x052C,IntPtr.Zero,IntPtr.Zero,0,1000,out r);EnumWindows(delegate(IntPtr h,IntPtr _){IntPtr x=FindWindowEx(h,IntPtr.Zero,"SHELLDLL_DefView",null);if(x!=IntPtr.Zero){w=FindWindowEx(IntPtr.Zero,h,"WorkerW",null);return false;}return true;},IntPtr.Zero);return w;}
public static IntPtr FindByPid(uint pid){IntPtr found=IntPtr.Zero;EnumWindows(delegate(IntPtr h,IntPtr _){uint proc;GetWindowThreadProcessId(h,out proc);if(proc==pid&&IsWindowVisible(h)){found=h;return false;}return true;},IntPtr.Zero);return found;}
public static void Icons(bool v){IntPtr s=IntPtr.Zero,pg=FindWindow("Progman",null);s=FindWindowEx(pg,IntPtr.Zero,"SHELLDLL_DefView",null);if(s==IntPtr.Zero)EnumWindows(delegate(IntPtr h,IntPtr _){IntPtr x=FindWindowEx(h,IntPtr.Zero,"SHELLDLL_DefView",null);if(x!=IntPtr.Zero){s=x;return false;}return true;},IntPtr.Zero);IntPtr l=s!=IntPtr.Zero?FindWindowEx(s,IntPtr.Zero,"SysListView32","FolderView"):IntPtr.Zero;if(l!=IntPtr.Zero)ShowWindow(l,v?5:0);IntPtr t=FindWindow("Shell_TrayWnd",null);if(t!=IntPtr.Zero)ShowWindow(t,v?5:0);IntPtr x2=FindWindow("Shell_SecondaryTrayWnd",null);if(x2!=IntPtr.Zero)ShowWindow(x2,v?5:0);}
public static void Embed(IntPtr h,bool edge){if(edge){int s=GetWindowLong(h,-16);s&=~(0xC00000|0x800000|0x40000|0x400000|0x20000|0x10000);SetWindowLong(h,-16,s);}int e=GetWindowLong(h,-20);e|=0x80;SetWindowLong(h,-20,e);SetWindowPos(h,IntPtr.Zero,0,0,0,0,0x20|0x2|0x1|0x4|0x40);}
public static void NoTrans(IntPtr h){int e=GetWindowLong(h,-20);e&=~0x20;SetWindowLong(h,-20,e);}
public static bool IsFull(IntPtr h,int x,int y,int w,int hh){if(h==IntPtr.Zero)return false;RECT r;GetWindowRect(h,out r);return r.L<=x&&r.T<=y&&r.R>=x+w&&r.B>=y+hh;}}

class Cfg{
    public bool AutoStart,LaunchLast,Optimized;
    public List<WpEntry> Last=new List<WpEntry>();public Dictionary<string,string> Modes=new Dictionary<string,string>();
    static string F=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets\\data\\settings.json");
    public static Cfg Load(){if(!File.Exists(F))return new Cfg();try{string t=File.ReadAllText(F);Cfg c=new Cfg();c.AutoStart=Rx(t,"AutoStart")=="true";c.LaunchLast=Rx(t,"LaunchLast")=="true";c.Optimized=Rx(t,"Optimized")=="true";foreach(Match m in Regex.Matches(t,@"""Monitor""\s*:\s*(\d+).*?""Path""\s*:\s*""([^""]+)"".*?""Title""\s*:\s*""([^""]+)"".*?""Mode""\s*:\s*""([^""]+)""",RegexOptions.Singleline))c.Last.Add(new WpEntry{Monitor=int.Parse(m.Groups[1].Value),Path=m.Groups[2].Value.Replace("\\\\","\\"),Title=m.Groups[3].Value,Mode=m.Groups[4].Value});foreach(Match m in Regex.Matches(t,@"""(\[(?:APP|GRAPHIC)\][^""]+)""\s*:\s*""([^""]+)"""))c.Modes[m.Groups[1].Value]=m.Groups[2].Value;return c;}catch{return new Cfg();}}
    static string Rx(string t,string k){Match m=Regex.Match(t,"\""+k+"\"\\s*:\\s*\"?([^,\"\\}\\]]+)\"?");return m.Success?m.Groups[1].Value.Trim():"";}
    public void Save(){Directory.CreateDirectory(Path.GetDirectoryName(F));StringBuilder sb=new StringBuilder("{\n");sb.AppendFormat("  \"AutoStart\": {0},\n  \"LaunchLast\": {1},\n  \"Optimized\": {2},\n",AutoStart.ToString().ToLower(),LaunchLast.ToString().ToLower(),Optimized.ToString().ToLower());sb.Append("  \"LastWallpapers\": [");Dictionary<int,WpEntry> d=new Dictionary<int,WpEntry>();foreach(WpEntry w in Last)d[w.Monitor]=w;sb.Append(string.Join(",",d.Values.Select(w=>"\n    {\"Monitor\":"+w.Monitor+",\"Path\":\""+w.Path.Replace("\\","\\\\")+"\",\"Title\":\""+w.Title+"\",\"Mode\":\""+w.Mode+"\"}")));sb.Append("\n  ],\n  \"ModeMemory\": {");sb.Append(string.Join(",",Modes.Select(kv=>"\n    \""+kv.Key+"\": \""+kv.Value+"\"")));sb.Append("\n  }\n}");File.WriteAllText(F,sb.ToString());}}
class WpEntry{public int Monitor;public string Path,Title,Mode;}

static class Engine{
    static readonly string Base=AppDomain.CurrentDomain.BaseDirectory;
    static readonly string RendererPath=Path.Combine(Base,"tools\\wall-renderer\\bin\\wall-renderer.exe");
    static readonly string WallDir=Path.Combine(Base,"assets\\wallpapers");
    static readonly string DefHtml=Path.Combine(Base,"assets\\wallpapers\\default.html");
    public static string[] Scan(out Dictionary<string,string> map){map=new Dictionary<string,string>();if(!Directory.Exists(WallDir))return new string[0];List<string> names=new List<string>();foreach(string f in Directory.GetFiles(WallDir,"*.html",SearchOption.AllDirectories)){string cat=f.IndexOf("\\apps\\")>=0?"APP":"GRAPHIC";string name="[ "+cat+" ] / "+Path.GetFileNameWithoutExtension(f).ToUpper();names.Add(name);map[name]=f;}return names.ToArray();}
    public static void KillAll(){foreach(Process p in Process.GetProcessesByName("wall-renderer"))try{p.Kill();}catch{}}
    public static void KillMon(int mon){foreach(Process p in Process.GetProcessesByName("wall-renderer"))try{if(GetCl(p).Contains("--monitor "+mon))p.Kill();}catch{}}
    static string GetCl(Process p){try{System.Management.ManagementObjectSearcher s=new System.Management.ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId="+p.Id);foreach(System.Management.ManagementObject o in s.Get())return (o["CommandLine"]??"").ToString();return "";}catch{return "";}}
    static string GetTitle(string path){try{Match m=Regex.Match(File.ReadAllText(path),"<title>(.*?)</title>");return m.Success?m.Groups[1].Value:"Wallpaper";}catch{return "Wallpaper";}}

    public static void Launch(string path,int monitor,string mode,bool span){
        if(!File.Exists(path)){if(File.Exists(DefHtml))path=DefHtml;else return;}
        string uri=new Uri(path).AbsoluteUri;Screen[] scr=Screen.AllScreens;
        Rectangle b=span?scr.Aggregate(Rectangle.Empty,(acc,s)=>acc==Rectangle.Empty?s.Bounds:Rectangle.Union(acc,s.Bounds)):(monitor>=scr.Length?scr[0]:scr[monitor]).Bounds;
        if(mode=="Active")W32.Icons(false);else W32.Icons(true);
        if(!File.Exists(RendererPath)){MessageBox.Show("wall-renderer.exe not found.","Error");return;}ProcessStartInfo psi=new ProcessStartInfo(RendererPath,"--url \""+uri+"\" --x "+b.X+" --y "+b.Y+" --width "+b.Width+" --height "+b.Height+" --title \""+GetTitle(path)+"\" --monitor "+monitor);psi.UseShellExecute=false;psi.CreateNoWindow=true;psi.WindowStyle=ProcessWindowStyle.Hidden;psi.WorkingDirectory=Path.GetDirectoryName(RendererPath);psi.EnvironmentVariables["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"]="--limit-fps=24 --disable-features=Translate,HardwareMediaKeyHandling,MediaSessionService";Process proc=Process.Start(psi);Rectangle bb=b;bool act=mode=="Active";ThreadPool.QueueUserWorkItem(delegate{Thread.Sleep(1200);if(proc.HasExited)return;IntPtr hwnd=IntPtr.Zero;for(int i=0;i<20&&hwnd==IntPtr.Zero;i++){Thread.Sleep(500);proc.Refresh();if(proc.HasExited)return;hwnd=proc.MainWindowHandle!=IntPtr.Zero?proc.MainWindowHandle:W32.FindByPid((uint)proc.Id);}if(hwnd==IntPtr.Zero){try{proc.Kill();}catch{}return;}W32.Embed(hwnd,false);IntPtr ww=W32.WorkerW();if(ww==IntPtr.Zero)ww=W32.FindWindow("Progman",null);W32.MoveWindow(hwnd,bb.X,bb.Y,bb.Width,bb.Height,true);if(!act&&ww!=IntPtr.Zero){W32.SetParent(hwnd,ww);Thread.Sleep(150);W32.MoveWindow(hwnd,bb.X,bb.Y,bb.Width,bb.Height,true);W32.NoTrans(hwnd);}});
    }
}

static class Ui{
    public static readonly Color Bg=Color.FromArgb(18,18,18),Bg2=Color.FromArgb(26,26,26),Accent=Color.FromArgb(0,255,136),Dim=Color.FromArgb(85,85,85),Txt=Color.FromArgb(238,238,238),Blue=Color.FromArgb(0,180,255);
    public static readonly Font Sm=new Font("Segoe UI",9f),Mono=new Font("Segoe UI",8f);
    public static Button Btn(string t,Color bg,Color fg){Button b=new Button{Text=t,BackColor=bg,ForeColor=fg,FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,Font=Sm,Height=40};b.FlatAppearance.BorderSize=0;return b;}
    public static Button OutBtn(string t){Button b=Btn(t,Bg2,Color.FromArgb(255,85,85));b.FlatAppearance.BorderColor=Color.FromArgb(51,51,51);b.FlatAppearance.BorderSize=1;return b;}
    public static Label Lbl(string t,Color fg,Font f){return new Label{Text=t,ForeColor=fg,BackColor=Color.Transparent,Font=f??Mono,AutoSize=true};}
    public static ComboBox Combo(){return new ComboBox{BackColor=Bg2,ForeColor=Accent,FlatStyle=FlatStyle.Flat,Font=Sm,DropDownStyle=ComboBoxStyle.DropDownList};}
    public static CheckBox Chk(string t){return new CheckBox{Text=t,ForeColor=Txt,BackColor=Color.Transparent,Font=Sm};}
    public static ListBox LBox(){return new ListBox{BackColor=Bg2,ForeColor=Txt,BorderStyle=BorderStyle.None,Font=Sm,DrawMode=DrawMode.OwnerDrawFixed,ItemHeight=36};}}

class Mgr:Form{
    Cfg cfg;NotifyIcon tray;System.Windows.Forms.Timer poll;
    ComboBox cbFile,cbMon,cbMode,cbBrow;ListBox lbT;
    CheckBox ckA,ckL,ckO;Panel pSet;
    Dictionary<string,string> fmap=new Dictionary<string,string>();
    string sel="";int lc;DateTime ll=DateTime.MinValue;
    readonly string Base=AppDomain.CurrentDomain.BaseDirectory;

    public Mgr(){cfg=Cfg.Load();Build();Tray();Restore();Poll();}

    void Build(){
        Text="AAAAAAAAAA";Size=new Size(400,530);FormBorderStyle=FormBorderStyle.None;BackColor=Ui.Bg;StartPosition=FormStartPosition.CenterScreen;ShowInTaskbar=false;

        Panel bar=new Panel{Dock=DockStyle.Top,Height=30,BackColor=Color.FromArgb(12,12,12),Cursor=Cursors.SizeAll};
        bar.MouseDown+=delegate(object s,MouseEventArgs e){if(e.Button==MouseButtons.Left){ReleaseCapture();SendMsg(Handle,0xA1,0x2,0);}};
        Label cap=Ui.Lbl("AAAAAAAAAA MANAGER",Ui.Dim,new Font("Segoe UI",8f,FontStyle.Bold));cap.Location=new Point(14,8);bar.Controls.Add(cap);

        Button bC=TitleBtn("✕",362);bC.Click+=delegate{Hide();};bC.MouseEnter+=delegate{bC.ForeColor=Color.White;};bC.MouseLeave+=delegate{bC.ForeColor=Color.FromArgb(136,136,136);};
        Button bM=TitleBtn("—",332);bM.Click+=delegate{WindowState=FormWindowState.Minimized;};bM.MouseEnter+=delegate{bM.ForeColor=Color.White;};bM.MouseLeave+=delegate{bM.ForeColor=Color.FromArgb(136,136,136);};
        Button bSt=TitleBtn("⚙",300);bSt.Click+=delegate{pSet.Visible=true;};bSt.MouseEnter+=delegate{bSt.ForeColor=Color.White;};bSt.MouseLeave+=delegate{bSt.ForeColor=Color.FromArgb(136,136,136);};
        bar.Controls.AddRange(new Control[]{bC,bM,bSt});

        // ── Body ──
        Panel body=new Panel{Dock=DockStyle.Fill,Padding=new Padding(18,10,18,18),BackColor=Ui.Bg};

        Label badge=new Label{Text="ALPHA vA.A",BackColor=Ui.Blue,ForeColor=Color.Black,Font=new Font("Segoe UI",8f,FontStyle.Bold),AutoSize=false,Size=new Size(72,18),Location=new Point(18,42),TextAlign=ContentAlignment.MiddleCenter};
        Label appN=new Label{Text="A.A.A.A.A.A.A.A.A.A.",ForeColor=Color.White,BackColor=Color.Transparent,Font=new Font("Segoe UI",18f),AutoSize=true,Location=new Point(18,62)};

        Label lF=Ui.Lbl("SELECT WALLPAPER",Ui.Dim,Ui.Mono);lF.Location=new Point(18,106);
        Button bExp=new Button{Text="[ EXPLORE ]",FlatStyle=FlatStyle.Flat,ForeColor=Ui.Accent,BackColor=Color.Transparent,Font=Ui.Mono,AutoSize=true,Cursor=Cursors.Hand,Location=new Point(270,102)};bExp.FlatAppearance.BorderSize=0;bExp.Click+=delegate{Process.Start("explorer.exe",Path.Combine(Base,"assets\\wallpapers"));};

        cbFile=Ui.Combo();cbFile.SetBounds(18,122,362,30);
        cbFile.SelectedIndexChanged+=delegate{string n=cbFile.SelectedItem!=null?cbFile.SelectedItem.ToString():null;if(n!=null&&fmap.ContainsKey(n)){sel=fmap[n];if(cfg.Modes.ContainsKey(n))cbMode.SelectedIndex=cfg.Modes[n]=="Active"?1:0;}};

        Label lM=Ui.Lbl("MONITOR",Ui.Dim,Ui.Mono);lM.Location=new Point(18,162);
        Label lD=Ui.Lbl("BEHAVIOR",Ui.Dim,Ui.Mono);lD.Location=new Point(196,162);
        cbMon=Ui.Combo();cbMon.SetBounds(18,178,170,30);
        cbMode=Ui.Combo();cbMode.SetBounds(196,178,184,30);cbMode.Items.AddRange(new object[]{"Passive","Active (Game Mode)"});cbMode.SelectedIndex=0;
        foreach(string s in Screen.AllScreens.Select((s,i)=>"Monitor "+i+" - "+s.Bounds.Width+"x"+s.Bounds.Height+(s.Primary?" (Primary)":"")))cbMon.Items.Add(s);
        if(Screen.AllScreens.Length>1)cbMon.Items.Add("SPAN / SEAMLESS (All Monitors)");
        cbMon.SelectedIndex=0;

        Button bLaunch=Ui.Btn("▶  LAUNCH",Ui.Blue,Color.Black);bLaunch.SetBounds(18,222,220,42);
        Button bStop=Ui.OutBtn("STOP ALL");bStop.SetBounds(246,222,134,42);

        Label lT=Ui.Lbl("ACTIVE TASKS",Ui.Dim,Ui.Mono);lT.Location=new Point(18,318);
        lbT=Ui.LBox();lbT.SetBounds(18,334,362,136);lbT.DrawItem+=DrawT;
        lbT.SelectedIndexChanged+=delegate{if(lbT.SelectedItem!=null){TaskItem ti=(TaskItem)lbT.SelectedItem;try{Process.GetProcessById(ti.Pid).Kill();}catch{}lbT.SelectedIndex=-1;}};

        bLaunch.Click+=Go;bStop.Click+=delegate{Engine.KillAll();};

        body.Controls.AddRange(new Control[]{badge,appN,lF,bExp,cbFile,lM,lD,cbMon,cbMode,bLaunch,bStop,lT,lbT});

        // ── Settings overlay ──
        pSet=new Panel{Bounds=new Rectangle(0,30,400,500),BackColor=Ui.Bg,Visible=false};
        ckA=Ui.Chk("Start with Windows");ckA.Location=new Point(24,60);ckA.Width=350;
        ckL=Ui.Chk("Restore Last Wallpapers");ckL.Location=new Point(24,90);ckL.Width=350;
        ckO=Ui.Chk("Smart Performance (Hide on Fullscreen)");ckO.Location=new Point(24,120);ckO.Width=350;
        Label sPre=new Label{Text="PREFERENCES",ForeColor=Color.White,BackColor=Color.Transparent,Font=new Font("Segoe UI",14f),AutoSize=true,Location=new Point(24,16)};
        Button bBk=Ui.Btn("← BACK",Ui.Bg2,Color.FromArgb(100,100,100));bBk.FlatAppearance.BorderColor=Color.FromArgb(51,51,51);bBk.FlatAppearance.BorderSize=1;bBk.SetBounds(24,160,120,34);
        bBk.Click+=delegate{pSet.Visible=false;};
        pSet.Controls.AddRange(new Control[]{sPre,ckA,ckL,ckO,bBk});

        ApplyCfg();
        ckA.CheckedChanged+=Save;ckL.CheckedChanged+=Save;ckO.CheckedChanged+=Save;

        Controls.AddRange(new Control[]{pSet,body,bar});
        RefFiles();}

    Button TitleBtn(string t,int x){Button b=new Button{Text=t,Size=new Size(28,28),Location=new Point(x,1),FlatStyle=FlatStyle.Flat,ForeColor=Color.FromArgb(136,136,136),BackColor=Color.Transparent,Cursor=Cursors.Hand};b.FlatAppearance.BorderSize=0;return b;}

    void ApplyCfg(){ckA.Checked=cfg.AutoStart;ckL.Checked=cfg.LaunchLast;ckO.Checked=cfg.Optimized;}
    void Save(object s,EventArgs e){cfg.AutoStart=ckA.Checked;cfg.LaunchLast=ckL.Checked;cfg.Optimized=ckO.Checked;RegistryKey rk=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true);if(rk!=null){if(cfg.AutoStart)rk.SetValue("AAAAAAAAAA","\""+Application.ExecutablePath+"\"");else rk.DeleteValue("AAAAAAAAAA",false);}cfg.Save();}
    void RefFiles(){cbFile.Items.Clear();fmap.Clear();foreach(string n in Engine.Scan(out fmap))cbFile.Items.Add(n);}

    void Go(object s,EventArgs e){if(string.IsNullOrEmpty(sel))return;DateTime now=DateTime.Now;if((now-ll).TotalSeconds<2)lc++;else lc=1;ll=now;if(lc>5){MessageBox.Show("Launch throttle.","Safe");return;}bool span=cbMon.SelectedItem!=null&&cbMon.SelectedItem.ToString().Contains("SEAMLESS");int mon=span?0:cbMon.SelectedIndex;string mode=cbMode.SelectedIndex==1?"Active":"Passive";if(span)Engine.KillAll();else Engine.KillMon(mon);Thread.Sleep(300);string name=cbFile.SelectedItem!=null?cbFile.SelectedItem.ToString():null;WpEntry found=cfg.Last.FirstOrDefault(w=>w.Monitor==mon);if(found!=null){found.Path=sel;found.Mode=mode;}else cfg.Last.Add(new WpEntry{Monitor=mon,Path=sel,Title=name??"Wallpaper",Mode=mode});if(name!=null)cfg.Modes[name]=mode;cfg.Save();Engine.Launch(sel,mon,mode,span);}
    void Restore(){if(!cfg.LaunchLast||cfg.Last.Count==0)return;HashSet<int> seen=new HashSet<int>();foreach(WpEntry w in cfg.Last){if(seen.Contains(w.Monitor)||w.Monitor>=Screen.AllScreens.Length||!File.Exists(w.Path))continue;seen.Add(w.Monitor);Engine.Launch(w.Path,w.Monitor,"Passive",false);Thread.Sleep(200);}}

    void Tray(){tray=new NotifyIcon{Text="AAAAAAAAAA",Visible=true};string ico=Path.Combine(Base,"assets\\icon.png");tray.Icon=File.Exists(ico)?Icon.FromHandle(new Bitmap(ico).GetHicon()):SystemIcons.Application;ContextMenuStrip cm=new ContextMenuStrip();cm.Items.Add("Open Control Panel").Click+=delegate{Show();WindowState=FormWindowState.Normal;Activate();};cm.Items.Add(new ToolStripSeparator());cm.Items.Add("Close All Wallpapers").Click+=delegate{Engine.KillAll();};cm.Items.Add(new ToolStripSeparator());cm.Items.Add("Exit").Click+=delegate{tray.Visible=false;Engine.KillAll();Application.Exit();};tray.ContextMenuStrip=cm;tray.DoubleClick+=delegate{Show();WindowState=FormWindowState.Normal;Activate();};}

    protected override void OnFormClosing(FormClosingEventArgs e){e.Cancel=true;Hide();}

    void Poll(){poll=new System.Windows.Forms.Timer{Interval=1000};poll.Tick+=delegate{List<TaskItem> items=new List<TaskItem>();IntPtr fg=W32.GetForegroundWindow();Screen[] scr=Screen.AllScreens;foreach(Process p in Process.GetProcessesByName("wall-renderer")){try{string cl=GetCl(p);Match mm=Regex.Match(cl,@"--monitor (-?\d+)");int mon=mm.Success?int.Parse(mm.Groups[1].Value):-1;if(cfg.Optimized&&mon>=0&&mon<scr.Length){Screen sc=scr[mon];W32.ShowWindow(p.MainWindowHandle,W32.IsFull(fg,sc.Bounds.X,sc.Bounds.Y,sc.Bounds.Width,sc.Bounds.Height)?0:5);}string lbl=p.MainWindowTitle.Length>0?p.MainWindowTitle:"Live Wallpaper";items.Add(new TaskItem{D=lbl+"  |  Monitor "+mon+"  (PID "+p.Id+")",Pid=p.Id});}catch{}}BeginInvoke(new Action(delegate{lbT.Items.Clear();foreach(TaskItem ti in items)lbT.Items.Add(ti);}));};poll.Start();}

    static string GetCl(Process p){try{System.Management.ManagementObjectSearcher s=new System.Management.ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId="+p.Id);foreach(System.Management.ManagementObject o in s.Get()){object v=o["CommandLine"];return v!=null?v.ToString():"";}return "";}catch{return "";}}
    void DrawT(object s,DrawItemEventArgs e){if(e.Index<0)return;TaskItem t=(TaskItem)lbT.Items[e.Index];SolidBrush bg=new SolidBrush((e.State&DrawItemState.Selected)!=0?Color.FromArgb(26,42,58):Color.FromArgb(22,22,22));e.Graphics.FillRectangle(bg,e.Bounds);e.Graphics.DrawString(t.D,Ui.Sm,new SolidBrush(Ui.Txt),e.Bounds.X+8,e.Bounds.Y+10);if((e.State&DrawItemState.Selected)!=0)e.Graphics.DrawString("✕ click to kill",new Font("Segoe UI",8f),new SolidBrush(Color.FromArgb(255,68,68)),e.Bounds.X+8,e.Bounds.Y+22);}

    [DllImport("user32")]static extern bool ReleaseCapture();
    [DllImport("user32", EntryPoint="SendMessage")]static extern int SendMsg(IntPtr h,int m,int w,int l);}

class TaskItem{public string D;public int Pid;public override string ToString(){return D;}}
