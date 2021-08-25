/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@tayx94)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            05-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using Tayx.Graphy.Utils.NumString;

namespace Tayx.Graphy.Dev
{
	public class G_DevText : MonoBehaviour
    {
        #region Variables -> Serialized Private

		[SerializeField] private    Text            m_allocsCountText         = null;
		[SerializeField] private    Text            m_allocsMemoryText         = null;
		[SerializeField] private    Text            m_videoMemoryText         = null;
		[SerializeField] private    Text            m_texturesCountText         = null;
		[SerializeField] private    Text            m_texturesMemoryText         = null;
		[SerializeField] private    Text            m_meshesCountText         = null;
		[SerializeField] private    Text            m_meshesMemoryText         = null;
		[SerializeField] private    Text            m_materialsCountText         = null;
		[SerializeField] private    Text            m_materialsMemoryText         = null;
		[SerializeField] private    Text            m_assetsCountText         = null;
		[SerializeField] private    Text            m_objectsCountText         = null;

        #endregion

        #region Variables -> Private

        private                     GraphyManager   m_graphyManager                         = null;

		private G_DevMonitor m_devMonitor = null;

        private                     float           m_updateRate                            = 4f;  // 4 updates per sec.

        private                     float           m_deltaTime                             = 0.0f;

        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }

		private const int BytesInMB = 1024 * 1024;
		private const int BytesInKB = 1024;

		private void Update()
        {
            m_deltaTime += Time.unscaledDeltaTime;

            if (m_deltaTime > 1f / m_updateRate)
            {
				// update data
				m_allocsCountText.text = m_devMonitor.AllocatedInFrameCount.ToStringNonAlloc();
				m_allocsMemoryText.text = (m_devMonitor.AllocatedInFrameMemory / BytesInKB).ToStringNonAlloc();

				m_videoMemoryText.text = (m_devMonitor.VideoMemory/ BytesInMB).ToStringNonAlloc();

				m_texturesCountText.text = m_devMonitor.TextureCount.ToStringNonAlloc();
				m_texturesMemoryText.text = (m_devMonitor.TextureMemory / BytesInMB).ToStringNonAlloc();

				m_meshesCountText.text = m_devMonitor.MeshCount.ToStringNonAlloc();
				m_meshesMemoryText.text = (m_devMonitor.MeshMemory / BytesInMB).ToStringNonAlloc();

				m_materialsCountText.text = m_devMonitor.MaterialCount.ToStringNonAlloc();
				m_materialsMemoryText.text = (m_devMonitor.MaterialMemory / BytesInKB).ToStringNonAlloc();

				m_assetsCountText.text = m_devMonitor.AssetsCount.ToStringNonAlloc();
				m_objectsCountText.text = m_devMonitor.ObjectCount.ToStringNonAlloc();

				m_deltaTime                     = 0f;
            }
        }

        #endregion
        
        #region Methods -> Public
        
        public void UpdateParameters()
		{
			m_allocsCountText.color = m_graphyManager.AllocationDevColor;
			m_allocsMemoryText.color = m_graphyManager.AllocationDevColor;
			m_videoMemoryText.color = m_graphyManager.VideoDevColor;
			m_texturesCountText.color = m_graphyManager.TexturesDevColor;
			m_texturesMemoryText.color = m_graphyManager.TexturesDevColor;
			m_meshesCountText.color = m_graphyManager.MeshesDevColor;
			m_meshesMemoryText.color = m_graphyManager.MeshesDevColor;
			m_materialsCountText.color = m_graphyManager.MaterialsDevColor;
			m_materialsMemoryText.color = m_graphyManager.MaterialsDevColor;
			m_assetsCountText.color = m_graphyManager.AssetsDevColor;
			m_objectsCountText.color = m_graphyManager.ObjectsDevColor;

			m_updateRate = m_graphyManager.DevTextUpdateRate;
        }
        
        #endregion

        #region Methods -> Private

        private void Init()
        {
            // We assume no game will consume more than 16GB of RAM.
            // If it does, who cares about some minuscule garbage allocation lol.
            G_IntString.Init( 0, 16386 ); 

            m_graphyManager = transform.root.GetComponentInChildren<GraphyManager>();

			m_devMonitor = GetComponent<G_DevMonitor>();
           
            UpdateParameters();
        }

        #endregion
    }
}