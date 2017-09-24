// Created by AlanLiu Sep/23/2017
// 材质效果管理器

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace kara
{
    public class MaterialEffectManager : Singleton<MaterialEffectManager>
    {
        private class RendererData
        {
            public Material RawMaterial;
            public Dictionary<string, EffectModifierBase> Modifiers;

            public RendererData()
            {
                Modifiers = new Dictionary<string, EffectModifierBase>();
            }
        }

        private Dictionary<string, System.Type> m_EffectModifiers;
        private Dictionary<string, string[]> m_EffectKeywords;

        private Dictionary<int, RendererData> m_RendererDataTable;
        private Dictionary<string, Dictionary<int, Material>> m_KeywordsMaterials;
        private List<EffectModifierBase> m_ModifiersToUpdate;
        private List<EffectModifierBase> m_ModifiersToStopUpdate;

        private List<string> m_KeywordList;
        private StringBuilder m_StringBuilder = new StringBuilder();
        private MaterialPropertyBlock m_EmptyMPB;

        // 初始化
        public override void Initialize()
        {
            base.Initialize();

            m_EffectModifiers = new Dictionary<string, System.Type>();
            m_EffectModifiers.Add(BlendColorEffectModifier.Name, typeof(BlendColorEffectModifier));

            m_EffectKeywords = new Dictionary<string, string[]>();
            m_EffectKeywords.Add(BlendColorEffectModifier.Name, BlendColorEffectModifier.Keywords);

            m_RendererDataTable = new Dictionary<int, RendererData>();
            m_KeywordsMaterials = new Dictionary<string, Dictionary<int, Material>>();
            m_ModifiersToUpdate = new List<EffectModifierBase>();
            m_ModifiersToStopUpdate = new List<EffectModifierBase>();
            m_KeywordList = new List<string>();
            m_EmptyMPB = new MaterialPropertyBlock();

            EffectModifierBase.Initialize();

            ScheduleManager.Instance.RegisterUpdateCallbacks(OnUpdate);
        }

        // 关闭
        public override void Shutdown()
        {
            ScheduleManager.Instance.UnregisterUpdateCallbacks(OnUpdate);

            m_EffectModifiers.Clear();
            m_EffectKeywords.Clear();
            m_RendererDataTable.Clear();
            m_KeywordsMaterials.Clear();
            m_KeywordList.Clear();

            base.Shutdown();
        }

        // 应用效果
        public void ApplyEffect(string effect, Renderer renderer, ArgumentTable args)
        {
            if (!m_EffectModifiers.ContainsKey(effect))
            {
                Debug.LogError("效果不存在：" + effect);
                return;
            }

            int rendererId = renderer.GetInstanceID();

            RendererData data;
            Material rawMat;
            EffectModifierBase modifier = null;

            if (m_RendererDataTable.TryGetValue(rendererId, out data))
            {
                if (data.Modifiers.TryGetValue(effect, out modifier) && 
                    modifier.GetOverlayMode() == EffectModifierBase.OverlayMode.Ignore)
                    // 效果已存在，且不能叠加，不处理
                    return;

                rawMat = data.RawMaterial;
            }
            else
            {
                rawMat = renderer.sharedMaterial;
                data = new RendererData();
                data.RawMaterial = rawMat;
                m_RendererDataTable.Add(rendererId, data);
            }

            Assert.IsNotNull(rawMat);

            if (modifier != null)
            {
                modifier.UpdateArguments(renderer, args);
            }
            else
            {
                // 取出所有关键字
                m_KeywordList.Clear();
                foreach (var e in data.Modifiers)
                {
                    var keywords = m_EffectKeywords[e.Key];
                    foreach (var keyword in keywords)
                        m_KeywordList.Add(keyword);
                }

                foreach (var k in m_EffectKeywords[effect])
                    m_KeywordList.Add(k);

                // 设置材质
                var mat = GetMaterial(m_KeywordList, rawMat);
                renderer.sharedMaterial = mat;

                // 应用效果
                modifier = (EffectModifierBase)Activator.CreateInstance(m_EffectModifiers[effect]);
                modifier.Apply(renderer, args);
                data.Modifiers.Add(effect, modifier);
            }
        }

        // 移除效果
        public void RemoveEffect(string effect, Renderer renderer)
        {
            int rendererId = renderer.GetInstanceID();

            RendererData data;
            if (!m_RendererDataTable.TryGetValue(rendererId, out data))
                // 渲染器不存在。返回
                return;

            EffectModifierBase modifier;
            if (!data.Modifiers.TryGetValue(effect, out modifier))
                // 效果不存在，返回
                return;

            // 移除效果
            data.Modifiers.Remove(effect);
            modifier.Remove(renderer);

            // 取出所有关键字
            m_KeywordList.Clear();
            foreach (var e in data.Modifiers)
            {
                var keywords = m_EffectKeywords[e.Key];
                foreach (var keyword in keywords)
                    m_KeywordList.Add(keyword);
            }

            // 设置材质
            var mat = GetMaterial(m_KeywordList, data.RawMaterial);
            renderer.sharedMaterial = mat;
        }

        // 移除所有效果
        public void RemoveAllEffects(Renderer renderer)
        {
            RendererData data;
            if (!m_RendererDataTable.TryGetValue(renderer.GetInstanceID(), out data))
                // 渲染器不存在。返回
                return;

            foreach (var modifier in data.Modifiers)
                modifier.Value.Remove(renderer);

            data.Modifiers.Clear();
            renderer.sharedMaterial = data.RawMaterial;
            renderer.SetPropertyBlock(m_EmptyMPB);
        }

        // 获取材质
        private Material GetMaterial(List<string> keywordList, Material material)
        {
            if (keywordList.Count == 0)
                // 没有关键字，返回原始材质
                return material;

            keywordList.Sort();

            m_StringBuilder.Length = 0;
            m_StringBuilder.Append(keywordList[0]);
            for (int i = 1; i < keywordList.Count; ++i)
            {
                if (keywordList[i] != keywordList[i - 1])
                    m_StringBuilder.Append(keywordList[i]);
            }

            var keywords = m_StringBuilder.ToString();

            int matId = material.GetInstanceID();
            Dictionary<int, Material> matTable;
            if (m_KeywordsMaterials.TryGetValue(keywords, out matTable))
            {
                Material mat;
                if (matTable.TryGetValue(matId, out mat))
                    return mat;
            }
            else
                matTable = new Dictionary<int, Material>();

            Material newMat = new Material(material);
            matTable.Add(matId, newMat);
            return newMat;
        }

        // 更新
        private void OnUpdate()
        {
            foreach (var modifier in m_ModifiersToUpdate)
            {
                if (!modifier.Update())
                    m_ModifiersToStopUpdate.Add(modifier);
            }

            if (m_ModifiersToStopUpdate.Count > 0)
            {
                int j = 0, k = 0;
                for (int i = 0; i < m_ModifiersToUpdate.Count; ++i)
                {
                    if (j == m_ModifiersToStopUpdate.Count ||
                        m_ModifiersToUpdate[i] != m_ModifiersToStopUpdate[j])
                    {
                        m_ModifiersToUpdate[k++] = m_ModifiersToUpdate[i];
                        continue;
                    }

                    ++j;
                }

                Assert.IsTrue(j == m_ModifiersToStopUpdate.Count);
                m_ModifiersToUpdate.RemoveRange(k, m_ModifiersToUpdate.Count - k);
                m_ModifiersToStopUpdate.Clear();
            }
        }
    }
}
