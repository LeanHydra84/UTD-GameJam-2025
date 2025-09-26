using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "TextShaderRenderSettings", menuName = "Scriptable Objects/TextShaderRenderSettings")]
public class TextShaderRenderSettings : ScriptableObject
{
	[Min(1)] public int width;
	[Min(1)] public int height;
	
	[Header("Fuzz stencil")]
	public Shader simpleDrawShader;
	public Shader simpleBlackShader;
	public LayerMask layerMask;

	[Header("Text Generation")] 
	public string glyphSet;
	public TMP_FontAsset font;
	public ComputeShader textGenerationComputeShader;
	public Material textGenerationMaterial;
	
	[Header("Mask")]
	public Material textMaskMaterial;
	public bool showInSceneView = false;

	// public event System.Action<TextShaderRenderSettings> OnChanged;
	//
	// void OnValidate()
	// {
	// 	OnChanged?.Invoke(this);
	// }

}

