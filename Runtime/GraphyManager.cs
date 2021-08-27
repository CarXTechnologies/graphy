/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@tayx94)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            15-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using System;
using UnityEngine;
using Tayx.Graphy.Audio;
using Tayx.Graphy.Fps;
using Tayx.Graphy.Ram;
using Tayx.Graphy.Dev;
using Tayx.Graphy.Utils;
using Tayx.Graphy.Advanced;
using Tayx.Graphy.Utils.NumString;

#if GRAPHY_NEW_INPUT
using UnityEngine.InputSystem;
#endif

namespace Tayx.Graphy
{
    /// <summary>
    /// Main class to access the Graphy API.
    /// </summary>
    public class GraphyManager : G_Singleton<GraphyManager>
    {
        protected GraphyManager () { }

        //Enums
        #region Enums -> Public

        public enum Mode
        {
            FULL            = 0,
            LIGHT           = 1
        }

        public enum ModuleType
        {
            FPS             = 0,
            RAM             = 1,
            AUDIO           = 2,
			ADVANCED = 3,
			DEV = 4
        }

        public enum ModuleState
        {
            OFF             = 0,
            FULL            = 1,
            TEXT            = 2,
            BASIC           = 3,
            BACKGROUND      = 4
        }

        public enum ModulePosition
        {
            TOP_RIGHT       = 0,
            TOP_LEFT        = 1,
            BOTTOM_RIGHT    = 2,
            BOTTOM_LEFT     = 3,
            FREE            = 4
        }

        public enum LookForAudioListener
        {
            ALWAYS,
            ON_SCENE_LOAD,
            NEVER
        }

		internal enum Shift : int
		{
			FPS             = 0,
            RAM             = 3,
			DEV             = 6,
            AUDIO           = 9,
			ADVANCED = 12,
		}

		[Flags]
		public enum ModuleStateFlags : int
		{
			FPS_BASIC = ModuleState.BASIC << Shift.FPS,
			FPS_TEXT = ModuleState.TEXT << Shift.FPS,
			FPS_FULL = ModuleState.FULL << Shift.FPS,
			FPS_BACKGROUND = ModuleState.BACKGROUND << Shift.FPS,

			RAM_TEXT = ModuleState.TEXT << Shift.RAM,
			RAM_FULL = ModuleState.FULL << Shift.RAM,
			RAM_BACKGROUND = ModuleState.BACKGROUND << Shift.RAM,

			// 2 bits
			DEV_TEXT = ModuleState.TEXT << Shift.DEV,
			DEV_FULL = ModuleState.FULL << Shift.DEV,
			DEC_BACKGROUND = ModuleState.BACKGROUND << Shift.DEV,

			// 2 bits
			AUDIO_TEXT = ModuleState.TEXT << Shift.AUDIO,
			AUDIO_FULL = ModuleState.FULL << Shift.AUDIO,
			AUDIO_BACKGROUND = ModuleState.BACKGROUND << Shift.AUDIO,

			// 2 bits
			ADVANCED_FULL = ModuleState.FULL << Shift.ADVANCED,
		}

		public ModuleStateFlags[] m_modulePresets;

		public void ResetModulePresetsToDefaults()
		{
			m_modulePresets = new ModuleStateFlags[]
			{
				ModuleStateFlags.FPS_BASIC,
				ModuleStateFlags.FPS_TEXT,
				ModuleStateFlags.FPS_FULL,

				ModuleStateFlags.FPS_TEXT | ModuleStateFlags.RAM_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_FULL,

				ModuleStateFlags.FPS_TEXT | ModuleStateFlags.RAM_TEXT | ModuleStateFlags.DEV_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_TEXT | ModuleStateFlags.DEV_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_FULL | ModuleStateFlags.DEV_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_FULL | ModuleStateFlags.DEV_FULL,

				ModuleStateFlags.FPS_TEXT | ModuleStateFlags.RAM_TEXT | ModuleStateFlags.AUDIO_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_TEXT | ModuleStateFlags.AUDIO_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_FULL | ModuleStateFlags.AUDIO_TEXT,
				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_FULL | ModuleStateFlags.AUDIO_FULL,

				ModuleStateFlags.FPS_FULL | ModuleStateFlags.RAM_FULL | ModuleStateFlags.AUDIO_FULL | ModuleStateFlags.ADVANCED_FULL,
				ModuleStateFlags.FPS_BASIC | ModuleStateFlags.ADVANCED_FULL
			};
		}

		#endregion

