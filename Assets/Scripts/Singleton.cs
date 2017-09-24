// Created by AlanLiu Sep/23/2017
// 单件

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kara
{
    public class Singleton<T> where T : Singleton<T>
    {
        public T Instance { get { return m_Instance; } }

        private static T m_Instance;

        public virtual void Initialize()
        {
            m_Instance = (T) this;
        }

        public virtual void Shutdown()
        {
            m_Instance = null;
        }
    }
}