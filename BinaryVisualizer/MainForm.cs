using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BinaryVisualizer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void OpenFileButton_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "開くファイルを選択してください",
                RestoreDirectory = true
            };
            
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileTextBox.Text = fileDialog.FileName;
                await OpenFileAsBinary(fileDialog.FileName);
            }
        }

        private async Task OpenFileAsBinary(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                int cnt = 0;
                foreach (var b in buffer)
                {
                    if (cnt++ % 16 == 0)
                    {
                        richTextBox1.Text += "\n";
                        richTextBox2.Text += "\n";
                    }
                    var hex = Convert.ToString(b, 16);
                    if (hex.Length == 1) hex = $"0{hex}";
                    richTextBox1.Text += $"{hex} ";
                    richTextBox2.Text += $" {(char)b} ";
                }
            }
        }
    }
}
