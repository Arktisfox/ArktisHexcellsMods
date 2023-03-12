using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HexcellsMpMod
{
    class Utils
    {
		public static Texture2D MakeBoxShadow(int res)
		{
			Texture2D texture2D = new Texture2D(res, res, TextureFormat.RGBA32, false);
			for (int i = 0; i < res / 2; i++)
			{
				float a = (float)i / (float)res;
				for (int j = i; j < res - i; j++)
				{
					Color color = new Color(0f, 0f, 0f, a);
					texture2D.SetPixel(j, i, color);
					texture2D.SetPixel(j, texture2D.height - 1 - i, color);
					texture2D.SetPixel(texture2D.width - 1 - i, j, color);
					texture2D.SetPixel(i, j, color);
				}
			}
			texture2D.Apply();
			return texture2D;
		}

		public static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pixels = new Color[width * height];
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = col;
			}
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			return texture2D;
		}
	}
}
