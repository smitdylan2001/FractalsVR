using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class LVL2_Fractals : SceneViewFilter
{
    public Vector3 CapsuleBegin, CapsuleEnd, Link;
    public int ModInterval;

    // Default RayMarch properties
    [SerializeField] private Shader _shader;
    [SerializeField] private Transform _lightSource;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _maxSteps = 64;
    [SerializeField] private Color _mainColor;
    [SerializeField] private float _sphereAroundPlayerSize;
    [SerializeField] private float _amount, _capsuleThickness, _opSmoothness;

    [HideInInspector] public Vector3 CapsuleBeginBase, CapsuleEndBase, LinkBase;
    [HideInInspector] public float ModIntervalBase;

    private Material _raymarchMaterial;
    private Camera _cam;

    private void Start()
    {
        ModIntervalBase = ModInterval;
        CapsuleBeginBase = CapsuleBegin;
        CapsuleEndBase = CapsuleEnd;
        LinkBase = Link;
    }

    private void Update()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_raymarchMaterial && _shader)
        {
            _raymarchMaterial = new Material(_shader);
            _raymarchMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        if (!_cam)
        {
            _cam = GetComponent<Camera>();
        }

        _cam.depthTextureMode |= DepthTextureMode.Depth;

        if (!_raymarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }

        /////////// THIS WORKS YAY:) ////////////////
        var p = GL.GetGPUProjectionMatrix(_cam.projectionMatrix, true);
        // Undo some of the weird projection-y things so it's more intuitive to work with.
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;

        // I'll confess I don't understand entirely why this is right,
        // I just kept fiddling with numbers until it worked.
        p = Matrix4x4.Inverse(p * _cam.worldToCameraMatrix)
           * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);

        // Debug.Log(_cam.transform.position);
 

        _raymarchMaterial.SetMatrix("_CamToWorld", p);
        _raymarchMaterial.SetVector("_CapsuleBegin", CapsuleBegin);
        _raymarchMaterial.SetVector("_CapsuleEnd", CapsuleEnd);
        _raymarchMaterial.SetVector("_LinkSize", Link);
        _raymarchMaterial.SetFloat("_CapsuleThickness", _capsuleThickness);

        _raymarchMaterial.SetColor("_mainColor", _mainColor);
        _raymarchMaterial.SetVector("_LightDir", _lightSource ? _lightSource.forward : Vector3.down);
        _raymarchMaterial.SetFloat("_ModInterval", ModInterval);
        _raymarchMaterial.SetFloat("_Amount", _amount);
        _raymarchMaterial.SetFloat("_OpSmoothness", _opSmoothness);

        // optimization variables
        _raymarchMaterial.SetFloat("_maxDistance", _maxDistance);
        _raymarchMaterial.SetFloat("_maxSteps", _maxSteps);
        _raymarchMaterial.SetFloat("_SphereAroundPlayerSize", _sphereAroundPlayerSize);

          // Debug.Log(_raymarchMaterial.GetVector("rayDirection"));

        Graphics.Blit(source, destination, _raymarchMaterial, 0);
    }
}