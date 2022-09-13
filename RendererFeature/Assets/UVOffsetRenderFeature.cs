using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UVOffsetRenderFeature : ScriptableRendererFeature
{
    class UVOffsetRenderPass : ScriptableRenderPass
    {
        private static readonly string _UVOffsetCmdName = "Render UV Offset";
        
        #region ShaderId

        private static readonly int _MainTexShaderId = Shader.PropertyToID("_MainTex");
        private static readonly int _TempTargetShaderId = Shader.PropertyToID("_TempTarget");
        private static readonly int _StrengthShaderId = Shader.PropertyToID("_Strength");
        private static readonly int _OffsetDirShaderId = Shader.PropertyToID("_OffsetDir");

        #endregion
        private UVOffset _uvOffset;
        private Material _uvOffsetMaterial;
        private RenderTargetIdentifier _currentTarget;
        
        public UVOffsetRenderPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("MyEffect/UVOffset");
            if (shader == null)
            { 
                Debug.LogError("UVOffset Shader not found!");
                return;
            }
            _uvOffsetMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            _currentTarget = currentTarget;
        }

        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var width = renderingData.cameraData.camera.scaledPixelWidth;
            var height = renderingData.cameraData.camera.scaledPixelHeight;
            //pass顺序
            int shaderPass = -1;
            //传参
            _uvOffsetMaterial.SetFloat(_StrengthShaderId, _uvOffset.offsetStrength.value);
            _uvOffsetMaterial.SetVector(_OffsetDirShaderId, _uvOffset.offsetDirection.value);
            
            cmd.SetGlobalTexture(_MainTexShaderId, _currentTarget);
            cmd.GetTemporaryRT(_TempTargetShaderId, width, height, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.Blit(_currentTarget, _TempTargetShaderId);
            cmd.Blit(_TempTargetShaderId, _currentTarget, _uvOffsetMaterial, shaderPass);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            #region 资源检查
            if (_uvOffsetMaterial == null)
            {
                Debug.LogError("ZoomBlur Material not created!");
                return;
            }
            
            if (!renderingData.cameraData.postProcessEnabled)
            {
                //若关闭后效，则不执行
                return;
            }

            var stack = VolumeManager.instance.stack;
            _uvOffset = stack.GetComponent<UVOffset>();

            if (_uvOffset == null)
            {
                return;
            }

            if (!_uvOffset.IsActive())
            {
                return;
            }
            #endregion

            var cmd = CommandBufferPool.Get(_UVOffsetCmdName);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    UVOffsetRenderPass uvOffsetRenderPass;
    
    public override void Create()
    {
        uvOffsetRenderPass = new UVOffsetRenderPass(RenderPassEvent.BeforeRenderingPostProcessing);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        uvOffsetRenderPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(uvOffsetRenderPass);
    }
}
