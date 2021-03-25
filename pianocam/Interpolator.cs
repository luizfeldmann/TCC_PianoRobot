#define HARMONIC

using System;
using MathNet.Numerics.LinearAlgebra;

namespace pianocam
{
    public static class Interpolator
    {
        public static double Linear(double x, InterpolationPoint p1, InterpolationPoint p2)
        {
            return p1.y + (p2.y - p1.y) * (x - p1.x) / (p2.x - p1.x);
        }

        public static double Sigmoid(double x, InterpolationPoint p1, InterpolationPoint p2, out double velocity)
        {
#if (HARMONIC)
            double A = (p2.y + p1.y) / 2f;
            double B = (p1.y - p2.y) / 2f;
            double C = Math.PI / (p2.x - p1.x);
            double D = 0.5f * Math.PI * (3 * p2.x - p1.x) / (p1.x - p2.x);

            velocity = C * B * Math.Cos(C * x + D);
            return A + B * Math.Sin( C * x + D);
#else
            var A = Matrix<double>.Build.DenseOfArray(new double[,] {
                {Math.Pow(p1.x,3), Math.Pow(p1.x, 2), p1.x, 1},
                {Math.Pow(p2.x,3), Math.Pow(p2.x, 2), p2.x, 1},
                {3*Math.Pow(p1.x,2), 2*p1.x, 1, 0},
                {3*Math.Pow(p2.x,2), 2*p2.x, 1, 0}});

            var B = Vector<double>.Build.DenseOfArray(new double[] { p1.y, p2.y, 0, 0 });

            var X = Vector<double>.Build.DenseOfArray(new double[] { Math.Pow(x, 3), Math.Pow(x, 2), x, 1 });
            var Xl = Vector<double>.Build.DenseOfArray(new double[] { 3 * Math.Pow(x, 2), 2 * x, 1, 0 });

            var coef = A.Solve(B);

            velocity = Xl * coef;
            return X * coef;
#endif
        }

    }

    public struct InterpolationPoint
    {
        public double y;
        public double x;
    }
}
