#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace SpearClimb.Editor
{
    public class VisualDebuggerWindow : EditorWindow
    {
        private int mSelectedEventIndex;
        private int mSelectedComponentIndex;
        private int mSelectedMethodIndex;

        [MenuItem("Custom windows/Visual Debugger")]
        public static void MenuItemVisualDebuggerWindow()
        {
            ShowVisualDebuggerWindow();
        }

        public static void ShowVisualDebuggerWindow()
        {
            VisualDebuggerWindow wnd = GetWindow<VisualDebuggerWindow>();
            wnd.titleContent.text = "Visual Debugger";
        }


        public void OnGUI()
        {
            EditorGUILayout.LabelField("Visual Debugger");
            
            EditorGUILayout.Space(); EditorGUILayout.Space();

            EditorGUILayout.LabelField("Selected GameObject");

            EditorGUILayout.Space();

            // Show mesh normals
            if (GUILayout.Button("Show mesh normals"))
            {
                foreach(GameObject gameObject in Selection.gameObjects)
                {
                    foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
                    {
                        for (int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
                        {
                            Vector3 vertex = (meshFilter.gameObject.transform.localToWorldMatrix * meshFilter.sharedMesh.vertices[i]);
                            vertex = meshFilter.gameObject.transform.position + vertex;
                            Vector3 normal = meshFilter.gameObject.transform.rotation * meshFilter.sharedMesh.normals[i];
                            normal *= 10.0f;
                            Debug.DrawLine(vertex, vertex + normal, Color.green, 60.0f);
                        }
                    }
                }  
            }


            EditorGUILayout.Space();

            if(Selection.activeGameObject != null && Selection.activeGameObject.GetComponents<Component>().Length > 0)
            {
                GameObject gobj = Selection.activeGameObject;

                List<MonoBehaviour> componentList = new List<MonoBehaviour>();
                List<string> componentNameList = new List<string>();
                foreach (MonoBehaviour comp in gobj.GetComponents<MonoBehaviour>())
                {
                    componentList.Add(comp);
                    componentNameList.Add(comp.name);
                }
                mSelectedComponentIndex = EditorGUILayout.Popup("Component", Math.Min(mSelectedComponentIndex, componentList.Count), componentNameList.ToArray());

                MonoBehaviour selectedComponent = componentList[mSelectedComponentIndex];

                Type objType = selectedComponent.GetType();

                List<MethodInfo> methodInfoList = new List<MethodInfo>();
                List<string> methodInfoNameList = new List<string>();
                foreach (MethodInfo methodInfo in objType.GetMethods())
                {
                    if (methodInfo.GetParameters().Length > 0)
                        continue;
                    methodInfoList.Add(methodInfo);
                    methodInfoNameList.Add(methodInfo.Name);
                }

                mSelectedMethodIndex = EditorGUILayout.Popup("Function name", Math.Min(mSelectedMethodIndex, methodInfoList.Count), methodInfoNameList.ToArray());

                if(GUILayout.Button("Call function"))
                {
                    MethodInfo methodInfo = methodInfoList[mSelectedMethodIndex];
                    methodInfo.Invoke(selectedComponent, new object[]{ });
                }
            }
        
        }




    }
}

#endif //UNITY_EDITOR
