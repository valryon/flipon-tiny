using System;
using UnityEngine;

namespace PhotoshopFile
{
	public class ImageDecoder
	{
		/// <summary>
		/// Decodes a <see cref="Layer"/> into a <see cref="Texture2D"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to decode.</param>
		/// <returns>The <see cref="Texture2D"/> decoded from the layer.</returns>
		public static Texture2D DecodeImage(Layer layer)
		{
			if (layer.Rect.width == 0 || layer.Rect.height == 0)
			{
				return null;
			}

			var texture = new Texture2D((int)layer.Rect.width, (int)layer.Rect.height, TextureFormat.ARGB32, false);

			var colors = new Color32[(int)(layer.Rect.width * layer.Rect.height)];

			for (var y = 0; y < layer.Rect.height; ++y)
			{
				var layerRow = y * (int)layer.Rect.width;

				// we need to reverse the Y position for the Unity texture
				var textureRow = ((int)layer.Rect.height - 1 - y) * (int)layer.Rect.width;

				for (var x = 0; x < layer.Rect.width; ++x)
				{
					var layerPosition = layerRow + x;
					var texturePosition = textureRow + x;

					GetColor(layer, layerPosition, ref colors[texturePosition]);
					colors[texturePosition].a = 255;

					// set the alpha
					if (layer.ChannelN1 != null)
					{
						colors[texturePosition].a = layer.ChannelN1.ImageData[layerPosition];
					}

					// set the alpha
					if (layer.ChannelN2 != null)
					{
						var color = GetColor(layer.Masks.LayerMask, x, y);
						colors[texturePosition].a = (byte)(colors[texturePosition].a * color);
					}
				}
			}

			texture.SetPixels32(colors);
			return texture;
		}

		/// <summary>
		/// Gets the color at the given position in the given <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to sample.</param>
		/// <param name="position">The position to sample.</param>
		/// <param name="color">Information is writen here for performance</param>
		/// <returns>The sampled color.</returns>
		private static void GetColor(Layer layer, int position, ref Color32 color)
		{
			color.a = 1;
			switch (layer.PsdFile.ColorMode)
			{
				case PsdColorMode.Grayscale:
				case PsdColorMode.Duotone:
					color.r = color.g = color.b = layer.Channel0.ImageData[position];
					break;
				case PsdColorMode.Indexed:
					int index = layer.Channel0.ImageData[position];
					color.r = layer.PsdFile.ColorModeData[index];
					color.g = layer.PsdFile.ColorModeData[index + 256];
					color.b = layer.PsdFile.ColorModeData[index + 512];
					break;
				case PsdColorMode.RGB:
					color.r = layer.Channel0.ImageData[position];
					color.g = layer.Channel1.ImageData[position];
					color.b = layer.Channel2.ImageData[position];
					break;
				case PsdColorMode.CMYK:
					color = CMYKToRGB(layer.Channel0.ImageData[position], layer.Channel1.ImageData[position],
					                  layer.Channel2.ImageData[position], layer.Channel3.ImageData[position]);
					break;
				case PsdColorMode.Multichannel:
					color = CMYKToRGB(layer.Channel0.ImageData[position], layer.Channel1.ImageData[position],
					                  layer.Channel2.ImageData[position], 0);
					break;
				case PsdColorMode.Lab:
					color = LabToRGB(layer.Channel0.ImageData[position], layer.Channel1.ImageData[position],
					                 layer.Channel2.ImageData[position]);
					break;
				default:
					color = new Color32(1, 1, 1, 1);
					break;
			}
		}

		/// <summary>
		/// Gets the color at the given pixel position in the given mask.
		/// </summary>
		/// <param name="mask">The mask to sample.</param>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		/// <returns>The mask color.</returns>
		private static byte GetColor(Mask mask, int x, int y)
		{
			var num = byte.MaxValue;
			if (mask.PositionVsLayer)
			{
				x -= (int)mask.Rect.x;
				y -= (int)mask.Rect.y;
			}
			else
			{
				x = x + (int)mask.Layer.Rect.x - (int)mask.Rect.x;
				y = y + (int)mask.Layer.Rect.y - (int)mask.Rect.y;
			}

			if (y >= 0 && (y < mask.Rect.height && x >= 0) && x < mask.Rect.width)
			{
				var index = (y * (int)mask.Rect.width) + x;
				num = index >= mask.ImageData.Length ? byte.MaxValue : mask.ImageData[index];
			}

			return num;
		}

