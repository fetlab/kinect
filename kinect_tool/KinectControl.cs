using Microsoft.Kinect;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplication6
{
  class KinectControl
  {
    public KinectControl(Func<byte[], int, int, Task> frameHandler)
    {
      this.frameHandler = frameHandler;
    }
    public Size FrameSize
    {
      get;
      private set;
    }
    public void GetSensor()
    {
      this.sensor = KinectSensor.GetDefault();
      this.sensor.Open();

      // New bit - ask the sensor for the sizes rather than define constants.
      var description = this.sensor.ColorFrameSource.FrameDescription;
      

      this.frameArray = new byte[
        description.Width * description.Height * description.BytesPerPixel * 2]; // TBD on the 2

      this.FrameSize = new Size(
        this.sensor.ColorFrameSource.FrameDescription.Width,
        this.sensor.ColorFrameSource.FrameDescription.Height);
    }
    public void OpenReader()
    {
      this.reader = this.sensor.ColorFrameSource.OpenReader();
      this.reader.FrameArrived += OnFrameArrived;
    }
    public void CloseReader()
    {
      this.reader.FrameArrived -= OnFrameArrived;
      this.reader.Dispose();
      this.reader = null;
    }
    void OnFrameArrived(object sender, ColorFrameArrivedEventArgs e)
    {
      // I don't *think* this event is re-entrant in the sense that I
      // don't think we'll get it fired while we're handling it. Once
      // we return to the caller it can call us again though and that
      // can happen after I've called Task.Run and returned below.
      if (this.bufferIdle)
      {
        this.bufferIdle = false;

        Task.Run(
          async () =>
          {
            var frame = e.FrameReference.AcquireFrame();

            if (frame != null)
            {
              frame.CopyConvertedFrameDataToArray(this.frameArray, ColorImageFormat.Bgra);

              await this.frameHandler(
                this.frameArray,
                (int)this.FrameSize.Width,
                (int)this.FrameSize.Height);

              frame.Dispose();

              this.bufferIdle = true;
            }
            else
            {
              this.bufferIdle = true;
            }
          });
      }
    }
    public void ReleaseSensor()
    {
      this.sensor.Close();
      this.sensor = null;
    }
    KinectSensor sensor;
    ColorFrameReader reader;
    Func<byte[], int, int, Task> frameHandler;
    byte[] frameArray;
    volatile bool bufferIdle = true;
  }
}