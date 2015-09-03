using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;

using ReadMe;
/* 
 * Intrinsic matrix for the depth camera
 * 
 * (The values for intrinsic matrices for depth camera and projector have been hardcoded because
 *  they never change)
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
        KinectSensor _sensor;           // Kinect sensor instant
        MultiSourceFrameReader _reader;     // Reader for multi source sensors
        UInt16[] depthframe = new UInt16[512 * 424];    

       

        public canvas()
        {
            InitializeComponent();
        }

        double f_x = 366.09138096351234;
        double f_y = 366.11949696912916;
        double c_x = 258.40131496255316;
        double c_y = 214.52516849276577;
        

        int x1 = 0;
        int x2 = 0;
        int y1 = 0;
        int y2 = 0;
        //int count = 0;
        bool flag = false;


        private void canvas_Load(object sender, EventArgs e)
        {

            _sensor = KinectSensor.GetDefault();
            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

        }

        private void Reader_MultiSourceFrameArrived(object sender, Microsoft.Kinect.MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();                  
            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                // It will accept color frame also, so if Mode is color it will work on the color frame. 
                if (frame != null)
                {

                   // if (_mode == CameraMode.Color)
                   // {
                   //       IMAGESORCE = frame.ToBitmap();
                  //  }
                }
            }
            var col_frame = reference.ColorFrameReference.AcquireFrame();
            
            CameraSpacePoint[] camerapoints = null;
            FrameDescription colorFrameDes = col_frame.FrameDescription;
            int colorWidth = colorFrameDes.Width;
            int colorHeight = colorFrameDes.Height;
            camerapoints = new CameraSpacePoint[colorWidth * colorHeight];
            

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                        FrameDescription depth_Frame_Des = frame.FrameDescription;
                        int depth_height = depth_Frame_Des.Height;
                        int depth_width = depth_Frame_Des.Width;
                        // depthdata variable which contains 512*424 values of depthdata. 
                        UInt16[] depthdata = new UInt16[depth_height * depth_width];
                        frame.CopyFrameDataToArray(depthdata);

                        // If wants to display some depth values. 
                        //for (int i = 1; i < 100; i++)
                        //{
                        //    Console.WriteLine(depthdata[i]);

                        //}
                        Console.Write("In the depth control.");
                        BitmapSource bmpsource  = ToBitmap(frame);
                        Bitmap bti = BitmapFromSource(bmpsource);
                        System.Drawing.Image img = bti;
                        pictureBox1.Image = img;
          
                    }
                }
          
        }
        
        

        // This part of the code stores the boundaries of the rectangle to be drawn on the image
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            x1 = e.X;
            y1 = e.Y;

            //flag = true;
        }



        private System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        private  BitmapSource ToBitmap(DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] depthData = new ushort[width * height];
            byte[] pixelData = new byte[width * height * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(depthData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < depthData.Length; ++depthIndex)
            {
                ushort depth = depthData[depthIndex];
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixelData[colorIndex++] = intensity; // Blue
                pixelData[colorIndex++] = intensity; // Green
                pixelData[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            //int stride = width * format.BitsPerPixel / 8;
            int stride = width * PixelFormats.Bgra32.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixelData, stride);
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
        // This occurs when draw shape button is clicked
/**
 * Here the world co-ordinates of the points of the rectangle on the depth image.
 * With the given 
 */
        private void DrawShape_Click(object sender, EventArgs e)
        {
            System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.Red);
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

                // z_world is the depth

                int count = 0;

                while(count < worldCoords.Length)
                {
                    int temp1;
                    int temp2;
                    for (int i = 0; i < hor.Length; i++)
                    {
                        temp1 = ver[0];
                        temp2 = ver[ver.Length - 1];

                        z_world = depthframe[(temp1 * 512) + (hor[i])];
                        Console.WriteLine(z_world);
                        x_world = (hor[i] - c_x) * z_world/f_x;
                        y_world = (temp1 - c_y) * z_world/f_y;
                        // z_world = relation btw x,y
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;

                        z_world = depthframe[(temp2 * 512) + (hor[i])];
                        y_world = (temp2 - c_y) * z_world / f_y;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                    }

                    for (int i = 0; i < ver.Length; i++)
                    {
                        temp1 = hor[0];
                        temp2 = hor[hor.Length - 1];

                        z_world = depthframe[(ver[i] * 512) + (temp1)];
                        x_world = (temp1 - c_x) * z_world / f_x;
                        y_world = (ver[i] - c_y) * z_world / f_y;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;

                        z_world = depthframe[(ver[i] * 512) + (temp2)];
                        x_world = (temp2 - c_x) * z_world / f_x;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                    }
                }

                // Load the calibration.xml file.
                XDocument doc = XDocument.Load("cal.xml");

                // projector extrinsic

                double[,] projector_pose = new double[4,4];
           
                // Get the values as needed. Change the descendents as per requirement.
                var calibration_data = doc.Descendants("projectors").Descendants("Projector").Descendants("pose").Descendants("double");
                double[] temp = new double[16];
                int k = 0;
                foreach (var data in calibration_data)
                {
                    try
                    {
                        temp[k] = Convert.ToDouble(data.Value);
                        k++;
                    }
                    catch (FormatException exp)
                    {
                        Console.WriteLine(exp);
                    }
                }

                k = 0;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        projector_pose[j, i] = temp[k];
                        k++;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Console.WriteLine(projector_pose[j, i]);
                    }
                }
                

                // projector intrinsic

                double[,] projector_intrinsic = new double[3, 3];

                calibration_data = doc.Descendants("projectors").Descendants("Projector").Descendants("cameraMatrix").Descendants("double");
                temp = new double[16];
                k = 0;
                foreach (var data in calibration_data)
                {
                    try
                    {
                        temp[k] = Convert.ToDouble(data.Value);
                        k++;
                    }
                    catch (FormatException exp)
                    {
                        Console.WriteLine(exp);
                    }
                }

                k = 0;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        projector_pose[j, i] = temp[k];
                        k++;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Console.WriteLine(projector_pose[j, i]);
                    }
                }


                // projector 3d points 
                _3dWorld[] projector_3d = new _3dWorld[worldCoords.Length];

                // projector image points
                int[,] projector_2d = new int[worldCoords.Length, 2];

                double p_x;
                double p_y;
                double p_z;

