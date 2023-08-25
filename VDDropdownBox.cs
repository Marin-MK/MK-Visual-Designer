namespace VisualDesigner;

public class VDDropdownBox : RPGStudioMK.Widgets.DropdownBox
{
	public VDDropdownBox(IContainer Parent) : base(Parent) { }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Color ArrowColor = new Color(86, 108, 134);
        Color ArrowShadow = new Color(17, 27, 38);
        int x = Size.Width - 18;
        int y = Size.Height / 2 - 4;
        if (DropdownWidget != null) y -= 5;
        Sprites["bg"].Bitmap.FillRect(x, y, 11, 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 3, y + 4, x + 7, y + 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 5, x + 6, y + 5, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 5, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x, y + 2, x + 5, y + 7, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x, y + 3, x + 5, y + 8, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 6, x + 10, y + 2, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 7, x + 10, y + 3, ArrowShadow);
        if (DropdownWidget != null) Sprites["bg"].Bitmap.FlipVertically(x, y, 11, 11);
        Sprites["bg"].Bitmap.Lock();
        this.Drawn = true;
    }
}
