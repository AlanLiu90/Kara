// Created by AlanLiu Sep/23/2017
// 效果调节器基类

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace kara
{
    public abstract class EffectModifierBase
    {
        public enum OverlayMode
        {
            Overlay,
            Ignore,
        }

        // Shader属性名
        protected static int BlendColor = -1;

        protected static MaterialPropertyBlock m_TempMPB;

        protected MaterialPropertyBlock m_OldMPB = new MaterialPropertyBlock();

        public static void Initialize()
        {
            BlendColor = Shader.PropertyToID("BlendColor");

            m_TempMPB = new MaterialPropertyBlock();
        }

        public abstract string GetName();
        public abstract OverlayMode GetOverlayMode();
        public abstract void Apply(Renderer renderer, ArgumentTable args);
        public abstract void Remove(Renderer renderer);

        public virtual bool Update()
        {
            return false;
        }

        public virtual void UpdateArguments(Renderer renderer, ArgumentTable args)
        {
            Assert.IsTrue(false, "子类没有实现该方法");
        }
    }
}