		#region Variables -> Serialized Private

		[SerializeField] private    Mode                    m_graphyMode                        = Mode.FULL;

        [SerializeField] private    bool                    m_enableOnStartup                   = true;

        [SerializeField] private    bool                    m_keepAlive                         = true;
        
        [SerializeField] private    bool                    m_background                        = true;
        [SerializeField] private    Color                   m_backgroundColor                   = new Color(0, 0, 0, 0.3f);

        [SerializeField] private    bool                    m_enableHotkeys                     = true;

#if GRAPHY_NEW_INPUT
        [SerializeField] private    Key                     m_toggleModeKeyCode                 = Key.G;
#else
        [SerializeField] private    KeyCode                 m_toggleModeKeyCode                 = KeyCode.G;
#endif
        [SerializeField] private    bool                    m_toggleModeCtrl                    = true;
        [SerializeField] private    bool                    m_toggleModeAlt                     = false;

#if GRAPHY_NEW_INPUT
        [SerializeField] private    Key                     m_toggleActiveKeyCode               = Key.H;
#else
        [SerializeField] private    KeyCode                 m_toggleActiveKeyCode               = KeyCode.H;
#endif
        [SerializeField] private    bool                    m_toggleActiveCtrl                  = true;
        [SerializeField] private    bool                    m_toggleActiveAlt                   = false;
        
        [SerializeField] private    ModulePosition          m_graphModulePosition               = ModulePosition.TOP_RIGHT;
        
        // Fps ---------------------------------------------------------------------------

        [SerializeField] private    ModuleState             m_fpsModuleState                    = ModuleState.FULL;

        [SerializeField] private    Color                   m_goodFpsColor                      = new Color32(118, 212, 58, 255);
        [SerializeField] private    int                     m_goodFpsThreshold                  = 60;

        [SerializeField] private    Color                   m_cautionFpsColor                   = new Color32(243, 232, 0, 255);
        [SerializeField] private    int                     m_cautionFpsThreshold               = 30;

        [SerializeField] private    Color                   m_criticalFpsColor                  = new Color32(220, 41, 30, 255);

        [Range(10, 300)]
        [SerializeField] private    int                     m_fpsGraphResolution                = 150;

        [Range(1, 200)]
        [SerializeField] private    int                     m_fpsTextUpdateRate                 = 3;  // 3 updates per sec.

        // Ram ---------------------------------------------------------------------------

        [SerializeField] private    ModuleState             m_ramModuleState                    = ModuleState.FULL;

        [SerializeField] private    Color                   m_allocatedRamColor                 = new Color32(255, 190, 60, 255);
        [SerializeField] private    Color                   m_reservedRamColor                  = new Color32(205, 84, 229, 255);
        [SerializeField] private    Color                   m_monoRamColor                      = new Color(0.3f, 0.65f, 1f, 1);

        [Range(10, 300)]
        [SerializeField] private    int                     m_ramGraphResolution                = 150;


        [Range(1, 200)]
        [SerializeField] private    int                     m_ramTextUpdateRate                 = 3;  // 3 updates per sec.

		// Dev ---------------------------------------------------------------------------

		[SerializeField] private ModuleState m_devModuleState = ModuleState.FULL;

		[SerializeField] private Color m_videoDevColor = new Color32(205, 84, 229, 255);
		[SerializeField] private Color m_texturesDevColor = new Color(1f, 0.65f, 1f, 1);
		[SerializeField] private Color m_meshesDevColor = new Color(0.35f, 0.6f, 1f, 1);
		[SerializeField] private Color m_materialsDevColor = new Color(0.63f, 0.25f, 0.1f, 1);
		[SerializeField] private Color m_assetsDevColor = new Color(0.23f, 0.5f, 1f, 1);
		[SerializeField] private Color m_objectsDevColor = new Color(0.63f, 0.3f, 0.1f, 1);

        [SerializeField] private    Color                   m_criticalAllocsColor                      = new Color32(220, 41, 30, 255);
        [SerializeField] private    int                     m_criticalAllocsThresholdKB                  = 50;

        [SerializeField] private    Color                   m_cautionAllocsColor                   = new Color32(243, 232, 0, 255);
        [SerializeField] private    int                     m_cautionAllocsThresholdKB               = 30;

        [SerializeField] private    Color                   m_goodAllocsColor                  = new Color32(118, 212, 58, 255);


		[Range(10, 300)]
		[SerializeField] private int m_devGraphResolution = 150;


