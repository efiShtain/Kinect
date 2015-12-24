using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhysicsSimulator
{

    public partial class Form1 : Form
    {
        Moveable _p;
        Timer _t;
        public Form1()
        {
            InitializeComponent();
            _p = new Moveable(0, 0, 0, 1, 2, 0.3f, 0,3f, 0);
            _p.Init();
            _t = new Timer();
            _t.Tick += _t_Tick;
            _t.Interval = 20;
        }

        private void _t_Tick(object sender, EventArgs e)
        {
            var res = _p.GetNextPosition();
            chart1.Series["x"].Points.AddXY(res.X, res.Y);        
                //chart1.Series["x"].Points.AddY(res.X);
            //chart1.Series["y"].Points.AddY(res.Y);
            //chart1.Series["z"].Points.AddY(res.Z);
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() =>
                {
                    textBox1.AppendText(res.ToString() + Environment.NewLine);
                }));
            }
            else
            {
                textBox1.AppendText(res.ToString() + Environment.NewLine);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _t.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _t.Stop();
        }
    }
}