/*
          <ValuesByColumn>
          <ArrayOfDouble>
            <double>1403.0505051879177</double>
            <double>0</double>
            <double>0</double>
          </ArrayOfDouble>
          <ArrayOfDouble>
            <double>0</double>
            <double>1403.0505051879177</double>
            <double>0</double>
          </ArrayOfDouble>
          <ArrayOfDouble>
            <double>668.580206649565</double>
            <double>-50.8839993065837</double>
            <double>1</double>
*/
                double f_xp = 1403.0505051879177;// projector_intrinsic[0, 0];
                double f_yp = 1403.0505051879177;// projector_intrinsic[1, 1];
                double c_xp = 668.580206649565;// projector_intrinsic[0, 2];
                double c_yp = -50.8839993065837;// projector_intrinsic[1, 2];

                for (int counter = 0; counter < worldCoords.Length; counter++)
                {
                    p_x = projector_pose[0, 0] * worldCoords[counter].getX() + projector_pose[0, 1] * worldCoords[counter].getY() + projector_pose[0, 2] * worldCoords[counter].getZ() + projector_pose[0, 3];

                    p_y = projector_pose[1, 0] * worldCoords[counter].getX() + projector_pose[1, 1] * worldCoords[counter].getY() + projector_pose[1, 2] * worldCoords[counter].getZ() + projector_pose[1, 3];

                    p_z = projector_pose[2, 0] * worldCoords[counter].getX() + projector_pose[2, 1] * worldCoords[counter].getY() + projector_pose[2, 2] * worldCoords[counter].getZ() + projector_pose[2, 3];

                    //projector_3d[counter] = new _3dWorld(p_x, p_y, p_z);

                    projector_2d[counter, 0] = (int)(f_xp * p_x + c_xp * p_z);
                    projector_2d[counter, 1] = (int)(f_yp * p_y + c_yp * p_z);
                }

                

                g.DrawRectangle(p, x1 < x2 ? x1 : x2, y1 < y2 ? y1 : y2, width, height);

                Bitmap Bmp = new Bitmap(512, 424);
                //...

                for (int i = 0; i < projector_2d.Length/2; i++)
                {
                    Bmp.SetPixel(projector_2d[i,0], projector_2d[i,1], System.Drawing.Color.Red);
                }
                Console.WriteLine("jaja");
            }
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
        }
        private System.Drawing.Point RectStartPoint;
        private Rectangle Rect = new Rectangle();
        private System.Drawing.Brush selectionBrush = new SolidBrush(System.Drawing.Color.FromArgb(128, 72, 145, 220));
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Determine the initial rectangle coordinates...
            RectStartPoint = e.Location;
            Invalidate();
        }

        // Draw Area
        //
        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            System.Drawing.Point tempEndPoint = e.Location;
            Rect.Location = new System.Drawing.Point(
                Math.Min(RectStartPoint.X, tempEndPoint.X),
                Math.Min(RectStartPoint.Y, tempEndPoint.Y));
            Rect.Size = new System.Drawing.Size(
                Math.Abs(RectStartPoint.X - tempEndPoint.X),
                Math.Abs(RectStartPoint.Y - tempEndPoint.Y));
            pictureBox1.Invalidate();
            Console.WriteLine(Rect.Size.ToString());
        }


        // Draw Rectangle
        //
       
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Draw the rectangle...
            if (pictureBox1.Image != null)
            {
                if (Rect != null && Rect.Width > 0 && Rect.Height > 0)
                {
                    e.Graphics.FillRectangle(selectionBrush, Rect);
                }
            }

        }        
    }
}