		[Range(1, 200)]
		[SerializeField] private int m_devTextUpdateRate = 3;  // 3 updates per sec.

		[Range(10, 300)]
		[SerializeField] private int m_devAllocsResolution = 150;

		// Audio -------------------------------------------------------------------------

		[SerializeField] private ModuleState m_audioModuleState = ModuleState.FULL;

        [SerializeField] private    LookForAudioListener    m_findAudioListenerInCameraIfNull   = LookForAudioListener.ON_SCENE_LOAD;

        [SerializeField] private    AudioListener           m_audioListener                     = null;
        
        [SerializeField] private    Color                   m_audioGraphColor                   = Color.white;

        [Range(10, 300)]
        [SerializeField] private    int                     m_audioGraphResolution              = 81;
        
        [Range(1, 200)]
        [SerializeField] private    int                     m_audioTextUpdateRate               = 3;  // 3 updates per sec.
        
        [SerializeField] private    FFTWindow               m_FFTWindow                         = FFTWindow.Blackman;

        [Tooltip("Must be a power of 2 and between 64-8192")]
        [SerializeField] private    int                     m_spectrumSize                      = 512;

        // Advanced ----------------------------------------------------------------------

        [SerializeField] private    ModulePosition          m_advancedModulePosition            = ModulePosition.BOTTOM_LEFT;

        [SerializeField] private    ModuleState             m_advancedModuleState               = ModuleState.FULL;

        #endregion

        #region Variables -> Private

        private                     bool                    m_initialized                       = false;
        private                     bool                    m_active                            = true;
        private                     bool                    m_focused                           = true;

        private                     G_FpsManager            m_fpsManager                        = null;
        private                     G_RamManager            m_ramManager                        = null;
		private                     G_DevManager            m_devManager                        = null;
        private                     G_AudioManager          m_audioManager                      = null;
        private                     G_AdvancedData          m_advancedData                      = null;

        private                     G_FpsMonitor            m_fpsMonitor                        = null;
        private                     G_RamMonitor            m_ramMonitor                        = null;
		private                     G_DevMonitor            m_devMonitor                        = null;
        private                     G_AudioMonitor          m_audioMonitor                      = null;

        private                     int            m_modulePresetIndex                 = 0;

        #endregion

        //TODO: Maybe sort these into Get and GetSet sections.
        #region Properties -> Public

        public Mode GraphyMode                          { get { return m_graphyMode; }
                                                          set { m_graphyMode = value; UpdateAllParameters(); } }

        public bool EnableOnStartup                     { get { return m_enableOnStartup; } }

        public bool KeepAlive                           { get { return m_keepAlive; } }

        public bool Background                          { get { return m_background; } 
                                                          set { m_background = value; UpdateAllParameters(); } }

        public Color BackgroundColor                    { get { return m_backgroundColor; } 
                                                          set { m_backgroundColor = value; UpdateAllParameters(); } }

        public ModulePosition GraphModulePosition
        {
            get { return m_graphModulePosition; }
            set
            {
                m_graphModulePosition = value;
                m_fpsManager    .SetPosition(m_graphModulePosition);
                m_ramManager    .SetPosition(m_graphModulePosition);
                m_audioManager  .SetPosition(m_graphModulePosition);
            }
        }

        // Fps ---------------------------------------------------------------------------

        // Setters & Getters

        public ModuleState FpsModuleState               { get { return m_fpsModuleState; }             
                                                          set { m_fpsModuleState = value; m_fpsManager.SetState(m_fpsModuleState); } }

        public Color GoodFPSColor                       { get { return m_goodFpsColor; } 
                                                          set { m_goodFpsColor = value; m_fpsManager.UpdateParameters(); } }
        public Color CautionFPSColor                    { get { return m_cautionFpsColor; } 
                                                          set { m_cautionFpsColor = value; m_fpsManager.UpdateParameters(); } }
        public Color CriticalFPSColor                   { get { return m_criticalFpsColor; } 
                                                          set { m_criticalFpsColor = value; m_fpsManager.UpdateParameters(); } }

        public int GoodFPSThreshold                     { get { return m_goodFpsThreshold; } 
                                                          set { m_goodFpsThreshold = value; m_fpsManager.UpdateParameters(); } }
        public int CautionFPSThreshold                  { get { return m_cautionFpsThreshold; } 
                                                          set { m_cautionFpsThreshold = value; m_fpsManager.UpdateParameters(); } }