		/// <summary>
		/// Converts Lab color to RGB color.
		/// </summary>
		/// <param name="lb">The lb channel.</param>
		/// <param name="ab">The ab channel.</param>
		/// <param name="bb">The bb channel.</param>
		/// <returns>The RGB color.</returns>
		private static Color32 LabToRGB(byte lb, byte ab, byte bb)
		{
			double num1 = lb;
			double num2 = ab;
			double num3 = bb;
			var num4 = 2.56;
			var num5 = 1.0;
			var num6 = 1.0;
			var num7 = (int)(num1 / num4);
			var num8 = (int)((num2 / num5) - 128.0);
			var num9 = (int)((num3 / num6) - 128.0);
			var x1 = (num7 + 16.0) / 116.0;
			var x2 = (num8 / 500.0) + x1;
			var x3 = x1 - (num9 / 200.0);
			var num10 = Math.Pow(x1, 3.0) <= 0.008856 ? (x1 - 0.0) / 7.787 : Math.Pow(x1, 3.0);
			var num11 = Math.Pow(x2, 3.0) <= 0.008856 ? (x2 - 0.0) / 7.787 : Math.Pow(x2, 3.0);
			var num12 = Math.Pow(x3, 3.0) <= 0.008856 ? (x3 - 0.0) / 7.787 : Math.Pow(x3, 3.0);
			return XYZToRGB(95.047 * num11, 100.0 * num10, 108.883 * num12);
		}

		/// <summary>
		/// Converts XYZ color to RGB color.
		/// </summary>
		/// <param name="x">The x channel.</param>
		/// <param name="y">The y channel.</param>
		/// <param name="z">The z channel.</param>
		/// <returns>The RGB color.</returns>
		private static Color32 XYZToRGB(double x, double y, double z)
		{
			var num1 = x / 100.0;
			var num2 = y / 100.0;
			var num3 = z / 100.0;

			var x1 = (num1 * 3.2406) + (num2 * -1.5372) + (num3 * -0.4986);
			var x2 = (num1 * -0.9689) + (num2 * 1.8758) + (num3 * 0.0415);
			var x3 = (num1 * 0.0557) + (num2 * -0.204) + (num3 * 1.057);

			var num4 = x1 <= 0.0031308 ? 12.92 * x1 : (1.055 * Math.Pow(x1, 5.0 / 12.0)) - 0.055;
			var num5 = x2 <= 0.0031308 ? 12.92 * x2 : (1.055 * Math.Pow(x2, 5.0 / 12.0)) - 0.055;
			var num6 = x3 <= 0.0031308 ? 12.92 * x3 : (1.055 * Math.Pow(x3, 5.0 / 12.0)) - 0.055;

			var red = (int)(num4 * 256.0);
			var green = (int)(num5 * 256.0);
			var blue = (int)(num6 * 256.0);

			if (red > byte.MaxValue)
			{
				red = byte.MaxValue;
			}

			if (green > byte.MaxValue)
			{
				green = byte.MaxValue;
			}

			if (blue > byte.MaxValue)
			{
				blue = byte.MaxValue;
			}

			return new Color32((byte)red, (byte)green, (byte)blue, 1);
		}

		/// <summary>
		/// Converts CMYK color to RGB color.
		/// </summary>
		/// <param name="c">The c channel.</param>
		/// <param name="m">The m channel.</param>
		/// <param name="y">The y channel.</param>
		/// <param name="k">The k channel.</param>
		/// <returns>The RGB color.</returns>
		private static Color32 CMYKToRGB(byte c, byte m, byte y, byte k)
		{
			var num1 = Math.Pow(2.0, 8.0);
			var num6 = 1.0 - (c / num1);
			var num7 = 1.0 - (m / num1);
			var num8 = 1.0 - (y / num1);
			var num9 = 1.0 - (k / num1);

			var red = (int)((1.0 - ((num6 * (1.0 - num9)) + num9)) * byte.MaxValue);
			var green = (int)((1.0 - ((num7 * (1.0 - num9)) + num9)) * byte.MaxValue);
			var blue = (int)((1.0 - ((num8 * (1.0 - num9)) + num9)) * byte.MaxValue);
			
			if (red > byte.MaxValue)
			{
				red = byte.MaxValue;
			}

			if (green > byte.MaxValue)
			{
				green = byte.MaxValue;
			}

			if (blue > byte.MaxValue)
			{
				blue = byte.MaxValue;
			}

			return new Color32((byte)red, (byte)green, (byte)blue, 1);
		}
	}
}