using Spire.Pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace GetImg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//为了能够跨线程调用控件
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listPdfDir.Count > 0 && textBox4.Text != "")
            {
                foreach (string strPath in listPdfDir)
                {
                    Directory.CreateDirectory(textBox4.Text.Trim(new char[] { '\\' }) + "\\" + Path.GetFileNameWithoutExtension(strPath));
                }
                progressBar1.Value = 0;//设置进度条初始值为0
                progressBar1.Maximum = listPdfDir.Count;//设置进度条最大值为要操作文件的个数
                System.Threading.ThreadPool.QueueUserWorkItem(//使用线程池
                     (P_temp) =>
                     {
                         button8.Enabled = false;
                         foreach (string strPath in listPdf)
                         {
                             PdfDocument doc = new PdfDocument();
                             doc.LoadFromFile(strPath);

                             #region 使用字典存储所有图片及对应页码
                             Dictionary<Image, string> images = new Dictionary<Image, string>();
                             for (int i = 0; i < doc.Pages.Count; i++)
                             {
                                 if (doc.Pages[i].ExtractImages() != null)
                                 {
                                     foreach (Image image in doc.Pages[i].ExtractImages())
                                     {
                                         images.Add(image, (i + 1).ToString("000"));
                                     }
                                 }
                             }
                             #endregion
                             doc.Close();

                             int index = 1;//图片编号
                             string page, tempPage = "1";//当前页码/上一页页码，为了图片重新编号
                             Image tempImage;//当前图片
                             string imageFileName, imageID;//要保存的图片文件名/图片编号
                             #region 遍历字典中存储的所有图片及对应页码，并保存为图片
                             foreach (var image in images)
                             {
                                 page = image.Value;
                                 tempImage = image.Key;
                                 if (page != tempPage)
                                     index = 1;
                                 imageID = index++.ToString("000");
                                 imageFileName = String.Format(textBox4.Text.Trim(new char[] { '\\' }) + "\\" + Path.GetFileNameWithoutExtension(strPath) + "\\" + "第" + page + "页-{0}.png", imageID);
                                 tempImage.Save(imageFileName, ImageFormat.Png);
                                 tempImage.Dispose();
                                 tempPage = page;
                             }
                             #endregion
                             progressBar1.Value += 1;//设置进度条的值加1
                         }
                         //成功提示
                         MessageBox.Show("提取完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                         button8.Enabled = true;
                     });
            }
            else
            {
                MessageBox.Show("请选择PDF文档路径！", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                button5.Focus();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(textBox4.Text);//打开重命名后的路径进行查看
        }

        ArrayList listPdfDir = new ArrayList(), listPdf = new ArrayList();

        private void button6_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir = new FolderBrowserDialog();//创建浏览文件对话框
            if (dir.ShowDialog() == DialogResult.OK)//判断是否选择了路径
            {
                textBox4.Text = dir.SelectedPath;//显示选择的路径
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listPdfDir.Clear();
            FolderBrowserDialog dir = new FolderBrowserDialog();//创建浏览文件对话框
            if (dir.ShowDialog() == DialogResult.OK)//判断是否选择了路径
            {
                textBox3.Text = dir.SelectedPath;//显示选择的路径
                //存储所有文件夹路径
                foreach (string str in Directory.GetFiles(textBox3.Text, "*.pdf"))
                {
                    listPdfDir.Add(Path.GetFileNameWithoutExtension(str));
                    listPdf.Add(str);
                }
            }
        }
    }
}
