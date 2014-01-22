﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FerretLib.WinForms.Controls
{
    internal partial class TagLabelControl : UserControl
    {
        public delegate void TagEvent(object sender, string tag);
        public event TagEvent DeleteClicked;
        public event TagEvent DoubleClicked;

        #region ctor
        public TagLabelControl() : this("New Tag") { }
        public TagLabelControl(string value)
        {
            InitializeComponent();
            Value = value;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            backbufferContext = BufferedGraphicsManager.Current;

            RecreateBuffers();
            Redraw();
        }

        protected override void Dispose(bool disposing)
        {                                
            isDisposing = true;
            if (disposing)
            {                
                if (backbufferGraphics != null) backbufferGraphics.Dispose();
                if (backbufferContext != null) backbufferContext.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Properties
        public string Value
        {
            get
            {
                return lblText.Text;
            }
            set
            {
                lblText.Text = value;
                ResizeControl();
            }
        }

        #endregion

        #region Input events
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (DeleteClicked != null) DeleteClicked(this, Value);
        }

        private void lblText_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleClicked != null) DoubleClicked(this, Value);
        }
        #endregion        

        #region Rendering
        private bool isDisposing;
        private BufferedGraphicsContext backbufferContext;
        private BufferedGraphics backbufferGraphics;
        private Graphics canvas;

        private void ResizeControl()
        {
            var width = lblText.Width + 18;
            var height = 20;
            MaximumSize = new Size(width, height);
            MinimumSize = MaximumSize;
            Width = width;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecreateBuffers();
            Redraw();
        }

        private void RecreateBuffers()
        {
            if (backbufferContext == null || isDisposing) return;

            backbufferContext.MaximumBuffer = new Size(Math.Max(1, Width), Math.Max(1, Height));
            if (backbufferGraphics != null) backbufferGraphics.Dispose();

            backbufferGraphics = backbufferContext.Allocate(this.CreateGraphics(),
                new Rectangle(0, 0, Math.Max(Width, 1), Math.Max(Height, 1)));

            canvas = backbufferGraphics.Graphics;
            canvas.SmoothingMode = SmoothingMode.None;
            Invalidate();
        }

        private void Redraw()
        {
            var font = new Font(FontFamily.GenericSerif, 14, FontStyle.Bold);
            if (canvas == null) return;
            canvas.Clear(Color.Transparent);
            canvas.DrawString("Test", font, new SolidBrush(Color.Red), 0, 0);
            // Force the control to both invalidate and update. 
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // If we've initialized the backbuffer properly, render it on the control. 
            // Otherwise, do just the standard control paint.
            if (!isDisposing && backbufferGraphics != null)
                backbufferGraphics.Render(e.Graphics);
        }

        #endregion
    }
}
