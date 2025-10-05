using Connect4;
namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            // Create PvP button
            Button btnPvP = new Button();
            btnPvP.Text = "PvP";
            btnPvP.Size = new Size(100, 40);
            btnPvP.Location = new Point(50, 50);
            btnPvP.Click += BtnPvP_Click;
            
            // Create PvE button
            Button btnPvE = new Button();
            btnPvE.Text = "PvE";
            btnPvE.Size = new Size(100, 40);
            btnPvE.Location = new Point(200, 50);
            btnPvE.Click += BtnPvE_Click;
            
            // Add buttons to form
            this.Controls.Add(btnPvP);
            this.Controls.Add(btnPvE);
            
            // Set form properties
            this.Text = "Connect 4 modified";
            this.Size = new Size(350, 150);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void BtnPvP_Click(object sender, EventArgs e)
        {
            PvPForm pvpForm = new PvPForm();
            pvpForm.Show();
        }

        private void BtnPvE_Click(object sender, EventArgs e)
        {
            PvEForm pveForm = new PvEForm();
            pveForm.Show();
        }
    }
}
