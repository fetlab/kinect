import SimpleOpenNI.*;
import gab.opencv.*;

SimpleOpenNI  kinect;
OpenCV opencv;

ArrayList<Contour> contours;

void setup()
{
  kinect = new SimpleOpenNI(this);
  kinect.setMirror(true);
  kinect.enableDepth();
  size(kinect.depthWidth(), kinect.depthHeight());
 
 opencv = new OpenCV(this,  kinect.depthWidth(), kinect.depthHeight());
}

void draw()
{
  kinect.update();
  opencv.loadImage(kinect.depthImage());
  contours = opencv.findContours();
  
  image(kinect.depthImage(),0,0);
  for(Contour contour : contours)
  {
    stroke(0, 255, 0);
    contour.draw();
  }
}
