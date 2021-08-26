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

using Tayx.Graphy.Graph;
using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Tayx.Graphy.Dev
{
	public class G_DevGraph : G_Graph
    {
        #region Variables -> Serialized Private

        [SerializeField] private    Image           m_imageVideo = null;
        [SerializeField] private    Image           m_imageTexture = null;
        [SerializeField] private    Image           m_imageMeshes = null;

		[SerializeField] private    Image           m_imageAllocs = null;

        [SerializeField] private    Shader          ShaderFull = null;
        [SerializeField] private    Shader          ShaderLight = null;

        [SerializeField] private    bool            m_isInitialized = false;

        #endregion

        #region Variables -> Private

        private                     GraphyManager   m_graphyManager = null;

		private G_DevMonitor m_devMonitor = null;

        private                     int             m_resolution                = 150;

        private                     G_GraphShader   m_shaderGraphVideo = null;
        private                     G_GraphShader   m_shaderGraphTexture = null;
        private                     G_GraphShader   m_shaderGraphMesh = null;
		private                     G_GraphShader   m_shaderGraphAllocs = null;

        private                     float[]         m_videoArray;
        private                     float[]         m_textureArray;
        private                     float[]         m_meshArray;
        private                     float           m_highestMemory = 0;
        private                     int[]           m_allocsArray;
		private                     int             m_highestAlloc;

        #endregion

        #region Methods -> Unity Callbacks

        private void Update()
        {
            UpdateGraph();
        }

        #endregion
        
        #region Methods -> Public

        public void UpdateParameters()
        { 
            if (    m_shaderGraphVideo  == null
                ||  m_shaderGraphTexture   == null
                ||  m_shaderGraphMesh       == null
				||  m_shaderGraphAllocs     == null)
            {
                /*
                 * Note: this is fine, since we don't much
                 * care what granularity we use if the graph
                 * has not been initialized, i.e. it's disabled.
                 * There is no chance that for some reason 
                 * parameters will not stay up to date if
                 * at some point in the future the graph is enabled:
                 * at the end of Init(), UpdateParameters() is
                 * called again.
                 */
                return;
            }
             
            switch (m_graphyManager.GraphyMode)
            {
                case GraphyManager.Mode.FULL:
                    m_shaderGraphVideo  .ArrayMaxSize = G_GraphShader.ArrayMaxSizeFull;
                    m_shaderGraphTexture   .ArrayMaxSize = G_GraphShader.ArrayMaxSizeFull;
                    m_shaderGraphMesh       .ArrayMaxSize = G_GraphShader.ArrayMaxSizeFull;
					m_shaderGraphAllocs       .ArrayMaxSize = G_GraphShader.ArrayMaxSizeFull;

                    m_shaderGraphVideo  .Image.material = new Material(ShaderFull);
                    m_shaderGraphTexture   .Image.material = new Material(ShaderFull);
                    m_shaderGraphMesh       .Image.material = new Material(ShaderFull);
					m_shaderGraphAllocs       .Image.material = new Material(ShaderFull);
                    break;

                case GraphyManager.Mode.LIGHT:
                    m_shaderGraphVideo  .ArrayMaxSize = G_GraphShader.ArrayMaxSizeLight;
                    m_shaderGraphTexture   .ArrayMaxSize = G_GraphShader.ArrayMaxSizeLight;
                    m_shaderGraphMesh       .ArrayMaxSize = G_GraphShader.ArrayMaxSizeLight;
					m_shaderGraphAllocs       .ArrayMaxSize = G_GraphShader.ArrayMaxSizeLight;

                    m_shaderGraphVideo  .Image.material = new Material(ShaderLight);
                    m_shaderGraphTexture   .Image.material = new Material(ShaderLight);
                    m_shaderGraphMesh       .Image.material = new Material(ShaderLight);
					m_shaderGraphAllocs       .Image.material = new Material(ShaderLight);
                    break;
            }

            m_shaderGraphVideo.InitializeShader();
            m_shaderGraphTexture.InitializeShader();
            m_shaderGraphMesh.InitializeShader();
			m_shaderGraphAllocs.InitializeShader();

			m_resolution = m_graphyManager.DevGraphResolution;
            
            CreatePoints();
        }

        #endregion

        #region Methods -> Protected Override

        protected override void UpdateGraph()
        {
            // Since we no longer initialize by default OnEnable(), 
            // we need to check here, and Init() if needed
            if (!m_isInitialized)
            {
                Init();
            }

			float videoMemory = m_devMonitor.VideoMemory;
			float textureMemory = m_devMonitor.TextureMemory;
			float meshMemory = m_devMonitor.MeshMemory;

            m_highestMemory = 0;

            for (int i = 0; i <= m_resolution - 1; i++)
            {
                if (i >= m_resolution - 1)
                {
                    m_videoArray[i] = videoMemory;
                    m_textureArray[i]  = textureMemory;
                    m_meshArray[i]      = meshMemory;
                }
                else
                {
                    m_videoArray[i] = m_videoArray[i + 1];
                    m_textureArray[i]  = m_textureArray[i + 1];
                    m_meshArray[i]      = m_meshArray[i + 1];
                }

                if (m_highestMemory < m_textureArray[i])
                {
                    m_highestMemory = m_textureArray[i];
                }
            }

            for (int i = 0; i <= m_resolution - 1; i++)
            {
                m_shaderGraphVideo.ShaderArrayValues[i] = m_videoArray[i] / m_highestMemory;

                m_shaderGraphTexture.ShaderArrayValues[i]  = m_textureArray[i] / m_highestMemory;

                m_shaderGraphMesh.ShaderArrayValues[i]      = m_meshArray[i] / m_highestMemory;
            }

            m_shaderGraphVideo.UpdatePoints();
            m_shaderGraphTexture.UpdatePoints();
            m_shaderGraphMesh.UpdatePoints();

			UpdateAllocsGraph();
		}

		private void UpdateAllocsGraph()
		{
			int allocsKB = m_devMonitor.AllocatedInFrameMemory / 1024;

            int currentMaxAllocs = 0;


			for (int i = 0; i <= m_resolution - 1; i++)
            {
                if (i >= m_resolution - 1)
                {
                    m_allocsArray[i] = allocsKB;
                }
                else
                {
                    m_allocsArray[i] = m_allocsArray[i + 1];
                }

				int cur = m_allocsArray[i]; 

                // Store the highest allocs to use as the highest point in the graph

                if (currentMaxAllocs < cur)
                {
                    currentMaxAllocs = cur;
                }
            }

            m_highestAlloc = m_highestAlloc < 1 || m_highestAlloc <= currentMaxAllocs ? currentMaxAllocs : m_highestAlloc - ( m_highestAlloc > 1000 ? m_highestAlloc / 10 : 1);

            m_highestAlloc = m_highestAlloc > 0 ? m_highestAlloc : 1;

            if (m_shaderGraphAllocs.ShaderArrayValues == null)
            {
                m_allocsArray                  = new int[m_resolution];
                m_shaderGraphAllocs.ShaderArrayValues         = new float[m_resolution];
            }

            for (int i = 0; i <= m_resolution - 1; i++)
            {
                m_shaderGraphAllocs.ShaderArrayValues[i]      = m_allocsArray[i] / (float) m_highestAlloc;
            }

            // Update the material values

            m_shaderGraphAllocs.UpdatePoints();

            m_shaderGraphAllocs.Average           = m_devMonitor.AverageAllocs / m_highestAlloc;
            m_shaderGraphAllocs.UpdateAverage();

            m_shaderGraphAllocs.GoodThreshold     = (float)m_graphyManager.CriticalAllocsThresholdKB / m_highestAlloc;
            m_shaderGraphAllocs.CautionThreshold  = (float)m_graphyManager.CautionAllocsThresholdKB / m_highestAlloc;
            m_shaderGraphAllocs.UpdateThresholds();
		}

		protected override void CreatePoints()
        {
            if (m_shaderGraphVideo.ShaderArrayValues == null || m_shaderGraphVideo.ShaderArrayValues.Length != m_resolution)
            {
                m_videoArray                = new float[m_resolution];
                m_textureArray                 = new float[m_resolution];
                m_meshArray                     = new float[m_resolution];
				m_allocsArray                   = new int[m_resolution];

                m_shaderGraphVideo.ShaderArrayValues    = new float[m_resolution];
                m_shaderGraphTexture.ShaderArrayValues     = new float[m_resolution];
                m_shaderGraphMesh.ShaderArrayValues         = new float[m_resolution];
				m_shaderGraphAllocs.ShaderArrayValues         = new float[m_resolution];
            }

            for (int i = 0; i < m_resolution; i++)
            {
                m_shaderGraphVideo.ShaderArrayValues[i] = 0;
                m_shaderGraphTexture.ShaderArrayValues[i]  = 0;
                m_shaderGraphMesh.ShaderArrayValues[i]      = 0;
				m_shaderGraphAllocs.ShaderArrayValues[i]      = 0;
            }

			// Initialize the material values

			// Colors

			m_shaderGraphVideo.GoodColor = m_graphyManager.VideoDevColor;
			m_shaderGraphVideo.CautionColor = m_graphyManager.VideoDevColor;
			m_shaderGraphVideo.CriticalColor = m_graphyManager.VideoDevColor;
            
            m_shaderGraphVideo.UpdateColors();

			m_shaderGraphTexture.GoodColor = m_graphyManager.TexturesDevColor;
			m_shaderGraphTexture.CautionColor = m_graphyManager.TexturesDevColor;
			m_shaderGraphTexture.CriticalColor = m_graphyManager.TexturesDevColor;
            
            m_shaderGraphTexture.UpdateColors();

			m_shaderGraphMesh.GoodColor = m_graphyManager.MeshesDevColor;
			m_shaderGraphMesh.CautionColor = m_graphyManager.MeshesDevColor;
			m_shaderGraphMesh.CriticalColor = m_graphyManager.MeshesDevColor;
            
            m_shaderGraphMesh.UpdateColors();

			m_shaderGraphAllocs.GoodColor = m_graphyManager.CriticalAllocsColor;
			m_shaderGraphAllocs.CautionColor = m_graphyManager.CautionAllocsColor;
			m_shaderGraphAllocs.CriticalColor = m_graphyManager.GoodAllocsColor;
            
            m_shaderGraphAllocs.UpdateColors();

            // Thresholds
            
            m_shaderGraphVideo.GoodThreshold    = 0;
            m_shaderGraphVideo.CautionThreshold = 0;
            m_shaderGraphVideo.UpdateThresholds();
            
            m_shaderGraphTexture.GoodThreshold     = 0;
            m_shaderGraphTexture.CautionThreshold  = 0;
            m_shaderGraphTexture.UpdateThresholds();
            
            m_shaderGraphMesh.GoodThreshold         = 0;
            m_shaderGraphMesh.CautionThreshold      = 0;
            m_shaderGraphMesh.UpdateThresholds();

            m_shaderGraphVideo.UpdateArray();
            m_shaderGraphTexture.UpdateArray();
            m_shaderGraphMesh.UpdateArray();
			m_shaderGraphAllocs.UpdateArray();

			// Average

			m_shaderGraphVideo.Average  = 0;
            m_shaderGraphTexture.Average   = 0;
            m_shaderGraphMesh.Average       = 0;

			m_shaderGraphVideo.UpdateAverage();
            m_shaderGraphTexture.UpdateAverage();
            m_shaderGraphMesh.UpdateAverage();
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            m_graphyManager = transform.root.GetComponentInChildren<GraphyManager>();

			m_devMonitor = GetComponent<G_DevMonitor>();
            
            m_shaderGraphVideo  = new G_GraphShader();
            m_shaderGraphTexture   = new G_GraphShader();
            m_shaderGraphMesh       = new G_GraphShader();
			m_shaderGraphAllocs     = new G_GraphShader();

            m_shaderGraphVideo  .Image = m_imageVideo;
            m_shaderGraphTexture   .Image = m_imageTexture;
            m_shaderGraphMesh       .Image = m_imageMeshes;
			m_shaderGraphAllocs      .Image = m_imageAllocs;

            UpdateParameters();

            m_isInitialized = true;
        }

        #endregion
    }
}
