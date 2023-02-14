using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Script.Audio;
using BML.Script.MMFFeedbacks;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts;
using MoreMountains.Tools;
using UnityEngine;

public class SettingsManager : MMPersistentSingleton<SettingsManager>
{
    [SerializeField] private LoadVariables _loadSettings;

    private bool _loaded;

    void OnEnable() {
        if(!_loaded) {
            _loadSettings.Load();
            _loaded = true;
        }
    }
}
