using System;
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

public class MaskTextRenderFeature : ScriptableRendererFeature
{

    class TextTextureData : ContextItem
    {
        public TextureHandle TextDrawTexture;
        public bool IsInitialized;
        
        public override void Reset()
        {
            TextDrawTexture = TextureHandle.nullHandle;
        }
    }
    
    class SimpleDrawData : ContextItem
    {
        public TextureHandle SimpleDrawTexture;
        public bool IsInitialized;
        
        public override void Reset()
        {
            SimpleDrawTexture = TextureHandle.nullHandle;
        }
    }
    
    class SimpleDrawRenderPass : ScriptableRenderPass
    {

        public TextShaderRenderSettings Settings => settings;
        
        private TextShaderRenderSettings settings;
        
        private Material drawMaterial;
        private Material blackMaterial;
        
        private List<ShaderTagId> shaderOverrideTags;
        private RenderTextureDescriptor descriptor;
        private FilteringSettings filterSettings;
        private FilteringSettings inverseFilterSettings;

        public SimpleDrawRenderPass(TextShaderRenderSettings renderSettings)
        {
            OnSettingsChanged(renderSettings);
        }

        public void OnSettingsChanged(TextShaderRenderSettings renderSettings)
        {
            settings = renderSettings;
            if (renderSettings == null)
                return;
            
            if (renderSettings.simpleDrawShader)
                drawMaterial = new Material(renderSettings.simpleDrawShader);
            if (renderSettings.simpleBlackShader)
                blackMaterial = new Material(renderSettings.simpleBlackShader);
            
            descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.R8); // default depthBufferBits?
            shaderOverrideTags = new List<ShaderTagId>() { new ("UniversalForward"), new ("SRPDefaultUnlit") };
            filterSettings = new FilteringSettings(new RenderQueueRange(0, 5000), renderSettings.layerMask);
            inverseFilterSettings = new FilteringSettings(new RenderQueueRange(0, 5000), ~renderSettings.layerMask);
        }

        private class PassData
        {
            public RendererListHandle RendererListHandle;
            public TextureHandle DrawTexture;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            var simpleDrawData = frameContext.Create<SimpleDrawData>();
            simpleDrawData.IsInitialized = false;

            if (settings == null) return;
            if (!settings.enabled) return;
            if (drawMaterial == null)
            {
                if (settings.simpleDrawShader)
                    drawMaterial = new Material(settings.simpleDrawShader);
                else return;
            }

            if (blackMaterial == null)
            {
                if (settings.simpleBlackShader)
                    drawMaterial = new Material(settings.simpleBlackShader);
                else return;
            }
            
            UniversalRenderingData renderingData = frameContext.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            UniversalLightData lightData = frameContext.Get<UniversalLightData>();
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
                return;

            if (cameraData.isSceneViewCamera)
                return;
            
            SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
            TextureHandle txHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "R8 Simple Draw Texture", false);
            
            simpleDrawData.SimpleDrawTexture = txHandle;
            simpleDrawData.IsInitialized = true;
            
            // BLACKOUT PASS FOR NOT DEPTH
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Simple Draw", out var passData))
            {
                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderOverrideTags, renderingData, cameraData, lightData, sortFlags);
                drawSettings.overrideMaterial = drawMaterial;

                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
                passData.RendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
                
                // Create render texture
                passData.DrawTexture = txHandle;
                
                // simpleDrawData.simpleDrawTexture = passData.drawTexture;
                
