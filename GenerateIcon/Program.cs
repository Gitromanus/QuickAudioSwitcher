using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main()
    {
        string outputPath = Path.Combine(Environment.CurrentDirectory, "QuickAudioSwitcher.ico");
        
        using var bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // Speaker body (trapezoid)
        using var brush = new SolidBrush(Color.White);
        var pts = new PointF[] { new(6, 10), new(14, 10), new(20, 4), new(20, 28), new(14, 22), new(6, 22) };
        g.FillPolygon(brush, pts);

        // Sound waves
        using var pen = new Pen(Color.White, 2);
        g.DrawArc(pen, 20, 8, 10, 16, 290, 140);
        g.DrawArc(pen, 26, 4, 14, 24, 290, 140);

        // Save as ICO with multiple sizes
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // ICO header
        bw.Write((short)0); // reserved
        bw.Write((short)1); // ICO type
        bw.Write((short)1); // number of images

        // ICO directory entry
        bw.Write((byte)32); // width
        bw.Write((byte)32); // height
        bw.Write((byte)0);  // colors
        bw.Write((byte)0);  // reserved
        bw.Write((short)1); // planes
        bw.Write((short)32); // bpp

        // Save PNG data
        byte[] pngData;
        using (var pms = new MemoryStream())
        {
            bmp.Save(pms, ImageFormat.Png);
            pngData = pms.ToArray();
        }

        bw.Write(pngData.Length); // size
        bw.Write(22); // offset (header + directory entry)

        // Write PNG data
        bw.Write(pngData);

        File.WriteAllBytes(outputPath, ms.ToArray());
        Console.WriteLine($"Icon saved to: {outputPath}");
    }
}