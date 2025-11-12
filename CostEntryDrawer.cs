using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(CostEntry))]
    public class CostEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the label on the left
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            // Adjust position for the fields
            float fieldStartX = position.x + EditorGUIUtility.labelWidth;
            float fieldWidth = position.width - EditorGUIUtility.labelWidth;
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Draw each ResourceAmount on a new line
            SerializedProperty metalAmount =
                property.FindPropertyRelative("metalAmount").FindPropertyRelative("Amount");
            SerializedProperty gasAmount = property.FindPropertyRelative("gasAmount").FindPropertyRelative("Amount");
            SerializedProperty scienceAmount =
                property.FindPropertyRelative("scienceAmount").FindPropertyRelative("Amount");

            Rect metalRect = new Rect(fieldStartX, position.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            Rect gasRect = new Rect(fieldStartX, position.y + lineHeight, fieldWidth,
                EditorGUIUtility.singleLineHeight);
            Rect scienceRect = new Rect(fieldStartX, position.y + 2 * lineHeight, fieldWidth,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(metalRect, metalAmount, new GUIContent("Metal Amount"));
            EditorGUI.PropertyField(gasRect, gasAmount, new GUIContent("Gas Amount"));
            EditorGUI.PropertyField(scienceRect, scienceAmount, new GUIContent("Science Amount"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calculate height for three lines plus spacing
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
        }
    }
}
