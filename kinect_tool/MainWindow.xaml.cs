using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Threading;
using System.IO;


namespace WpfApplication6
{
  
  public partial class MainWindow : Window
  {
    KinectControl controller;
    public int X_co, Y_co;


    public MainWindow()
    {
      InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.controller = new KinectControl(this.OnFrame);
    }
    async Task OnFrame(byte[] frame, int width, int height)
    {
      await Dispatcher.InvokeAsync(() =>
        {
          this.bitmapSource.Lock();

          this.bitmapSource.WritePixels(invalidRect, frame,
          width * Constants.BytesPerPixel, 0);

          this.bitmapSource.AddDirtyRect(invalidRect);

          this.bitmapSource.Unlock();

         


    

          //this.txtFps.Text = ((++this.frameCount) /
            //((DateTime.Now.Ticks - this.startTicks) / 10e6)).ToString();
        });
    }

    void OnGetSensor(object sender, RoutedEventArgs e)
    {
      this.controller.GetSensor();

      this.bitmapSource = new WriteableBitmap(
        (int)this.controller.FrameSize.Width,
        (int)this.controller.FrameSize.Height,
        Constants.Dpi,
        Constants.Dpi,
        PixelFormats.Bgra32,
        null);

      this.imgColour.Source = this.bitmapSource;
      Bitmap bm;
      bm = Bitmapfromsource(this.bitmapSource);
      bm.GetPixel(X_co,Y_co);

      
        

      invalidRect = new Int32Rect(0, 0, 
        (int)this.controller.FrameSize.Width, (int)this.controller.FrameSize.Height);
    }
    void OnOpenReader(object sender, RoutedEventArgs e)
    {
      this.startTicks = DateTime.Now.Ticks;
      this.frameCount = 0;
      this.controller.OpenReader();
    }
    void OnCloseReader(object sender, RoutedEventArgs e)
    {
      this.controller.CloseReader();
    }
    void OnReleaseSensor(object sender, RoutedEventArgs e)
    {
      this.controller.ReleaseSensor();
    }
    static Int32Rect invalidRect;
    WriteableBitmap bitmapSource;
    long startTicks;
    long frameCount;

    public static Bitmap Bitmapfromsource(BitmapSource source)
    {
        Bitmap bitmap; 
        using (var outStream = new MemoryStream())
        {
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(source));
            enc.Save(outStream);
            bitmap = new Bitmap(outStream);

        }
        return bitmap;
    }

    private void imgColour_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        X_co =  Convert.ToInt32(e.GetPosition(imgColour).X);
        Y_co = Convert.ToInt32(e.GetPosition(imgColour).Y);
        Int32[] val = new Int32[5];
        this.textbox1.Text = X_co.ToString();
        this.textbox2.Text = Y_co.ToString();
        Bitmap bm;
        bm = Bitmapfromsource(this.bitmapSource);
        System.Drawing.Color clr;
        clr = bm.GetPixel(X_co, Y_co);
        //int argb = clr.ToArgb();
        String str;
        str = clr.Name;
        this.textbox3.Text = str;
        this.textbox4.Text = clr.G.ToString();

        // Projector part start
        Int32 temp_height, temp_width;
        temp_height = Convert.ToInt32(this.bitmapSource.Height);
        temp_width = Convert.ToInt32(this.bitmapSource.Width);
        Project_Window.Height = temp_height;
        Project_Window.Width = temp_width;
        Bitmap pro_bitmap;
        pro_bitmap = new Bitmap(temp_width, temp_width);
        for (int i = X_co - 20; i <= X_co + 20; i++)
        {
            for (int j = Y_co - 20; j <= Y_co + 20; j++)
            {
                pro_bitmap.SetPixel(i, j, System.Drawing.Color.Green);
            }
        }
        BitmapSource temp_source;
        temp_source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(pro_bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        var brush = new ImageBrush(temp_source);
        Project_Window.Background = brush; 
    }


    Window Project_Window = new Window();
    public void Button_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {

        Project_Window.Show();
  
       // Window win = new Window();
       
       // Bitmap bitm;

     //   bitm = new Bitmap(800, 800);
     //   for (int i = X_co-20; i <= X_co+20; i++)
    //    {
     //       for (int j =Y_co-20; j <= Y_co+20; j++ )
      //      {
     //           bitm.SetPixel(i,j,System.Drawing.Color.Green);
    //    }

    //    }
     //   BitmapSource bitms;
     //   bitms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitm.GetHbitmap(),IntPtr.Zero,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());
     //   var brush = new ImageBrush(bitms);
    //    win.Background = brush;
    //    win.Show();
        
        

  
       
       // imgColour.Source = bmp; 
    }
    }

  
  }
