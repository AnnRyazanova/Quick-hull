using QuickHull.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickHull
{
    public partial class Form1 : Form
    {
        private Graphics graphics;

        private List<Point2D> points = new List<Point2D>();
        private List<Point2D> vertex;

        private Point2D lastPoint;

        private Primitive SelectedPrimitive
        {
            get
            {
                if (null == treeView1.SelectedNode) return null;
                var p = (Primitive)treeView1.SelectedNode.Tag;
                return p;
            }
            set
            {
                Redraw();
            }
        }

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(2048, 2048);
            graphics = Graphics.FromImage(pictureBox1.Image);
            graphics.Clear(Color.White);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedPrimitive = (Primitive)e.Node.Tag;
            if (SelectedPrimitive is Point2D)
                lastPoint = (Point2D)SelectedPrimitive;

            Redraw();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keys.Delete != e.KeyCode && Keys.Back != e.KeyCode) return;
            if (null == SelectedPrimitive) return;
            if (SelectedPrimitive is Point2D) points.Remove((Point2D)SelectedPrimitive);
            
            treeView1.SelectedNode.Remove();
            if (null != treeView1.SelectedNode)
                SelectedPrimitive = (Primitive)treeView1.SelectedNode.Tag;
            else
                SelectedPrimitive = null;
            Redraw();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs args = (MouseEventArgs)e;
            Point2D p = Point2D.FromPoint(args.Location);
            
            TreeNode node = treeView1.Nodes.Add("Точка");
            node.Tag = p;
            points.Add(p);
            Redraw();
        }

        private void Redraw()
        {
            graphics.Clear(Color.White);
            points.ForEach((p) => p.Draw(graphics, p == SelectedPrimitive));
            pictureBox1.Invalidate();
        }

        
        // Находим самую левую и самую правую точки
        private void findLeftAndRightPoints(out Point2D left, out Point2D right)
        {
            left = right = points[0];

            for (int i = 0; i < points.Count; ++i)
            {
                if (points[i].X < left.X)
                    left = points[i];
                else if (points[i].X > right.X)
                    right = points[i];
            }

        }

        // Находим все точки над ребром
        // Флаг нужен для понимания, какую полуплоскость мы проверяем относительно первого ребра
        private void findPointsAbove(Point2D left, Point2D right, out List<Point2D> above, out Point2D maxAbove, List<Point2D> ppoints, bool flag)
        {
            above = new List<Point2D>();

            if (flag)
                maxAbove = (left.Y > right.Y) ? left : right;
            else
                maxAbove = (left.Y < right.Y) ? left : right;


            Edge edgeBetweenLeftAndRight = new Edge(left, right);

            for (int i = 0; i < ppoints.Count; ++i)
            {
                if (ppoints[i] != left && ppoints[i] != right)
                {
                    if (edgeBetweenLeftAndRight.Distance(ppoints[i]) < 0)
                    {
                        // Точки над edge
                        above.Add(ppoints[i]);
                        // Находим максимально отдаленную вершину (если их несколько, то находим самую правую)
                        if (flag)
                        {
                            if ((maxAbove.Y > ppoints[i].Y) || (maxAbove.Y == ppoints[i].Y && maxAbove.X < ppoints[i].X))
                                maxAbove = ppoints[i];
                        }
                        else
                        {
                            if ((maxAbove.Y < ppoints[i].Y) || (maxAbove.Y == ppoints[i].Y && maxAbove.X > ppoints[i].X))
                                maxAbove = ppoints[i];
                        }

                    }
                }
            }
        }


        // left - левая точка ребра
        // right - правая точка ребра
        // p - точки, из которых будем выбирать те, что выше ребра
        // flag - если true, то проверяем все то, что находится выше самого первого ребра, false - то, что ниже
        private void quickHull(Point2D left, Point2D right, List<Point2D> p, bool flag)
        { 
            List<Point2D> aboveEdge;
            Point2D maxAbove;

			//(new Edge(left, right)).Draw(graphics);
			//pictureBox1.Refresh();

			findPointsAbove(left, right, out aboveEdge, out maxAbove, p, flag);

            if (aboveEdge.Count == 0)
                vertex.Add(right);
            else     
            {
                quickHull(left, maxAbove, aboveEdge, flag);
                quickHull(maxAbove, right, aboveEdge, flag);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (points.Count < 3)
                MessageBox.Show("Должно быть хотя бы 3 точки");
            else
            {
                vertex = new List<Point2D>();

                Point2D left, right;
                findLeftAndRightPoints(out left, out right);
				
				quickHull(left, right, points, true);
                quickHull(right, left, points, false);

                for (int i = 0; i < vertex.Count - 1; ++i)
                    (new Edge(vertex[i], vertex[i + 1])).Draw(graphics);
                (new Edge(vertex[vertex.Count - 1], vertex[0])).Draw(graphics);
                pictureBox1.Refresh();

            }
        }
    }
}
