using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZoomBlurRenderFeature : ScriptableRendererFeature
{
    class ZoomBlurRenderPass : ScriptableRenderPass
    {
        private static readonly string _ZoomBlurCmdName = "Render ZoomBlur Effects";
        
        #region ShaderId
 
        private static readonly int _MainTexShaderId = Shader.PropertyToID("_MainTex");
        private static readonly int _TempTargetShaderId = Shader.PropertyToID("_TempTarget");
        private static readonly int _FocusPowerShaderId = Shader.PropertyToID("_FocusPower");
        private static readonly int _FocusDetailShaderId = Shader.PropertyToID("_FocusDetail");
        private static readonly int _FocusScreenPosShaderId = Shader.PropertyToID("_FocusScreenPos");
        private static readonly int _ReferenceResShaderId = Shader.PropertyToID("_ReferenceRes");
        
        #endregion

        private ZoomBlur _zoomBlur;
        private Material _zoomBlurMaterial;
        private RenderTargetIdentifier _currentTarget;

        public ZoomBlurRenderPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find("MyEffect/ZoomBlur");
            if (shader == null)
            {
                Debug.LogError("ZoomBlur Shader not found!");
                return;
            }
            _zoomBlurMaterial = CoreUtils.CreateEngineMaterial(shader);
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
            int shaderPass = 0;
            //传参
            _zoomBlurMaterial.SetFloat(_FocusPowerShaderId, _zoomBlur.focusPower.value);
            _zoomBlurMaterial.SetInt(_FocusDetailShaderId, _zoomBlur.focusDetail.value);
            _zoomBlurMaterial.SetVector(_FocusScreenPosShaderId, _zoomBlur.focusScreenPosition.value);
            _zoomBlurMaterial.SetInt(_ReferenceResShaderId, _zoomBlur.referenceResolutionX.value);
            
            cmd.SetGlobalTexture(_MainTexShaderId, _currentTarget);
            cmd.GetTemporaryRT(_TempTargetShaderId, width, height, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.Blit(_currentTarget, _TempTargetShaderId);
            cmd.Blit(_TempTargetShaderId, _currentTarget, _zoomBlurMaterial, shaderPass);
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            #region 资源检查
            if (_zoomBlurMaterial == null)
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
            _zoomBlur = stack.GetComponent<ZoomBlur>();

            if (_zoomBlur == null)
            {
                return;
            }

            if (!_zoomBlur.IsActive())
            {
                return;
            }
            #endregion

            var cmd = CommandBufferPool.Get(_ZoomBlurCmdName);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    ZoomBlurRenderPass zoomBlurRenderPass;
    
    public override void Create()
    {
        zoomBlurRenderPass = new ZoomBlurRenderPass(RenderPassEvent.BeforeRenderingPostProcessing);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        zoomBlurRenderPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(zoomBlurRenderPass);
    }
}


