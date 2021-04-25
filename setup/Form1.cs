using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace setup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 1;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                RunAdmin();
                this.tabControl1.Region = new Region(new RectangleF(this.tabPage1.Left, this.tabPage1.Top, this.tabPage1.Width, this.tabPage1.Height));
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化安装程序失败");
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 2;

        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Tab | Keys.Control):
                    return true;
                default:
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox1.Text = folderBrowserDialog1.SelectedPath + @"\setup\";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
                this.tabControl1.SelectedIndex = 3;
                string time = DateTime.Now.ToLongTimeString().ToString();


                richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "初始化...";
                #region 初始化

                string 应用目录temp = textBox1.Text;
                if (Directory.Exists(应用目录temp))//检测是否有文件夹
                {

                    DelectDir(应用目录temp);//有则清空
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(应用目录temp);//无则创建
                    directoryInfo.Create();
                }
                byte[] temp = setup.Properties.Resources.Debug;
                System.IO.FileStream fileStream = new System.IO.FileStream(textBox1.Text + @"\Debug.zip", System.IO.FileMode.CreateNew);
                fileStream.Write(temp, 0, (int)(temp.Length));
                fileStream.Close();
            byte[] cer = setup.Properties.Resources.GoodBoyboy;
            System.IO.FileStream fileinfo = new System.IO.FileStream(textBox1.Text + @"GoodBoyboy.cer", System.IO.FileMode.CreateNew);
            fileinfo.Write(cer, 0, (int)(cer.Length));
            fileinfo.Close();

            #endregion

            richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "初始化完成";
                richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "开始解压...";
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(textBox1.Text + @"\Debug.zip", textBox1.Text); //解压
            }
            catch (Exception ex)
            {
                MessageBox.Show("解压文件失败,请检查是否有相关权限");

            }
            richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "解压完成";
            richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "正在创建快捷方式";
            try
            {
                快捷方式.CreateShortcutOnDesktop("湘少智能", textBox1.Text + "湘少智能.exe", "湘少智能", textBox1.Text + "湘少智能.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建快捷方式失败,请检查是否有相关权限");
            }
            richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "安装证书";
            #region 安装证书


            //安装CA的根证书到受信任根证书颁发机构
               X509Certificate2 certificate = new X509Certificate2(textBox1.Text+ "GoodBoyboy.cer");
                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(certificate);   //可省略
                store.Add(certificate);
                store.Close();
                File.Delete(textBox1.Text+@"\GoodBoyboy.cer");
                    #endregion
                    richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "安装证书完成";
                    richTextBox2.Text = richTextBox2.Text + "\r\n" + "[" + time + "]" + "安装程序完成";
                    File.Delete(textBox1.Text + @"\Debug.zip");
                    button5.Enabled = true;
                

            
            
        }

        #region 清空文件
        public static void DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        //如果 使用了 streamreader 在删除前 必须先关闭流 ，否则无法删除 sr.close();
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion

        #region 检测权限
        /// <summary>
        /// 判断程序是否是以管理员身份运行。
        /// </summary>
        public bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public void RunAdmin()
        {
            try
            {
                //判断是否以管理员身份运行，不是则提示
                if (!IsAdministrator())
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.WorkingDirectory = Environment.CurrentDirectory;
                    psi.FileName = Application.ExecutablePath;
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + "请使用管理员权限启动");
                Application.Exit();
            }
        }

        #endregion
        

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 4;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string 应用目录temp = textBox1.Text;
            if (Directory.Exists(应用目录temp))//检测是否有文件夹
            {
                label6.Visible = true;

            }
            else
            {
                label6.Visible = false;
            }
        }
    }
}
