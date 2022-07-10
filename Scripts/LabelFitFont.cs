using Godot;
using System;

public class LabelFitFont : Label
{
    private DynamicFont font;
    private int originalSize;
    private int minSize = 2;
    public override void _EnterTree()
    {
        font = Get("custom_fonts/font") as DynamicFont;
        originalSize = font.Size;
    }

    public void FitOnBox()
    {
        font.Size = originalSize;
        if (Autowrap)
        { // MultiLine Mode check the count of visible lines and real lines
            while (GetVisibleLineCount() < GetLineCount())
            {
                font.Size -= 1;
                if (font.Size <= minSize)
                {
                    font.Size = minSize;
                    break;
                }
            }
        }
        else
        { // Single Line Check for the string size at this font
            string text = Tr(Text);
            var rectSize = font.GetStringSize(text);

            while (rectSize.y > RectSize.y || rectSize.x > RectSize.x)
            {
                text = Tr(Text);
                rectSize = font.GetStringSize(text);
                font.Size -= 1;
                if (font.Size <= minSize)
                {
                    font.Size = minSize;
                    break;
                }
            }
        }
    }
    //  }
}
