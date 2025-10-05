using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Connect4
{
    public partial class PvEForm : Form
    {
        public PvEForm()
        {
            InitializeComponent();
            this.Text = "Connect 4 - Player vs Computer";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
