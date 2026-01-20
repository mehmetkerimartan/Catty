using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP Renderer Feature that adds outline effect to objects with a specific layer.
/// </summary>
public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public Material outlineMaterial;
        public LayerMask layerMask = -1;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public OutlineSettings settings = new OutlineSettings();
    private OutlinePass outlinePass;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings);
        outlinePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial != null)
        {
            renderer.EnqueuePass(outlinePass);
        }
    }

    class OutlinePass : ScriptableRenderPass
    {
        private OutlineSettings settings;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> shaderTagIds = new List<ShaderTagId>();

        public OutlinePass(OutlineSettings settings)
        {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);
            
            shaderTagIds.Add(new ShaderTagId("UniversalForward"));
            shaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));
            shaderTagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("OutlinePass");
            
            var drawingSettings = CreateDrawingSettings(shaderTagIds, ref renderingData, SortingCriteria.CommonOpaque);
            drawingSettings.overrideMaterial = settings.outlineMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
