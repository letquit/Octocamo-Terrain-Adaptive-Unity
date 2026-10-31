using System.Collections.Generic;
using UnityEngine;

public class CamoDatabase
{
    private Dictionary<Texture2D, Color> camoColours = new Dictionary<Texture2D, Color>();

    private static CamoDatabase instance = null;

    public static CamoDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CamoDatabase();
            }
            return instance;
        }
    }

    public Color GetCamoColour(Texture2D texture)
    {
        if (texture == null)
        {
            return Color.gray;
        }
        if (camoColours.ContainsKey(texture))
        {
            return camoColours[texture];
        }
        return AddCamoColour(texture);
    }

    public Color AddCamoColour(Texture2D texture)
    {
        if (texture == null)
        {
            return Color.gray;
        }
        if (camoColours.ContainsKey(texture))
        {
            return camoColours[texture];
        }

        var color = ComputeAverageColour(texture);
        camoColours.Add(texture, color);
        return color;
    }

    private Color ComputeAverageColour(Texture2D texture)
    {
        // 纹理若在导入设置里没有勾选 Read/Write，GetPixels 会抛异常
        //（错误类型是 ArgumentException，不是 UnityException，所以必须捕获基类 Exception）；
        // 此时退回用 GPU 缩到 8x8 再读回，保证迷彩指数依然能算出来。
        try
        {
            var pixels = texture.GetPixels();
            if (pixels != null && pixels.Length > 0)
            {
                return Average(pixels);
            }
        }
        catch (System.Exception)
        {
            // 不可读 / 被压缩的纹理，走 GPU 回读
        }

        try
        {
            return AverageFromGPU(texture);
        }
        catch (System.Exception)
        {
            return Color.gray;
        }
    }

    private static Color Average(Color[] pixels)
    {
        Vector3 color = Vector3.zero;
        for (int i = 0; i < pixels.Length; ++i)
        {
            color.x += pixels[i].r;
            color.y += pixels[i].g;
            color.z += pixels[i].b;
        }
        color /= pixels.Length;
        return new Color(color.x, color.y, color.z);
    }

    private static Color AverageFromGPU(Texture texture)
    {
        RenderTexture rt = RenderTexture.GetTemporary(8, 8, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(texture, rt);
            RenderTexture.active = rt;

            var readable = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, 8, 8), 0, 0);
            readable.Apply();

            var average = Average(readable.GetPixels());
            Object.Destroy(readable);
            return average;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}
