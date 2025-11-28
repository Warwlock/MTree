using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(WindZone))]
public class MtreeWind : MonoBehaviour {

    [Header("Global Windzone")]
    public WindZone windZone;
    [Header("Mtree Wind Offset")]
    public float windStrength = 0;
    public float windDirection = 0;
    public float windPulse = 0;
    public float windTurbulence = 0;
    [Range(0,1)]public float windRandomness = 1;
    [Header("Billboard")]
    public bool BillboardWind = false;
    [Range(0,1)]public float BillboardWindInfluence = .5f;

    // Updatecheck values
    float m_windStrength, m_windDirection, m_windPulse, m_windTurbulence, m_windRadius;
    Vector3 m_position;
    void Awake(){
        if(!TryGetComponent(out windZone))
        {
			windZone = gameObject.AddComponent<WindZone>();
        }
    }

	void Update () {
        if(windZone != null){
            if(m_windStrength != windZone.windMain || m_windDirection != windZone.transform.rotation.eulerAngles.y || m_position != transform.position ||
                m_windPulse != windZone.windPulseFrequency || m_windTurbulence != windZone.windTurbulence || m_windRadius != windZone.radius)
            {
                UpdateWindZone();
                m_windStrength = windZone.windMain;
                m_windDirection = windZone.transform.rotation.eulerAngles.y;
                m_windPulse = windZone.windPulseFrequency;
                m_windTurbulence = windZone.windTurbulence;
                m_windRadius = windZone.radius;
                m_position = transform.position;
            }
        }
    }
	
    public void UpdateWind()
    {
        Shader.SetGlobalFloat("_WindStrength", windStrength);
        Shader.SetGlobalFloat("_WindDirection",windDirection);
        Shader.SetGlobalFloat("_WindPulse",windPulse);
        Shader.SetGlobalFloat("_WindTurbulence",windTurbulence);
    }
    public void UpdateWindZone()
    {
        Shader.SetGlobalFloat("_WindRadius", windZone.radius);
        Shader.SetGlobalVector("_WindPosition", transform.position);
        Shader.SetGlobalInt("_IsLocalWind", windZone.mode == WindZoneMode.Spherical ? 1 : 0);

        Shader.SetGlobalFloat("_WindStrength",windZone.windMain + windStrength);
        Shader.SetGlobalFloat("_WindDirection",windZone.transform.localRotation.eulerAngles.y + windDirection);
        Shader.SetGlobalFloat("_WindPulse",windZone.windPulseFrequency + windPulse);
        Shader.SetGlobalFloat("_WindTurbulence",windZone.windTurbulence + windTurbulence);
    }
    public void OnValidate(){

        Shader.SetGlobalFloat("_RandomWindOffset",windRandomness);

        if(windZone != null)
            UpdateWindZone();

        if(BillboardWind){
            Shader.SetGlobalInt("BillboardWindEnabled",0);
            Shader.SetGlobalFloat("Billboard_WindStrength",BillboardWindInfluence);
            }
        if(!BillboardWind){
            Shader.SetGlobalInt("BillboardWindEnabled",1);
            }
        
    }
	public void ResetToZero(){
		windStrength = 0;
		windDirection = 0;
		windTurbulence = 0;
		windPulse = 0;
		UpdateWind ();
	}
	public void OnDisable(){
		ResetToZero();
	}
	public void OnDestroy(){
		ResetToZero ();
	}
	public void OnEnable(){
		if (windZone != null)
			UpdateWindZone ();
	}
        
}
