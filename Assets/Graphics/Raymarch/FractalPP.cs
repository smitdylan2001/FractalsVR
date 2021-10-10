using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class FractalPP : SceneViewFilter
{
    public Vector4 Sphere, Box;
    public Vector3 ModInterval;

    [SerializeField] private Shader _shader;
    [SerializeField] private Shader _secondShader;
    [SerializeField] private Shader _thirdShader;
    [SerializeField] private Shader _EmptyShader;

    [SerializeField] private BunBun _bunner;
    
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _maxSteps = 64;
    [SerializeField] public Color _mainColor;

    [HideInInspector] public Vector4 SphereBase, BoxBase, LittleSphere;
    [HideInInspector] public Vector3 ModIntervalBase;
    private Vector3 _BoxRotation;

    //Light variables
    [SerializeField] private Transform _lightSource;
    [SerializeField] private Color _lightColor;
    [SerializeField] private float _lightIntensity;
    [SerializeField] private float _shadowIntensity, _ShadowPenumbra;
    [SerializeField] private Vector2 _shadowDistance;

    //AO
    [SerializeField] private float _AOStepSize, _AOIntensity;
    [SerializeField] private int _AOIterations;

    private int _LastSwitch;
    private float _Pulsation;
    [Range(0, 6)][SerializeField] public int _CurrentSwitch;

    private Material _raymarchMaterial;
    private Camera _cam;

    private float _displacement = 90;
    private float _CS = 0;
    private float _CS2 = 0;
    private float _CS3 = 0;

    public bool _bunnyTime = false;


    private void Start()
	{
        ModIntervalBase = ModInterval;
        BoxBase = Box;
        SphereBase = Sphere;
        _CurrentSwitch = 0;
	}

    private void Awake()
    {
        
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

        //Set common variables
        _raymarchMaterial.SetMatrix("_CamToWorld", p);
        _raymarchMaterial.SetVector("_Sphere", Sphere);
        _raymarchMaterial.SetVector("_LittleSphere", LittleSphere);
        _raymarchMaterial.SetVector("_BoxRotation", _BoxRotation);
        _raymarchMaterial.SetVector("_Box", Box);        
       
        _raymarchMaterial.SetVector("_modInterval", ModInterval);
        _raymarchMaterial.SetColor("_mainColor", _mainColor);

        // Light variables
        _raymarchMaterial.SetVector("_LightDir", _lightSource ? _lightSource.forward : Vector3.down);
        _raymarchMaterial.SetColor("_LightCol", _lightColor);
        _raymarchMaterial.SetFloat("_AOStepSize", _AOStepSize);
        _raymarchMaterial.SetFloat("_AOIntensity", _AOIntensity);
        _raymarchMaterial.SetInt("_AOIterations", _AOIterations);
        _raymarchMaterial.SetVector("_ShadowDist", _shadowDistance);

        //AO
        _raymarchMaterial.SetFloat("_LightIntensity", _lightIntensity);
        _raymarchMaterial.SetFloat("_ShadowIntensity", _shadowIntensity);
        _raymarchMaterial.SetFloat("_ShadowIntensity", _shadowIntensity);

        // optimization variables
        _raymarchMaterial.SetFloat("_maxDistance", _maxDistance);
        _raymarchMaterial.SetFloat("_maxSteps", _maxSteps);
        _raymarchMaterial.SetFloat("_Displacement", _displacement);
        _raymarchMaterial.SetFloat("pulsation", _Pulsation);

        if (_CurrentSwitch < 2) if (_raymarchMaterial.shader != _shader) _raymarchMaterial.shader = _shader;

        switch (_CurrentSwitch)
        {

            case 0:
                InitFirstSwitch();
                break;

            case 1:
                Box = Vector4.Lerp(Box, new Vector4(0, 0, 0, 90), Time.deltaTime);
                Sphere = Vector4.Lerp(Sphere, new Vector4(0, 0, 0, 70), Time.deltaTime);

                // Shift hue
                Color.RGBToHSV(_mainColor, out float hue, out float sat, out float val);

                hue += Time.deltaTime * 0.1f;
                sat = 0.5f;
                val = 1.0f;

                _mainColor = Color.HSVToRGB(hue, sat, val);
                ModInterval = new Vector3(100, 100, 100);
                // StartCoroutine(SwitchOneToTwo());

                _displacement = 110;
                break;

            case 2:
                _CS = 0;

                if (Box.w < 0.1f)
                {
                    if (_raymarchMaterial.shader != _secondShader) _raymarchMaterial.shader = _secondShader;


                   // Sphere = Vector4.Lerp(Sphere, new Vector4(0, 0, 0, 30), Mathf.PingPong(.05f * Time.time, 1));
                    Sphere = Vector4.Lerp(Sphere, new Vector4(0, 0, 0, 10 * (1 + Mathf.Sin(Time.time) / 20)), Time.deltaTime);
                    if (Sphere.w >= 1.9) ModInterval = Vector3.Lerp(ModInterval, new Vector3(100, 100, 100), Time.deltaTime);

                    _displacement = Mathf.Lerp(0f, 3.14f, Mathf.PingPong(.05f * Time.time, 1));


                    _raymarchMaterial.SetFloat("changeShape", _CS);
                }
                else
                {
                    if (_raymarchMaterial.shader != _shader) _raymarchMaterial.shader = _shader;

                    Box = Vector4.Lerp(Box, new Vector4(0, 0, 0, 0), 2 * Time.deltaTime);
                    Sphere = Vector4.Lerp(Sphere, new Vector4(0, 0, 0, 0), Time.deltaTime);

                    ModInterval = new Vector3(100, 100, 100);

                    _displacement = .1f;
                }


                _raymarchMaterial.SetFloat("changeShape", _CS);
                break;

            case 3:
                _displacement = Mathf.Lerp(_displacement, 0, Time.deltaTime);

                Sphere = Vector4.Lerp(Sphere, new Vector4(0, 0, 0, 5), Time.deltaTime);

                _Pulsation = Mathf.Lerp(1.3f, 1f, Mathf.PingPong(Time.time * 0.2f, 1));


                _CS = Mathf.Lerp(_CS, 1, Time.deltaTime * 0.5f);

                // Set variables shader
                _raymarchMaterial.SetFloat("changeShape", _CS);

                _CS2 = 0;
                break;

            case 4:
                _CS = 0;

                if (_raymarchMaterial.shader != _thirdShader) _raymarchMaterial.shader = _thirdShader;

                _BoxRotation = new Vector3(Mathf.Sin(Time.time * 5), 0, 0);

                _CS2 = Mathf.Lerp(_CS2, 1, Time.deltaTime * 0.02f);
                _raymarchMaterial.SetFloat("changeShape", _CS2);
                break;

            case 5:
                _bunnyTime = false;
                if (_raymarchMaterial.shader != _thirdShader) _raymarchMaterial.shader = _thirdShader;

                _CS = Mathf.Lerp(_CS, 1, Time.deltaTime * 0.5f);
                _CS3 = 0;
                _raymarchMaterial.SetFloat("changeShapeAgain", _CS);

                break;

            case 6:               

                _CS = 0;
                _raymarchMaterial.SetFloat("changeShapeAgainAgain", _CS3);


                if (_CS3 >= 0.9f && _CS3 < 1)
                {
                    _bunnyTime = true;
                    break;
                }
                else if (_CS3 >= 1) _bunnyTime = false;
                else _CS3 += Mathf.Lerp(_CS, 1, Time.deltaTime * 0.5f); 

                break;

        };

        if (_bunnyTime)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, destination, _raymarchMaterial, 0);

        _LastSwitch = _CurrentSwitch;

          

        }

    private void InitFirstSwitch()
    {
        _displacement = 5;

        // Set Color
        _mainColor = new Color(.6f, .1f, .3f);

        // Set Mod interval
        ModInterval = new Vector3(40, 40, 10);

        // Sphere pos/size
        Sphere = new Vector4(0, 0, 6, Mathf.Lerp(11, 12, Mathf.PingPong(Time.time * .5f, 1)));

        // Box pos/size
        Box = new Vector4(0, 0, 0, 13.36f);

      
    }

    private void PlayEnding()
    {
        _CS3 = Mathf.Lerp(_CS, 1, Time.deltaTime * 0.5f);
    }

}