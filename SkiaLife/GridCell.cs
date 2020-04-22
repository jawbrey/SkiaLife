using System;
namespace SkiaLife
{
    public class GridCell
    {
        public bool Alive { get; set; }
        public long EventID { get; set; } = long.MinValue;

        public void Tapped(long id)
        {
            if (id != EventID)
            {
                id = EventID;
                Alive = !Alive;
            }
        }
    }
}
