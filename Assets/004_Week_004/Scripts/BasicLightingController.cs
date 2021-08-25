using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicLightingController : MonoBehaviour
{
    private const string _SHADER_VARIABLE_LIGHT_COUNT = "g_LightCount";
    private const string _SHADER_VARIABLE_LIGHT_COLOR = "g_LightColor";
    private const string _SHADER_VARIABLE_LIGHT_POSITION = "g_LightPosition";
    private const string _SHADER_VARIABLE_LIGHT_DIRECTION = "g_LightDirection";
    private const string _SHADER_VARIABLE_LIGHT_PARAMETER = "g_LightParameter";

    [SerializeField] private Light[] _lights = default;

    private List<Vector4> _lightColor = new List<Vector4>();
    private List<Vector4> _lightPosition = new List<Vector4>();
    private List<Vector4> _lightDirection = new List<Vector4>();
    private List<Vector4> _lightParameter = new List<Vector4>();

    #region Unity_Lifecycle
    //private void Awake() { }
    //private void OnEnable() { }
    //private void Start() { }
    //private void FixedUpdate() { }
    //private void Update() { }
    //private void LateUpdate() { UpdateLightInfo(); }
    private void OnDrawGizmos() { UpdateLightInfo(); }
    //private void OnDisable() { }
    //private void OnDestroy() { }
    #endregion

    private void UpdateLightInfo()
    {
        ResetLightInfoLists();
        LoadLightInfo();
        PassLightInfoToShader();
    }

    private void ResetLightInfoLists()
    {
        _lightColor.Clear();
        _lightPosition.Clear();
        _lightDirection.Clear();
        _lightParameter.Clear();
    }

    private void LoadLightInfo()
    {
        for (int i = 0; i < _lights.Length; i++)
        {
            var light = _lights[i];

            if (!light.gameObject.activeSelf || !light.enabled) { continue; }

            Vector4 lightColor = light.color;
            lightColor.w = light.intensity;

            Vector4 lightPosition = light.transform.position;

            lightPosition.w = 1;

            Vector4 lightDirection = light.transform.forward;
            lightDirection.w = light.type == LightType.Directional ? 1 : 0;

            Vector4 lightParameter = Vector4.zero;
            if (light.type == LightType.Spot)
            {
                lightParameter.x = 1;
                lightParameter.y = Mathf.Cos(light.spotAngle * Mathf.Deg2Rad * 0.5f);
                lightParameter.z = Mathf.Cos(light.innerSpotAngle * Mathf.Deg2Rad * 0.5f);
            }

            lightParameter.w = light.range;

            _lightColor.Add(lightColor);
            _lightPosition.Add(lightPosition);
            _lightDirection.Add(lightDirection);
            _lightParameter.Add(lightParameter);
        }
    }

    private void PassLightInfoToShader()
    {
        Shader.SetGlobalInt(_SHADER_VARIABLE_LIGHT_COUNT, _lightColor.Count);
        Shader.SetGlobalVectorArray(_SHADER_VARIABLE_LIGHT_COLOR, _lightColor);
        Shader.SetGlobalVectorArray(_SHADER_VARIABLE_LIGHT_POSITION, _lightPosition);
        Shader.SetGlobalVectorArray(_SHADER_VARIABLE_LIGHT_DIRECTION, _lightDirection);
        Shader.SetGlobalVectorArray(_SHADER_VARIABLE_LIGHT_PARAMETER, _lightParameter);
    }
}