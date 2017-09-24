// Created by AlanLiu Sep/23/2017
// 颜色混合效果调节器

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kara
{
    public class BlendColorEffectModifier : EffectModifierBase
    {
        public static string Name { get { return "BlendColor"; } }
        public static string[] Keywords { get { return new string[] { "BLEND_COLOR" }; } }
        public static OverlayMode Overlay { get { return OverlayMode.Overlay; } }

        private Vector4 m_BlendColor;

        public override string GetName()
        {
            return Name;
        }

        public override OverlayMode GetOverlayMode()
        {
            return Overlay;
        }

        public override void Apply(Renderer renderer, ArgumentTable args)
        {
            renderer.GetPropertyBlock(m_TempMPB);
            m_BlendColor = m_TempMPB.GetVector(BlendColor);

            var color = args.GetColor("BlendColor", Color.white);
            m_TempMPB.SetColor(BlendColor, color);
            renderer.SetPropertyBlock(m_TempMPB);
        }

        public override void Remove(Renderer renderer)
        {
            renderer.GetPropertyBlock(m_TempMPB);
            m_TempMPB.SetVector(BlendColor, m_BlendColor);
            renderer.SetPropertyBlock(m_TempMPB);
        }

        public override void UpdateArguments(Renderer renderer, ArgumentTable args)
        {
            var color = args.GetColor("BlendColor", Color.white);
            renderer.GetPropertyBlock(m_TempMPB);
            m_TempMPB.SetColor(BlendColor, color);
            renderer.SetPropertyBlock(m_TempMPB);
        }
    }
}
