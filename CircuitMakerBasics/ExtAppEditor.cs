using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CircuitMaker.Basics;

namespace CircuitMaker.GUI.ExtApp
{
    public partial class ExtAppEditor : UserControl
    {
        private IBoardContainerComponent boardContainerComp;
        public ColourScheme colourScheme;

        public ExtAppEditor(IBoardContainerComponent boardContainerComp, ColourScheme colourScheme)
        {
            InitializeComponent();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            Graphics graphics = e.Graphics;

            Matrix matrix = new Matrix();

            RectangleF rect = boardContainerComp.GetShape();
            matrix.Translate(-rect.X, -rect.Y);

            graphics.MultiplyTransform(matrix);

            boardContainerComp.RenderMainShape(graphics, false, colourScheme);

            matrix.Invert();
            graphics.MultiplyTransform(matrix);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            Matrix matrix = new Matrix();

            RectangleF rect = boardContainerComp.GetShape();
            matrix.Translate(-rect.X, -rect.Y);

            graphics.MultiplyTransform(matrix);

            boardContainerComp.Render(graphics, false, colourScheme);

            matrix.Invert();
            graphics.MultiplyTransform(matrix);
        }
    }
}
