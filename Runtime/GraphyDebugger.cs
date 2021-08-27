﻿/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@tayx94)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            23-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

using System;
using System.Collections.Generic;
using System.Linq;

using Tayx.Graphy.Audio;
using Tayx.Graphy.Fps;
using Tayx.Graphy.Ram;
using Tayx.Graphy.Utils;
using Tayx.Graphy.Dev;

namespace Tayx.Graphy
{
    /// <summary>
    /// Main class to access the Graphy Debugger API.
    /// </summary>
    public class GraphyDebugger : G_Singleton<GraphyDebugger>
    {
        /* ----- TODO: ----------------------------
         * Add summaries to the variables.
         * Add summaries to the functions.
         * Ask why we're not using System.Serializable instead for the helper class.
         * Simplify the initializers of the DebugPackets, but check wether we should as some wont work with certain lists.
         * --------------------------------------*/

        protected GraphyDebugger () { }

        #region Enums -> Public

        public enum DebugVariable
        {
            Fps,
            Fps_Min,
            Fps_Max,
            Fps_Avg,
            Ram_Allocated,
            Ram_Reserved,
            Ram_Mono,
			Dev_VideoMem,
			Dev_TexturesMem,
			Dev_MeshesMem,
			Dev_MaterialsMem,
			Dev_Assets,
			Dev_Objects,
			Dev_AllocsCount,
			Dev_Allocs,
			Dev_Allocs_Avg,
            Audio_DB
        }

        public enum DebugComparer
        {
            Less_than,
            Equals_or_less_than,
            Equals,
            Equals_or_greater_than,
            Greater_than
        }

        public enum ConditionEvaluation
        {
            All_conditions_must_be_met,
            Only_one_condition_has_to_be_met,

        }

        public enum MessageType
        {
            Log,
            Warning,
            Error
        }

        #endregion

        #region Structs -> Public

        [System.Serializable]
        public struct DebugCondition
        {
            [Tooltip("Variable to compare against")]
            public DebugVariable Variable;
            [Tooltip("Comparer operator to use")]
            public DebugComparer Comparer;
            [Tooltip("Value to compare against the chosen variable")]
            public float         Value;
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        public class DebugPacket
        {

            [Tooltip("If false, it won't be checked")]
            public bool                 Active                  = true;
            [Tooltip("Optional Id. It's used to get or remove DebugPackets in runtime")]
            public int                  Id;
            [Tooltip("If true, once the actions are executed, this DebugPacket will delete itself")]
            public bool                 ExecuteOnce             = true;
            [Tooltip("Time to wait before checking if conditions are met (use this to avoid low fps drops triggering the conditions when loading the game)")]
            public float                InitSleepTime           = 2;
            [Tooltip("Time to wait before checking if conditions are met again (once they have already been met and if ExecuteOnce is false)")]
            public float                ExecuteSleepTime        = 2;

            public ConditionEvaluation  ConditionEvaluation     = ConditionEvaluation.All_conditions_must_be_met;
            [Tooltip("List of conditions that will be checked each frame")]
            public List<DebugCondition> DebugConditions         = new List<DebugCondition>();

            // Actions on conditions met

            public MessageType          MessageType;
            [Multiline]
            public string               Message                 = string.Empty;
            public bool                 TakeScreenshot          = false;
            public string               ScreenshotFileName      = "Graphy_Screenshot";
            [Tooltip("If true, it pauses the editor")]
            public bool                 DebugBreak              = false;
            public UnityEvent           UnityEvents;
            public List<System.Action>  Callbacks               = new List<System.Action>();


            private bool canBeChecked = false;
            private bool executed = false;

            private float timePassed = 0;
            
            public bool Check { get { return canBeChecked; } }

            public void Update()
            {
                if (!canBeChecked)
                {
                    timePassed += Time.deltaTime;

                    if (    (executed && timePassed >= ExecuteSleepTime)
                        || (!executed && timePassed >= InitSleepTime))
                    {
                        canBeChecked = true;

                        timePassed = 0;
                    }
                }
            }

            public void Executed()
            {
                canBeChecked = false;
                executed = true;
            }
        }

        #endregion

        #region Variables -> Serialized Private

        [SerializeField] private    List<DebugPacket>   m_debugPackets = new List<DebugPacket>();

        #endregion

        #region Variables -> Private

        private                     G_FpsMonitor          m_fpsMonitor = null;
        private                     G_RamMonitor          m_ramMonitor = null;
        private                     G_DevMonitor          m_devMonitor = null;
		private                     G_AudioMonitor        m_audioMonitor = null;

        #endregion

        #region Methods -> Unity Callbacks

        private void Start()
        {
            m_fpsMonitor    = GetComponentInChildren<G_FpsMonitor>();
            m_ramMonitor    = GetComponentInChildren<G_RamMonitor>();
			m_devMonitor    = GetComponentInChildren<G_DevMonitor>();
            m_audioMonitor  = GetComponentInChildren<G_AudioMonitor>();
        }

