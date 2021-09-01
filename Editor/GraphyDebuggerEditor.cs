/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@tayx94)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            02-Jan-18
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Tayx.Graphy
{
	[CustomEditor(typeof(GraphyDebugger))]
	internal class GraphyDebuggerEditor : DebugPacketsEditor
	{
		#region Variables -> Private
		private GraphyDebugger m_target;

		#endregion

		#region Methods -> Unity Callbacks

		private void OnEnable()
		{
			m_target = (GraphyDebugger)target;
		}

		#endregion

		#region Methods -> Public Override

		public override void OnInspectorGUI()
		{
			if (m_target == null && target == null)
			{
				BaseEditorOnInspectorGUI();

				return;
			}

			base.OnInspectorGUI();
		}

		#endregion
	}
}
