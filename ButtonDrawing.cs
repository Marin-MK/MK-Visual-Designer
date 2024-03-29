﻿

namespace VisualDesigner;

public class ButtonDrawing : Widget
{
    public string Text { get; protected set; }
    public Font Font { get; protected set; }
    public bool Enabled { get; protected set; } = true;

    int MaxWidth;

    public ButtonDrawing(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.ParagraphBold;

        Bitmap corner = DesignButton.ButtonCornerFade;
        Bitmap hor = DesignButton.ButtonHorizontalFade;
        Bitmap vert = DesignButton.ButtonVerticalFade;

        Sprites["topleft"] = new Sprite(this.Viewport, corner);
        Sprites["topleft"].DestroyBitmap = false;

        Sprites["bottomleft"] = new Sprite(this.Viewport, corner);
        Sprites["bottomleft"].MirrorY = true;
        Sprites["bottomleft"].DestroyBitmap = false;

        Sprites["topright"] = new Sprite(this.Viewport, corner);
        Sprites["topright"].MirrorX = true;
        Sprites["topright"].DestroyBitmap = false;

        Sprites["bottomright"] = new Sprite(this.Viewport, corner);
        Sprites["bottomright"].MirrorX = Sprites["bottomright"].MirrorY = true;
        Sprites["bottomright"].DestroyBitmap = false;

        Sprites["left"] = new Sprite(this.Viewport, hor);
        Sprites["left"].Y = corner.Width;
        Sprites["left"].DestroyBitmap = false;
        Sprites["right"] = new Sprite(this.Viewport, hor);
        Sprites["right"].Y = Sprites["left"].Y;
        Sprites["right"].MirrorX = true;
        Sprites["right"].DestroyBitmap = false;

        Sprites["top"] = new Sprite(this.Viewport, vert);
        Sprites["top"].X = corner.Width;
        Sprites["top"].DestroyBitmap = false;
        Sprites["bottom"] = new Sprite(this.Viewport, vert);
        Sprites["bottom"].X = Sprites["top"].X;
        Sprites["bottom"].MirrorY = true;
        Sprites["bottom"].DestroyBitmap = false;

        Sprites["filler"] = new Sprite(this.Viewport);
        Sprites["filler"].X = Sprites["filler"].Y = corner.Width;

        Sprites["text"] = new Sprite(this.Viewport);

        SetSize(85, 33);
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            RedrawText();
        }
    }
    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            RedrawFiller();
            RedrawText();
        }
    }

    public void RedrawText(bool Now = false)
    {
        Sprites["text"].Bitmap?.Dispose();
        if (string.IsNullOrEmpty(this.Text)) return;
        List<string> Lines = this.Text.Split('\n').ToList();
        MaxWidth = 0;
        Lines.ForEach(l => MaxWidth = Math.Max(MaxWidth, this.Font.TextSize(l).Width));
        Sprites["text"].Bitmap = new Bitmap(MaxWidth, Size.Height);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        Color c = this.Enabled ? Color.WHITE : new Color(147, 158, 169);
        for (int i = 0; i < Lines.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Lines[i], MaxWidth / 2, i * 18, c, DrawOptions.CenterAlign);
        }
        Sprites["text"].Bitmap.Lock();
        Sprites["text"].X = Size.Width / 2 - MaxWidth / 2;
        Sprites["text"].Y = Size.Height / 2 - 9 * Lines.Count - Font.Size / 2 + 4;
    }

    public void RedrawFiller()
    {
        Sprites["filler"].Bitmap?.Dispose();
        int w = Size.Width - Sprites["filler"].X * 2;
        int h = Size.Height - Sprites["filler"].Y * 2;
        Sprites["filler"].Bitmap = new Bitmap(w, h);
        Sprites["filler"].Bitmap.Unlock();
        if (this.Enabled) Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(51, 86, 121));
        else Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(51, 86, 121));
        Sprites["filler"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawFiller();

        int o = Sprites["filler"].X;

        Sprites["bottomleft"].Y = Size.Height - o;
        Sprites["topright"].X = Size.Width - o;
        Sprites["bottomright"].X = Sprites["topright"].X;
        Sprites["bottomright"].Y = Sprites["bottomleft"].Y;

        Sprites["right"].X = Sprites["topright"].X;
        Sprites["left"].ZoomY = Size.Height - 2 * o;
        Sprites["right"].ZoomY = Sprites["left"].ZoomY;

        Sprites["top"].ZoomX = Size.Width - 2 * o;
        Sprites["bottom"].Y = Sprites["bottomleft"].Y;
        Sprites["bottom"].ZoomX = Sprites["top"].ZoomX;

        if (!string.IsNullOrEmpty(this.Text))
        {
            Sprites["text"].X = Size.Width / 2 - MaxWidth / 2;
            Sprites["text"].Y = Size.Height / 2 - 9 * this.Text.Split('\n').Length - Font.Size / 2 + 4;
        }
    }
}
