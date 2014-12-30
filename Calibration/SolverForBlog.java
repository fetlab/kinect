import Jama.Matrix;
import Jama.QRDecomposition;

import java.util.ArrayList;
import java.util.Random;

public class SolverForBlog {

    /**
     * The system of liear equations is X*A = B
     * X: Vector of unknown variables q1..q11 (result)
     * A: Matrix of coefficients of the system (foundCoordinatesMatrix)
     * B: Right side matrix
     */
    Matrix foundCoordinatesMatrix;
    Matrix rightSideMatrix;
    Matrix result;

    public static final int MINIMUM_POINTS = 8;

    public static void main(String[] args) {

        //----  FINDING THE TRANSFORMATION ----//
        //(needs to be done only once, unless the Kinect-projector position changes)

        // these are fitting coordinate pairs both lists have the same size
        ArrayList kinectCoordinates = new ArrayList();
        ArrayList projectorCoordinates = new ArrayList();

        for (int i = 0; i < 80; i++) {
            Random rn = new Random();
            Point3D point3D = new Point3D();
            point3D.x = rn.nextInt(7 - 2 + 1) + 2;
            point3D.y = rn.nextInt(7 - 2 + 1) + 2;
            point3D.z = rn.nextInt(7 - 2 + 1) + 2;
            kinectCoordinates.add(point3D);
            Point2D point2D = new Point2D();
            point2D.x = rn.nextInt(7 - 2 + 1) + 2;
            point2D.y = rn.nextInt(7 - 2 + 1) + 2;
            projectorCoordinates.add(point2D);
        }

        //fill in the coordinate point pairs into the lists above (MINIMUM is 8, but you'll want >50)
        //...
        //...
        //...

        SolverForBlog solver = new SolverForBlog();

        //Solve linear equations, get coefficients q1..q11
        solver.findTransformation(kinectCoordinates, projectorCoordinates);

        //----  CONVERTING COORDINATES  ----//

        //having any Kinect coordinates, get corresponding projector coordinates
        Point3D p = new Point3D();

        //sample point with Kinect coordinates
        p.x = 1;
        p.y = 2;
        p.z = 3;

        Point2D projPoint = solver.convertKinectToProjector(p);
        System.out.println(projPoint);

        //do something with projPoint
    }

    /**
     * Solves system of linear equations -> finds coefficients of transformation (these are variables of linear equations) from kinect to the projector point.
     */
    public void findTransformation(ArrayList kinectCoors, ArrayList projectorCoors) {
        prepareMatrices(kinectCoors, projectorCoors);
        QRDecomposition problem = new QRDecomposition(foundCoordinatesMatrix);
        result = problem.solve(rightSideMatrix);
        result.print(4,4);
    }

    private void prepareMatrices(ArrayList kinectCoors, ArrayList projectorCoors) {
        foundCoordinatesMatrix = new Matrix(projectorCoors.size() * 2, 11);
        rightSideMatrix = new Matrix(projectorCoors.size() * 2, 1);
        for (int i = 0; i < projectorCoors.size() * 2; i = i + 2) {
            Point3D kc = (Point3D) kinectCoors.get(i / 2);
            Point2D projC = (Point2D) projectorCoors.get(i / 2);
            foundCoordinatesMatrix.set(i, 0, kc.x);
            foundCoordinatesMatrix.set(i, 1, kc.y);
            foundCoordinatesMatrix.set(i, 2, kc.z);
            foundCoordinatesMatrix.set(i, 3, 1);
            foundCoordinatesMatrix.set(i, 4, 0);
            foundCoordinatesMatrix.set(i, 5, 0);
            foundCoordinatesMatrix.set(i, 6, 0);
            foundCoordinatesMatrix.set(i, 7, 0);
            foundCoordinatesMatrix.set(i, 8, -projC.x * kc.x);
            foundCoordinatesMatrix.set(i, 9, -projC.x * kc.y);
            foundCoordinatesMatrix.set(i, 10, -projC.x * kc.z);
            rightSideMatrix.set(i, 0, projC.x);

            foundCoordinatesMatrix.set(i + 1, 0, 0);
            foundCoordinatesMatrix.set(i + 1, 1, 0);
            foundCoordinatesMatrix.set(i + 1, 2, 0);
            foundCoordinatesMatrix.set(i + 1, 3, 0);
            foundCoordinatesMatrix.set(i + 1, 4, kc.x);
            foundCoordinatesMatrix.set(i + 1, 5, kc.y);
            foundCoordinatesMatrix.set(i + 1, 6, kc.z);
            foundCoordinatesMatrix.set(i + 1, 7, 1);
            foundCoordinatesMatrix.set(i + 1, 8, -projC.y * kc.x);
            foundCoordinatesMatrix.set(i + 1, 9, -projC.y * kc.y);
            foundCoordinatesMatrix.set(i + 1, 10, -projC.y * kc.z);
            rightSideMatrix.set(i + 1, 0, projC.y);
        }
    }

    public Point2D convertKinectToProjector(Point3D kinectPoint) {
        Point2D out = new Point2D();
        //xp = (q1*xk + q2*yk + q3*zk + q4)/(q9*xk + q10*yk + q11*zk + 1)
        out.x = (result.get(0, 0) * kinectPoint.x + result.get(1, 0) * kinectPoint.y + result.get(2, 0) * kinectPoint.z + result.get(3, 0)) /
                (result.get(8, 0) * kinectPoint.x + result.get(9, 0) * kinectPoint.y + result.get(10, 0) * kinectPoint.z + 1);

        //yp = (q5*xk + q6*yk + q7*zk + q8)/(q9*xk + q10*yk + q11*zk + 1)
        out.y = (result.get(4, 0) * kinectPoint.x + result.get(5, 0) * kinectPoint.y + result.get(6, 0) * kinectPoint.z + result.get(7, 0)) /
                (result.get(8, 0) * kinectPoint.x + result.get(9, 0) * kinectPoint.y + result.get(10, 0) * kinectPoint.z + 1);
        return out;
    }

}

class Point3D {
    double x, y, z;

    @Override
    public String toString() {
        return x + ", " + y + ", " + z;
    }
}

class Point2D {
    double x, y;

    @Override
    public String toString() {
        return x + ", " + y;
    }
}