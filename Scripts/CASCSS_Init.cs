#if UNITY_EDITOR
// using System.Collections;
// using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace com.NovaVoidHowl.dev.unity.CASCSS
{
  [InitializeOnLoad]
  public class CASCSS_Init
  {
    static CASCSS_Init()
    {
      AddSymbolIfNeeded();
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
      AddSymbolIfNeeded();
    }

    private static void AddSymbolIfNeeded()
    {
      string symbol = "NVH_CASCSS_EXISTS";
      string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup
      );
      if (!defines.Contains(symbol))
      {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
          EditorUserBuildSettings.selectedBuildTargetGroup,
          (defines + ";" + symbol)
        );
        Debug.Log("[CASCSS_Init] Added NVH_CASCSS_EXISTS Scripting Symbol.");
      }
    }
  }
}
#endif