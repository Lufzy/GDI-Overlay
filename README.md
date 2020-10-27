# [C#] GDI+ Overlay

- [EN] GDI Overlay is advanced overlay class.
- [TR] GDI Overlay gelişmiş bir overlay sınıfıdır.

## Example Usage / Örnek Kullanım



```csharp
string ProcessName = "csgo";
GDIOverlay.Initialize(ProcessName );
GDIOverlay.Overlay.Paint += new PaintEventHandler(Drawing_Event);
```

```csharp
private void Drawing_Event(object sender, PaintEventArgs e)
{
   if(GDIOverlay.OverlayIsShowed)
   {
       e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
       e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
       e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

       GDIOverlay.Drawing.DrawOutlineText(e.Graphics, "GDI Overlay+", 20, 20, new Font("Consolas", 15f), Brushes.Red, Brushes.Black);   // Style 1
       GDIOverlay.Drawing.DrawOutlineText(e.Graphics, "GDI Overlay+", new Font("Consolas", 15f), 3f,  20, 40, Brushes.Red, Color.Black); // Style 2
       GDIOverlay.Drawing.FillCircle(e.Graphics, Brushes.Red, GDIOverlay.Overlay.Width / 2, GDIOverlay.Overlay.Height / 2, 30);
       GDIOverlay.Drawing.DrawCircle(e.Graphics, new Pen(Brushes.White, 2f), GDIOverlay.Overlay.Width / 2, GDIOverlay.Overlay.Height / 2, 30);
   }
}
```

## Output / Çıktı

![alt text](https://raw.githubusercontent.com/Lufzy/GDI-Overlay/main/example_output.PNG?raw=true)
## License
[MIT](https://choosealicense.com/licenses/mit/)
