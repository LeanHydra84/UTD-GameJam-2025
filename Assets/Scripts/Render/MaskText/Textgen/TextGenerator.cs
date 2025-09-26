using System;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

public class TextGenerator : IDisposable
{

	public ComputeShader shader;
	private ComputeBuffer buffer;
	
	public int Width { get; private set; }
	public int Height { get; private set; }
	public int GlyphCount { get; set; }

	public ComputeBuffer Buffer => buffer;
	
	public TextGenerator(ComputeShader shader, int glyphCount, int width, int height)
	{
		this.shader = shader;
		GlyphCount = glyphCount;
		Resize(width, height);
	}

	private void SetSize(int count)
	{
		buffer?.Release();
		buffer = new ComputeBuffer(count, 4, ComputeBufferType.Structured);
	}

	public void Resize(int width, int height)
	{
		Width = width;
		Height = height;

		if (buffer == null)
		{
			SetSize(width * height);
			return;
		}
		
		int newCount = width * height;
		int oldCount = buffer.count;

		if (newCount <= oldCount && newCount > (oldCount / 2))
		{
			// if buffer resize is less than the old count but greater than half
			// of the old count, don't resize to save precious allocation time
			// also catches edge case of same dimensions
			return;
		}
		
		SetSize(width * height);
	}

	private static readonly System.Random Rand = new();
	
	/*
	 Properties:
	    RWStructuredBuffer<int> Result;
	    int width;
	    int height;
	    int charCount;
	    uint4 randomSeed;
	 */
	
	public string CurrentKernel { get; set; } = "GenBrightnessText";
	
	private static readonly int WidthID = Shader.PropertyToID("width");
	private static readonly int HeightID = Shader.PropertyToID("height");
	private static readonly int CharCountID = Shader.PropertyToID("charCount");
	private static readonly int RandomSeedID = Shader.PropertyToID("randomSeed");
	private static readonly int StringBufferID = Shader.PropertyToID("Result");
	private static readonly int ScreenColorID = Shader.PropertyToID("ScreenColor");
	private static readonly int StencilTextureID = Shader.PropertyToID("StencilTexture");

	private int[] GetSeed()
	{
		int[] seed = new int[4];
		seed[0] = Rand.Next();
		seed[1] = Rand.Next();
		seed[2] = Rand.Next();
		seed[3] = Rand.Next();
		return seed;
	}

	public void SetStencilTextureHandle(TextureHandle handle)
	{
		if (!handle.IsValid())
			return;
		int kernel = shader.FindKernel(CurrentKernel);
		shader.SetTexture(kernel, StencilTextureID, handle);
	}
	
	public void SetScreenColorTextureHandle(TextureHandle handle)
	{
		if (!handle.IsValid())
			return;
		int kernel = shader.FindKernel(CurrentKernel);
		shader.SetTexture(kernel, ScreenColorID, handle);
	}
    
	
	public void Generate()
	{
		int kernel = shader.FindKernel(CurrentKernel);
		shader.GetKernelThreadGroupSizes(kernel, out uint x, out uint y, out uint z);
        
		shader.SetBuffer(kernel, StringBufferID, buffer);
		shader.SetInts(RandomSeedID, GetSeed());
		shader.SetInt(WidthID, Width);
		shader.SetInt(HeightID, Height);
        shader.SetInt(CharCountID, GlyphCount);
		
		shader.Dispatch(kernel, Width / (int)x + 1, Height / (int)y + 1, 1);
	}

	public void Dispose()
	{
		buffer?.Release();
	}
}
