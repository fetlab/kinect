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
using SharpDX;

/* 
 * (The values for intrinsic matrices for depth camera and projector have been hardcoded because
 *  they never change)
 * 
 * Depth camera intrinsic matrix, values are taken from calibration.xml file
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
        UInt16[] depthframe = new UInt16[512 * 424];    // array to store depth values
        CameraSpacePoint[] camera3D = new CameraSpacePoint[512 * 424];

        //Initialize the canvas window(UI).
        public canvas()
        {
            InitializeComponent();
        }


        // Load the calibration.xml file.
        XDocument doc = XDocument.Load("C:/Users/sk1846/Documents/GitHub/kinect/DrawTest/Cal.xml");

        
    /* intrinsic matrices for depth camera have been hardcoded because they never change.
     * 
     * Depth camera intrinsic matrix, values are taken from calibration.xml file
     * 
     * | fx  0   c_x |        fx,fy are focal lengths in pixels. 
     * | 0   fy  c_y |        c_x,c_y are principle point coordinates.
     * | 0   0    1  |
    */
    
        double f_x = 366.09138096351234;
        double f_y = 366.11949696912916;
        double c_x = 258.40131496255316;
        double c_y = 214.52516849276577;
        
        //(x1,y1), (x2,y2),(x3,y3),(x4,y4) are four vertices of rectangle, which is drawn to select any object. 
        int x1 = 0;
        int x2 = 0;
        int y1 = 0;
        int y2 = 0;
        //int count = 0;
        bool flag = false;

        //Open the kinect sensor.
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


        //Multisource frame reader. 
        private void Reader_MultiSourceFrameArrived(object sender, Microsoft.Kinect.MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();                  
            // Color
            /*  Color frame to be used later use. 
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    
                //Perform anything with color frame data.
                
                }
            }
            //Save color frame for later use. 
            var col_frame = reference.ColorFrameReference.AcquireFrame();
            
             */
             // Work with depth data.


            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                        FrameDescription depth_Frame_Des = frame.FrameDescription;
                        int depth_height = depth_Frame_Des.Height;
                        int depth_width = depth_Frame_Des.Width;
                        UInt16[] depth_flipped = new UInt16[512 * 424];
                        UInt16[] depthframe = new UInt16[depth_height * depth_width];
                        /*
                        This for loop is flipping the depth image with respect to vertical axis because kinect by default 
                        gives mirrored image. 
                        */
                        frame.CopyFrameDataToArray(depthframe);
                        for (int k1 = 0; k1 < 424; k1++)
                        {
                            int idx = k1*512;
                            UInt16[] temp = new UInt16[512];
                            Array.Copy(depthframe, idx, temp, 0, 512);
                            Array.Reverse(temp);
                            Array.Copy(temp, 0, depth_flipped, idx, 512);
                        }
                        /*
                        This coordinate mapper maps every depth data from depth image to 3D point(x,y,z)
                        which represents corresponding point in 3D view of depth camera.
                        */
                        _sensor.CoordinateMapper.MapDepthFrameToCameraSpace(depth_flipped,camera3D);
               
                        BitmapSource bmpsource  = ToBitmap(frame);
                        Bitmap bti = BitmapFromSource(bmpsource);
                        //flipping the image for display also. 
                        bti.RotateFlip(RotateFlipType.Rotate180FlipY);
                        System.Drawing.Image img = bti;
                        pictureBox1.Image = img;
                        
                    }
                }
          
        }
        
        
        // Function to create Bitmap from Bitmapsource.
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
        //Function to convert a frame into Bitmap. 
        private  BitmapSource ToBitmap(DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            UInt16[] depthframe = new ushort[width * height];
            byte[] pixelData = new byte[width * height * (PixelFormats.Bgr32.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(depthframe);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < depthframe.Length; ++depthIndex)
            {
                ushort depth = depthframe[depthIndex];
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

        int width;
        int height;
        
        //Get all the four points. 
        private void DrawShape_Click(object sender, EventArgs e)
        {
            int x3 = x1;
            int y3 = y2;

            int x4 = x2;
            int y4 = y1;

            
            System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.Red);
            Graphics g = this.CreateGraphics();
            flag = true;
            if (flag)
            {
                width = Math.Abs(x1 - x2);
                //Console.WriteLine("In X, start point \t" + x1 + " End pont is \t" + x2);                
                height = Math.Abs(y1 - y2);
                //Console.WriteLine("In Y, start point \t" + y1 + " End pont is \t" + y2);                
                
                int[] hor = new int[2];//width + 1];
                //int horcount = 0;
                int[] ver = new int[2];//height + 1];
          
                hor[0] = x1;
                hor[1] = x2;
                //hor[2] = x3;
                //hor[3] = x2;

                ver[0] = y1;
                ver[1] = y2;
                //ver[2] = y3;
                //ver[3] = y2;
                _3dWorld[] worldCoords = new _3dWorld[4];//(hor.Length * 2) + (ver.Length * 2)];

                double x_world = 0;
                double y_world = 0;
                double z_world = 0;

                // z_world is the depth

                int count = 0;
             
                while(count < worldCoords.Length)
                {
                    int temp1;
                    int temp2;
                    
                    for (int i = 0; i < 2; i++)//hor.Length; i++)
                    {
                        temp1 = ver[0];
                        temp2 = ver[ver.Length - 1];

                        z_world = depthframe[(temp1 * 512) + (hor[i])];
                        //Console.WriteLine(z_world);
                        x_world = (hor[i] - c_x) * z_world/f_x;
                        //Console.WriteLine(x_world.ToString());
                        y_world = (temp1 - c_y) * z_world/f_y;
                        //Console.WriteLine(y_world.ToString());
                        // z_world = relation btw x,y
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;

                        z_world = depthframe[(temp2 * 512) + (hor[i])];
                        y_world = (temp2 - c_y) * z_world / f_y;
                        worldCoords[count] = new _3dWorld(x_world, y_world, z_world);
                        count++;
                    }
                
                }

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
                        temp[k] = Convert.ToDouble(data.Value) ;
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

                calibration_data = doc.Descendants("projectors").Descendants("Projector").Descendants("pose").Descendants("double");
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

                
                 SharpDX.Matrix view1 = new SharpDX.Matrix();
                 for (int i = 0; i < 4;i++ )
                 {
                     for(int j=0;j<4;j++)
                     {
                         view1[i, j] = (float)projector_pose[i, j];
                     }
                 }
                 view1.Invert();
                 view1.Transpose();

                     for (int i = 0; i < 4; i++)
                     {
                         for (int j = 0; j < 4; j++)
                         {
                             //Console.WriteLine(projector_pose[j, i]);
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
                //Hardcoding the projector intrinsic matrix vlaues. 
                double f_xp = 1974.87157373946;// projector_intrinsic[0, 0];
                double f_yp = 1974.87157373946;// projector_intrinsic[1, 1];
                double c_xp = 1031.8680099552494;//1013.8355612110823;// projector_intrinsic[0, 2];
                double c_yp = -106.89190621825618;//-184.66338516416516;//-93.840102011746026;// projector_intrinsic[1, 2];

                /*
                 * 
                 *  temp1 = ver[0];
                 *  temp2 = ver[ver.Length - 1];
                 *  z_world = depthframe[(temp1 * 512) + (hor[i])];
                 * 
                 */

                CameraSpacePoint[] my_3D = new CameraSpacePoint[4];
                my_3D[0] = camera3D[(ver[0] * 512) + hor[0]];
                my_3D[1] = camera3D[(ver[1] * 512) + hor[0]];
                my_3D[2] = camera3D[(ver[0] * 512) + hor[1]];
                my_3D[3] = camera3D[(ver[1] * 512) + hor[1]];
                //This for loop maps four points from depth camera 3D view to 3D view of projector.
                //It uses pose matrix of projector optained by the calibration process. 
                for (int counter = 0; counter <4 ; counter++)
                {
                    CameraSpacePoint temp_point = my_3D[counter];

                    p_x = view1[0, 0] * temp_point.X * 1000 + view1[0, 1] * temp_point.Y * 1000 + view1[0, 2] * temp_point.Z * 1000 + view1[0, 3];
                    //p_x = projector_pose[0, 0] * worldCoords[counter].getX() + projector_pose[0, 1] * worldCoords[counter].getY() + projector_pose[0, 2] * worldCoords[counter].getZ() + projector_pose[0, 3];

                    p_y = view1[1, 0] * temp_point.X * 1000 + view1[1, 1] * temp_point.Y * 1000 + view1[1, 2] * temp_point.Z * 1000 + view1[1, 3];
                    //p_y = projector_pose[1, 0] * worldCoords[counter].getX() + projector_pose[1, 1] * worldCoords[counter].getY() + projector_pose[1, 2] * worldCoords[counter].getZ() + projector_pose[1, 3];

                    p_z = view1[2, 0] * temp_point.X * 1000 + view1[2, 1] * temp_point.Y * 1000 + view1[2, 2] * temp_point.Z * 1000 + view1[2, 3];
                    //p_z = projector_pose[2, 0] * worldCoords[counter].getX() + projector_pose[2, 1] * worldCoords[counter].getY() + projector_pose[2, 2] * worldCoords[counter].getZ() + projector_pose[2, 3];

                    projector_3d[counter] = new _3dWorld(p_x, p_y, p_z);

                    double p_x_homo, p_y_homo, p_z_homo;
                    //Homogenize the values. 
                    p_x_homo = p_x / p_z;
                    p_y_homo = p_y / p_z;
                    p_z_homo = 1;
                    
                    projector_2d[counter, 0] = (int)(f_xp * p_x_homo + c_xp * p_z_homo);
                    projector_2d[counter, 1] = (int)(f_yp * p_y_homo + c_yp * p_z_homo);
                }

                // Generating an image in the projector 2D view with the corresponding selected region in deptgh image. 
                g.DrawRectangle(p, x1 < x2 ? x1 : x2, y1 < y2 ? y1 : y2, width, height);
                Bitmap Bmp = new Bitmap(512,424);
                for (int i = 0; i < projector_2d.Length/2; i++)
                {
                    Bmp.SetPixel(projector_2d[i,0], projector_2d[i,1], System.Drawing.Color.Red);
                }
                //Bmp.Save();
                Bmp.Save("img.png",ImageFormat.Png);
                Console.WriteLine("jaja");
            }
        }

        private System.Drawing.Point RectStartPoint;
        private System.Drawing.Rectangle Rect = new System.Drawing.Rectangle();
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
            int Lx, Ly, Rx, Ry;
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
            // X- coordinate of top lelt point
            Lx = Rect.X;
            x1 = Lx;
            // Y- coordinate of top lelt point
            Ly = Rect.Y;
            y1 = Ly;
            // X- coordinate of bottom right point
            Rx = Lx + Rect.Width;
            x2 = Rx;
            // Y- coordinate of bottom right point
            Ry = Ly + Rect.Height;
            y2 = Ry;
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
