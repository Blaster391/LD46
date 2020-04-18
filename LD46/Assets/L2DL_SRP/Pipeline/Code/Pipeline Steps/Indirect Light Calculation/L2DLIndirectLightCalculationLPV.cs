using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class L2DLIndirectLightCalculationLPV : IL2DLIndirectLightCalculationStep
{
    List<LPVIterationData> m_lpvIterationsData;
    public List<LPVIterationData> LPVIterationData => m_lpvIterationsData;

    ComputeShader m_directionalLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Shaders/Direct Light/Occlusion Trace Compute/DirectionalOcclusionMapTrace");

    int m_directLightOcclusionMapId = Shader.PropertyToID("_L2DLDirectLightOcclusionMap");
    
    private CommandBuffer m_polyphaseMipmapBuffer = new CommandBuffer() { name = "Polyphase Mipmapping" };
    private ComputeShader m_occlusionPolyphaseMipmapComputeShader = null;
    private ComputeShader m_emissionPolyphaseMipmapComputeShader = null;

    private CommandBuffer m_lpvIterationsBuffer = new CommandBuffer() { name = "LPV Iterations" };

    private ComputeShader m_lpvIterationsComputeShader = null;
    private int m_lpvIterationsFlipKernel = 0;
    private int m_lpvIterationsFlopKernel = 0;
    private int flipTextureId = Shader.PropertyToID("_L2DLLPVFlipTexture");
    private int flopTextureId = Shader.PropertyToID("_L2DLLPVFlopTexture");

    private ComputeShader m_moveLPVDataDownMipLevelsComputeShader = null;
    private int m_moveLPVDataDownMipLevelsKernel = 0;
    
    //private ComputeShader m_ambientLightInjectionComputeShader = null; // TODO

    // --------------------------------------------------------------------
    public L2DLIndirectLightCalculationLPV(List<LPVIterationData> _lpvIterationsData)
    {
        m_lpvIterationsData = _lpvIterationsData;
    }

    // --------------------------------------------------------------------
    public void CalculateIndirectLight(
        ScriptableRenderContext _context,
        CommandBuffer _buffer,
        Camera _camera,
        RenderTargetIdentifier i_emissiveTexture,
        RenderTargetIdentifier i_occlusionTexture,
        RenderTargetIdentifier o_indirectLightTexture
        )
    {
        if (!L2DLRenderHelpers.SupportsComputeShaders())
        {
            return;
        }

        ReloadComputeShaders();

        // We'll assume that the emissive texture is all we care about, any reflected direct light can be added in beforehand as part of the general IndirectLight renderer
        if (m_emissionPolyphaseMipmapComputeShader == null)
        {
            m_emissionPolyphaseMipmapComputeShader = (ComputeShader)Resources.Load("Compute Shaders/Indirect Light/Polyphase Mipmap/GenerateEmissionPolyphaseMipmapsCompute");
        }
        if (m_occlusionPolyphaseMipmapComputeShader == null)
        {
            m_occlusionPolyphaseMipmapComputeShader = (ComputeShader)Resources.Load("Compute Shaders/Indirect Light/Polyphase Mipmap/GenerateOcclusionPolyphaseMipmapsCompute");
        }

        GeneratePolyphaseTextureMipmaps(_context, m_emissionPolyphaseMipmapComputeShader, i_emissiveTexture, new Vector2(_camera.pixelWidth, _camera.pixelHeight), 4); //TODO: Firgure out max mips from LPV data
        GeneratePolyphaseTextureMipmaps(_context, m_occlusionPolyphaseMipmapComputeShader, i_occlusionTexture, new Vector2(_camera.pixelWidth, _camera.pixelHeight), 4);

        // So we can grab some temporary textures that are a power of two above the camera size which generate mips automatically
        // We'll then render the emissive and occlusion into them

        // We can then use the mipmapped textures 

        // LPV iterations into the direct light texture ready for final presenting
        GenerateIndirectLight(_context, i_emissiveTexture, i_occlusionTexture, o_indirectLightTexture, _camera, m_lpvIterationsData, Color.black);
    }

    // --------------------------------------------------------------------
    private void GeneratePolyphaseTextureMipmaps(
       ScriptableRenderContext _context,
       ComputeShader _shader,
       RenderTargetIdentifier _textureID,
       Vector2 _textureSize,
       int maxMipLevelGenerated = -1
       )
    {
        //Polyphase box filter for non-power of two texture mip-mapping
        
        int[] previousSize;
        int[] currentSize;

        //We need to check if width or height has been rounded
        bool[] rounded = new bool[2];

        //The initial size needs to be recalculated when restarting for each layer so it has to happen within the loop
        previousSize = new int[] { (int)_textureSize.x, (int)_textureSize.y };

        //Calculate the max possible mip levels this texture can have so we know how may iterations to do
        float mipLevels = (int)Mathf.Floor(Mathf.Log(Mathf.Max(_textureSize.x, _textureSize.y), 2));
        if (maxMipLevelGenerated != -1) mipLevels = Mathf.Min(mipLevels, maxMipLevelGenerated);

        //First mip has a special case for occlusion mipmapping
        m_polyphaseMipmapBuffer.SetComputeFloatParam(_shader, "isFirstMip", 1f);

        for (var lowerMipLevel = 0; lowerMipLevel < mipLevels; lowerMipLevel++)
        {
            //Get the current size of this mipmap
            currentSize = new int[] { previousSize[0] / 2, previousSize[1] / 2 };  //n for width and height
            rounded = new bool[] { previousSize[0] / 2f > currentSize[0], previousSize[1] / 2f > currentSize[1] };    //Must be floats for the division

            //Pick a kernel given the state of the rounded edges (0, 1, 2, 3)
            int kernelIndex;
            if (rounded[0] == false)
            {
                if (rounded[1] == false)
                {
                    //Box - box
                    kernelIndex = 0;
                }
                else
                {
                    //Box - poly
                    kernelIndex = 1;
                }
            }
            else
            {
                if (rounded[1] == false)
                {
                    //Poly - box
                    kernelIndex = 2;
                }
                else
                {
                    //Poly - poly
                    kernelIndex = 3;
                }
            }

            // Set shader properties

            //Bind the lower mip level to read from
            m_polyphaseMipmapBuffer.SetComputeTextureParam(_shader, kernelIndex, "LowerMipTex", _textureID, lowerMipLevel);

            //Bind the higher mip level to write to
            m_polyphaseMipmapBuffer.SetComputeTextureParam(_shader, kernelIndex, "HigherMipTex", _textureID, lowerMipLevel + 1);

            //Setup additional working variables
            m_polyphaseMipmapBuffer.SetComputeVectorParam(_shader, "LowerMipTexSize", new Vector2(previousSize[0], previousSize[1]));
            m_polyphaseMipmapBuffer.SetComputeVectorParam(_shader, "HigherMipTexSize", new Vector2(currentSize[0], currentSize[1]));
            m_polyphaseMipmapBuffer.SetComputeVectorParam(_shader, "WasTextureSizeRounded", new Vector2(rounded[0] ? 1f : 0f, rounded[1] ? 1f : 0f));

            //Calculate these once so doesn't need to be repeated for each pixel
            m_polyphaseMipmapBuffer.SetComputeVectorParam(_shader, "HigherMipTexelUVSize", new Vector2(1f / currentSize[0], 1f / currentSize[1]));
            m_polyphaseMipmapBuffer.SetComputeVectorParam(_shader, "LowerMipTexelUVSize", new Vector2(1f / previousSize[0], 1f / previousSize[1]));

            m_polyphaseMipmapBuffer.DispatchCompute(
                _shader,
                kernelIndex,
                Mathf.CeilToInt(currentSize[0] / 8f),
                Mathf.CeilToInt(currentSize[1] / 8f),
                1
            );

            //Set previous size for next mip
            previousSize = currentSize;

            if (lowerMipLevel == 0)
            {
                m_polyphaseMipmapBuffer.SetComputeFloatParam(_shader, "isFirstMip", 0f);
            }
        }

        L2DLRenderHelpers.ExecuteBuffer(_context, m_polyphaseMipmapBuffer);
    }

    // --------------------------------------------------------------------
    private void GenerateIndirectLight
        (
        ScriptableRenderContext _context,
        RenderTargetIdentifier i_emissiveTexture,
        RenderTargetIdentifier i_occlusionTexture,
        RenderTargetIdentifier o_indirectLightTexture,
        Camera _camera, 
        List<LPVIterationData> _lpvIterationsData, 
        Color _ambientLightColour
        )
    {
        // Get working textures
        RenderTextureDescriptor bufferTextureDescriptor = new RenderTextureDescriptor(_camera.pixelWidth, _camera.pixelHeight, RenderTextureFormat.ARGBHalf, 0)
        {
            useMipMap = true,
            autoGenerateMips = false,
            enableRandomWrite = true
        };

        m_lpvIterationsBuffer.GetTemporaryRT(flipTextureId, bufferTextureDescriptor, FilterMode.Point);
        m_lpvIterationsBuffer.GetTemporaryRT(flopTextureId, bufferTextureDescriptor, FilterMode.Point);

        // Check shaders
        if (m_lpvIterationsComputeShader == null)
        {
            m_lpvIterationsComputeShader = (ComputeShader)Resources.Load("Compute Shaders/Indirect Light/LPV/LPVIterationCompute");
        }
        m_lpvIterationsFlipKernel = m_lpvIterationsComputeShader.FindKernel("LPVIterationFlip");
        m_lpvIterationsFlopKernel = m_lpvIterationsComputeShader.FindKernel("LPVIterationFlop");

        m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlipKernel, "OcclusionInput", i_occlusionTexture);
        m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlopKernel, "OcclusionInput", i_occlusionTexture);
        m_lpvIterationsBuffer.SetComputeFloatParam(m_lpvIterationsComputeShader, "OcclusionMultiplier", 1f); // TODO

        if (m_moveLPVDataDownMipLevelsComputeShader == null)
        {
            m_moveLPVDataDownMipLevelsComputeShader = (ComputeShader)Resources.Load("Compute Shaders/Indirect Light/LPV/MoveLPVDataDownMipLevels");
        }
        m_moveLPVDataDownMipLevelsKernel = m_moveLPVDataDownMipLevelsComputeShader.FindKernel("MoveLPVDataDownMipLevels");

        // LPV Iterations
        float[] textureWidths = new float[_lpvIterationsData.Count];
        float[] textureHeights = new float[_lpvIterationsData.Count];

        int totalIterationsSoFar = 0;

        for (int i = 0; i < _lpvIterationsData.Count; i++)
        {
            float lightingOutputResolutionDivisor = Mathf.Pow(2, _lpvIterationsData[i].MipLevel);

            textureWidths[i] = _camera.pixelWidth / lightingOutputResolutionDivisor;
            textureHeights[i] = _camera.pixelHeight / lightingOutputResolutionDivisor;

            int threadGroupsX = Mathf.CeilToInt(_camera.pixelWidth / lightingOutputResolutionDivisor / 8f);
            int threadGroupsY = Mathf.CeilToInt(_camera.pixelHeight / lightingOutputResolutionDivisor / 8f);

            if (i == 0)
            {
                //Copy
                m_lpvIterationsBuffer.CopyTexture(i_emissiveTexture, 0, _lpvIterationsData[i].MipLevel, flipTextureId, 0, _lpvIterationsData[i].MipLevel);

                //We just want to inject the ambient light colour into the specified levels
                //if (_lpvIterationsData[i].InjectAmbientLight)
                //{
                //    m_lpvIterationsBuffer.SetComputeTextureParam(shaders.ambientLightInjectionComputeShader, shaders.ambientLightInjectionComputeKernel, "Texture", flipTextureId, _lpvIterationsData[i].MipLevel);
                //    m_lpvIterationsBuffer.SetComputeVectorParam(shaders.ambientLightInjectionComputeShader, "AmbientLight", _ambientLightColour);
                //    m_lpvIterationsBuffer.DispatchCompute(shaders.ambientLightInjectionComputeShader, shaders.ambientLightInjectionComputeKernel, threadGroupsX, threadGroupsY, 1);
                //}
            }
            else
            {
                //Instead of copying across I need to shift all of the data down some mip levels using a compute shader for:
                // - The lighting flip tex (from whichever one was last used)1
                // - The lighting total tex
                m_lpvIterationsBuffer.SetComputeVectorParam(m_moveLPVDataDownMipLevelsComputeShader, "OutputCellSize", new Vector2(textureWidths[i] / textureWidths[i - 1], textureHeights[i] / textureHeights[i - 1]));

                m_lpvIterationsBuffer.SetComputeTextureParam(m_moveLPVDataDownMipLevelsComputeShader, m_moveLPVDataDownMipLevelsKernel, "CurrentTotalInput", o_indirectLightTexture, _lpvIterationsData[i - 1].MipLevel);
                m_lpvIterationsBuffer.SetComputeTextureParam(m_moveLPVDataDownMipLevelsComputeShader, m_moveLPVDataDownMipLevelsKernel, "EmissiveSourcesInput", i_emissiveTexture, _lpvIterationsData[i].MipLevel);
                m_lpvIterationsBuffer.SetComputeTextureParam(m_moveLPVDataDownMipLevelsComputeShader, m_moveLPVDataDownMipLevelsKernel, "Output1", (totalIterationsSoFar % 2) == 0 ? flipTextureId : flopTextureId, _lpvIterationsData[i].MipLevel);
                m_lpvIterationsBuffer.SetComputeTextureParam(m_moveLPVDataDownMipLevelsComputeShader, m_moveLPVDataDownMipLevelsKernel, "Output2", o_indirectLightTexture, _lpvIterationsData[i].MipLevel);
                m_lpvIterationsBuffer.DispatchCompute(m_moveLPVDataDownMipLevelsComputeShader, m_moveLPVDataDownMipLevelsKernel, threadGroupsX, threadGroupsY, 1);

                //if (_lpvIterationsData[i].InjectAmbientLight)
                //{
                //    m_lpvIterationsBuffer.SetComputeTextureParam(shaders.ambientLightInjectionComputeShader, shaders.ambientLightInjectionComputeKernel, "Texture", (totalIterationsSoFar % 2) == 0 ? flipTextureId : flopTextureId, _lpvIterationsData[i].MipLevel);
                //    m_lpvIterationsBuffer.SetComputeVectorParam(shaders.ambientLightInjectionComputeShader, "AmbientLight", _ambientLightColour);
                //    m_lpvIterationsBuffer.DispatchCompute(shaders.ambientLightInjectionComputeShader, shaders.ambientLightInjectionComputeKernel, threadGroupsX, threadGroupsY, 1);
                //}
            }

            //Clear only the FIRST TIME as it's the start of a new total counting
            if (_lpvIterationsData[i].MipLevel != 0)
            {
                m_lpvIterationsBuffer.SetRenderTarget(o_indirectLightTexture, _lpvIterationsData[i].MipLevel);
                m_lpvIterationsBuffer.ClearRenderTarget(true, true, Color.black);
            }

            m_lpvIterationsBuffer.SetComputeIntParam(m_lpvIterationsComputeShader, "WorkingMipLevel", _lpvIterationsData[i].MipLevel);
            m_lpvIterationsBuffer.SetComputeIntParam(m_lpvIterationsComputeShader, "StartTotalSavingIteration", _lpvIterationsData[i].TotalStartIteration);

            m_lpvIterationsBuffer.SetComputeFloatParam(m_lpvIterationsComputeShader, "TexelWorldWidth", _camera.orthographicSize / (_camera.pixelHeight / lightingOutputResolutionDivisor / 2f));
            m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlipKernel, "LightTotal", o_indirectLightTexture, _lpvIterationsData[i].MipLevel);
            m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlopKernel, "LightTotal", o_indirectLightTexture, _lpvIterationsData[i].MipLevel);
            m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlipKernel, "LightFlip", flipTextureId, _lpvIterationsData[i].MipLevel);
            m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlipKernel, "LightFlop", flopTextureId, _lpvIterationsData[i].MipLevel);
            m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlopKernel, "LightFlip", flipTextureId, _lpvIterationsData[i].MipLevel);
            m_lpvIterationsBuffer.SetComputeTextureParam(m_lpvIterationsComputeShader, m_lpvIterationsFlopKernel, "LightFlop", flopTextureId, _lpvIterationsData[i].MipLevel);
            
            for (var j = 0; j < _lpvIterationsData[i].Iterations; j++)
            {
                m_lpvIterationsBuffer.SetComputeIntParam(m_lpvIterationsComputeShader, "Iteration", j);
                m_lpvIterationsBuffer.DispatchCompute(m_lpvIterationsComputeShader, (totalIterationsSoFar + j) % 2 == 0 ? m_lpvIterationsFlipKernel : m_lpvIterationsFlopKernel, threadGroupsX, threadGroupsY, 1);
            }

            totalIterationsSoFar += _lpvIterationsData[i].Iterations;
        }

        //Release textures
        m_lpvIterationsBuffer.ReleaseTemporaryRT(flipTextureId);
        m_lpvIterationsBuffer.ReleaseTemporaryRT(flopTextureId);

        L2DLRenderHelpers.ExecuteBuffer(_context, m_lpvIterationsBuffer);
    }

    // --------------------------------------------------------------------
    [Conditional("UNITY_EDITOR")]
    private void ReloadComputeShaders()
    {
        m_directionalLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Compute Shaders/Direct Light/Occlusion Trace Compute/DirectionalOcclusionMapTrace");
    }
}
