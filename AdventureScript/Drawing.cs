namespace AdventureScript
{
    public interface IDrawingSink
    {
        void DrawRectangle(
            int left,
            int top,
            int width,
            int height,
            int fillColor,
            int strokeColor,
            int strokeThickness
            );
        void DrawEllipse(
            int left,
            int top,
            int width,
            int height,
            int fillColor,
            int strokeColor,
            int strokeThickness
            );
    }

    public sealed class Drawing
    {
        List<IShape> m_shapes = new List<IShape>();

        public Drawing(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        internal void AddShape(IShape shape)
        {
            m_shapes.Add(shape);
        }

        public int Width { get; }
        public int Height { get; }

        public void Draw(IDrawingSink sink)
        {
            foreach (var shape in m_shapes)
            {
                shape.Draw(sink);
            }
        }
    }

    interface IShape
    {
        void Draw(IDrawingSink sink);
    }

    sealed class Rectangle : IShape
    {
        int m_left;
        int m_top;
        int m_width;
        int m_height;
        int m_fillColor;
        int m_strokeColor;
        int m_strokeThickness;

        public Rectangle(
            int left,
            int top,
            int width,
            int height,
            int fillColor,
            int strokeColor,
            int strokeThickness
            )
        {
            m_left = left;
            m_top = top;
            m_width = width;
            m_height = height;
            m_fillColor = fillColor;
            m_strokeColor = strokeColor;
            m_strokeThickness = strokeThickness;
        }

        public void Draw(IDrawingSink sink)
        {
            sink.DrawRectangle(
                m_left,
                m_top,
                m_width,
                m_height,
                m_fillColor,
                m_strokeColor,
                m_strokeThickness
                );
        }
    }

    sealed class Ellipse : IShape
    {
        int m_left;
        int m_top;
        int m_width;
        int m_height;
        int m_fillColor;
        int m_strokeColor;
        int m_strokeThickness;

        public Ellipse(
            int left,
            int top,
            int width,
            int height,
            int fillColor,
            int strokeColor,
            int strokeThickness
            )
        {
            m_left = left;
            m_top = top;
            m_width = width;
            m_height = height;
            m_fillColor = fillColor;
            m_strokeColor = strokeColor;
            m_strokeThickness = strokeThickness;
        }

        public void Draw(IDrawingSink sink)
        {
            sink.DrawEllipse(
                m_left,
                m_top,
                m_width,
                m_height,
                m_fillColor,
                m_strokeColor,
                m_strokeThickness
                );
        }
    }
}
