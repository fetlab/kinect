// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Drawing.Imaging;
using System.IO;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace MiniTri
{
    /// <summary>
    ///   Screen capture of the desktop using DXGI OutputDuplication.
    /// </summary>
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            const string outputFileName = "CroppedImage.bmp";

            // Create DXGI Factory1
            var factory = new Factory1();
            var adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            var device = new Device(adapter);

            // Get DXGI.Output
            var output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();


            //Define the co-ordinates of working region in illustrator or MS-paint. 
            // (x1,y1) is top left point and (x2,y2) is bottom right point. 
            int x1 = 120, y1 = 120;
            int x2 = 600, y2 = 700;

            //Get the height and width for the cropped window. 

            int croppedHeight = Math.Abs(y1 - y2);
            int croppedWidth = Math.Abs(x2 - x1); 



            // Width/Height of desktop to capture
            int width = ((Rectangle)output.Description.DesktopBounds).Width;
            int height = ((Rectangle)output.Description.DesktopBounds).Height;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
                                  {
                                      CpuAccessFlags = CpuAccessFlags.Read,
                                      BindFlags = BindFlags.None,
                                      Format = Format.B8G8R8A8_UNorm,
                                      Width = width,
                                      Height = height,
                                      OptionFlags = ResourceOptionFlags.None,
                                      MipLevels = 1,
                                      ArraySize = 1,
                                      SampleDescription = { Count = 1, Quality = 0 },
                                      Usage = ResourceUsage.Staging
                                  };
            var screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            var duplicatedOutput = output1.DuplicateOutput(device);

            bool captureDone = false;
            for (int i = 0; !captureDone; i++)
            {
                try
                {
                    SharpDX.DXGI.Resource screenResource;
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);

                    if (i > 0)
                    {
                        // copy resource into memory that can be accessed by the CPU
                        using (var screenTexture2D = screenResource.QueryInterface<Texture2D>()) 
                            device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                        // Get the desktop capture texture
                        var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);

                        // Create Drawing.Bitmap
                        //var bitmap = new System.Drawing.Bitmap(width, height, PixelFormat.Format32bppArgb);
                        //var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);


                        var croppedBitmap = new System.Drawing.Bitmap(croppedWidth, croppedHeight, PixelFormat.Format32bppArgb);
                        var resizeBitmap = new System.Drawing.Bitmap(1920,1080, PixelFormat.Format32bppArgb);
                        var croppedBoundsRect = new System.Drawing.Rectangle(0, 0, croppedWidth, croppedHeight);


                        // Copy pixels from screen capture Texture to GDI bitmap
                        //var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

                        var croppedMapDest = croppedBitmap.LockBits(croppedBoundsRect, ImageLockMode.WriteOnly, croppedBitmap.PixelFormat);

                        var sourcePtr = mapSource.DataPointer;
                        //var destPtr = mapDest.Scan0;

                        var croppedDestPtr = croppedMapDest.Scan0;

                        for (int y = y1; y < y2; y++)
                        {
                            // Copy a single line 
                            Utilities.CopyMemory(croppedDestPtr, sourcePtr,  croppedWidth* 4);

                            // Advance pointers
                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                            croppedDestPtr = IntPtr.Add(croppedDestPtr, croppedMapDest.Stride);
                        }

                        // Release source and dest locks
                        croppedBitmap.UnlockBits(croppedMapDest);
                        device.ImmediateContext.UnmapSubresource(screenTexture, 0);


                        //Rescale image from lower resolution to 1920x1080
                        var resizedBitmap = new System.Drawing.Bitmap(croppedBitmap, new System.Drawing.Size(1920, 1080));


                        // Save the output
                        resizedBitmap.Save(outputFileName);

                        // Capture done
                        captureDone = true;
                    }

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();

                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        throw e;
                    }
                }
            }

            // Display the texture using system associated viewer
            System.Diagnostics.Process.Start(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputFileName)));

            // TODO: We should cleanp up all allocated COM objects here
        }
    }
}