        public int FpsGraphResolution                   { get { return m_fpsGraphResolution; } 
                                                          set { m_fpsGraphResolution = value; m_fpsManager.UpdateParameters(); } }

        public int FpsTextUpdateRate                    { get { return m_fpsTextUpdateRate; } 
                                                          set { m_fpsTextUpdateRate = value; m_fpsManager.UpdateParameters(); } }

        // Getters

        public float CurrentFPS                         { get { return m_fpsMonitor.CurrentFPS; } }
        public float AverageFPS                         { get { return m_fpsMonitor.AverageFPS; } }
        public float MinFPS                             { get { return m_fpsMonitor.OnePercentFPS; } }
        public float MaxFPS                             { get { return m_fpsMonitor.Zero1PercentFps; } }

        // Ram ---------------------------------------------------------------------------

        // Setters & Getters

        public ModuleState RamModuleState               { get { return m_ramModuleState; } 
                                                          set { m_ramModuleState = value; m_ramManager.SetState(m_ramModuleState); } }


        public Color AllocatedRamColor                  { get { return m_allocatedRamColor; } 
                                                          set { m_allocatedRamColor = value; m_ramManager.UpdateParameters(); } }
        public Color ReservedRamColor                   { get { return m_reservedRamColor; } 
                                                          set { m_reservedRamColor = value; m_ramManager.UpdateParameters(); } }
        public Color MonoRamColor                       { get { return m_monoRamColor; } 
                                                          set { m_monoRamColor = value; m_ramManager.UpdateParameters(); } }

        public int RamGraphResolution                   { get { return m_ramGraphResolution; } 
                                                          set { m_ramGraphResolution = value; m_ramManager.UpdateParameters(); } }

        public int RamTextUpdateRate                    { get { return m_ramTextUpdateRate; } 
                                                          set { m_ramTextUpdateRate = value; m_ramManager.UpdateParameters(); } }

        // Getters

        public float AllocatedRam                       { get { return m_ramMonitor.AllocatedRam; } }
        public float ReservedRam                        { get { return m_ramMonitor.ReservedRam; } }
        public float MonoRam                            { get { return m_ramMonitor.MonoRam; } }

		// Dev ---------------------------------------------------------------------------

		// Setters & Getters

		public ModuleState DevModuleState
		{
			get { return m_devModuleState; }
			set { m_devModuleState = value; m_devManager.SetState(m_devModuleState); }
		}

		public Color VideoDevColor
		{
			get { return m_videoDevColor; }
			set { m_videoDevColor = value; m_devManager.UpdateParameters(); }
		}
		public Color TexturesDevColor
		{
			get { return m_texturesDevColor; }
			set { m_texturesDevColor = value; m_devManager.UpdateParameters(); }
		}

		public Color MeshesDevColor
		{
			get { return m_meshesDevColor; }
			set { m_meshesDevColor = value; m_devManager.UpdateParameters(); }
		}

		public Color MaterialsDevColor
		{
			get { return m_materialsDevColor; }
			set { m_materialsDevColor = value; m_devManager.UpdateParameters(); }
		}

		public Color AssetsDevColor
		{
			get { return m_assetsDevColor; }
			set { m_assetsDevColor = value; m_devManager.UpdateParameters(); }
		}

		public Color ObjectsDevColor
		{
			get { return m_objectsDevColor; }
			set { m_texturesDevColor = value; m_devManager.UpdateParameters(); }
		}

		public int DevGraphResolution
		{
			get { return m_devGraphResolution; }
			set { m_devGraphResolution = value; m_devManager.UpdateParameters(); }
		}

		public int DevAllocsResolution
		{
			get { return m_devAllocsResolution; }
			set { m_devAllocsResolution = value; m_devManager.UpdateParameters(); }
		}

		public int DevTextUpdateRate
		{
			get { return m_devTextUpdateRate; }
			set { m_devTextUpdateRate = value; m_devManager.UpdateParameters(); }
		}

		public Color CriticalAllocsColor
		{
			get { return m_criticalAllocsColor; }
			set { m_criticalAllocsColor = value; m_fpsManager.UpdateParameters(); }
		}
		public Color CautionAllocsColor
		{
			get { return m_cautionAllocsColor; }
			set { m_cautionAllocsColor = value; m_fpsManager.UpdateParameters(); }
		}
		public Color GoodAllocsColor
		{
			get { return m_goodAllocsColor; }
			set { m_goodAllocsColor = value; m_fpsManager.UpdateParameters(); }
		}

