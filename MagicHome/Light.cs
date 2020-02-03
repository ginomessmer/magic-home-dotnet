using System;
using System.Drawing;

namespace MagicHome
{
    public class Light
    {
        public bool Connected { get; set; }

        public bool IsOn { get; set; }

        public Color Color { get; set; }

        public Light()
        {
        }
    }
}
