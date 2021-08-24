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

using UnityEngine;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Tayx.Graphy.Dev
{
	public class G_DevMonitor : MonoBehaviour
    {
		#region Properties -> Public

		public float AllocatedDev { get; private set; }
		public float ReservedDev { get; private set; }
		public float MonoDev { get; private set; }

        #endregion

        #region Methods -> Unity Callbacks

        private void Update()
        {
			AllocatedDev = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
			ReservedDev = Profiler.GetTotalReservedMemoryLong() / 1048576f;
			MonoDev = Profiler.GetMonoUsedSizeLong() / 1048576f;
        }

        #endregion 
    }
}