using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class Alpha8Util
{
	private static Texture2D SaveAlpha8AsGrayscale(Texture2D texture)
	{
		if (!texture.isReadable)
		{
			Debug.Log("Please mark Texture2D as readable");
			return null;
		}

		var newTexture = new Texture2D(texture.width, texture.height);

		var pixels = texture.GetPixels().Select(a => new Color(a.a, a.a, a.a, 1)).ToArray();
		newTexture.SetPixels(pixels);

		var pngBytes = newTexture.EncodeToPNG();

		var filepath = EditorUtility.SaveFilePanel("Save Texture Atlas", "", "atlas", "PNG");
		if (!string.IsNullOrWhiteSpace(filepath))
		{
			var fs = File.Open(filepath, FileMode.Create, FileAccess.Write);
			fs.Write(pngBytes);
			fs.Close();
		}

		return newTexture;
	}

	private static void ReimportTexture2DAsReadable(Texture2D texture)
	{
		Debug.Log("Attempting to make readable...");
		TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			
		importer.isReadable = true;
		importer.SaveAndReimport();
	}

	[MenuItem("Lean/Reimport as Readable")]
	private static void MakeTexture2DReadable()
	{
		var selected = Selection.objects.First(a => a is Texture2D);
		if (selected != null)
		{
			var img = (Texture2D)selected;
			// ReimportTexture2DAsReadable(img);
			img.Apply(false, false);
		}
		else
		{
			Debug.Log("Selected Object is not Texture2D OR nothing selected");
		}
	}

	[MenuItem("Lean/Save Alpha8 as Grayscale")]
	private static void Texture2DMenuItem()
	{
		var selected = Selection.objects.First(a => a is Texture2D);

		if (selected != null)
		{
			var img = (Texture2D)selected;
			SaveAlpha8AsGrayscale(img);
		}
		else
		{
			Debug.Log("Selected Object is not Texture2D OR nothing selected");
		}
	}
}