		public int CriticalAllocsThresholdKB
		{
			get { return m_criticalAllocsThresholdKB; }
			set { m_criticalAllocsThresholdKB = value; m_fpsManager.UpdateParameters(); }
		}
		public int CautionAllocsThresholdKB
		{
			get { return m_cautionAllocsThresholdKB; }
			set { m_cautionAllocsThresholdKB = value; m_fpsManager.UpdateParameters(); }
		}


		// Getters
		//TODO: upadte getters for dev monitor

		// public float AllocatedDev { get { return m_devMonitor.AllocatedDev; } }
		// public float ReservedDev { get { return m_devMonitor.ReservedDev; } }
		// public float MonoDev { get { return m_devMonitor.MonoDev; } }

		// Audio -------------------------------------------------------------------------

		// Setters & Getters

		public ModuleState AudioModuleState
		{
			get { return m_audioModuleState; }
			set { m_audioModuleState = value; m_audioManager.SetState(m_audioModuleState); }
		}

        public AudioListener AudioListener              { get { return m_audioListener; } 
                                                          set { m_audioListener = value; m_audioManager.UpdateParameters(); } }
        
        public LookForAudioListener 
                    FindAudioListenerInCameraIfNull     { get { return m_findAudioListenerInCameraIfNull; } 
                                                          set { m_findAudioListenerInCameraIfNull = value; m_audioManager.UpdateParameters(); } }

        public Color AudioGraphColor                    { get { return m_audioGraphColor; } 
                                                          set { m_audioGraphColor = value; m_audioManager.UpdateParameters(); } }

        public int AudioGraphResolution                 { get { return m_audioGraphResolution; } 
                                                          set { m_audioGraphResolution = value; m_audioManager.UpdateParameters(); } }

        public int AudioTextUpdateRate                  { get { return m_audioTextUpdateRate; } 
                                                          set { m_audioTextUpdateRate = value; m_audioManager.UpdateParameters(); } }

        public FFTWindow FftWindow                      { get { return m_FFTWindow; } 
                                                          set { m_FFTWindow = value; m_audioManager.UpdateParameters(); } }

        public int SpectrumSize                         { get { return m_spectrumSize; } 
                                                          set { m_spectrumSize = value; m_audioManager.UpdateParameters(); } }

        // Getters

        /// <summary>
        /// Current audio spectrum from the specified AudioListener.
        /// </summary>
        public float[] Spectrum                         { get { return m_audioMonitor.Spectrum; } }

        /// <summary>
        /// Maximum DB registered in the current spectrum.
        /// </summary>
        public float MaxDB                              { get { return m_audioMonitor.MaxDB; } }


        // Advanced ---------------------------------------------------------------------

        // Setters & Getters

        public ModuleState AdvancedModuleState          { get { return m_advancedModuleState; } 
                                                          set { m_advancedModuleState = value; m_advancedData.SetState(m_advancedModuleState); } }
        
        public ModulePosition AdvancedModulePosition    { get { return m_advancedModulePosition; } 
                                                          set { m_advancedModulePosition = value; m_advancedData.SetPosition(m_advancedModulePosition); } }

        #endregion

        #region Methods -> Unity Callbacks

        private void Start()
        {
            Init();
        }

        private void OnDestroy()
        {
            G_IntString.Dispose();
            G_FloatString.Dispose();
        }

        private void Update()
        {
            if (m_focused && m_enableHotkeys)
            {
                CheckForHotkeyPresses();
            }
        }

        private void OnApplicationFocus(bool isFocused)
        {
            m_focused = isFocused;

            if (m_initialized && isFocused)
            {
                RefreshAllParameters();
            }
        }

        #endregion

        #region Methods -> Public

		public Color GetAllocaRelatedColor(int allocsKB)
		{
            if (allocsKB >= m_criticalAllocsThresholdKB)
            {
                return m_criticalAllocsColor;
            }
            else if (allocsKB >= m_cautionAllocsThresholdKB)
            {
                return m_cautionAllocsColor;
            }
            else
            {
                return m_goodAllocsColor;
            }
		}

        public void SetModulePosition(ModuleType moduleType, ModulePosition modulePosition)
        {
            switch (moduleType)
            {
                case ModuleType.FPS:
                case ModuleType.RAM:
                case ModuleType.AUDIO:
				case ModuleType.DEV:
                    m_graphModulePosition = modulePosition;

                    m_ramManager.SetPosition(modulePosition);
					m_devManager.SetPosition(modulePosition);
                    m_fpsManager.SetPosition(modulePosition);
                    m_audioManager.SetPosition(modulePosition);
					m_devManager.SetPosition(modulePosition);
					break;

                case ModuleType.ADVANCED:
                    m_advancedData.SetPosition(modulePosition);
                    break;
            }
        }

