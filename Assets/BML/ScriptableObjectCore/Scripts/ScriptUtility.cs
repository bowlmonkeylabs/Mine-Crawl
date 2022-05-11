namespace BML.ScriptableObjectCore.Scripts
{
    // Decompiled with JetBrains decompiler
// Type: Raskulls.ScriptableSystemEditor.ScriptUtility
// Assembly: ScriptableSystemEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 993696AB-263C-4C4A-A2A0-37BF89B79867
// Assembly location: C:\Users\Gabriel\Documents\UnityProjects\BMLAssetRefactor\Assets\Raskulls\ScriptableSystem\Editor\ScriptableSystemEditor.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

  public class ScriptUtility
  {
    public static string RemoveCommentsFromScript(string script)
    {
      string str1 = "/\\*(.*?)\\*/";
      string str2 = "//(.*?)\\r?\\n";
      string str3 = "\"((\\\\[^\\n]|[^\"\\n])*)\"";
      string str4 = "@(\"[^\"]*\")+";
      return Regex.Replace(script, str1 + "|" + str2 + "|" + str3 + "|" + str4, (MatchEvaluator) (me =>
      {
        if (!me.Value.StartsWith("/*") && !me.Value.StartsWith("//"))
          return me.Value;
        return !me.Value.StartsWith("//") ? "" : Environment.NewLine;
      }), RegexOptions.Singleline);
    }

    public static string GetScriptableObjectAssetName(ScriptableObject obj) => (obj).name;
  }
}