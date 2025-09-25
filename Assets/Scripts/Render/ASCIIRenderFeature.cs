using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class ASCIIRenderFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {

        private Material material;
        
        private RenderTextureDescriptor descriptor;

        public void SetMaterial(Material material)
        {
            this.material = material;
        }
        
        public CustomRenderPass(Material material)
        {
            this.material = material;
            descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
            requiresIntermediateTexture = true;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            descriptor.width = cameraData.cameraTargetDescriptor.width;
            descriptor.height = cameraData.cameraTargetDescriptor.height;
            descriptor.depthBufferBits = 0;

            TextureHandle src = resourceData.activeColorTexture;
            TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "NCT", false);
            
            RenderGraphUtils.BlitMaterialParameters parameters = new RenderGraphUtils.BlitMaterialParameters(src, dst, material, 0);
            renderGraph.AddBlitPass(parameters);
            resourceData.cameraColor = dst;
        }

    }

    CustomRenderPass m_ScriptablePass;

    public Material asciiMaterial;
    
    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(asciiMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    void OnValidate()
    {
        m_ScriptablePass.SetMaterial(asciiMaterial);
    }
    
}
