using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MaskTextRenderFeature : ScriptableRendererFeature
{

    class GlyphBufferContext : ContextItem
    {
        public GraphicsBuffer GlyphBufferHandle;
        
        public override void Reset()
        {
            GlyphBufferHandle = null;
        }
    }

    class TextTextureData : ContextItem
    {
        public TextureHandle simpleDrawTexture;
        
        public override void Reset()
        {
            simpleDrawTexture = TextureHandle.nullHandle;
        }
    }

    class TextRenderPass : ScriptableRenderPass
    {
        private Material drawMaterial;
        private RenderTextureDescriptor descriptor;

        private ComputeBuffer glyphBuffer;
        private int glyphCount;
        
        private static readonly int GlyphBufferID = Shader.PropertyToID("_glyphBuffer");
        private static readonly int CharacterBufferID = Shader.PropertyToID("_characterBuffer");
        private static readonly int WidthID = Shader.PropertyToID("_Width");
        private static readonly int HeightID = Shader.PropertyToID("_Height");

        private TextShaderRenderSettings settings;
        private TextGenerator textGenerator;

        public string GlyphSet { get; private set; }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct ComputeGlyph
        {
            public float x, y, width, height;
            public float scale;
            
            public ComputeGlyph(Glyph glyph, Vector2Int txDim)
            {
                var rect = glyph.glyphRect;
                x = (float)rect.x / txDim.x;
                y = (float)rect.y / txDim.y;
                width = (float)rect.width / txDim.x;
                height = (float)rect.height / txDim.y;
                scale = glyph.scale;
            }
        }

        private IEnumerable<Glyph> GetGlyphsFromString(TMP_FontAsset fontAsset, string str)
        {
            if (string.IsNullOrEmpty(str))
                return fontAsset.glyphTable;
            
            return str.Select(a => fontAsset.characterLookupTable[a].glyph);
        }

        private void CalculateFontBuffers(TMP_FontAsset fontAsset, string glyphString)
        {
            if (glyphBuffer != null)
            {
                glyphBuffer.Release();
            }
            
            Vector2Int txDim = new Vector2Int(fontAsset.atlasWidth, fontAsset.atlasHeight);
            ComputeGlyph[] glyphs = GetGlyphsFromString(fontAsset, glyphString)
                .Select(a => new ComputeGlyph(a, txDim))
                .ToArray();
            glyphBuffer = new ComputeBuffer(glyphs.Length, Marshal.SizeOf(typeof(ComputeGlyph)));
            glyphBuffer.name = "Glyph Buffer";
            glyphBuffer.SetData(glyphs);
            
            glyphCount = glyphs.Length;
            
            GlyphSet = glyphString;
            drawMaterial.SetBuffer(GlyphBufferID, glyphBuffer);
        }
        
        public void SetGlyphSet(string str)
        {
            CalculateFontBuffers(settings.font, str);
            if (textGenerator != null)
                textGenerator.GlyphCount = glyphCount;
        }
        
        public TextRenderPass(Material material, TextShaderRenderSettings settings, string glyphString)
        {
            this.settings = settings;
            drawMaterial = material;
            CalculateFontBuffers(settings.font, glyphString);
            textGenerator = new TextGenerator(settings.textGenerationComputeShader, glyphCount, settings.width, settings.height);
            descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32, 0);
        }

        private class IntermediatePassData
        {
            public TextureHandle screenColor;
            public TextGenerator text;
            public TextShaderRenderSettings settings;
            public Material drawMaterial;
            public ComputeBuffer glyphBuffer;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            if (drawMaterial == null) return;
            
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            if (cameraData.isSceneViewCamera)
                return;

            descriptor.width = cameraData.cameraTargetDescriptor.width;
            descriptor.height = cameraData.cameraTargetDescriptor.height;
            descriptor.depthBufferBits = 0;

            TextureHandle src = resourceData.activeColorTexture;
            TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "TextAtlasTexture", false);
            TextTextureData textTextureData = frameContext.Create<TextTextureData>();
            textTextureData.simpleDrawTexture = dest;

            if (textGenerator.Width != settings.width || textGenerator.Height != settings.height)
            {
                textGenerator.Resize(settings.width, settings.height);
            }

            using (var builder = renderGraph.AddRasterRenderPass("Text Generation Pass", out IntermediatePassData data))
            {
                data.settings = settings;
                data.drawMaterial = drawMaterial;
                data.text = textGenerator;
                data.screenColor = resourceData.activeColorTexture;
                data.glyphBuffer = glyphBuffer;
                
                builder.UseTexture(resourceData.activeColorTexture);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((IntermediatePassData idp, RasterGraphContext context) => IntermediateDataSetFunc(idp, context));
            }
            
            RenderGraphUtils.BlitMaterialParameters parameters = new(src, dest, drawMaterial, 0);
            renderGraph.AddBlitPass(parameters, "Render Text Blit");
        }

        private static void IntermediateDataSetFunc(IntermediatePassData i, RasterGraphContext context)
        {
            // Generate Text
            i.text.SetScreenColorTextureHandle(i.screenColor);
            i.text.Generate();

            i.drawMaterial.SetInt(WidthID, i.settings.width);
            i.drawMaterial.SetInt(HeightID, i.settings.height);
            
            i.drawMaterial.SetBuffer(GlyphBufferID, i.glyphBuffer);
            i.drawMaterial.SetBuffer(CharacterBufferID, i.text.Buffer);
        }

    }
    
    class MaskRenderPass : ScriptableRenderPass
    {

        public Material Material => _material;
        
        private Material _material;
        private RenderTextureDescriptor descriptor;
        private bool showInSceneView = false;
        
        public MaskRenderPass(Material mat)
        {
            _material = mat;
            descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
            requiresIntermediateTexture = true;
        }

        public void SetMaterial(Material mat)
        {
            _material = mat;
        }

        public void SetSceneCameraView(bool canView)
        {
            showInSceneView = canView;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            if (cameraData.isSceneViewCamera && !showInSceneView)
                return;

            descriptor.width = cameraData.cameraTargetDescriptor.width;
            descriptor.height = cameraData.cameraTargetDescriptor.height;
            descriptor.depthBufferBits = 0;

            TextureHandle src = resourceData.activeColorTexture;
            TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "New Camera Texture", false);
            TextTextureData textTextureData = frameContext.Get<TextTextureData>();

            RenderGraphUtils.BlitMaterialParameters parameters = new(textTextureData.simpleDrawTexture, dest, _material, 0);
            renderGraph.AddBlitPass(parameters);

            resourceData.cameraColor = dest;
        }

    }

    private TextRenderPass textRenderPass;
    private MaskRenderPass maskRenderPass;

    [SerializeField] private TextShaderRenderSettings settings;

    [Header("Drawing Text")] 
    [SerializeField] private string glyphSet;
    [SerializeField] private Material textRenderMaterial;
    
    [Header("Masking")]
    [SerializeField] private Material textMaskMaterial;
    [SerializeField] private bool showInSceneView = false;
    
    public override void Create()
    {
        if (textMaskMaterial == null) return;

        textRenderPass = new TextRenderPass(textRenderMaterial, settings, glyphSet);
        maskRenderPass = new MaskRenderPass(textMaskMaterial);

        textRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        maskRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(textRenderPass);
        renderer.EnqueuePass(maskRenderPass);
    }
    
    void OnValidate()
    {
        if (textRenderPass == null) return;
        if (maskRenderPass == null) return;
        
        // Text
        if (glyphSet != textRenderPass.GlyphSet)
        {
            textRenderPass.SetGlyphSet(glyphSet);
        }
        
        // Mask
        if (maskRenderPass == null) return;

        if (textMaskMaterial != maskRenderPass.Material)
        {
            maskRenderPass.SetMaterial(textMaskMaterial);
        }
        maskRenderPass.SetSceneCameraView(showInSceneView);
    }
    
}
