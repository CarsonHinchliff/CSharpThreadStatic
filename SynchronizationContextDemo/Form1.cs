using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SynchronizationContextDemo
{
    public delegate void Delegate1();
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ThreadPool.QueueUserWorkItem(_ => Text = "Hello World!");//直接调用抛出跨线程异常
            //ThreadPool.QueueUserWorkItem(SetText, "abc");//work

            //以下写法不work，即在委托中访问同步上下文的方式会取到null
            //ThreadPool.QueueUserWorkItem(_ => SynchronizationContext.Current.Post(a => Text = "Hello World!" + a, "abc"));
            var context = SynchronizationContext.Current;
            ThreadPool.QueueUserWorkItem(_ => context.Post(a => Text = "Hello World!" + a, "abc"));//work

            //new Thread(new ParameterizedThreadStart(SetText)).Start("test");//work
        }

        private void SetText(object text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Delegate1(() => this.SetText(text)));//在委托中再次调用此方法，则进入到下面的else
            }
            else
            {
                this.Text = "Hello World!" + text;
            }
        }
    }
}
