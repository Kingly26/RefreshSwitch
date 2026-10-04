using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

// Blackletter glyphs drawn by sweeping a slanted flat nib along a skeleton, like a calligraphy pen.
static class Gothic
{
    class Stroke { public float[] P; public float Angle, Width; }
    class Glyph { public float Adv; public List<Stroke> Strokes = new List<Stroke>(); }

    const float NibAngle = 38f, NibWidth = 2.0f;
    static readonly Dictionary<char, Glyph> Glyphs = Build();

    static Glyph G(float adv, params float[][] strokes)
    {
        var g = new Glyph { Adv = adv };
        foreach (var s in strokes) g.Strokes.Add(new Stroke { P = s, Angle = NibAngle, Width = NibWidth });
        return g;
    }
    static float[] S(params float[] pts) { return pts; }
    static Glyph Add(this Glyph g, float angle, float width, params float[][] strokes)
    {
        foreach (var s in strokes) g.Strokes.Add(new Stroke { P = s, Angle = angle, Width = width });
        return g;
    }

    // copy of a glyph scaled by k and moved by (dx, dy)
    static void Place(Glyph into, Glyph src, float k, float dx, float dy)
    {
        foreach (var s in src.Strokes)
        {
            var p = new float[s.P.Length];
            for (int i = 0; i < p.Length; i += 2) { p[i] = s.P[i] * k + dx; p[i + 1] = s.P[i + 1] * k + dy; }
            into.Strokes.Add(new Stroke { P = p, Angle = s.Angle, Width = s.Width * k });
        }
    }

    // skeletons on a grid 10 units tall, y down
    static Dictionary<char, Glyph> Build()
    {
        var d = new Dictionary<char, Glyph>();
        d['0'] = G(6.6f, S(2.6f,0.9f, 5,2.2f, 5,7.8f, 3.4f,9.1f, 1,7.8f, 1,2.2f, 2.6f,0.9f));
        d['1'] = G(5.4f, S(1.2f,2.6f, 2.9f,1.0f, 2.9f,8.3f), S(1.8f,7.9f, 4.0f,9.2f));
        d['2'] = G(6.6f, S(1,2.4f, 2.6f,0.9f, 5,2.2f, 5,4.0f, 1.1f,7.9f, 2.5f,9.0f, 5.3f,9.0f));
        d['3'] = G(6.6f, S(1,2.2f, 2.6f,0.9f, 5,2.2f, 5,3.7f, 3,4.9f, 5,6.1f, 5,7.8f, 3.4f,9.1f, 1,7.8f));
        d['4'] = G(6.8f, S(0.9f,6.3f, 4.3f,0.9f, 4.3f,8.3f), S(0.9f,6.3f, 5.8f,6.3f), S(3.2f,7.9f, 5.4f,9.2f));
        d['5'] = G(6.6f, S(5.2f,1.2f, 1.2f,1.2f, 1.2f,4.7f, 2.8f,3.6f, 5,4.8f, 5,7.8f, 3.4f,9.1f, 1,7.8f));
        d['6'] = G(6.6f, S(4.8f,2.0f, 2.6f,0.9f, 1,2.2f, 1,7.8f, 3.4f,9.1f, 5,7.8f, 5,5.7f, 3.2f,4.5f, 1,5.9f));
        d['7'] = G(6.4f, S(0.9f,1.2f, 5.2f,1.2f, 3.3f,4.4f, 3.0f,9.2f));
        d['8'] = G(6.6f, S(3,4.9f, 1.2f,3.8f, 1.2f,2.2f, 2.8f,0.9f, 4.8f,2.2f, 4.8f,3.8f, 3,4.9f),
                         S(3,4.9f, 1,6.0f, 1,7.8f, 3.4f,9.1f, 5,7.8f, 5,6.0f, 3,4.9f));
        d['9'] = G(6.6f, S(1.2f,8.0f, 3.4f,9.1f, 5,7.8f, 5,2.2f, 2.6f,0.9f, 1,2.2f, 1,4.3f, 2.8f,5.5f, 5,4.1f));
        d['W'] = G(9.6f, S(0.3f,1.8f, 1.2f,1.0f, 1.2f,7.6f, 3.0f,9.1f, 4.6f,7.8f),
                         S(3.8f,2.2f, 4.6f,1.4f, 4.6f,7.6f, 6.4f,9.1f, 8.2f,7.8f, 8.2f,1.6f, 7.2f,0.9f));
        d['L'] = G(6.6f, S(0.6f,1.8f, 1.7f,0.9f, 1.7f,7.9f, 3.0f,9.0f, 5.4f,9.0f, 5.9f,7.9f));
        d['+'] = G(7.0f, S(3.3f,2.2f, 3.3f,8.4f), S(0.6f,5.3f, 6.0f,5.3f));
        d['x'] = G(6.6f, S(1,2.4f, 5,8.4f), S(5,2.4f, 1,8.4f));

        // Wi-Fi: pointed (ogive) arches drawn with an upright nib so both sides match, over a diamond dot
        d['w'] = G(10.4f, S(4.6f,8.4f, 5.6f,9.0f))
            .Add(90, 1.8f, S(0.8f,4.9f, 1.8f,3.1f, 3.3f,1.8f, 5.1f,1.0f, 6.9f,1.8f, 8.4f,3.1f, 9.4f,4.9f),
                           S(2.7f,6.6f, 3.6f,5.2f, 5.1f,4.3f, 6.6f,5.2f, 7.5f,6.6f));
        // Ethernet plug: RJ45 outline with a latch on top and two pins
        // (the outline starts mid-base so it has no loose vertical end that would grow a thorn)
        d['e'] = new Glyph { Adv = 9.0f }
            .Add(NibAngle, 1.5f, S(4.5f,9.0f, 7.8f,9.0f, 7.8f,3.6f, 6.0f,3.6f, 6.0f,1.4f, 3.0f,1.4f, 3.0f,3.6f, 1.2f,3.6f, 1.2f,9.0f, 4.5f,9.0f))
            .Add(NibAngle, -1.3f, S(3.4f,5.4f, 3.4f,7.2f), S(5.6f,5.4f, 5.6f,7.2f));
        // Network tree: one node on top, three hanging below
        d['t'] = G(10.0f, S(5,1.6f, 5,8.2f), S(1.4f,8.2f, 1.4f,4.9f, 8.6f,4.9f, 8.6f,8.2f),
                          S(4.3f,1.2f, 5.7f,2.0f), S(0.7f,8.0f, 2.1f,8.9f), S(4.3f,8.0f, 5.7f,8.9f), S(7.9f,8.0f, 9.3f,8.9f));

        // both: Wi-Fi arches stacked over the wired symbol
        float[] outer = S(0.8f,3.9f, 1.8f,2.4f, 3.3f,1.4f, 5.1f,0.8f, 6.9f,1.4f, 8.4f,2.4f, 9.4f,3.9f);
        float[] inner = S(3.0f,5.2f, 3.9f,4.1f, 5.1f,3.5f, 6.3f,4.1f, 7.2f,5.2f);
        var b = new Glyph { Adv = 10.4f }.Add(90, 1.5f, outer, inner);
        Place(b, d['e'], 0.5f, 2.85f, 4.9f);
        d['b'] = b;
        d['c'] = new Glyph { Adv = 10.4f }.Add(90, 1.5f, outer, inner)
            .Add(NibAngle, 1.6f, S(5.1f,5.6f, 5.1f,9.0f), S(1.7f,9.0f, 1.7f,6.9f, 8.5f,6.9f, 8.5f,9.0f));
        return d;
    }

