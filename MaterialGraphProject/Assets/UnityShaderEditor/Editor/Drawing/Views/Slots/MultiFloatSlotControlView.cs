﻿using System;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Graphing;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.ShaderGraph.Drawing.Slots
{
    public class MultiFloatSlotControlView : VisualElement
    {
        readonly INode m_Node;
        readonly Func<Vector4> m_Get;
        readonly Action<Vector4> m_Set;
        int m_UndoGroup = -1;

        public MultiFloatSlotControlView(INode node, int components, Func<Vector4> get, Action<Vector4> set)
        {
            m_Node = node;
            m_Get = get;
            m_Set = set;
            var initialValue = get();
            AddField(initialValue, 0, "X");
            if (components > 1)
                AddField(initialValue, 1, "Y");
            if (components > 2)
                AddField(initialValue, 2, "Z");
            if (components > 3)
                AddField(initialValue, 3, "W");
        }

        void AddField(Vector4 initialValue, int index, string subLabel)
        {
            Add(new Label(subLabel));
            var doubleField = new DoubleField { userData = index, value = initialValue[index] };
            doubleField.OnValueChanged(evt =>
            {
                var value = m_Get();
                value[index] = (float)evt.newValue;
                m_Set(value);
                if (m_Node.onModified != null)
                    m_Node.onModified(m_Node, ModificationScope.Node);
                m_UndoGroup = -1;
            });
            doubleField.RegisterCallback<InputEvent>(evt =>
            {
                if (m_UndoGroup == -1)
                {
                    m_UndoGroup = Undo.GetCurrentGroup();
                    m_Node.owner.owner.RegisterCompleteObjectUndo("Change " + m_Node.name);
                }
                float newValue;
                if (!float.TryParse(evt.newData, out newValue))
                    newValue = 0f;
                var value = m_Get();
                if (Math.Abs(value[index] - newValue) > 1e-9)
                {
                    value[index] = newValue;
                    m_Set(value);
                    if (m_Node.onModified != null)
                        m_Node.onModified(m_Node, ModificationScope.Node);
                }
            });
            doubleField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape && m_UndoGroup > -1)
                {
                    Undo.RevertAllDownToGroup(m_UndoGroup);
                    m_UndoGroup = -1;
                    evt.StopPropagation();
                }
                Dirty(ChangeType.Repaint);
            });
            Add(doubleField);
        }
    }
}
