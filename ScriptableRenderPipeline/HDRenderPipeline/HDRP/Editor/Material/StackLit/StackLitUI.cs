using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    class StackLitGUI : BaseMaterialGUI
    {
        protected static class StylesStackLit
        {
            public static GUIContent useLocalPlanarMapping = new GUIContent("Use Local Planar Mapping", "Use local space for planar/triplanar mapping instead of world space");
        };

        #region Strings

        protected const string k_DoubleSidedNormalMode = "_DoubleSidedNormalMode";

        protected const string k_UVBase = "_UVBase";

        // Base
        protected const string k_BaseColor = "_BaseColor";
        protected const string k_BaseColorMap = "_BaseColorMap";
        protected const string k_BaseColorMapUV = "_BaseColorMapUV";

        protected const string k_Metallic = "_Metallic";
        protected const string k_MetallicMap = "_MetallicMap";
        protected const string k_MetallicMapUV = "_MetallicMapUV";

        protected const string k_Smoothness1 = "_SmoothnessA";
        protected const string k_Smoothness1Map = "_SmoothnessAMap";
        protected const string k_Smoothness1MapUV = "_SmoothnessAMapUV";

        protected const string k_NormalMap = "_NormalMap";
        protected const string k_NormalMapUV = "_NormalMapUV";
        protected const string k_NormalScale = "_NormalScale";

        protected const string k_AmbientOcclusion = "_AmbientOcclusion";
        protected const string k_AmbientOcclusionMap = "_AmbientOcclusionMap";
        protected const string k_AmbientOcclusionMapUV = "_AmbientOcclusionMapUV";

        // Emissive
        protected const string k_EmissiveColor = "_EmissiveColor";
        protected const string k_EmissiveColorMap = "_EmissiveColorMap";
        protected const string k_EmissiveColorMapUV = "_EmissiveColorMapUV";
        protected const string k_EmissiveIntensity = "_EmissiveIntensity";
        protected const string k_AlbedoAffectEmissive = "_AlbedoAffectEmissive";

        // Coat
        protected const string k_EnableCoat = "_EnableCoat";
        protected const string k_CoatSmoothness = "_CoatSmoothness";
        protected const string k_CoatIor = "_CoatIor";
        protected const string k_CoatThickness = "_CoatThickness";
        protected const string k_CoatExtinction = "_CoatExtinction";

        // SSS
        protected const string k_EnableSubsurfaceScattering = "_EnableSubsurfaceScattering";
        protected const string k_DiffusionProfile = "_DiffusionProfile";
        protected const string k_SubsurfaceMask = "_SubsurfaceMask";
        protected const string k_SubsurfaceMaskMap = "_SubsurfaceMaskMap";
        protected const string k_SubsurfaceMaskMapUV = "_SubsurfaceMaskMapUV";

        // Translucency
        protected const string k_EnableTransmission = "_EnableTransmission";
        protected const string k_Thickness = "_Thickness";
        protected const string k_ThicknessMap = "_ThicknessMap";
        protected const string k_ThicknessMapUV = "_ThicknessMapUV";

        // Second Lobe.
        protected const string k_EnableDualSpecularLobe = "_EnableDualSpecularLobe";
        protected const string k_Smoothness2 = "_SmoothnessB";
        protected const string k_Smoothness2Map = "_SmoothnessBMap";
        protected const string k_Smoothness2MapUV = "_SmoothnessBMapUV";

        protected const string k_LobeMix = "_LobeMix";

        // Anisotropy
        protected const string k_EnableAnisotropy = "_EnableAnisotropy";
        protected const string k_Anisotropy = "_Anisotropy";
        protected const string k_AnisotropyMap = "_AnisotropyMap";
        protected const string k_AnisotropyMapUV = "_AnisotropyMapUV";

        // Iridescence
        protected const string k_EnableIridescence = "_EnableIridescence";
        protected const string k_IridescenceIor = "_IridescenceIor";
        protected const string k_IridescenceThickness = "_IridescenceThickness";
        protected const string k_IridescenceThicknessMap = "_IridescenceThicknessMap";
        protected const string k_IridescenceThicknessMapUV = "_IridescenceThicknessMapUV";

        // Stencil is use to control lighting mode (regular, split lighting)
        protected const string kStencilRef = "_StencilRef";
        protected const string kStencilWriteMask = "_StencilWriteMask";
        protected const string kStencilRefMV = "_StencilRefMV";
        protected const string kStencilWriteMaskMV = "_StencilWriteMaskMV";

        #endregion

        // Add the properties into an array.
        private readonly GroupProperty _baseMaterialProperties = null;
        private readonly GroupProperty _materialProperties = null;

        private Property EnableSSS;
        private Property EnableTransmission;
        private Property EnableCoat;
        private Property EnableAnisotropy;
        private Property EnableDualSpecularLobe;
        private Property EnableIridescence;

        public StackLitGUI()
        {
            _baseMaterialProperties = new GroupProperty(this, "_BaseMaterial", new BaseProperty[]
            {
                // JFFTODO: Find the proper condition, and proper way to display this.
                new Property(this, k_DoubleSidedNormalMode, "Normal mode", "This will modify the normal base on the selected mode. Mirror: Mirror the normal with vertex normal plane, Flip: Flip the normal.", false),
            });

            //
            EnableSSS = new Property(this, k_EnableSubsurfaceScattering, "Enable Subsurface Scattering", "Enable Subsurface Scattering", true);
            EnableTransmission = new Property(this, k_EnableTransmission, "Enable Transmission", "Enable Transmission", true);
            EnableCoat = new Property(this, k_EnableCoat, "Enable Coat", "Enable coat layer with true vertical physically based BSDF mixing", true);
            EnableAnisotropy = new Property(this, k_EnableAnisotropy, "Enable Anisotropy", "Enable anisotropy, correct anisotropy for punctual light but very coarse approximated for reflection", true);
            EnableDualSpecularLobe = new Property(this, k_EnableDualSpecularLobe, "Enable Dual Specular Lobe", "Enable a second specular lobe, aim to simulate a mix of a narrow and a haze lobe that better match measured material", true);
            EnableIridescence = new Property(this, k_EnableIridescence, "Enable Iridescence", "Enable physically based iridescence layer", true);

            // All material properties
            _materialProperties = new GroupProperty(this, "_Material", new BaseProperty[]
            {
                new GroupProperty(this, "_MaterialFeatures", "Material Features", new BaseProperty[]
                {
                    EnableSSS,
                    EnableTransmission,
                    EnableCoat,
                    EnableAnisotropy,
                    EnableDualSpecularLobe,
                    EnableIridescence
                }),

                new GroupProperty(this, "_Standard", "Standard", new BaseProperty[]
                {
                    new TextureProperty(this, k_BaseColorMap, k_BaseColor, "Base Color + Opacity", "Albedo (RGB) and Opacity (A)", true, false),
                    new TextureProperty(this, k_MetallicMap, k_Metallic, "Metallic", "Metallic", false, false),
                    new TextureProperty(this, k_Smoothness1Map, k_Smoothness1, "Smoothness", "Smoothness", false, false),
                    // TODO: Special case for normal maps.
                    new TextureProperty(this, k_NormalMap, k_NormalScale, "Normal", "Normal Map", false, false, true),

                    //new TextureProperty(this, k_BaseColorMap, k_BaseColor, "Dielectric IoR", "Index of Refraction for Dielectric", false),

                    new TextureProperty(this, k_AmbientOcclusionMap, k_AmbientOcclusion, "AmbientOcclusion", "AmbientOcclusion Map", false, false),
                }),

                new GroupProperty(this, "_DualSpecularLobe", "Dual Specular Lobe", new BaseProperty[]
                {
                    new TextureProperty(this, k_Smoothness2Map, k_Smoothness2, "Smoothness2", "Smoothness2", false, false),
                    new Property(this, k_LobeMix, "Lobe Mix", "Lobe Mix", false),
                }, _ => EnableDualSpecularLobe.BoolValue == true),

                new GroupProperty(this, "_Anisotropy", "Anisotropy", new BaseProperty[]
                {
                    new Property(this, k_Anisotropy, "Anisotropy", "Anisotropy of base layer", false),
                    // TODO: Tangent map and rotation
                }, _ => EnableAnisotropy.BoolValue == true),

                new GroupProperty(this, "_Coat", "Coat", new BaseProperty[]
                {
                    new Property(this, "_CoatSmoothness", "Coat Smoothness", "Top layer smoothness", false),
                    new Property(this, "_CoatIor", "Coat IOR", "Index of refraction", false),
                    new Property(this, "_CoatThickness", "Coat Thickness", "Coat thickness", false),
                    new Property(this, "_CoatExtinction", "Coat Absorption", "Coat absorption tint (the thicker the coat, the more that color is removed)", false),
                }, _ =>EnableCoat.BoolValue == true),

                new GroupProperty(this, "_Iridescence", "Iridescence", new BaseProperty[]
                {
                    new Property(this, "_IridescenceIor", "IOR", "Index of refraction of iridescence layer", false),
                    new Property(this, "_IridescenceThickness", "Thickness", "Iridescence thickness (Remap to 0..3000nm)", false),
                }, _ => EnableIridescence.BoolValue == true),

                new GroupProperty(this, "_SSS", "Sub-Surface Scattering", new BaseProperty[]
                {
                    new DiffusionProfileProperty(this, k_DiffusionProfile, "Diffusion Profile", "A profile determines the shape of the SSS/transmission filter.", false),
                    new TextureProperty(this, k_SubsurfaceMaskMap, k_SubsurfaceMask, "Subsurface mask map (R)", "Determines the strength of the subsurface scattering effect.", false, false),
                }, _ => EnableSSS.BoolValue == true ),

                new GroupProperty(this, "_Transmission", "Transmission", new BaseProperty[]
                {
                    new DiffusionProfileProperty(this, k_DiffusionProfile, "Diffusion Profile", "A profile determines the shape of the SSS/transmission filter.", false, _ => EnableSSS.BoolValue == false),
                    new TextureProperty(this, k_ThicknessMap, k_Thickness, "Thickness", "If subsurface scattering is enabled, low values allow some light to be transmitted through the object.", false),
                }, _ => EnableTransmission.BoolValue == true),

                new GroupProperty(this, "_Emissive", "Emissive", new BaseProperty[]
                {
                    new TextureProperty(this, k_EmissiveColorMap, k_EmissiveColor, "Emissive Color", "Emissive", true, false),
                    new Property(this, k_EmissiveIntensity, "Emissive Intensity", "Emissive", false),
                    new Property(this, k_AlbedoAffectEmissive, "Albedo Affect Emissive", "Specifies whether or not the emissive color is multiplied by the albedo.", false),
                }),

                new GroupProperty(this, "_Debug", "Debug", new BaseProperty[]
                {
                    new Property(this, "_DebugEnable", "Debug Enable", "Switch to a debug version of the shader", false),
                    new Property(this, "_DebugLobeMask", "DebugLobeMask", "xyz is Lobe 0 1 2 Enable, w is Enable VLayering", false),
                    new Property(this, "_DebugAniso", "DebugAniso", "x is Hack Enable, y is factor", false),
                }),
            });
        }

        protected override bool ShouldEmissionBeEnabled(Material material)
        {
            return material.GetFloat(k_EmissiveIntensity) > 0.0f;
        }

        protected override void FindBaseMaterialProperties(MaterialProperty[] props)
        {
            base.FindBaseMaterialProperties(props);
            _baseMaterialProperties.OnFindProperty(props);
        }

        protected override void FindMaterialProperties(MaterialProperty[] props)
        {
            //base.FindMaterialProperties(props);
            _materialProperties.OnFindProperty(props);
        }

        protected override void BaseMaterialPropertiesGUI()
        {
            base.BaseMaterialPropertiesGUI();
            _baseMaterialProperties.OnGUI();
        }

        protected override void MaterialPropertiesGUI(Material material)
        {
            //if (GUILayout.Button("Generate All Properties"))
            //{
            //    Debug.Log(_materialProperties.ToShaderPropertiesStringInternal());
            //}

            _materialProperties.OnGUI();
        }

        protected override void MaterialPropertiesAdvanceGUI(Material material)
        {
        }

        protected override void VertexAnimationPropertiesGUI()
        {
        }

        protected override void SetupMaterialKeywordsAndPassInternal(Material material)
        {
            SetupMaterialKeywordsAndPass(material);
        }

        protected static void SetupTextureMaterialProperty(Material material, string basePropertyName)
        {
            // TODO: Caution this can generate a lot of garbage collection call ?
            string useMapPropertyName = basePropertyName + "UseMap";
            string mapPropertyName = basePropertyName + "Map";
            string remapPropertyName = basePropertyName + "Remap";
            string invertPropertyName = basePropertyName + "RemapInverted";
            string rangePropertyName = basePropertyName + "Range";
            string channelPropertyName = basePropertyName + "MapChannel";
            string channelMaskPropertyName = basePropertyName + "MapChannelMask";

            if (material.GetTexture(mapPropertyName))
            {
                Vector4 rangeVector = material.GetVector(remapPropertyName);
                if (material.HasProperty(invertPropertyName) && material.GetFloat(invertPropertyName) > 0.0f)
                {
                    float s = rangeVector.x;
                    rangeVector.x = rangeVector.y;
                    rangeVector.y = s;
                }

                material.SetFloat(useMapPropertyName, 1.0f);
                material.SetVector(rangePropertyName, rangeVector);

                int channel = (int)material.GetFloat(channelPropertyName);
                switch (channel)
                {
                case 0:
                    material.SetVector(channelMaskPropertyName, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    break;
                case 1:
                    material.SetVector(channelMaskPropertyName, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
                    break;
                case 2:
                    material.SetVector(channelMaskPropertyName, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
                    break;
                case 3:
                    material.SetVector(channelMaskPropertyName, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                    break;
                }
            }
            else
            {
                material.SetFloat(useMapPropertyName, 0.0f);
                material.SetVector(rangePropertyName, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
                material.SetVector(channelMaskPropertyName, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
            }
        }

        // All Setup Keyword functions must be static. It allow to create script to automatically update the shaders with a script if code change
        public static void SetupMaterialKeywordsAndPass(Material material)
        {
            //TODO see BaseLitUI.cs:SetupBaseLitKeywords (stencil etc)
            SetupBaseUnlitKeywords(material);
            SetupBaseUnlitMaterialPass(material);

            bool doubleSidedEnable = material.GetFloat(kDoubleSidedEnable) > 0.0f;

            if (doubleSidedEnable)
            {
                BaseLitGUI.DoubleSidedNormalMode doubleSidedNormalMode =
                    (BaseLitGUI.DoubleSidedNormalMode) material.GetFloat(k_DoubleSidedNormalMode);
                switch (doubleSidedNormalMode)
                {
                    case BaseLitGUI.DoubleSidedNormalMode.Mirror: // Mirror mode (in tangent space)
                        material.SetVector("_DoubleSidedConstants", new Vector4(1.0f, 1.0f, -1.0f, 0.0f));
                        break;

                    case BaseLitGUI.DoubleSidedNormalMode.Flip: // Flip mode (in tangent space)
                        material.SetVector("_DoubleSidedConstants", new Vector4(-1.0f, -1.0f, -1.0f, 0.0f));
                        break;

                    case BaseLitGUI.DoubleSidedNormalMode.None: // None mode (in tangent space)
                        material.SetVector("_DoubleSidedConstants", new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                        break;
                }
            }

            //TODO: stencil state, displacement, wind, depthoffset, tessellation

            SetupMainTexForAlphaTestGI("_BaseColorMap", "_BaseColor", material);

            //TODO: disable DBUFFER

            SetupTextureMaterialProperty(material, k_Metallic);
            SetupTextureMaterialProperty(material, k_Smoothness1);
            SetupTextureMaterialProperty(material, k_Smoothness2);
            SetupTextureMaterialProperty(material, k_AmbientOcclusion);
            SetupTextureMaterialProperty(material, k_SubsurfaceMask);
            SetupTextureMaterialProperty(material, k_Thickness);
            SetupTextureMaterialProperty(material, k_Anisotropy);
            SetupTextureMaterialProperty(material, k_IridescenceThickness);

            // Check if we are using specific UVs.
            TextureProperty.UVMapping[] uvIndices = new[]
            {
                (TextureProperty.UVMapping) material.GetFloat(k_BaseColorMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_MetallicMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_NormalMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_Smoothness1MapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_Smoothness2MapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_AmbientOcclusionMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_EmissiveColorMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_SubsurfaceMaskMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_ThicknessMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_AnisotropyMapUV),
                (TextureProperty.UVMapping) material.GetFloat(k_IridescenceThicknessMapUV),
            };

            // Set keyword for mapping

            //bool requireUv2 = false;
            //bool requireUv3 = false;
            bool requireTriplanar = false;
            for (int i = 0; i < uvIndices.Length; ++i)
            {
                //requireUv2 = requireUv2 || uvIndices[i] == TextureProperty.UVMapping.UV2;
                //requireUv3 = requireUv3 || uvIndices[i] == TextureProperty.UVMapping.UV3;
                requireTriplanar = requireTriplanar || uvIndices[i] == TextureProperty.UVMapping.Triplanar;
            }
            CoreUtils.SetKeyword(material, "_USE_TRIPLANAR", requireTriplanar);

            bool dualSpecularLobeEnabled = material.HasProperty(k_EnableDualSpecularLobe) && material.GetFloat(k_EnableDualSpecularLobe) > 0.0f;
            CoreUtils.SetKeyword(material, "_MATERIAL_FEATURE_DUAL_SPECULAR_LOBE", dualSpecularLobeEnabled);

            bool anisotropyEnabled = material.HasProperty(k_EnableAnisotropy) && material.GetFloat(k_EnableAnisotropy) > 0.0f;
            CoreUtils.SetKeyword(material, "_MATERIAL_FEATURE_ANISOTROPY", anisotropyEnabled);

            bool iridescenceEnabled = material.HasProperty(k_EnableIridescence) && material.GetFloat(k_EnableIridescence) > 0.0f;
            CoreUtils.SetKeyword(material, "_MATERIAL_FEATURE_IRIDESCENCE", iridescenceEnabled);

            bool transmissionEnabled = material.HasProperty(k_EnableAnisotropy) && material.GetFloat(k_EnableAnisotropy) > 0.0f;
            CoreUtils.SetKeyword(material, "_MATERIAL_FEATURE_TRANSMISSION", transmissionEnabled);

            bool sssEnabled = material.HasProperty(k_EnableSubsurfaceScattering) && material.GetFloat(k_EnableSubsurfaceScattering) > 0.0f;
            CoreUtils.SetKeyword(material, "_MATERIAL_FEATURE_SUBSURFACE_SCATTERING", sssEnabled);

            // TEMP - Remove once dev is finish
            bool debugEnabled = material.HasProperty("_DebugEnable") && material.GetFloat("_DebugEnable") > 0.0f;
            CoreUtils.SetKeyword(material, "_STACKLIT_DEBUG", debugEnabled);


            // Set the reference value for the stencil test - required for SSS
            int stencilRef = (int)StencilLightingUsage.RegularLighting;
            if (sssEnabled)
            {
                stencilRef = (int)StencilLightingUsage.SplitLighting;
            }

            // As we tag both during velocity pass and Gbuffer pass we need a separate state and we need to use the write mask
            material.SetInt(kStencilRef, stencilRef);
            material.SetInt(kStencilWriteMask, (int)HDRenderPipeline.StencilBitMask.LightingMask);
            material.SetInt(kStencilRefMV, (int)HDRenderPipeline.StencilBitMask.ObjectVelocity);
            material.SetInt(kStencilWriteMaskMV, (int)HDRenderPipeline.StencilBitMask.ObjectVelocity);
        }
    }
} // namespace UnityEditor
