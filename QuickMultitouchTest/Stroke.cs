using System.Drawing;
using System.Collections;

namespace PeterPetkov.QuickMultitouchTest
{
    public class Stroke
    {
        private ArrayList points;               
        private Color color;                    
        private int id;                         

        private float penWidth;    
        public Stroke(float penWidth)
        {
            points = new ArrayList();
            this.penWidth = penWidth;
        }
        public void Draw(Graphics graphics)
        {
            if ((points.Count < 2) || (graphics == null))
            {
                return;
            }

            Pen pen = new Pen(color, penWidth);
            graphics.DrawLines(pen, (Point[]) points.ToArray(typeof(Point)));
        }
        public void DrawLast(Graphics graphics)
        {
            if ((points.Count < 2) || (graphics == null))
            {
                return;
            }

            Pen pen = new Pen(color, penWidth);
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            graphics.DrawLine(pen, (Point)points[points.Count - 2], (Point)points[points.Count - 1]);
        }
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public void Add(Point pt)
        {
            points.Add(pt);
        }

    }
    
    public class CollectionOfStrokes
    {
        
        public CollectionOfStrokes()
        {
            strokes = new ArrayList();
        }
        public void Draw(Graphics graphics)
        {
            foreach (Stroke stroke in strokes)
            {
                stroke.Draw(graphics);
            }
        }
        public void Add(Stroke stroke)
        {
            strokes.Add(stroke);
        }
        public Stroke Get(int id)
        {
            int i = _IndexFromId(id);
            if (i == -1)
            {
                return null;
            }
            return (Stroke)strokes[i];
        }
        public Stroke Remove(int id)
        {
            int i = _IndexFromId(id);
            if (i == -1)
            {
                return null;
            }
            Stroke s = (Stroke)strokes[i];
            strokes.RemoveAt(i);
            return s;
        }

        public void Clear()
        {
            
            strokes.Clear();
        }
        private int _IndexFromId(int id)
        {
            for (int i = 0; i < strokes.Count; ++i)
            {
                Stroke stroke = (Stroke)strokes[i];
                if (id == stroke.Id)
                {
                    return i;
                }
            }
            return -1;
        }
        private ArrayList strokes;          
    }
}
