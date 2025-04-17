namespace System.Numerics
{
    internal class Rectangle
    {
        private int v1;
        private int v2;
        private int width;
        private int height;
        private float x;
        private float y;

        public Rectangle(int v1, int v2, int width, int height)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.width = width;
            this.height = height;
        }

        public Rectangle(float x, float y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }
}