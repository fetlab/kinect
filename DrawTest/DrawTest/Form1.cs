using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Intrinsic
 * 
 * Values from calibration.xml file
 * 
 * | fx  0   c_x |
 * | 0   fy  c_y |
 * | 0   0    1  |
 */

namespace DrawTest
{
    public partial class canvas : Form
    {
        public canvas()
        {
            InitializeComponent();
        }

        double f_x ;
        double f_y ;
        double c_x ;
        double c_y ;
        

        int x1 = 0;
        int x2 = 0;
        int y1 = 0;
        int y2 = 0;
        //int count = 0;
        bool flag = false;


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            x1 = e.X;
            y1 = e.Y;

            //flag = true;
        }

        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            x2 = e.X;
            y2 = e.Y;
            flag = true;
        }
        

        int width;
        int height;
        
        
        /*
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            x1 = e.X;
            y1 = e.Y;
            flag = true;
        }
        */
        
       

        private void DrawShape_Click(object sender, EventArgs e)
        {
            Pen p = new Pen(Color.Red);
            Graphics g = this.CreateGraphics();
            if (flag)
            {
                width = Math.Abs(x1 - x2);
                height = Math.Abs(y1 - y2);
                //Console.WriteLine(width+" "+height);
                //Console.WriteLine(x1 + " " + x2 + " " + y1 + " " + y2);

                int[] hor = new int[width + 1];
                int horcount = 0;
                int[] ver = new int[height + 1];
                int vercount = 0;
                for (int XW = (x1 < x2 ? x1 : x2); XW <= (x1 > x2 ? x1 : x2); XW++)
                {
                    hor[horcount] = XW;
                    horcount++;
                }

                for (int YW = (y1 < y2 ? y1 : y2); YW <= (y1 > y2 ? y1 : y2); YW++)
                {
                    ver[vercount] = YW;
                    vercount++;
                }

                _3dWorld[] worldCoords = new _3dWorld[(hor.Length * 2) + (ver.Length * 2)];

                double x_world = 0;
                double y_world = 0;
                double z_world = 0;

                int count = 0;

                while(count < worldCoords.Length)
                {
                    int temp1;
                    int temp2;
                    for (int i = 0; i < hor.Length; i++)
                    {
                        temp1 = ver[0];
                        temp2 = ver[ver.Length - 1];

                        x_world = (hor[i] - c_x) * z_world/f_x;
                        y_world = (temp1 - c_y) * z_world/f_y;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                        y_world = (temp2 - c_y) * z_world / f_y;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                    }

                    for (int i = 0; i < ver.Length; i++)
                    {
                        temp1 = hor[0];
                        temp2 = hor[hor.Length - 1];

                        x_world = (temp1 - c_x) * z_world / f_x;
                        y_world = (ver[i] - c_y) * z_world / f_y;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                        x_world = (temp2 - c_x) * z_world / f_x;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                    }
                }

                    g.DrawRectangle(p, x1 < x2 ? x1 : x2, y1 < y2 ? y1 : y2, width, height);


            }
        }
    }
}
