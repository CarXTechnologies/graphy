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

		// OLD
		public float AllocatedDev { get; private set; }
		public float ReservedDev { get; private set; }
		public float MonoDev { get; private set; }

		public long AllocatedInFrameMemory { get; private set; }
		public long AllocatedInFrameCount { get; private set; }
		public long TextureMemory { get; private set; }
		public long TextureCount { get; private set; }
		public long MeshMemory { get; private set; }
		public long MeshCount { get; private set; }
		public long MaterialMemory { get; private set; }
		public long MaterialCount { get; private set; }
		public long AssetCount { get; private set; }
		public long TotalUnityObjectCount { get; private set; }
		public long VideoMemory { get; private set; }

		#endregion

		ProfilerRecorder m_allocInFrameMemoryRecorder;
		ProfilerRecorder m_allocInFrameCountRecorder;
		ProfilerRecorder m_textureMemoryRecorder;
		ProfilerRecorder m_textureCountRecorder;
		ProfilerRecorder m_meshMemoryRecorder;
		ProfilerRecorder m_meshCountRecorder;
		ProfilerRecorder m_materialMemoryRecorder;
		ProfilerRecorder m_materialCountRecorder;
		ProfilerRecorder m_assetCountRecorder;
		ProfilerRecorder m_totalUnityObjectCountRecorder;

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
			m_totalUnityObjectCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Unity Object Count");
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
			m_totalUnityObjectCountRecorder.Dispose();
		}

        #region Methods -> Unity Callbacks

        private void Update()
        {
			// NEW
			if (m_allocInFrameMemoryRecorder.Valid)
			{
				AllocatedInFrameMemory = m_allocInFrameMemoryRecorder.LastValue;
			}
			if (m_allocInFrameCountRecorder.Valid)
			{
				AllocatedInFrameCount = m_allocInFrameCountRecorder.LastValue;
			}
			if (m_textureMemoryRecorder.Valid)
			{
				TextureMemory = m_textureMemoryRecorder.LastValue;
			}
			if (m_textureCountRecorder.Valid)
			{
				TextureCount = m_textureCountRecorder.LastValue;
			}
			if (m_meshMemoryRecorder.Valid)
			{
				MeshMemory = m_meshMemoryRecorder.LastValue;
			}
			if (m_meshCountRecorder.Valid)
			{
				MeshCount = m_meshCountRecorder.LastValue;
			}
			if (m_materialMemoryRecorder.Valid)
			{
				MaterialMemory = m_materialMemoryRecorder.LastValue;
			}
			if (m_materialCountRecorder.Valid)
			{
				MaterialCount = m_materialCountRecorder.LastValue;
			}
			if (m_assetCountRecorder.Valid)
			{
				AssetCount = m_assetCountRecorder.LastValue;
			}
			if (m_totalUnityObjectCountRecorder.Valid)
			{
				TotalUnityObjectCount = m_totalUnityObjectCountRecorder.LastValue;
			}

			VideoMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();

			// TEMP - for debug
			AllocatedDev = AssetCount;
			ReservedDev = TextureMemory;
			MonoDev = AllocatedInFrameCount;
        }

        #endregion 
    }
}