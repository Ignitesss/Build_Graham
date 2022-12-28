using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Indiv2
{
    public class Coordinates
    {
        private double x = 0.0;
        private double y = 0.0;

        public double X
        {
            get { return x; }
        }

        public double Y
        {
            get { return y; }
        }

        public double PolarAngle 
        { 
            get { return CalculatePolarAngle(); } 
        }

        public Coordinates(double latitude, double longitude)
        {
            this.x = latitude;
            this.y = longitude;
        }

        public Coordinates()
        {
            this.x = 0.0;
            this.y = 0.0;
        }

        public Coordinates(Point p)
        {
            this.x = p.X;
            this.y = p.Y;
        }

        private double CalculatePolarAngle()
        {
            double polarangle = Math.Atan(x / y);
            if (polarangle > 0.0)
            {
                return polarangle;
            }
            return polarangle + Math.PI;
        }
    }

    public partial class Form1 : Form
    {
        Graphics g;
        Bitmap bitmap;

        Coordinates curr_point;
        int point_rad = 5;
        List<Coordinates> coordinateslist = new List<Coordinates>();
        int my_fl = 0;

        public Form1()
        {
            InitializeComponent();

            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            pictureBox1.Image = bitmap;

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            if (!button2.Enabled)
            {
                MouseEventArgs mouse = (MouseEventArgs)e;
                if (!button2.Enabled)
                {
                    curr_point = new Coordinates(new Point(mouse.X, mouse.Y));
                    coordinateslist.Add(curr_point);

                    draw_point(curr_point, point_rad, Color.DarkOrange);
                    pictureBox1.Refresh();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
            my_fl++;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;

            // если добавили точки, то очистили экран
            if (my_fl > 1)
                g.Clear(Color.White);

            if (coordinateslist.Count() < 2)
                return;
            else if (coordinateslist.Count() == 2)
            {
                draw_line(coordinateslist[0], coordinateslist[1]);
                pictureBox1.Refresh();
                return;
            }

            List<Coordinates> points_to_draw = alg_Graham();

            for (int i = 0; i < points_to_draw.Count() - 1; i++) 
                draw_line(points_to_draw[i], points_to_draw[i + 1]);
            draw_line(points_to_draw[points_to_draw.Count() - 1], points_to_draw[0]);

            for (int i = 0; i < coordinateslist.Count; i++)
                draw_point(coordinateslist[i], point_rad, Color.DarkOrange);

            pictureBox1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pictureBox1.Refresh();

            coordinateslist.Clear();
            curr_point = new Coordinates();
            my_fl = 0;

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = false;
        }

        private void draw_point(Coordinates p, int r, Color color)
        {
            g.FillEllipse(new SolidBrush(color), (float)p.X - r, (float)p.Y - r, 2 * r, 2 * r);
        }

        private void draw_line(Coordinates p1, Coordinates p2)
        {
            g.DrawLine(new Pen(Color.DarkCyan, 3), (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        enum RemovalFlag
        {
            None,
            MidPoint,
            EndPoint
        };

        static RemovalFlag WhichToRemoveFromBoundary(Coordinates p1, Coordinates p2, Coordinates p3)
        {
            double cross = 0;
            double cross1 = (p2.X - p1.X) * (p3.Y - p1.Y);
            double cross2 = (p2.Y - p1.Y) * (p3.X - p1.X);
            if (NearlyEqual(cross1, cross2))
                cross = 0;
            else cross = cross1 - cross2;

            // Убираем точку посередине p2
            if (cross < 0)
                return RemovalFlag.MidPoint;
            // Ничего не убираем
            if (cross > 0)
                return RemovalFlag.None;

            // Разница между векторами
            var dotp = (p3.X - p2.X) * (p2.X - p1.X) + (p3.Y - p2.Y) * (p2.Y - p1.Y);
            if (NearlyEqual(dotp, 0.0))
                // Убираем точку посередине p2
                return RemovalFlag.MidPoint;
            if (dotp < 0)
                // Убираем правую p3
                return RemovalFlag.EndPoint;
            else
                // Убираем точку посередине p2
                return RemovalFlag.MidPoint;
        }

        public static bool NearlyEqual(Coordinates a, Coordinates b)
        {
            return NearlyEqual(a.X, b.X) && NearlyEqual(a.Y, b.Y);
        }

        public static bool NearlyEqual(double a, double b)
        {
            return NearlyEqual(a, b, 1e-10);
        }

        public static bool NearlyEqual(double a, double b, double epsilon)
        {
            if (a == b)
                return true;

            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);
            double sum = absA + absB;
            if (diff < 4 * double.Epsilon || sum < 4 * double.Epsilon)
                return true;

            return diff / (absA + absB) < epsilon;
        }

        private List<Coordinates> alg_Graham()
        {
            // Выбираем начальную точку, минимальную по Y
            int iMin = Enumerable.Range(0, coordinateslist.Count).Aggregate((jMin, jCur) =>
            {
                if (coordinateslist[jCur].Y < coordinateslist[jMin].Y)
                    return jCur;
                if (coordinateslist[jCur].Y > coordinateslist[jMin].Y)
                    return jMin;
                if (coordinateslist[jCur].X < coordinateslist[jMin].X)
                    return jCur;
                return jMin;
            });

            // Сортируем по полярному углу относительно начальной
            var sortQuery = Enumerable.Range(0, coordinateslist.Count)
                .Where((i) => (i != iMin))
                .Select((i) => new KeyValuePair<double, Coordinates>(Math.Atan2(coordinateslist[i].Y - coordinateslist[iMin].Y, coordinateslist[i].X - coordinateslist[iMin].X), coordinateslist[i]))
                .OrderBy((pair) => pair.Key)
                .Select((pair) => pair.Value);
            List<Coordinates> points = new List<Coordinates>(coordinateslist.Count);
            points.Add(coordinateslist[iMin]);   
            points.AddRange(sortQuery);

            int M = 0;
            for (int i = 1; i < points.Count(); i++)
            {
                bool keepNewPoint = true;
                if (M == 0)
                {
                    // Ищем точку, отличную от начальной
                    keepNewPoint = !NearlyEqual(points[0], points[i]);
                }
                else
                {
                    while (true)
                    {
                        // Флаг для удаления точки из списка
                        var flag = WhichToRemoveFromBoundary(points[M - 1], points[M], points[i]);
                        if (flag == RemovalFlag.None)
                            break;
                        else if (flag == RemovalFlag.MidPoint)
                        {
                            if (M > 0)
                                M--;
                            if (M == 0)
                                break;
                        }
                        else if (flag == RemovalFlag.EndPoint) // дошли до конца
                        {
                            keepNewPoint = false;
                            break;
                        }
                        else throw new Exception("Don't know this RemovalFlag");
                    }
                }
                if (keepNewPoint)
                {
                    M++;
                    if (i != M)
                    {
                        var temp = points[M];
                        points[M] = points[i];
                        points[i] = temp;
                    }
                }
            }
            // points[M] - список точек оболочки, удаляем остальное
            points.RemoveRange(M + 1, points.Count - M - 1);
            return points;
        }
    }
}


/*
private List<int> alg_Graham()
{
    int cnt = points.Count;

    // выбираем начальную точку 
    var p0 = points[0];
    foreach (var p in points)
        if (p.X < p0.X || (p.X == p0.X && p.Y < p0.Y))
            p0 = p;

    // сортировка по полярному углу
    points.Sort(delegate (Point a, Point b)
    {
        return (a.X - p0.X) * (b.Y - p0.Y) - (b.X - p0.X) * (a.Y - p0.Y)
    });

    return new List<int> { };
}

public static List<Point> ConvexHull(List<Point> points)
{
    if (points.Count < 3)
    {
        throw new ArgumentException("At least 3 points reqired", "points");
    }

    List<Point> hull = new List<Point>();

    // get leftmost point
    Point vPointOnHull = points.Where(p => p.X == points.Min(min => min.X)).First();

    Point vEndpoint;
    do
    {
        hull.Add(vPointOnHull);
        vEndpoint = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            if ((vPointOnHull == vEndpoint)
                || (Orientation(vPointOnHull, vEndpoint, points[i]) == -1))
            {
                vEndpoint = points[i];
            }
        }

        vPointOnHull = vEndpoint;

    }
    while (vEndpoint != hull[0]);

    return hull;
}

private static int Orientation(Point p1, Point p2, Point p)
{
    // Determinant
    int Orin = (p2.X - p1.X) * (p.Y - p1.Y) - (p.X - p1.X) * (p2.Y - p1.Y);

    if (Orin > 0)
        return -1; // (* Orientation is to the left-hand side  *)
    if (Orin < 0)
        return 1; // (* Orientation is to the right-hand side *)

    return 0; //  (* Orientation is neutral aka collinear  *)
}




            List<int> point_Ind = new List<int>();
            for (int i = 0; i < cnt; i++)
                point_Ind.Add(i);
            // выбираем начальную точку 
            for (int i = 0; i < cnt; i++)
            {
                if (points[i].X > points[0].X || (points[i].X == points[0].X && points[i].Y < points[0].Y))
                {
                    var tmp = point_Ind[i];
                    point_Ind[i] = point_Ind[0];
                    point_Ind[0] = tmp;
                }
            }
 
 

            for (int i = 1; i < cnt; i++)
            {
                int j = i;
                while (j > 1 && my_point_check(points[point_Ind[0]], points[point_Ind[j - 1]], points[point_Ind[j]]) < 0)
                {
                    var tmp = point_Ind[j - 1];
                    point_Ind[j - 1] = point_Ind[j];
                    point_Ind[j] = tmp;

                    j--;
                }
            }




            List<int> new_point = new List<int>();
            new_point.Add(point_Ind[0]);
            new_point.Add(point_Ind[1]);

            // срезание углов
            for (int i = 2; i < cnt; i++)
            {
                while (my_point_check(points[new_point[new_point.Count - 2]], points[new_point[new_point.Count - 1]], points[point_Ind[i]]) < 0)
                    new_point.RemoveAt(new_point.Count - 1);
                new_point.Add(point_Ind[i]);
            }

            return new_point;
 */
