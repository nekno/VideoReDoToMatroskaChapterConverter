using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoReDo_to_Matroska_Chapter_Converter
{
    public partial class frmError : Form
    {
        public frmError()
        {
            InitializeComponent();
        }

        public void ShowDialog(string text)
        {
            txtError.Text = text;
            this.ShowDialog();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
