using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace InstantScreenCapture
{
    public partial class Form1 : Form
    {
        Hooker hooker = new Hooker();

        public string GetRootDir()
        {
            string document = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rootDir = Path.Combine(document, "InstantScreenCpture");

            if (Directory.Exists(rootDir) == false)
            {
                Directory.CreateDirectory(rootDir);
            }

            return rootDir;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hooker.OnKeyHook = OnKeyHook;
            checkBox1_CheckedChanged(null, null);
        }
        
        private void openRootDirButton_Click(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                hooker.Hook();
            }
            else
            {
                hooker.Unhook();
            }
        }

        bool OnKeyHook(int code, WM wParam, KBDLLHOOKSTRUCT lParam, Hooker hooker)
        {
            if (lParam.vkCode ==  44 && wParam == WM.KEYUP)
            {
                takeScreenshot();
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            takeScreenshot();
        }

        string getScreenshotSaveDirectoryPath(bool createIfNotExits = false)
        {
            string root = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dirName = "InstantScreenCapture";
            string dir = System.IO.Path.Combine(root, dirName);

            if (createIfNotExits)
            {
                if (System.IO.Directory.Exists(dir) == false)
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
            }

            return dir;
        }

        string getScreenshotSaveFilepath()
        {
            string dir = getScreenshotSaveDirectoryPath(true);

            var now = DateTime.Now;
            string filename = string.Format(
                "{0:0000}-{1:00}-{2:00}-{3:00}-{4:00}-{5:00}.png",
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                now.Minute,
                now.Second);

            string filepath = System.IO.Path.Combine(dir, filename);

            return filepath;
        }

        bool isCapturing_ = false;

        void takeScreenshot()
        {
            if (isCapturing_)
            {
                return;
            }

            isCapturing_ = true;

            var screenshot = TakeScreenshotInteractive(this, false);
            if (screenshot == null)
            {
                isCapturing_ = false;
                return;
            }

            // クリップボードに画像をコピー
            var clipboardImage = Clipboard.GetImage();
            if (clipboardImage is Bitmap)
            {
                clipboardImage.Dispose();
            }
            Clipboard.SetImage(screenshot);

            // 特定のフォルダ内に、画像をpngで保存する
            screenshot.Save(getScreenshotSaveFilepath());

            // canvasにも画像を表示
            if (canvas.Image != null)
            {
                canvas.Image.Dispose();
            }
            canvas.Image = CreateThumbnail(screenshot, canvas.Width, canvas.Height);
            canvas.Invalidate();
            isCapturing_ = false;
        }

        public static Bitmap TakeScreenshotInteractive(Form owner, bool hideOwnerForm = true)
        {
            using (var form = new TakeScreenshotForm())
            {
                return form.takeScreenshot(owner, hideOwnerForm);
            }
        }

        bool closeForce_ = false;

        private void exitFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // プロセスを完全に終了させる
            closeForce_ = true;
            Close();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closeForce_)
            {
                // closeForce_がtrueなら普通にフォームを閉じる
                return;
            }
            else
            {
                // さもなくば、ただフォームを隠すだけ（タスクバーに常駐させつづける）
                if (notifyIcon.Visible)
                {
                    e.Cancel = true;
                    Hide();
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // タスクバーからアイコンを消す
            notifyIcon.Visible = false;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            Activate();
        }

        private void openSaveDirectoryButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(getScreenshotSaveDirectoryPath(true));
        }

        private void openSaveDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(getScreenshotSaveDirectoryPath(true));
        }

        // サムネイルの作成
        public Bitmap CreateThumbnail(Bitmap bmp, int w, int h)
        {
            Bitmap thumbnail = new Bitmap(w, h, bmp.PixelFormat);
            using (Graphics g = Graphics.FromImage(thumbnail))
            {
                g.Clear(Color.White);
                SizeF size = GetFittingSize(bmp, w, h);
                g.DrawImage(bmp, new Rectangle(0, 0, (int)(size.Width), (int)(size.Height)));
            }
            return thumbnail;
        }

        SizeF GetFittingSize(Bitmap bmp, int w, int h)
        {
            float ratio = Math.Min((float)w / bmp.Width, (float)h / bmp.Height);
            float ww = bmp.Width * ratio;
            float hh = bmp.Height * ratio;
            return new SizeF(ww, hh);
        }
    }
}