        public void SetModuleMode(ModuleType moduleType, ModuleState moduleState)
        {
            switch (moduleType)
            {
                case ModuleType.FPS:
                    m_fpsManager.SetState(moduleState);
                    break;

                case ModuleType.RAM:
                    m_ramManager.SetState(moduleState);
                    break;

				case ModuleType.DEV:
					m_devManager.SetState(moduleState);
					break;

				case ModuleType.AUDIO:
                    m_audioManager.SetState(moduleState);
                    break;

                case ModuleType.ADVANCED:
                    m_advancedData.SetState(moduleState);
                    break;
            }
        }

        public void ToggleModes()
        {
			m_modulePresetIndex++;
			if (m_modulePresetIndex >= m_modulePresets.Length)
            {
                m_modulePresetIndex = 0;
            }

            SetPreset(m_modulePresets[m_modulePresetIndex]);
        }

		private ModuleState GetStateFromPreset(ModuleStateFlags preset, Shift shift)
		{
			return (ModuleState)(((int)preset >> (int)shift) & 3);
		}

		public void SetPreset(ModuleStateFlags modulePreset)
		{
			m_fpsManager.SetState(GetStateFromPreset(modulePreset, Shift.FPS));
			m_ramManager.SetState(GetStateFromPreset(modulePreset, Shift.RAM));
			m_devManager.SetState(GetStateFromPreset(modulePreset, Shift.DEV));
			m_audioManager.SetState(GetStateFromPreset(modulePreset, Shift.AUDIO));
			m_advancedData.SetState(GetStateFromPreset(modulePreset, Shift.ADVANCED));
        }

		public void ToggleActive()
        {
            if (!m_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }

        public void Enable()
        {
            if (!m_active)
            {
                if (m_initialized)
                {
                    m_fpsManager.RestorePreviousState();
                    m_ramManager.RestorePreviousState();
					m_devManager.RestorePreviousState();
                    m_audioManager.RestorePreviousState();
                    m_advancedData.RestorePreviousState();

                    m_active = true;
                }
                else
                {
                    Init();
                }
            }
        }

        public void Disable()
        {
            if (m_active)
            {
                m_fpsManager.SetState(ModuleState.OFF);
                m_ramManager.SetState(ModuleState.OFF);
				m_devManager.SetState(ModuleState.OFF);
				m_audioManager.SetState(ModuleState.OFF);
                m_advancedData.SetState(ModuleState.OFF);

                m_active = false;
            }
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            if (m_keepAlive)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }
            
            m_fpsMonitor    = GetComponentInChildren(typeof(G_FpsMonitor),    true) as G_FpsMonitor;
            m_ramMonitor    = GetComponentInChildren(typeof(G_RamMonitor),    true) as G_RamMonitor;
			m_devMonitor    = GetComponentInChildren(typeof(G_DevMonitor),    true) as G_DevMonitor;
            m_audioMonitor  = GetComponentInChildren(typeof(G_AudioMonitor),  true) as G_AudioMonitor;
            
            m_fpsManager    = GetComponentInChildren(typeof(G_FpsManager),    true) as G_FpsManager;
            m_ramManager    = GetComponentInChildren(typeof(G_RamManager),    true) as G_RamManager;
			m_devManager    = GetComponentInChildren(typeof(G_DevManager),    true) as G_DevManager;
			m_audioManager  = GetComponentInChildren(typeof(G_AudioManager),  true) as G_AudioManager;
            m_advancedData  = GetComponentInChildren(typeof(G_AdvancedData),  true) as G_AdvancedData;

            m_fpsManager    .SetPosition(m_graphModulePosition);
            m_ramManager    .SetPosition(m_graphModulePosition);
			m_devManager    .SetPosition(m_graphModulePosition);
			m_audioManager  .SetPosition(m_graphModulePosition);
            m_advancedData  .SetPosition(m_advancedModulePosition);

            // m_fpsManager    .SetState   (m_fpsModuleState);
            // m_ramManager    .SetState   (m_ramModuleState);
			// m_devManager    .SetState   (m_devModuleState);
            // m_audioManager  .SetState   (m_audioModuleState);
            // m_advancedData  .SetState   (m_advancedModuleState);

            if (!m_enableOnStartup)
            {
                ToggleActive();

                // We need to enable this on startup because we disable it in GraphyManagerEditor
                GetComponent<Canvas>().enabled = true;
            }

			if (m_modulePresets.Length == 0)
			{
				ResetModulePresetsToDefaults();
			}
			SetPreset(m_modulePresets[0]); // set first state

            m_initialized = true;
        }

