import gab.opencv.*;
import SimpleOpenNI.*;
import KinectProjectorToolkit.*;

SimpleOpenNI  kinect;
OpenCV opencv;
KinectProjectorToolkit kpc;

ArrayList<Contour> contours;
ArrayList<ProjectedContour> projectedContours;
ArrayList<PGraphics> projectedGraphics;


void setup()
{
  size(displayWidth, displayHeight, P2D);
  
  kinect = new SimpleOpenNI(this);
  kinect.enableDepth();
  kinect.alternativeViewPointDepthToImage();
  
  size(kinect.depthWidth(), kinect.depthHeight());
 
  opencv = new OpenCV(this,  kinect.depthWidth(), kinect.depthHeight());
  
  kpc = new KinectProjectorToolkit(this, kinect.depthWidth(), kinect.depthHeight());
  kpc.loadCalibration("calibration.txt");
  kpc.setContourSmoothness(4);
  
  projectedGraphics = initializeProjectedGraphics();
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
