using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mtree
{
    public class TreeFunctionAsset : ScriptableObject
    {
        public int seed;

        public static int positionOffset = 30; // offset in x position from parent
        public static int height = 37;
        public static int width = 200;
        public static int margin = -3;
        public static int deleteButtonSize = 18;

        public static GUIStyle nodeStyle;
        public static GUIStyle selectedNodeStyle;
        public static GUIStyle deleteButtonStyle;

        public int id;

        public TreeFunctionAsset parent;
        public List<TreeFunctionAsset> chiildren;
        public bool enabled = true;
        public bool showDeleteButton = true;

        public Rect rect;
        public Rect deleteRect;

        public virtual void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            this.parent = parent;
            if (parent != null)
                parent.chiildren.Add(this);
            chiildren = new List<TreeFunctionAsset>();
            id = System.Guid.NewGuid().GetHashCode();
            seed = Random.Range(0, 10000);
        }

        public virtual void Execute(MTree tree)
        {
            if (!enabled || (parent != null && !parent.enabled))
                return;
        }
        
        public void DrawFunction(bool isSelected = false, bool deleteButton = true)
        {
        #if UNITY_EDITOR
            showDeleteButton = deleteButton;
            GUIStyle style = isSelected ? selectedNodeStyle : nodeStyle;
            if (style == null)
                UpdateStyle();
            GUI.Box(rect, name, style);

            if (deleteButton)
                GUI.Button(deleteRect, "", deleteButtonStyle);

            if (parent != null)
            {
                Vector2 start_pos = parent.rect.position + new Vector2(6, height * 6 / 7);
                Vector2 end_pos = rect.position + new Vector2(6, height / 2);
                UnityEditor.Handles.DrawBezier(start_pos, end_pos, start_pos + Vector2.up * 15, end_pos + Vector2.left * 15, Color.white, null, 4f);
            }
        #endif
        }

        public void UpdateRectRec(ref int heightIndex, int marginIndex)
        {
            float x = marginIndex * positionOffset;
            float y = heightIndex * (height + margin);
            rect.Set(x, y, width, height);
            if (showDeleteButton)
                deleteRect.Set(x + width - 12 - deleteButtonSize, y + (height - deleteButtonSize) / 2, deleteButtonSize, deleteButtonSize);
            else
                deleteRect.Set(0, 0, 0, 0);
            foreach (TreeFunctionAsset child in chiildren)
            {
                heightIndex++;
                child.UpdateRectRec(ref heightIndex, marginIndex + 1);
            }
        }

        public virtual void DrawProperties() {}

        private static void UpdateStyle()
        {
        #if UNITY_EDITOR
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = Resources.Load("Mtree/Sprites/TreeFunctionSprite") as Texture2D;
            nodeStyle.border = new RectOffset(20, 10, 10, 10);
            nodeStyle.alignment = TextAnchor.MiddleCenter;
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.fontStyle = FontStyle.Bold;

            selectedNodeStyle = new GUIStyle(nodeStyle);
            selectedNodeStyle.normal.background = Resources.Load("Mtree/Sprites/TreeFunctionSpriteSelected") as Texture2D;

            deleteButtonStyle = new GUIStyle();
            deleteButtonStyle.normal.background = Resources.Load("Mtree/Sprites/DeleteCrossSprite") as Texture2D;
        #endif
        }

    }
}