        private void CheckForHotkeyPresses()
        {
#if GRAPHY_NEW_INPUT
            // Toggle Mode ---------------------------------------
            if (m_toggleModeCtrl && m_toggleModeAlt)
            {
                if (CheckFor3KeyPress(m_toggleModeKeyCode, Key.LeftCtrl, Key.LeftAlt)
                    || CheckFor3KeyPress(m_toggleModeKeyCode, Key.RightCtrl, Key.LeftAlt)
                    || CheckFor3KeyPress(m_toggleModeKeyCode, Key.RightCtrl, Key.RightAlt)
                    || CheckFor3KeyPress(m_toggleModeKeyCode, Key.LeftCtrl, Key.RightAlt))
                {
                    ToggleModes();
                }
            }
            else if (m_toggleModeCtrl)
            {
                if (CheckFor2KeyPress(m_toggleModeKeyCode, Key.LeftCtrl)
                    || CheckFor2KeyPress(m_toggleModeKeyCode, Key.RightCtrl))
                {
                    ToggleModes();
                }
            }
            else if (m_toggleModeAlt)
            {
                if (CheckFor2KeyPress(m_toggleModeKeyCode, Key.LeftAlt)
                    || CheckFor2KeyPress(m_toggleModeKeyCode, Key.RightAlt))
                {
                    ToggleModes();
                }
            }
            else
            {
                if (CheckFor1KeyPress(m_toggleModeKeyCode))
                {
                    ToggleModes();
                }
            }

            // Toggle Active -------------------------------------
            if (m_toggleActiveCtrl && m_toggleActiveAlt)
            {
                if (CheckFor3KeyPress(m_toggleActiveKeyCode, Key.LeftCtrl, Key.LeftAlt)
                    || CheckFor3KeyPress(m_toggleActiveKeyCode, Key.RightCtrl, Key.LeftAlt)
                    || CheckFor3KeyPress(m_toggleActiveKeyCode, Key.RightCtrl, Key.RightAlt)
                    || CheckFor3KeyPress(m_toggleActiveKeyCode, Key.LeftCtrl, Key.RightAlt))
                {
                    ToggleActive();
                }
            }

            else if (m_toggleActiveCtrl)
            {
                if (CheckFor2KeyPress(m_toggleActiveKeyCode, Key.LeftCtrl)
                    || CheckFor2KeyPress(m_toggleActiveKeyCode, Key.RightCtrl))
                {
                    ToggleActive();
                }
            }
            else if (m_toggleActiveAlt)
            {
                if (CheckFor2KeyPress(m_toggleActiveKeyCode, Key.LeftAlt)
                    || CheckFor2KeyPress(m_toggleActiveKeyCode, Key.RightAlt))
                {
                    ToggleActive();
                }
            }
            else
            {
                if (CheckFor1KeyPress(m_toggleActiveKeyCode))
                {
                    ToggleActive();
                }
            }
#else
            // Toggle Mode ---------------------------------------
            if (m_toggleModeCtrl && m_toggleModeAlt)
            {
                if (CheckFor3KeyPress(m_toggleModeKeyCode, KeyCode.LeftControl, KeyCode.LeftAlt)
                    || CheckFor3KeyPress(m_toggleModeKeyCode, KeyCode.RightControl, KeyCode.LeftAlt)
                    || CheckFor3KeyPress(m_toggleModeKeyCode, KeyCode.RightControl, KeyCode.RightAlt)
                    || CheckFor3KeyPress(m_toggleModeKeyCode, KeyCode.LeftControl, KeyCode.RightAlt))
                {
                    ToggleModes();
                }
            }
            else if (m_toggleModeCtrl)
            {
                if (    CheckFor2KeyPress(m_toggleModeKeyCode, KeyCode.LeftControl)
                    ||  CheckFor2KeyPress(m_toggleModeKeyCode, KeyCode.RightControl))
                {
                    ToggleModes();
                }
            }
            else if (m_toggleModeAlt)
            {
                if (    CheckFor2KeyPress(m_toggleModeKeyCode, KeyCode.LeftAlt)
                    ||  CheckFor2KeyPress(m_toggleModeKeyCode, KeyCode.RightAlt))
                {
                    ToggleModes();
                }
            }
            else
            {
                if (CheckFor1KeyPress(m_toggleModeKeyCode))
                {
                    ToggleModes();
                }
            }

            // Toggle Active -------------------------------------
            if (m_toggleActiveCtrl && m_toggleActiveAlt)
            {
                if (    CheckFor3KeyPress(m_toggleActiveKeyCode, KeyCode.LeftControl, KeyCode.LeftAlt)
                    ||  CheckFor3KeyPress(m_toggleActiveKeyCode, KeyCode.RightControl, KeyCode.LeftAlt)
                    ||  CheckFor3KeyPress(m_toggleActiveKeyCode, KeyCode.RightControl, KeyCode.RightAlt)
                    ||  CheckFor3KeyPress(m_toggleActiveKeyCode, KeyCode.LeftControl, KeyCode.RightAlt))
                {
                    ToggleActive();
                }
            }
            
            else if (m_toggleActiveCtrl)
            {
                if (    CheckFor2KeyPress(m_toggleActiveKeyCode, KeyCode.LeftControl)
                    ||  CheckFor2KeyPress(m_toggleActiveKeyCode, KeyCode.RightControl))
                {
                    ToggleActive();
                }
            }
            else if (m_toggleActiveAlt)
            {
                if (    CheckFor2KeyPress(m_toggleActiveKeyCode, KeyCode.LeftAlt)
                    ||  CheckFor2KeyPress(m_toggleActiveKeyCode, KeyCode.RightAlt))
                {
                    ToggleActive();
                }
            }
            else
            {
                if (CheckFor1KeyPress(m_toggleActiveKeyCode))
                {
                    ToggleActive();
                }
            }
#endif
        }

#if GRAPHY_NEW_INPUT
        private bool CheckFor1KeyPress(Key key)
        {
            Keyboard currentKeyboard = Keyboard.current;

            if (currentKeyboard != null)
            {
                return Keyboard.current[key].wasPressedThisFrame;
            }

            return false;
        }