    static void Polys(string text, bool spikes, List<PointF[]> outPolys)
    {
        float x0 = 0;
        foreach (char ch in text)
        {
            Glyph g;
            if (!Glyphs.TryGetValue(ch, out g)) continue;
            foreach (var st in g.Strokes)
            {
                double a = st.Angle * Math.PI / 180;
                // a negative width marks a stroke that never grows thorns
                bool thorns = spikes && st.Width > 0;
                float width = Math.Abs(st.Width);
                float nx = (float)Math.Cos(a) * width / 2, ny = -(float)Math.Sin(a) * width / 2;
                var s = st.P;
                int n = s.Length / 2;
                for (int i = 0; i + 1 < n; i++)
                {
                    float ax = x0 + s[i * 2], ay = s[i * 2 + 1], bx = x0 + s[i * 2 + 2], by = s[i * 2 + 3];
                    outPolys.Add(new[] { new PointF(ax - nx, ay - ny), new PointF(ax + nx, ay + ny), new PointF(bx + nx, by + ny), new PointF(bx - nx, by - ny) });
                }
                if (thorns)
                {
                    // thorn at each end of the stroke, tapering along its direction
                    float reach = 1.4f * width;
                    Thorn(outPolys, x0 + s[2], s[3], x0 + s[0], s[1], nx, ny, reach);
                    Thorn(outPolys, x0 + s[(n - 2) * 2], s[(n - 2) * 2 + 1], x0 + s[(n - 1) * 2], s[(n - 1) * 2 + 1], nx, ny, reach);
                }
            }
            x0 += g.Adv;
        }
    }

    static void Thorn(List<PointF[]> polys, float fromX, float fromY, float tipX, float tipY, float nx, float ny, float reach)
    {
        float dx = tipX - fromX, dy = tipY - fromY, len = (float)Math.Sqrt(dx * dx + dy * dy);
        if (len < 0.01f) return;
        dx /= len; dy /= len;
        // only vertical-ish strokes get a thorn, so spikes point up and down and never hit the next glyph
        if (Math.Abs(dy) < 0.75f) return;
        dx *= 0.35f;
        polys.Add(new[] { new PointF(tipX - nx, tipY - ny), new PointF(tipX + nx, tipY + ny), new PointF(tipX + dx * reach, tipY + dy * reach) });
    }

    // draws text scaled to fit the box, centered
    public static void Draw(Graphics g, string text, RectangleF box, Color c, bool spikes)
    {
        var polys = new List<PointF[]>();
        Polys(text, spikes, polys);
        if (polys.Count == 0) return;
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in polys) foreach (var q in p)
        {
            minX = Math.Min(minX, q.X); maxX = Math.Max(maxX, q.X);
            minY = Math.Min(minY, q.Y); maxY = Math.Max(maxY, q.Y);
        }
        float sc = Math.Min(box.Width / (maxX - minX), box.Height / (maxY - minY));
        float ox = box.X + (box.Width - (maxX - minX) * sc) / 2 - minX * sc;
        float oy = box.Y + (box.Height - (maxY - minY) * sc) / 2 - minY * sc;
        // one path filled with the winding rule, so overlapping pieces merge without antialiasing seams
        using (var path = new GraphicsPath(FillMode.Winding))
        using (var b = new SolidBrush(c))
        {
            foreach (var p in polys)
            {
                var t = new PointF[p.Length];
                for (int i = 0; i < p.Length; i++) t[i] = new PointF(ox + p[i].X * sc, oy + p[i].Y * sc);
                // same orientation for every piece, or opposite windings would cancel into holes
                float area = 0;
                for (int i = 0; i < t.Length; i++) { var u = t[i]; var v = t[(i + 1) % t.Length]; area += u.X * v.Y - v.X * u.Y; }
                if (area < 0) Array.Reverse(t);
                path.AddPolygon(t);
            }
            g.FillPath(b, path);
        }
    }
}
