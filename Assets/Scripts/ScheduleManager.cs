// Created by AlanLiu Jul/22/2017
// 调度管理器

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace kara
{
    public class ScheduleManager : MonoBehaviour
    {
        public static ScheduleManager Instance { get; private set; }

        private struct CallbackInfo
        {
            public Action Callback;
            public int Priority;
            public bool Removed;
        }

        private struct TempCallbackInfo
        {
            public Action Callback;
            public int Priority;
        }

        private int m_UpdateCallbacksHighPriorityToRemoveCount = 0;
        private int m_UpdateCallbacksLowPriorityToRemoveCount = 0;
        private int m_UpdateCallbacksToRemoveCount = 0;
        private List<TempCallbackInfo> m_CallbacksToAdd = new List<TempCallbackInfo>();

        // FixedUpdate
        private int m_IsInFixedUpdate = 0;
        private List<CallbackInfo> m_FixedUpdateCallbacksHighPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_FixedUpdateCallbacksLowPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_FixedUpdateCallbacksDefaultPriority = new List<CallbackInfo>();

        // Update
        private int m_IsInUpdate = 0;
        private List<CallbackInfo> m_UpdateCallbacksHighPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_UpdateCallbacksLowPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_UpdateCallbacksDefaultPriority = new List<CallbackInfo>();

        // LateUpdate
        private int m_IsInLateUpdate = 0;
        private List<CallbackInfo> m_LateUpdateCallbacksHighPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_LateUpdateCallbacksLowPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_LateUpdateCallbacksDefaultPriority = new List<CallbackInfo>();

        // ApplicationQuit
        private int m_IsInApplicationQuit = 0;
        private List<CallbackInfo> m_ApplicationQuitCallbacksHighPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_ApplicationQuitCallbacksLowPriority = new List<CallbackInfo>();
        private List<CallbackInfo> m_ApplicationQuitCallbacksDefaultPriority = new List<CallbackInfo>();

        public void Initialize()
        {
            if (Instance != null)
                throw new Exception("单例已存在");

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        public void Shutdown()
        {
            if (Instance == this)
                Instance = null;
        }

        private void FixedUpdate()
        {
            InvokeCallbacks(m_FixedUpdateCallbacksHighPriority, m_FixedUpdateCallbacksDefaultPriority,
                m_FixedUpdateCallbacksLowPriority, ref m_IsInFixedUpdate);
        }

        private void Update()
        {
            InvokeCallbacks(m_UpdateCallbacksHighPriority, m_UpdateCallbacksDefaultPriority,
                m_UpdateCallbacksLowPriority, ref m_IsInUpdate);
        }

        private void LateUpdate()
        {
            InvokeCallbacks(m_LateUpdateCallbacksHighPriority, m_LateUpdateCallbacksDefaultPriority,
                m_LateUpdateCallbacksLowPriority, ref m_IsInLateUpdate);
        }

        private void OnApplicationQuit()
        {
            InvokeCallbacks(m_ApplicationQuitCallbacksHighPriority, m_ApplicationQuitCallbacksDefaultPriority,
                m_ApplicationQuitCallbacksLowPriority, ref m_IsInApplicationQuit);
        }

        public void RegisterFixedUpdateCallbacks(Action callback, int priority = 0)
        {
            RegisterCallback(m_FixedUpdateCallbacksHighPriority, m_FixedUpdateCallbacksDefaultPriority,
                    m_FixedUpdateCallbacksLowPriority, ref m_IsInFixedUpdate, callback, priority);
        }

        public void UnregisterFixedUpdateCallbacks(Action callback, int priority = 0)
        {
            UnregisterCallback(m_FixedUpdateCallbacksHighPriority, m_FixedUpdateCallbacksDefaultPriority,
                    m_FixedUpdateCallbacksLowPriority, ref m_IsInFixedUpdate, callback, priority);
        }

        public void RegisterUpdateCallbacks(Action callback, int priority = 0)
        {
            RegisterCallback(m_UpdateCallbacksHighPriority, m_UpdateCallbacksDefaultPriority,
                    m_UpdateCallbacksLowPriority, ref m_IsInUpdate, callback, priority);
        }

        public void UnregisterUpdateCallbacks(Action callback, int priority = 0)
        {
            UnregisterCallback(m_UpdateCallbacksHighPriority, m_UpdateCallbacksDefaultPriority,
                    m_UpdateCallbacksLowPriority, ref m_IsInUpdate, callback, priority);
        }

        public void RegisterLateUpdateCallback(Action callback, int priority = 0)
        {
            RegisterCallback(m_LateUpdateCallbacksHighPriority, m_LateUpdateCallbacksDefaultPriority,
                    m_LateUpdateCallbacksLowPriority, ref m_IsInLateUpdate, callback, priority);
        }

        public void UnregisterLateUpdateCallback(Action callback, int priority = 0)
        {
            UnregisterCallback(m_LateUpdateCallbacksHighPriority, m_LateUpdateCallbacksDefaultPriority,
                    m_LateUpdateCallbacksLowPriority, ref m_IsInLateUpdate, callback, priority);
        }

        public void RegisterApplicationQuitCallback(Action callback, int priority = 0)
        {
            RegisterCallback(m_ApplicationQuitCallbacksHighPriority, m_ApplicationQuitCallbacksDefaultPriority,
                    m_ApplicationQuitCallbacksLowPriority, ref m_IsInApplicationQuit, callback, priority);
        }

        public void UnregisterApplicationQuitCallback(Action callback, int priority = 0)
        {
            UnregisterCallback(m_ApplicationQuitCallbacksHighPriority, m_ApplicationQuitCallbacksDefaultPriority,
                    m_ApplicationQuitCallbacksLowPriority, ref m_IsInApplicationQuit, callback, priority);
        }

        private void InvokeCallbacks(List<CallbackInfo> highPriorityList, List<CallbackInfo> defaultPriorityList,
            List<CallbackInfo> lowPriorityList, ref int updateCount)
        {
            Assert.IsTrue(updateCount == 0);
            Assert.IsTrue(m_CallbacksToAdd.Count == 0);

            ++updateCount;

            for (int i = 0; i < highPriorityList.Count; ++i)
            {
                try
                {
                    if (!highPriorityList[i].Removed)
                        highPriorityList[i].Callback.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            ++updateCount;

            for (int i = 0; i < defaultPriorityList.Count; ++i)
            {
                try
                {
                    if (!defaultPriorityList[i].Removed)
                        defaultPriorityList[i].Callback.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            ++updateCount;

            for (int i = 0; i < lowPriorityList.Count; ++i)
            {
                try
                {
                    if (!lowPriorityList[i].Removed)
                        lowPriorityList[i].Callback.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            updateCount = 0;

            if (m_UpdateCallbacksHighPriorityToRemoveCount > 0)
            {
                ClearRemovedCallbacks(highPriorityList);
                m_UpdateCallbacksHighPriorityToRemoveCount = 0;
            }

            if (m_UpdateCallbacksToRemoveCount > 0)
            {
                ClearRemovedCallbacks(defaultPriorityList);
                m_UpdateCallbacksToRemoveCount = 0;
            }

            if (m_UpdateCallbacksLowPriorityToRemoveCount > 0)
            {
                ClearRemovedCallbacks(lowPriorityList);
                m_UpdateCallbacksLowPriorityToRemoveCount = 0;
            }

            for (int i = 0; i < m_CallbacksToAdd.Count; ++i)
            {
                var info = m_CallbacksToAdd[i];
                RegisterCallback(highPriorityList, defaultPriorityList, 
                    lowPriorityList, ref updateCount, info.Callback, info.Priority);
            }

            m_CallbacksToAdd.Clear();
        }

        private void RegisterCallback(List<CallbackInfo> highPriorityList,
            List<CallbackInfo> defaultPriorityList, List<CallbackInfo> lowPriorityList, 
            ref int updateCount, Action callback, int priority)
        {
            if (updateCount > 0)
            {
                if (updateCount == 1)
                {
                    if (AddCallback(highPriorityList, callback,
                        ref m_UpdateCallbacksHighPriorityToRemoveCount))
                    {
                        return;
                    }
                }
                else if (updateCount == 2)
                {
                    if (AddCallback(defaultPriorityList, callback,
                        ref m_UpdateCallbacksToRemoveCount))
                    {
                        return;
                    }
                }
                else
                {
                    if (AddCallback(lowPriorityList, callback,
                        ref m_UpdateCallbacksLowPriorityToRemoveCount))
                    {
                        return;
                    }
                }

                {
                    var info = new TempCallbackInfo();
                    info.Callback = callback;
                    info.Priority = priority;
                    m_CallbacksToAdd.Add(info);
                }

                return;
            }

            if (priority == 0)
            {
                for (int i = 0; i < defaultPriorityList.Count; ++i)
                {
                    if (defaultPriorityList[i].Callback == callback)
                        // 已经存在
                        return;
                }

                var info = new CallbackInfo();
                info.Callback = callback;
                info.Priority = priority;
                info.Removed = false;
                defaultPriorityList.Add(info);
            }
            else
            {
                List<CallbackInfo> callbacks = null;
                if (priority < 0)
                    callbacks = highPriorityList;
                else
                    callbacks = lowPriorityList;

                int idx = -1;
                for (int i = 0; i < callbacks.Count; ++i)
                {
                    if (callbacks[i].Callback == callback)
                        // 已经存在
                        return;

                    if (callbacks[i].Priority > priority && idx == -1)
                        idx = i;
                }

                if (idx == -1)
                    idx = callbacks.Count;

                var info = new CallbackInfo();
                info.Callback = callback;
                info.Priority = priority;
                info.Removed = false;

                callbacks.Add(info);
                for (int i = callbacks.Count - 1; i > idx; --i)
                    callbacks[i] = callbacks[i - 1];

                callbacks[idx] = info;
            }
        }

        private void UnregisterCallback(List<CallbackInfo> highPriorityList,
            List<CallbackInfo> defaultPriorityList, List<CallbackInfo> lowPriorityList,
            ref int updateCount, Action callback, int priority)
        {
            if (updateCount > 0)
            {
                for (int i = 0; i < m_CallbacksToAdd.Count; ++i)
                {
                    if (m_CallbacksToAdd[i].Callback == callback)
                    {
                        for (int j = i; j < m_CallbacksToAdd.Count - 1; ++j)
                            m_CallbacksToAdd[j] = m_CallbacksToAdd[j + 1];

                        m_CallbacksToAdd.RemoveAt(m_CallbacksToAdd.Count - 1);

                        return;
                    }
                }

                if (updateCount == 1)
                {
                    RemoveCallback(highPriorityList, callback, 
                        ref m_UpdateCallbacksHighPriorityToRemoveCount);
                }
                else if (updateCount == 2)
                {
                    RemoveCallback(defaultPriorityList, callback,
                        ref m_UpdateCallbacksToRemoveCount);
                }
                else
                {
                    RemoveCallback(lowPriorityList, callback,
                        ref m_UpdateCallbacksLowPriorityToRemoveCount);
                }

                return;
            }

            List<CallbackInfo> callbacks = null;
            if (priority == 0)
                callbacks = defaultPriorityList;
            else if (priority < 0)
                callbacks = highPriorityList;
            else
                callbacks = lowPriorityList;

            for (int i = 0; i < callbacks.Count; ++i)
            {
                if (callbacks[i].Callback == callback)
                {
                    for (int j = i; j < callbacks.Count - 1; ++j)
                        callbacks[j] = callbacks[j + 1];

                    callbacks.RemoveAt(callbacks.Count - 1);

                    return;
                }
            }
        }

        private bool AddCallback(List<CallbackInfo> callbacks, Action callback, ref int toRemoveCount)
        {
            for (int i = 0; i < callbacks.Count; ++i)
            {
                if (callbacks[i].Callback == callback)
                {
                    // 检查是否已标记为移除
                    if (callbacks[i].Removed)
                    {
                        var info = callbacks[i];
                        info.Removed = false;
                        callbacks[i] = info;

                        --toRemoveCount;
                    }

                    return true;
                }
            }

            return false;
        }

        private void RemoveCallback(List<CallbackInfo> callbacks, Action callback, ref int toRemoveCount)
        {
            for (int i = 0; i < callbacks.Count; ++i)
            {
                if (callbacks[i].Callback == callback)
                {
                    if (!callbacks[i].Removed)
                    {
                        // 标记为已移除
                        var info = callbacks[i];
                        info.Removed = true;
                        callbacks[i] = info;

                        ++toRemoveCount;
                    }

                    break;
                }
            }
        }

        // 清除标记为移除的回调
        private void ClearRemovedCallbacks(List<CallbackInfo> callbacks)
        {
            int j = 0;
            for (int i = 0; i < callbacks.Count; ++i)
            {
                if (!callbacks[i].Removed)
                {
                    if (i != j)
                        callbacks[j] = callbacks[i];

                    ++j;
                }
            }

            callbacks.RemoveRange(j, callbacks.Count - j);
        }
    }
}