                builder.UseRendererList(passData.RendererListHandle);
                builder.SetRenderAttachment(passData.DrawTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteLightPass(data, context));
            }
            
            // BLACKOUT PASS FOR DEPTH
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blackout Draw", out var passData))
            {
                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderOverrideTags, renderingData, cameraData, lightData, sortFlags);
                drawSettings.overrideMaterial = blackMaterial;
                
                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, inverseFilterSettings);
                passData.RendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
                
                passData.DrawTexture = txHandle;
                
                builder.UseRendererList(passData.RendererListHandle);
                builder.SetRenderAttachment(passData.DrawTexture, 0);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteDarkPass(data, context));
            }
        }

        static void ExecuteLightPass(PassData data, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(true, true, Color.black);
            context.cmd.DrawRendererList(data.RendererListHandle);
        }
        
        static void ExecuteDarkPass(PassData data, RasterGraphContext context)
        {
            context.cmd.DrawRendererList(data.RendererListHandle);
        }
    }
    
    class TextRenderPass : ScriptableRenderPass
    {
        private static readonly int GlyphBufferID = Shader.PropertyToID("_glyphBuffer");
        private static readonly int CharacterBufferID = Shader.PropertyToID("_characterBuffer");
        private static readonly int WidthID = Shader.PropertyToID("_Width");
        private static readonly int HeightID = Shader.PropertyToID("_Height");

        private TextShaderRenderSettings settings;
        
        private Material drawMaterial;
        private ComputeBuffer glyphBuffer;
        private TextGenerator textGenerator;
        
        private int glyphCount;
        private RenderTextureDescriptor descriptor;

        // GLYPH OVERRIDE
        private Dictionary<char, int> characterMappings;
        private string currentOverrideString;
        private int[] currentOverrideBufferList;
        
        private int[] CalculateOverrideBufferList(string overrideString)
        {
            return overrideString
                .Where(a => characterMappings.ContainsKey(a))
                .Select(a => characterMappings[a])
                .ToArray();
        }
        
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

        // private IEnumerable<Glyph> GetGlyphsFromString(TMP_FontAsset fontAsset, string str)
        // {
        //     if (string.IsNullOrEmpty(str))
        //         return fontAsset.glyphTable;
        //     
        //     return str.Select(a => fontAsset.characterLookupTable[a].glyph);
        // }

        private ComputeGlyph[] CalculateGlyphSetAndStore(TMP_FontAsset fontAsset, string str, Vector2Int txDim)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (string.IsNullOrEmpty(str))
            {
                str = new string(fontAsset.characterTable.Select(a => (char)a.unicode).ToArray());
            }
            else
            {
                str += alphabet;
            }

            List<char> charList = str.Where(a => fontAsset.HasCharacter(a)).ToList();
            
            characterMappings = new Dictionary<char, int>();
            List<Glyph> result = new List<Glyph>();
            for (int i = 0; i < charList.Count; i++)
            {
                char c = str[i];
                Glyph current = fontAsset.characterLookupTable[c].glyph;
                result.Add(current);
                characterMappings[c] = i;
            }
            
            return result.Select(a => new ComputeGlyph(a, txDim)).ToArray();
        }

        private void CalculateFontBuffers()
        {
            if (drawMaterial == null)
                return;
            
            Vector2Int txDim = new Vector2Int(settings.font.atlasWidth, settings.font.atlasHeight);
            // ComputeGlyph[] glyphs = GetGlyphsFromString(settings.font, settings.glyphSet)
            //     .Select(a => new ComputeGlyph(a, txDim))
            //     .ToArray();

            ComputeGlyph[] glyphs = CalculateGlyphSetAndStore(settings.font, settings.glyphSet, txDim);
            
            if (glyphBuffer != null)
            {
                glyphBuffer.Release();
            }
            glyphBuffer = new ComputeBuffer(glyphs.Length, Marshal.SizeOf(typeof(ComputeGlyph)));
            glyphBuffer.name = "Glyph Buffer";
            glyphBuffer.SetData(glyphs);
            
            glyphCount = settings.glyphSet.Length;
            
            drawMaterial.SetBuffer(GlyphBufferID, glyphBuffer);
        }

        
        public TextRenderPass(TextShaderRenderSettings renderSettings)
        {
            OnSettingsChanged(renderSettings);
        }
        
        public void OnSettingsChanged(TextShaderRenderSettings renderSettings)
        {
            settings = renderSettings;
            if (renderSettings == null)
                return;
            
            drawMaterial = renderSettings.textGenerationMaterial;
            
            CalculateFontBuffers();

            textGenerator?.Dispose();
            textGenerator = new TextGenerator(renderSettings.textGenerationComputeShader, glyphCount, renderSettings.width, renderSettings.height);
            descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32, 0);
        }

        private class IntermediatePassData
        {
            public TextureHandle ScreenColor;
            public TextureHandle StencilTextureHandle;
            public TextShaderRenderSettings Settings;
            public ComputeBuffer GlyphBuffer;
            public TextGenerator Text;
            public Material DrawMaterial;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();

            TextTextureData textTextureData = frameContext.Create<TextTextureData>();
            textTextureData.IsInitialized = false;

            if (settings == null) return;
            if (settings == null) return;
            if (drawMaterial == null) return;
            
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            if (cameraData.isSceneViewCamera)
                return;

            descriptor.width = cameraData.cameraTargetDescriptor.width;
            descriptor.height = cameraData.cameraTargetDescriptor.height;
            descriptor.depthBufferBits = 0;

            TextureHandle src = resourceData.activeColorTexture;
            TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "TextAtlasTexture", false);
            textTextureData.TextDrawTexture = dest;
            textTextureData.IsInitialized = true;

            if (textGenerator.Width != settings.width || textGenerator.Height != settings.height)
            {
                textGenerator.Resize(settings.width, settings.height);
            }

            using (var builder = renderGraph.AddRasterRenderPass("Text Generation Pass", out IntermediatePassData data))
            {
                SimpleDrawData sdd = frameContext.Get<SimpleDrawData>();
                if (!sdd.IsInitialized) return;
                
                data.Settings = settings;
                data.DrawMaterial = drawMaterial;
                data.Text = textGenerator;
                data.ScreenColor = resourceData.activeColorTexture;
                data.GlyphBuffer = glyphBuffer;
                data.StencilTextureHandle = sdd.IsInitialized ? sdd.SimpleDrawTexture : TextureHandle.nullHandle;

                // Override strings
                
                string overrideStr;
                if (settings.textOverride == null || string.IsNullOrEmpty(settings.textOverride.text))
                {
                    overrideStr = string.Empty;
                }
                else
                {
                    overrideStr = settings.textOverride.text;
                }

                if (overrideStr != currentOverrideString)
                {
                    int[] overrideIntArray = CalculateOverrideBufferList(overrideStr);
                    textGenerator.SetOverrideString(overrideIntArray);
                }
                
                builder.UseTexture(sdd.SimpleDrawTexture);
                builder.UseTexture(resourceData.activeColorTexture);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((IntermediatePassData idp, RasterGraphContext context) => IntermediateDataSetFunc(idp, context));
            }
            
            RenderGraphUtils.BlitMaterialParameters parameters = new(src, dest, drawMaterial, 0);
            renderGraph.AddBlitPass(parameters, "Render Text Blit");
        }

        private static void IntermediateDataSetFunc(IntermediatePassData i, RasterGraphContext _)
        {
            // Generate Text
            i.Text.SetStencilTextureHandle(i.StencilTextureHandle);
            i.Text.SetScreenColorTextureHandle(i.ScreenColor);
            i.Text.Generate();

            i.DrawMaterial.SetInt(WidthID, i.Settings.width);
            i.DrawMaterial.SetInt(HeightID, i.Settings.height);
            
            i.DrawMaterial.SetBuffer(GlyphBufferID, i.GlyphBuffer);
            i.DrawMaterial.SetBuffer(CharacterBufferID, i.Text.Buffer);
        }

    }
    
    
    class MaskRenderPass : ScriptableRenderPass
    {
        
        private TextShaderRenderSettings settings;
        
        private Material material;
        private RenderTextureDescriptor descriptor;
        
        public MaskRenderPass(TextShaderRenderSettings renderSettings)
        {
            OnSettingsChanged(renderSettings);
            requiresIntermediateTexture = true;
        }

        public void OnSettingsChanged(TextShaderRenderSettings renderSettings)
        {
            settings = renderSettings;
            material = renderSettings.textMaskMaterial;
            descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();

            if (settings == null) return;
            if (!settings.enabled) return;
            
            if (resourceData.isActiveTargetBackBuffer)
                return;

            if (cameraData.isSceneViewCamera && !settings.showInSceneView)
                return;

            TextTextureData textTextureData = frameContext.Get<TextTextureData>();
            if (!textTextureData.IsInitialized)
                return;
            
            descriptor.width = cameraData.cameraTargetDescriptor.width;
            descriptor.height = cameraData.cameraTargetDescriptor.height;
            descriptor.depthBufferBits = 0;

            TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "New Camera Texture", false);
            RenderGraphUtils.BlitMaterialParameters parameters = new(textTextureData.TextDrawTexture, dest, material, 0);
            renderGraph.AddBlitPass(parameters);

            resourceData.cameraColor = dest;
        }

    }
    

    private SimpleDrawRenderPass simpleDrawPass;
    private TextRenderPass textRenderPass; 
    private MaskRenderPass maskRenderPass;

    [SerializeField] private TextShaderRenderSettings settings;

    // [Header("Stencil faker")]
    // [SerializeField] private Shader simpleDrawShader;
    // [SerializeField] private Shader simpleBlackShader;
    // [SerializeField] private LayerMask layerMask;
    
    // [Header("Drawing Text")] 
    // [SerializeField] private string glyphSet;
    // [SerializeField] private Material textRenderMaterial;
    
    // [Header("Masking")]
    // [SerializeField] private Material textMaskMaterial;
    // [SerializeField] private bool showInSceneView = false;
    
    public override void Create()
    {
        simpleDrawPass = new SimpleDrawRenderPass(settings);
        textRenderPass = new TextRenderPass(settings);
        maskRenderPass = new MaskRenderPass(settings);

        simpleDrawPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        textRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        maskRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(simpleDrawPass);
        renderer.EnqueuePass(textRenderPass);
        renderer.EnqueuePass(maskRenderPass);
    }
    
    void OnValidate()
    {
        if (simpleDrawPass == null) return;
        if (settings == simpleDrawPass.Settings) return;
        
        simpleDrawPass.OnSettingsChanged(settings);
        textRenderPass.OnSettingsChanged(settings);
        maskRenderPass.OnSettingsChanged(settings);
    }
    
}
