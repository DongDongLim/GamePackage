using System;
using UnityEditor;
using UnityEngine;

namespace Packages.ExcelParserPack.Editor
{
    public class TableTool : EditorWindow
    {
        public static string DirectorySaveTable => $"{Application.dataPath}/Table";
        public static string DirectoryLoadTable => $"{Application.dataPath}/../Table/";
        public static string DirectoryCreateTableScript => $"{Application.dataPath}/Scripts/Table/Tables";
        public static string DirectoryCreateTableKeyName => $"{Application.dataPath}/Scripts/Table/TableKeyName";
        public const string KeyName = "keyname";

        private TableBake _tableBake;
        private TableBake TableBake => _tableBake ??= new TableBake();
        private Vector2 _scrollPosition = Vector2.zero;
        private string _className;

        [MenuItem("GameTools/TableTool")]
        private static void Init()
        {
            GetWindow<TableTool>(false, "TableTool");
        }

        private void OnGUI()
        {
            ToolBarDraw();
            
            GUILayout.BeginVertical();
            
            GUILayout.Label("Table List", GUILayout.Width(100));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true), GUILayout.Width(250));
            TableCreateDraw();
            TableListDraw();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void ToolBarDraw()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
        
            if (GUILayout.Button("Open Excel", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                if (!System.IO.Directory.Exists(DirectoryLoadTable))
                {
                    System.IO.Directory.CreateDirectory(DirectoryLoadTable);
                }
                Application.OpenURL(DirectoryLoadTable);
            }
        
            if(GUILayout.Button("All Bake", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                AllBake();
            }
            GUILayout.EndHorizontal();
        }

        private void TableCreateDraw()
        {
            GUILayout.BeginHorizontal();
            _className = EditorGUILayout.TextField(_className);
            if (GUILayout.Button("CREATE", GUILayout.Width(60)))
            {
                TableBake.CreateTableScript(_className);
            }

            GUILayout.EndHorizontal();
        }

        private void TableListDraw()
        {
            foreach (var tableFileBake in TableBake.BakeList)
            {
                if (tableFileBake == null)
                {
                    continue;
                }
                
                var className = tableFileBake.ClassName.ToLower();
                if (!string.IsNullOrWhiteSpace(_className) && !className.Contains(_className.ToLower()))
                {
                    continue;
                }
                
                var isCheck = PlayerPrefs.GetInt(tableFileBake.ClassName, 0) == 1;
                var dest = tableFileBake.ClassName;
                var prevCheck = isCheck;
                var isToggleCheck = GUILayout.Toggle(isCheck, dest, GUILayout.ExpandWidth(true));
                
                if(isToggleCheck != prevCheck)
                {
                    GUIUtility.keyboardControl = 0;
                }
                
                if (isToggleCheck)
                {
                    PlayerPrefs.SetInt(tableFileBake.ClassName, 1);
                    continue;
                }
                
                PlayerPrefs.SetInt(tableFileBake.ClassName, 0);
            }
        }
    
        private void AllBake()
        {
            TableBake.BakeAll();
        }
    }
}
    