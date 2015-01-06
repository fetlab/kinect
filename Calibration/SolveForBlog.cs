using MathNet.Numerics.LinearAlgebra.Double;
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
        DenseMatrix foundCoordinatesMatrix;
        DenseMatrix rightSideMatrix;
        DenseMatrix result;

        public static int MininumPoints = 8;

        static void Main(String[] args)
        {
            ArrayList kinectCoordinates = new ArrayList();
            ArrayList projectorCoordinates = new ArrayList();

            for (int i = 0; i < 80; i++)
            {
                Random rn = new Random();
                Point3D point3D = new Point3D()
                {
                    X = rn.Next(7 - 2 + 1) + 2,
                    Y = rn.Next(7 - 2 + 1) + 2,
                    Z = rn.Next(7 - 2 + 1) + 2
                };
                kinectCoordinates.Add(point3D);
                Point2D point2D = new Point2D()
                {
                    X = rn.Next(7 - 2 + 1) + 2,
                    Y = rn.Next(7 - 2 + 1) + 2
                };
                projectorCoordinates.Add(point2D);
            }

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
            DenseMatrix problem = foundCoordinatesMatrix;
            result = (DenseMatrix) problem.Solve(rightSideMatrix);
        }

        private void PrepareMatrices(ArrayList kinectCoors, ArrayList projectorCoors)
        {
            foundCoordinatesMatrix = new DenseMatrix(projectorCoors.Count * 2, 11);
            rightSideMatrix = new DenseMatrix(projectorCoors.Count, 1);
            for (int i = 0; i < projectorCoors.Count; i = i + 2)
            {
                Point3D kc = (Point3D) kinectCoors[i / 2];
                Point2D projC = (Point2D) projectorCoors[i / 2];
                double[] valueArray = new double[] {kc.X, kc.Y, kc.Z, 1, 0, 0, 0, 0, -projC.X * kc.X, -projC.X * kc.Y, -projC.X * kc.Z};
                foundCoordinatesMatrix.SetColumn(i, valueArray);
                valueArray = new double[] {0, 0, 0, 0, kc.X, kc.Y, kc.Z, 1, -projC.Y * kc.X, -projC.Y * kc.Y, -projC.Y * kc.Z, projC.Y};
                foundCoordinatesMatrix.SetColumn(i + 1, valueArray);
            }
        }

        public Point2D convertKinectToProjector(Point3D kinectPoint)
        {
            Point2D projectorPoint = new Point2D();
        //xp = (q1*xk + q2*yk + q3*zk + q4)/(q9*xk + q10*yk + q11*zk + 1)
        projectorPoint.X = (result.At(0,0)*kinectPoint.X + result.At(1,0)*kinectPoint.Y + result.At(2,0)*kinectPoint.Z + result.At(3,0))/
                (result.At(8,0)*kinectPoint.X + result.At(9,0)*kinectPoint.Y + result.At(10,0)*kinectPoint.Z + 1);
 
        //yp = (q5*xk + q6*yk + q7*zk + q8)/(q9*xk + q10*yk + q11*zk + 1)
        projectorPoint.Y = (result.At(4,0)*kinectPoint.X + result.At(5,0)*kinectPoint.Y + result.At(6,0)*kinectPoint.Z + result.At(7,0))/
                (result.At(8,0)*kinectPoint.X + result.At(9,0)*kinectPoint.Y + result.At(10,0)*kinectPoint.Z + 1);

        return projectorPoint;
        }

        public class Point3D
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

        public class Point2D
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