        private bool CheckFor2KeyPress(Key key1, Key key2)
        {
            Keyboard currentKeyboard = Keyboard.current;

            if (currentKeyboard != null)
            {
                return Keyboard.current[key1].wasPressedThisFrame && Keyboard.current[key2].isPressed
                    || Keyboard.current[key2].wasPressedThisFrame && Keyboard.current[key1].isPressed;
            }

            return false;
        }

        private bool CheckFor3KeyPress(Key key1, Key key2, Key key3)
        {
            Keyboard currentKeyboard = Keyboard.current;

            if (currentKeyboard != null)
            {
                return Keyboard.current[key1].wasPressedThisFrame && Keyboard.current[key2].isPressed && Keyboard.current[key3].isPressed
                       || Keyboard.current[key2].wasPressedThisFrame && Keyboard.current[key1].isPressed && Keyboard.current[key3].isPressed
                       || Keyboard.current[key3].wasPressedThisFrame && Keyboard.current[key1].isPressed && Keyboard.current[key2].isPressed;
            }

            return false;
        }
#else
        private bool CheckFor1KeyPress(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        private bool CheckFor2KeyPress(KeyCode key1, KeyCode key2)
        {
            return Input.GetKeyDown(key1) && Input.GetKey(key2)
                || Input.GetKeyDown(key2) && Input.GetKey(key1);
        }

        private bool CheckFor3KeyPress(KeyCode key1, KeyCode key2, KeyCode key3)
        {
            return Input.GetKeyDown(key1) && Input.GetKey(key2) && Input.GetKey(key3)
                || Input.GetKeyDown(key2) && Input.GetKey(key1) && Input.GetKey(key3)
                || Input.GetKeyDown(key3) && Input.GetKey(key1) && Input.GetKey(key2);
        }
#endif
        private void UpdateAllParameters()
        {
            m_fpsManager    .UpdateParameters();
            m_ramManager    .UpdateParameters();
			m_devManager    .UpdateParameters();
            m_audioManager  .UpdateParameters();
            m_advancedData  .UpdateParameters();
        }

        private void RefreshAllParameters()
        {
            m_fpsManager    .RefreshParameters();
            m_ramManager    .RefreshParameters();
			m_devManager    .RefreshParameters();
            m_audioManager  .RefreshParameters();
            m_advancedData  .RefreshParameters();
        }
        
#endregion
    }
}
