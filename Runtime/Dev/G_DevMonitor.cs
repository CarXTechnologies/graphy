/* ---------------------------------------
 * Author:          Michael Pizik (m.pizik@carx-tech.com) (@MihaPro)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            27-Aug-21
 * Studio:          CarX Technologies
 *
 * Git repo:        https://github.com/CarXTechnologies/graphy.git
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using Unity.Profiling;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Tayx.Graphy.Dev
{
	public class G_DevMonitor : MonoBehaviour
    {
		#region Properties -> Public

		public float AllocatedInFrameMemory { get; private set; }
		public int AllocatedInFrameCount { get; private set; }
		public float TextureMemory { get; private set; }
		public int TextureCount { get; private set; }
		public float MeshMemory { get; private set; }
		public int MeshCount { get; private set; }
		public float MaterialMemory { get; private set; }
		public int MaterialCount { get; private set; }
		public int AssetsCount { get; private set; }
		public int ObjectCount { get; private set; }
		public float VideoMemory { get; private set; }
		public float AverageAllocs { get; private set; } = 0;

		#endregion

		private ProfilerRecorder m_allocInFrameMemoryRecorder;
		private ProfilerRecorder m_allocInFrameCountRecorder;
		private ProfilerRecorder m_textureMemoryRecorder;
		private ProfilerRecorder m_textureCountRecorder;
		private ProfilerRecorder m_meshMemoryRecorder;
		private ProfilerRecorder m_meshCountRecorder;
		private ProfilerRecorder m_materialMemoryRecorder;
		private ProfilerRecorder m_materialCountRecorder;
		private ProfilerRecorder m_assetCountRecorder;
		private ProfilerRecorder m_objectCountRecorder;

		private float[] m_allocsSamples;
		private int m_allocsSamplesCapacity = 1024;
		private int m_indexSample = 0;
		private int m_allocsSamplesCount = 0;

		private void Awake()
		{
			m_allocsSamples = new float[m_allocsSamplesCapacity];
		}

		private void OnEnable()
		{
			m_allocInFrameMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
			m_allocInFrameCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocation In Frame Count");
			m_textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
			m_textureCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Count");
			m_meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
			m_meshCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Count");
			m_materialMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Material Memory");
			m_materialCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Material Count");
			m_assetCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Asset Count");
			m_objectCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Object Count");
		}
		
		private void OnDisable()
		{
			m_allocInFrameMemoryRecorder.Dispose();
			m_allocInFrameCountRecorder.Dispose();
			m_textureMemoryRecorder.Dispose();
			m_textureCountRecorder.Dispose();
			m_meshMemoryRecorder.Dispose();
			m_meshCountRecorder.Dispose();
			m_materialMemoryRecorder.Dispose();
			m_materialCountRecorder.Dispose();
			m_assetCountRecorder.Dispose();
			m_objectCountRecorder.Dispose();
		}

        #region Methods -> Unity Callbacks

        private void Update()
        {
			const float ToKB = 1024;
			const float ToMB = ToKB * 1024;

			// NEW
			if (m_allocInFrameMemoryRecorder.Valid)
			{
				AllocatedInFrameMemory = m_allocInFrameMemoryRecorder.LastValue / ToKB;
			}
			if (m_allocInFrameCountRecorder.Valid)
			{
				AllocatedInFrameCount = (int)m_allocInFrameCountRecorder.LastValue;
			}
			if (m_textureMemoryRecorder.Valid)
			{
				TextureMemory = m_textureMemoryRecorder.LastValue / ToMB;
			}
			if (m_textureCountRecorder.Valid)
			{
				TextureCount = (int)m_textureCountRecorder.LastValue;
			}
			if (m_meshMemoryRecorder.Valid)
			{
				MeshMemory = m_meshMemoryRecorder.LastValue / ToMB;
			}
			if (m_meshCountRecorder.Valid)
			{
				MeshCount = (int)m_meshCountRecorder.LastValue;
			}
			if (m_materialMemoryRecorder.Valid)
			{
				MaterialMemory = m_materialMemoryRecorder.LastValue / ToKB;
			}
			if (m_materialCountRecorder.Valid)
			{
				MaterialCount = (int)m_materialCountRecorder.LastValue;
			}
			if (m_assetCountRecorder.Valid)
			{
				AssetsCount = (int)m_assetCountRecorder.LastValue;
			}
			if (m_objectCountRecorder.Valid)
			{
				ObjectCount = (int)m_objectCountRecorder.LastValue;
			}

			VideoMemory = Profiler.GetAllocatedMemoryForGraphicsDriver() / ToMB;


			m_indexSample++;

			float averageAllocs = 0;

			if ( m_indexSample >= m_allocsSamplesCapacity ) m_indexSample = 0;

            m_allocsSamples[ m_indexSample ] = AllocatedInFrameMemory;

            if (m_allocsSamplesCount < m_allocsSamplesCapacity)
            {
                m_allocsSamplesCount++;
            }

            for (int i = 0; i < m_allocsSamplesCount; i++)
            {
                averageAllocs += m_allocsSamples[i];
            }

            AverageAllocs = (int)((float)averageAllocs / (float)m_allocsSamplesCount);
        }

        #endregion 
    }
}