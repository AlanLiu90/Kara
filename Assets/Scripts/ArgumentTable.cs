// Created by AlanLiu Sep/23/2017
// 参数表

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kara
{
    public class ArgumentTable
    {
        public enum ArgumentType
        {
            Int,
            Float,
            String,
            Color,
            Vector2,
            Vector3,
            Vector4,
        }

        [SerializeField]
        public struct Argument
        {
            public ArgumentType Type;
            public string Name;
            public object Value;
        }

        public Argument[] Arguments;
        private Dictionary<string, Argument> m_Table;

        public void OnAfterDeserialize()
        {
            m_Table = new Dictionary<string, Argument>();

            foreach (var arg in Arguments)
            {
                m_Table.Add(arg.Name, arg);
            }
        }

        public int GetInt(string name, int def = 0)
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.Int)
                    return (int)arg.Value;
            }

            return def;
        }

        public float GetFloat(string name, float def = 0.0f)
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.Float)
                    return (float)arg.Value;
            }

            return def;
        }

        public string GetString(string name, string def = "")
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.String)
                    return (string)arg.Value;
            }

            return def;
        }

        public Color GetColor(string name, Color def = default(Color))
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.String)
                    return (Color)arg.Value;
            }

            return def;
        }

        public Vector2 GetVector2(string name, Vector2 def = default(Vector2))
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.Vector2)
                    return (Vector2)arg.Value;
            }

            return def;
        }

        public Vector3 GetVector3(string name, Vector3 def = default(Vector3))
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.Vector3)
                    return (Vector3)arg.Value;
            }

            return def;
        }

        public Vector4 GetVector4(string name, Vector4 def = default(Vector4))
        {
            Argument arg;
            if (m_Table.TryGetValue(name, out arg))
            {
                if (arg.Type == ArgumentType.Vector4)
                    return (Vector4)arg.Value;
            }

            return def;
        }
    }
}
