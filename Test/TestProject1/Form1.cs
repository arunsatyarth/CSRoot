using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestProject1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listView1.Items.Add("Sussex");
            listView1.Items.Add("Oslo");
            listView1.Items.Add("Durban");
            listView1.Items.Add("Melbourne");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text="Button was clicked";
        }
    }
}
