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
        public Form1()
        {
            InitializeComponent();
        }
        Timer t;
        Point init;
        long _startTime;
        long _timer;
        int _radius;
        private void Form1_Load(object sender, EventArgs e)
        {
            t = new Timer();
            t.Tick += t_Tick;
            t.Interval = 20;
            t.Start();
            init = new Point(cb.Top, cb.Left);
            _startTime = DateTime.Now.Ticks;
            _timer = 0;
            _radius = 50;
        }

        private int positionX()
        {
            var res = (int)(init.X + 0.1 * (_timer));
            return res;
        }
        private int positionY()
        {
            var res = (int)(init.Y + 1.8 * (_timer));
            return res;
        }
        
        //X^2+Y^2=r
        /*
         * x=sqrt(r-y^2)
         * 
         */
        private int circX()
        {
            var res = (int)(Math.Sqrt(_radius - Math.Pow(cb.Top, 2)));
            return res;
        }
        void t_Tick(object sender, EventArgs e)
        {
            _timer++;
            cb.Top =positionX();
            cb.Left =positionY();
        }


    }
}
