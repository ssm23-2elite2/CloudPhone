using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudPhoneTestServer
{
    public partial class CloudPhoneForm : Form
    {
        public CloudPhoneForm()
        {
            InitializeComponent();
            this.listLog.DrawMode = DrawMode.OwnerDrawFixed;
          
            statusBar.Text = "Hello, Jake";
        }

        private void btn_serverOn_Click(object sender, EventArgs e)
        {
            logi("서버를 켭니다. CloudPhoneTestServer.btn_serverOn_Click");
            logw("서버를 켭니다. CloudPhoneTestServer.btn_serverOn_Click");
            loge("서버를 켭니다. CloudPhoneTestServer.btn_serverOn_Click");
            logi("서버를 켭니다. CloudPhoneTestServer.btn_serverOn_Click");
            logi("서버를 켭니다. CloudPhoneTestServer.btn_serverOn_Click");

        }

        private void btn_serverOff_Click(object sender, EventArgs e)
        {
            logi("서버를 끕니다. CloudPhoneTestServer.btn_serverOff_Click");
        }


        private void logend()
        {
            listLog.TopIndex = Math.Max(listLog.Items.Count - listLog.ClientSize.Height / listLog.ItemHeight + 1, 0);
            listLog.SelectedIndex = listLog.Items.Count - 1;
        }

        private void logi(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Green, "정보 : " + date + msg));
            statusBar.Text = "정보 : " + date + msg; 
            logend();
        }

        private void logw(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Orange, "경고 : " + date + msg));
            statusBar.Text = "경고 : " + date + msg;
            logend();
        }

        private void loge(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Red, "에러 : " + date + msg));
            statusBar.Text = "에러 : " + date + msg; 
            logend();
        }

        private void listLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool isItemSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
            ListBoxItem item = listLog.Items[e.Index] as ListBoxItem; // Get the current item and cast it to MyListBoxItem
            if (item != null)
            {
                // Background Color
                SolidBrush backgroundColorBrush = new SolidBrush((isItemSelected) ? item.ItemColor : Color.White);
                e.Graphics.FillRectangle(backgroundColorBrush, e.Bounds);

                SolidBrush itemTextColorBrush = (isItemSelected) ? new SolidBrush(Color.White) : new SolidBrush(item.ItemColor);
                e.Graphics.DrawString(item.Message, e.Font, itemTextColorBrush, listLog.GetItemRectangle(e.Index).Location);
            }
            else
            {
                // The item isn't a MyListBoxItem, do something about it
            }
        }

        private void CloudPhoneForm_Load(object sender, EventArgs e)
        {

        }
    }

    public class ListBoxItem
    {
        public ListBoxItem(Color c, string m)
        {
            ItemColor = c;
            Message = m;
        }
        public Color ItemColor { get; set; }
        public string Message { get; set; }
    }
}
