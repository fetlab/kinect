using DotNetMatrix;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectCalibration
{
    class SolveForBlog
    {
        GeneralMatrix foundCoordinatesMatrix;
        GeneralMatrix rightSideMatrix;
        GeneralMatrix result;

        public static int MininumPoints = 8;

        public static void main(String[] args)
        {
            ArrayList kinectCoordinates = new ArrayList();
            ArrayList projectorCoordinates = new ArrayList();

            SolveForBlog solver = new SolveForBlog();
            solver.FindTransformation(kinectCoordinates, projectorCoordinates);

            Point3D testKinectCoord = new Point3D();
            testKinectCoord.X = 1;
            testKinectCoord.Y = 2;
            testKinectCoord.Z = 3;

            Point2D result = solver.convertKinectToProjector(testKinectCoord);
        }

        public void FindTransformation(ArrayList kinectCoors, ArrayList projectorCoors)
        {
            PrepareMatrices(kinectCoors, projectorCoors);
            QRDecomposition problem = new QRDecomposition(foundCoordinatesMatrix);
            result = problem.Solve(rightSideMatrix);
        }

        private void PrepareMatrices(ArrayList kinectCoors, ArrayList projectorCoors)
        {
            foundCoordinatesMatrix = new GeneralMatrix(projectorCoors.Count*2,11);
            rightSideMatrix = new GeneralMatrix(projectorCoors.Count, 1);
            for (int i = 0; i < projectorCoors.Count; i = i + 2)
            {
                Point3D kc = (Point3D) kinectCoors[i / 2];
                Point2D projC = (Point2D) projectorCoors[i / 2];
                foundCoordinatesMatrix.SetElement(i, 0, kc.X);
                foundCoordinatesMatrix.SetElement(i, 1, kc.Y);
                foundCoordinatesMatrix.SetElement(i, 2, kc.Z);
                foundCoordinatesMatrix.SetElement(i, 3, 1);
                foundCoordinatesMatrix.SetElement(i, 4, 0);
                foundCoordinatesMatrix.SetElement(i, 5, 0);
                foundCoordinatesMatrix.SetElement(i, 6, 0);
                foundCoordinatesMatrix.SetElement(i, 7, 0);
                foundCoordinatesMatrix.SetElement(i, 8, -projC.X * kc.X);
                foundCoordinatesMatrix.SetElement(i, 9, -projC.X * kc.Y);
                foundCoordinatesMatrix.SetElement(i, 10, -projC.X * kc.Z);
                rightSideMatrix.SetElement(i, 0, projC.X);

                foundCoordinatesMatrix.SetElement(i + 1, 0, 0);
                foundCoordinatesMatrix.SetElement(i + 1, 1, 0);
                foundCoordinatesMatrix.SetElement(i + 1, 2, 0);
                foundCoordinatesMatrix.SetElement(i + 1, 3, 0);
                foundCoordinatesMatrix.SetElement(i + 1, 4, kc.X);
                foundCoordinatesMatrix.SetElement(i + 1, 5, kc.Y);
                foundCoordinatesMatrix.SetElement(i + 1, 6, kc.Z);
                foundCoordinatesMatrix.SetElement(i + 1, 7, 1);
                foundCoordinatesMatrix.SetElement(i + 1, 8, -projC.Y * kc.X);
                foundCoordinatesMatrix.SetElement(i + 1, 9, -projC.Y * kc.Y);
                foundCoordinatesMatrix.SetElement(i + 1, 10, -projC.Y * kc.Z);
                rightSideMatrix.SetElement(i + 1, 0, projC.Y);
            }
        }

        public Point2D convertKinectToProjector(Point3D kinectPoint)
        {
            Point2D projectorPoint = new Point2D();
        //xp = (q1*xk + q2*yk + q3*zk + q4)/(q9*xk + q10*yk + q11*zk + 1)
        projectorPoint.X = (result.GetElement(0,0)*kinectPoint.X + result.GetElement(0,1)*kinectPoint.Y + result.GetElement(0,2)*kinectPoint.Z + result.GetElement(0,3))/
                (result.GetElement(0,8)*kinectPoint.X + result.GetElement(0,9)*kinectPoint.Y + result.GetElement(0,10)*kinectPoint.Z + 1);
 
        //yp = (q5*xk + q6*yk + q7*zk + q8)/(q9*xk + q10*yk + q11*zk + 1)
        projectorPoint.Y = (result.GetElement(0,4)*kinectPoint.X + result.GetElement(0,5)*kinectPoint.Y + result.GetElement(0,6)*kinectPoint.Z + result.GetElement(0,7))/
                (result.GetElement(0,8)*kinectPoint.X + result.GetElement(0,9)*kinectPoint.Y + result.GetElement(0,10)*kinectPoint.Z + 1);

        return projectorPoint;
        }

        class Point3D
        {

            public double X
            {
                get { return this.X;}
                set { this.X = value; }
            }
            
            public double Y
            {
                get { return this.Y; }
                set { this.Y = value; }
            }
            
            public double Z
            {
                get { return this.Z; }
                set { this.Z = value; }
            }
        }

        class Point2D
        {
            public double X
            {
                get { return this.X; }
                set { this.X = value; }
            }

            public double Y
            {
                get { return this.Y; }
                set { this.Y = value; }
            }
        }
    }
}