        private void Update()
        {
            CheckDebugPackets();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket(DebugPacket newDebugPacket)
        {
            m_debugPackets?.Add(newDebugPacket);
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            DebugCondition newDebugCondition,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            System.Action newCallback
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions.Add(newDebugCondition);
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks.Add(newCallback);

            AddNewDebugPacket(newDebugPacket);
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            List<DebugCondition> newDebugConditions,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            System.Action newCallback
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions = newDebugConditions;
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks.Add(newCallback);

            AddNewDebugPacket(newDebugPacket);
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            DebugCondition newDebugCondition,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            List<System.Action> newCallbacks
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions.Add(newDebugCondition);
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks = newCallbacks;

            AddNewDebugPacket(newDebugPacket);
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            List<DebugCondition> newDebugConditions,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            List<System.Action> newCallbacks
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions = newDebugConditions;
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks = newCallbacks;

            AddNewDebugPacket(newDebugPacket);
        }

        /// <summary>
        /// Returns the first Packet with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public DebugPacket GetFirstDebugPacketWithId(int packetId)
        {
            return m_debugPackets.First(x => x.Id == packetId);
        }

        /// <summary>
        /// Returns a list with all the Packets with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public List<DebugPacket> GetAllDebugPacketsWithId(int packetId)
        {
            return m_debugPackets.FindAll(x => x.Id == packetId);
        }

        /// <summary>
        /// Removes the first Packet with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public void RemoveFirstDebugPacketWithId(int packetId)
        {
            if (m_debugPackets != null && GetFirstDebugPacketWithId(packetId) != null)
            {
                m_debugPackets.Remove(GetFirstDebugPacketWithId(packetId));
            }
        }

        /// <summary>
        /// Removes all the Packets with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public void RemoveAllDebugPacketsWithId(int packetId)
        {
            if (m_debugPackets != null)
            {
                m_debugPackets.RemoveAll(x => x.Id == packetId);
            }
        }

        /// <summary>
        /// Add an Action callback to the first Packet with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="id"></param>
        public void AddCallbackToFirstDebugPacketWithId(System.Action callback, int id)
        {
            if (GetFirstDebugPacketWithId(id) != null)
            {
                GetFirstDebugPacketWithId(id).Callbacks.Add(callback);
            }
        }

        /// <summary>
        /// Add an Action callback to all the Packets with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="id"></param>
        public void AddCallbackToAllDebugPacketWithId(System.Action callback, int id)
        {
            if (GetAllDebugPacketsWithId(id) != null)
            {
                foreach (var debugPacket in GetAllDebugPacketsWithId(id))
                {
                    if (callback != null)
                    {
                        debugPacket.Callbacks.Add(callback);
                    }
                }
            }
        }

        #endregion

        #region Methods -> Private

        /// <summary>
        /// Checks all the Debug Packets to see if they have to be executed.
        /// </summary>
        private void CheckDebugPackets()
        {
            if (m_debugPackets == null)
            {
                return;
            }

            for (int i = 0; i < m_debugPackets.Count; i++)
            {
                DebugPacket packet = m_debugPackets[i];

                if (packet != null && packet.Active)
                {
                    packet.Update();

                    if (packet.Check)
                    {
                        switch (packet.ConditionEvaluation)
                        {
                            case ConditionEvaluation.All_conditions_must_be_met:
                                int count = 0;

								float lastValue = 0;
								foreach (var packetDebugCondition in packet.DebugConditions)
                                {
                                    if (CheckIfConditionIsMet(packetDebugCondition))
                                    {
										lastValue = m_lastValue;
										count++;
                                    }
                                }

                                if (count >= packet.DebugConditions.Count)
                                {
                                    ExecuteOperationsInDebugPacket(packet, lastValue);

                                    if (packet.ExecuteOnce)
                                    {
                                        m_debugPackets[i] = null;
                                    }
                                }

                                break;

                            case ConditionEvaluation.Only_one_condition_has_to_be_met:
                                foreach (var packetDebugCondition in packet.DebugConditions)
                                {
                                    if (CheckIfConditionIsMet(packetDebugCondition))
                                    {
                                        ExecuteOperationsInDebugPacket(packet, m_lastValue);

                                        if (packet.ExecuteOnce)
                                        {
                                            m_debugPackets[i] = null;
                                        }

                                        break;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            m_debugPackets.RemoveAll((packet) => packet == null);
        }

        /// <summary>
        /// Returns true if a condition is met.
        /// </summary>
        /// <param name="debugCondition"></param>
        /// <returns></returns>
        private bool CheckIfConditionIsMet(DebugCondition debugCondition)
        {
            switch (debugCondition.Comparer)
            {
                case DebugComparer.Less_than:
                    return GetRequestedValueFromDebugVariable(debugCondition.Variable) < debugCondition.Value;
                case DebugComparer.Equals_or_less_than:
                    return GetRequestedValueFromDebugVariable(debugCondition.Variable) <= debugCondition.Value;
                case DebugComparer.Equals:
                    return Mathf.Approximately(GetRequestedValueFromDebugVariable(debugCondition.Variable), debugCondition.Value);
                case DebugComparer.Equals_or_greater_than:
                    return GetRequestedValueFromDebugVariable(debugCondition.Variable) >= debugCondition.Value;
                case DebugComparer.Greater_than:
                    return GetRequestedValueFromDebugVariable(debugCondition.Variable) > debugCondition.Value;

                default:
                    return false;
            }
        }

		/// <summary>
		/// Obtains the requested value from the specified variable.
		/// </summary>
		/// <param name="debugVariable"></param>
		/// <returns></returns>
		private float GetRequestedValueFromDebugVariable(DebugVariable debugVariable)
		{
			m_lastValue = DoGetRequestedValueFromDebugVariable(debugVariable);
			return m_lastValue;
		}

		private float m_lastValue;

		private float DoGetRequestedValueFromDebugVariable(DebugVariable debugVariable)
		{
			if (m_fpsMonitor != null)
			{
				switch (debugVariable)
				{
					case DebugVariable.Fps:
						return m_fpsMonitor.CurrentFPS;
					case DebugVariable.Fps_Min:
						return m_fpsMonitor.OnePercentFPS;
					case DebugVariable.Fps_Max:
						return m_fpsMonitor.Zero1PercentFps;
					case DebugVariable.Fps_Avg:
						return m_fpsMonitor.AverageFPS;
				}
			}

			if (m_ramMonitor != null)
			{
				switch (debugVariable)
				{
					case DebugVariable.Ram_Allocated:
						return m_ramMonitor.AllocatedRam;
					case DebugVariable.Ram_Reserved:
						return m_ramMonitor.AllocatedRam;
					case DebugVariable.Ram_Mono:
						return m_ramMonitor.AllocatedRam;
				}
			}

			if (m_devMonitor != null)
			{
				const int toKB = 1024;
				const int toMB = 1024 * 1024;
				switch (debugVariable)
				{
					case GraphyDebugger.DebugVariable.Dev_VideoMem:
						return m_devMonitor.VideoMemory / toMB;
					case GraphyDebugger.DebugVariable.Dev_TexturesMem:
					return m_devMonitor.TextureMemory / toMB;
					case GraphyDebugger.DebugVariable.Dev_MeshesMem:
						return m_devMonitor.MeshMemory / toMB;
					case GraphyDebugger.DebugVariable.Dev_MaterialsMem:
						return m_devMonitor.VideoMemory / toKB;
					case GraphyDebugger.DebugVariable.Dev_Assets:
						return m_devMonitor.AssetsCount;
					case GraphyDebugger.DebugVariable.Dev_Objects:
						return m_devMonitor.ObjectCount;
					case GraphyDebugger.DebugVariable.Dev_AllocsCount:
						return m_devMonitor.AllocatedInFrameCount;
					case GraphyDebugger.DebugVariable.Dev_Allocs:
						return m_devMonitor.AllocatedInFrameMemory / toKB;
					case GraphyDebugger.DebugVariable.Dev_Allocs_Avg:
						return m_devMonitor.AverageAllocs / toKB;
				}
			}

			if (debugVariable == DebugVariable.Audio_DB)
			{
				return m_audioMonitor != null ? m_audioMonitor.MaxDB : 0;
			}
			
			return 0;
		}

        /// <summary>
        /// Executes the operations in the DebugPacket specified.
        /// </summary>
        /// <param name="debugPacket"></param>
        private void ExecuteOperationsInDebugPacket(DebugPacket debugPacket, float value)
        {
            if (debugPacket != null)
            {
                if (debugPacket.DebugBreak)
                {
                    Debug.Break();
                }

                if (debugPacket.Message != "")
                {
                    string message = "[Graphy] (" + System.DateTime.Now + "): " + debugPacket.Message;

                    switch (debugPacket.MessageType)
                    {
                        case MessageType.Log:
                            Debug.Log(string.Format(message, value));
                            break;
                        case MessageType.Warning:
                            Debug.LogWarning(string.Format(message, value));
                            break;
                        case MessageType.Error:
                            Debug.LogError(string.Format(message, value));
                            break;
                    }
                }

                if (debugPacket.TakeScreenshot)
                {
                    string path = debugPacket.ScreenshotFileName + "_" + System.DateTime.Now + ".png";
                    path = path.Replace("/", "-").Replace(" ", "_").Replace(":", "-");

#if UNITY_2017_1_OR_NEWER
                    ScreenCapture.CaptureScreenshot(path);
#else
                    Application.CaptureScreenshot(path);
#endif
                }

                debugPacket.UnityEvents.Invoke();

                foreach (var callback in debugPacket.Callbacks)
                {
                    if (callback != null) callback();
                }

                debugPacket.Executed();
            }
        }

        #endregion
    }
}