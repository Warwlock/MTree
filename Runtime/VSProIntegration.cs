using System.Collections;
using System.Collections.Generic;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;
 
namespace AwesomeTechnologies.Shaders
{
    public class MtreeShaderController : IShaderController
    {
        public bool MatchShader(string shaderName)
        {
            return (shaderName == "Mtree/Bark" || shaderName == "Mtree/Leafs");
        }
 
        public bool MatchBillboardShader(Material[] materials)
        {
            return false;
        }
 
        public ShaderControllerSettings Settings { get; set; }
        bool CustomLightSource;
 
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "Mtree settings",
                Description = "",
                LODFadePercentage = true,
                LODFadeCrossfade = true,
                SampleWind = true,
            };
  
            Settings.AddLabelProperty("Foliage settings");
            Settings.AddColorProperty("BarkTintColor", "Bark Color", "", GetColor(materials, "_Color","Bark"));
            Settings.AddColorProperty("FoliageTintColor", "Leaf Color", "", GetColor(materials,"_Color","Leafs"));
            Settings.AddFloatProperty("MtreeCutoff","Alpha Cutoff","",GetFloatValue(materials,"_Cutoff","Leafs"),0,1);
            

            Settings.AddLabelProperty("Translucency Settings");
            Settings.AddFloatProperty("TranslucentStrength","Srength", "", GetFloatValue(materials, "_Translucency", "Leafs"), 0, 50);
            Settings.AddFloatProperty("TransNormalDistortion", "Normal Distortion", "", GetFloatValue(materials, "_TransNormalDistortion", "Leafs"), 0, 1);
            Settings.AddFloatProperty("TransScattering", "Scattering Falloff", "", GetFloatValue(materials, "_TransScattering", "Leafs"), 1, 50);
            Settings.AddColorProperty("TranslucentColor","Translucent Color","Only Works if Translucent Light Mode set to Custom!",GetColor(materials, "_TranslucencyTint", "Leafs"));

            Settings.AddLabelProperty("Other Settings");
            Settings.AddFloatProperty("AmbientOcclusion", "AO Strength", "", GetGlobalFloatValue(materials, "_OcclusionStrength"), 0, 1);
            Settings.AddFloatProperty("MtreeGlobalWindInfluence", "Global Wind Influence","",GetGlobalFloatValue(materials,"_GlobalWindInfluence"),0,1);
            Settings.AddFloatProperty("MtreeGlobalWindTurbulence", "Global Wind Turbulence Influence","",GetFloatValue(materials,"_GlobalTurbulenceInfluence","Leafs"),0,1);

        }
       
        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
                float GlobalWindInfluence = Settings.GetFloatPropertyValue("MtreeGlobalWindInfluence");
                material.SetFloat("_GlobalWindInfluence", GlobalWindInfluence);
                float aoStrength = Settings.GetFloatPropertyValue("AmbientOcclusion");
                material.SetFloat("_OcclusionStrength", aoStrength);

            if (material.shader.name == "Mtree/Bark")
            {
                Color barkTintColor = Settings.GetColorPropertyValue("BarkTintColor");
                material.SetColor("_Color", barkTintColor);
            }
            if (material.shader.name == "Mtree/Leafs")
            {
                Color foliageTintColor = Settings.GetColorPropertyValue("FoliageTintColor");
                material.SetColor("_Color", foliageTintColor);

                float foliageCutoff = Settings.GetFloatPropertyValue("MtreeCutoff");
                material.SetFloat("_Cutoff", foliageCutoff);

                float translucentStrength = Settings.GetFloatPropertyValue("TranslucentStrength");
                material.SetFloat("_Translucency", translucentStrength);

                float TransNormalDistortion = Settings.GetFloatPropertyValue("TransnormalDistortion");
                material.SetFloat("_TransNormalDistortion", TransNormalDistortion);

                float TransScattering = Settings.GetFloatPropertyValue("TransScattering");
                material.SetFloat("_TransScattering", TransScattering);

                Color translucentColor = Settings.GetColorPropertyValue("TranslucentColor");
                material.SetColor("_TranslucencyTint",translucentColor);

                float GlobalWindTurbulence = Settings.GetFloatPropertyValue("MtreeGlobalWindTurbulence");
                material.SetFloat("_GlobalTurbulenceInfluence",GlobalWindTurbulence);
            }
                
                
               


         
        }

        Color GetColor(Material[] materials, string property,string shadertype)
        {
            foreach (Material mat in materials)
            {
                if (mat.shader.name == "Mtree/"+shadertype)
                    return mat.GetColor(property);
            }
            return Color.black;
        }
        float GetFloatValue(Material[] materials,string property, string shadertype)
        {
            foreach(Material mat in materials)
                if (mat.shader.name == "Mtree/" + shadertype)
                    return mat.GetFloat(property);
            return 0.1f;
        }
        float GetGlobalFloatValue(Material[] materials, string property)
        {
            foreach (Material mat in materials)
                if (mat.shader.name == "Mtree/Bark" || mat.shader.name == "Mtree/Leafs")
                    return mat.GetFloat(property);
            return 0.1f;
        }





        public void UpdateWind(Material material, WindSettings windSettings)
        {
        }
    }
}
#endif