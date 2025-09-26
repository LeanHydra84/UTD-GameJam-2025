using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "TextShaderRenderSettings", menuName = "Scriptable Objects/TextShaderRenderSettings")]
public class TextShaderRenderSettings : ScriptableObject
{
	[Min(1)] public int width;
	[Min(1)] public int height;

	[Min(0)] public float generationDelay;
	
	public TMP_FontAsset font;
	public ComputeShader textGenerationComputeShader;
}

