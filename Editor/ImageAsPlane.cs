using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace orels1EScripts {
  public class ImageAsPlane : Editor {
    [MenuItem("GameObject/3D Object/Image as Plane", false, 99999)]
    public static void ShowCreationWindow() {
      ImageAsPlaneWindow.ShowWindow();
    }
  }

  public class ImageAsPlaneWindow : EditorWindow {
    
    [MenuItem("Window/orels1/Image as Plane")]
    public static void ShowWindow() {
      var pos = new Vector2(0, 0);
      var size = new Vector2(350, 160);
      var window = GetWindowWithRect(typeof(ImageAsPlaneWindow), new Rect(pos, size),  true, "Import Image as Plane") as ImageAsPlaneWindow;
      window.Show();
    }

    private Dictionary<string, Material> materialList = new Dictionary<string, Material>();
    private Texture2D sourceTexture;
    private Material sourceMaterial;
    private string objectName = "";
    private bool unlit = true;
    private bool staticFlag = true;

    private void OnGUI() {
      if (materialList.Count == 0) {
        var path = Application.dataPath + "/ImagesAsPlanes";
        if (Directory.Exists(path)) {
          string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] {"Assets/ImagesAsPlanes"});
          foreach (var guid in matGuids) {
            var mat = (Material) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Material));
            var texGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mat.mainTexture));
            materialList.Add(texGuid, mat);
          }
        }
      }
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Image");
      sourceTexture = (Texture2D) EditorGUILayout.ObjectField(sourceTexture, typeof(Texture2D), false);
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.LabelField("Or");
      sourceMaterial = (Material) EditorGUILayout.ObjectField("Material", sourceMaterial, typeof(Material), false);

      objectName = EditorGUILayout.TextField("Object Name", objectName);
      staticFlag = EditorGUILayout.Toggle("Static", staticFlag);
      unlit = EditorGUILayout.Toggle("Unlit", unlit);
      
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup(sourceTexture == null);
      if (GUILayout.Button("Create", GUILayout.Height(30))) {
        CreateImage();
      }
      EditorGUI.EndDisabledGroup();
      if (GUILayout.Button("Clear", GUILayout.Height(30))) {
        sourceMaterial = null;
        sourceTexture = null;
        objectName = "";
        unlit = true;
        staticFlag = true;
      }
      EditorGUILayout.EndHorizontal();
    }

    private void CreateImage() {
      Undo.SetCurrentGroupName("Import Image as Plane");
      int group = Undo.GetCurrentGroup();
      Material mat;
      if (sourceMaterial == null) {
        var texGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sourceTexture));
        if (materialList.ContainsKey(texGuid)) {
          mat = materialList[texGuid];
        }
        else {
          mat = new Material(Shader.Find(unlit ? "Unlit/Texture" : "Standard"));
          var path = Application.dataPath + "/ImagesAsPlanes";
          if (!Directory.Exists(path)) {
            AssetDatabase.CreateFolder("Assets", "ImagesAsPlanes");
          }
          AssetDatabase.CreateAsset(mat, $"Assets/ImagesAsPlanes/Image-{GUID.Generate()}.mat");
          mat.mainTexture = sourceTexture;
          materialList.Add(texGuid, mat);
        }
      }
      else {
        if (sourceMaterial.mainTexture == null) {
          Debug.Log("Material doesn't have a main texture");
          return;
        }
        mat = sourceMaterial;
      }

      GetImageSize((Texture2D) mat.mainTexture, out var w, out var h);
      var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
      Undo.RegisterCreatedObjectUndo(obj, "Created Plane Object");
      Undo.RecordObject(obj, "Edited Plane Object");
      obj.name = objectName.Length == 0 ? "Image" : objectName;
      var col = obj.GetComponent<MeshCollider>();
      DestroyImmediate(col);
      obj.isStatic = staticFlag;
      var trans = obj.transform;
      trans.position = SceneView.lastActiveSceneView.pivot;
      trans.localScale = new Vector3(w * 0.01f, h * 0.01f, 1);
      trans.LookAt(SceneView.lastActiveSceneView.camera.transform.position, Vector3.up);
      trans.Rotate(Vector3.up, 180f);
      trans.Translate(Vector3.back);
      obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
      Undo.CollapseUndoOperations(group);
    }
    
    // from https://forum.unity.com/threads/getting-original-size-of-texture-asset-in-pixels.165295/
    private static bool GetImageSize(Texture2D asset, out int width, out int height) {
      if (asset != null) {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
 
        if (importer != null) {
          object[] args = new object[2] { 0, 0 };
          MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
          mi.Invoke(importer, args);
 
          width = (int)args[0];
          height = (int)args[1];
 
          return true;
        }
      }
 
      height = width = 0;
      return false;
    }
  }
  
}
