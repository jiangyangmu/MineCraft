using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PutBlock
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public void LockMouse()
        {
            isMouseLocked = true;
            Cursor.Hide();
            MouseMove += DoLockMouse;
        }
        public void UnlockMouse()
        {
            MouseMove -= DoLockMouse;
            Cursor.Show();
            isMouseLocked = false;
        }

        public Point AbsMousePosition { get => absMousePos; }
        public bool IsMouseLocked { get => isMouseLocked; }

        private void DoLockMouse(object sender, MouseEventArgs e)
        {
            var rect = ClientRectangle;
            var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            absMousePos.X += Cursor.Position.X - center.X;
            absMousePos.Y += Cursor.Position.Y - center.Y;
            Cursor.Position = center;
        }

        private Point absMousePos = new Point();
        private bool isMouseLocked = false;
    }
}
