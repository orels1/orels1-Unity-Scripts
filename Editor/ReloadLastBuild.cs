using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace orels1EScripts {
  public class ReloadLastBuild : Editor {
    [MenuItem("Tools/Reload Last Build")]
    public static void RunReloadLastBuild() {
      var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      path = Path.Combine(path, @"..\LocalLow\VRChat\VRChat\Worlds");
      try {
        var lastWorldFile = new DirectoryInfo(path).GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
        if (lastWorldFile == null) {
          Debug.Log("[ReloadLastBuild]: Failed to find world file. Have you built before?");
          return;
        }

        File.SetLastWriteTime(lastWorldFile.FullName, DateTime.Now);
        Debug.Log($"[ReloadLastBuild]: Reloaded {lastWorldFile.Name}");
      }
      catch (DirectoryNotFoundException e) {
        Debug.Log("[ReloadLastBuild]: Failed to find world file. Have you built before?");
        Debug.LogError(e);
      }
      catch (Exception e) {
        Debug.Log("[ReloadLastBuild]: Encountered an error");
        Debug.LogError(e);
      }
    }
  }
}
