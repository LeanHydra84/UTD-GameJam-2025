using System;
using System.Text;
using UnityEngine;
using Random = System.Random;

public class GenerateText : MonoBehaviour
{

    public int width;
    public int height;

    public ComputeShader shader;

    private ComputeBuffer buffer;

    
    private static readonly Random Rand = new Random();
    private static readonly int Width = Shader.PropertyToID("width");
    private static readonly int Height = Shader.PropertyToID("height");
    private static readonly int RandomSeed = Shader.PropertyToID("randomSeed");
    private static readonly int StringBuffer = Shader.PropertyToID("stringbuffer");
    
    [SerializeField] private TMPro.TMP_Text text;

    private int[] GetSeed()
    {
        int[] seed = new int[4];
        seed[0] = Rand.Next();
        seed[1] = Rand.Next();
        seed[2] = Rand.Next();
        seed[3] = Rand.Next();
        return seed;
    }
    
    private string GenerateString()
    {
        int alignedSize = ((width * height) + 3) / 4 * 4;
        buffer = new ComputeBuffer(alignedSize / 2, 4);

        int kernel = shader.FindKernel("CSMain");
        shader.GetKernelThreadGroupSizes(kernel, out uint x, out uint y, out uint z);
        
        shader.SetBuffer(kernel, StringBuffer, buffer);
        shader.SetInts(RandomSeed, GetSeed());
        shader.SetInt(Width, width);
        shader.SetInt(Height, height);
        
        shader.Dispatch(kernel, width / (int)x + 1, height / (int)y + 1, 1);
        
        byte[] characters = new byte[width * height];
        buffer.GetData(characters);
        buffer.Release();
        
        string txt = Encoding.Unicode.GetString(characters);

        return txt;
    }
    
    void Update()
    {
        text.text = GenerateString();
    }
}
