using System;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI
{
    [ExecuteAlways]
    public class UiTextProjectInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text _productNameText;
        
        [SerializeField] private TMP_Text _companyNameText;
        [SerializeField] private bool _appendYearToCompanyName = true;
        
        [SerializeField] private TMP_Text _versionNumberText;
        [SerializeField] private string _versionNumberPrefix;

        
        private void Awake()
        {
            UpdateText();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateText();
        }
        #endif
        
        private void UpdateText()
        {
            if (!_productNameText.SafeIsUnityNull())
            {
                _productNameText.text = $"{Application.productName}";
            }
            if (!_companyNameText.SafeIsUnityNull())
            {
                _companyNameText.text = GetCompanyNameString();
            }
            if (!_versionNumberText.SafeIsUnityNull())
            {
                _versionNumberText.text = $"{_versionNumberPrefix}{Application.version}";
            }
        }

        private string GetCompanyNameString()
        {
            var currentDate = DateTime.Now;
            return _appendYearToCompanyName
                ? $"{Application.companyName} {currentDate.Year}"
                : Application.companyName;
        }
    }
}