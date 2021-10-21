using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EventDev
{
    public partial class FormEventDev : Form
    {
        public FormEventDev()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 事件处理函数——显示时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_showTIme_Click(object sender, EventArgs e)
        {
            this.label1.Text = DateTime.Now.ToString();
            this.textBox1.Text = DateTime.Now.ToString();
        }
        /// <summary>
        /// 取消注册事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_noShow_Click(object sender, EventArgs e)
        {
            this.label1.Text = "RevitAPI HelloWorld";
            this.textBox1.Text = "现在不能够更新时间了";
            this.btn_showTIme.Click -= new System.EventHandler(this.btn_showTIme_Click);
        }
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_RegAgain_Click(object sender, EventArgs e)
        {
            this.btn_showTIme.Click += new System.EventHandler(this.btn_showTIme_Click);
            this.label1.Text = "RevitAPI HelloWorld";
            this.textBox1.Text = "现在可以更新时间了";
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OK_Click(object sender, EventArgs e)
        {
            MessageBox.Show("执行成功，我要关闭啦！");
            this.Close();
        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            //DialogResult = DialogResult.Cancel;
            MessageBox.Show("执行被取消，我要关闭啦！");
            this.Close();

        }
       
    }
}
