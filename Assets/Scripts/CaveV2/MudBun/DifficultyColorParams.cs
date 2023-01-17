using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.CaveV2.MudBun
{
    [CreateAssetMenu(fileName = "DifficultyColorParams", menuName = "BML/DifficultyColorParams", order = 0)]
    public class DifficultyColorParams : ScriptableObject
    {
        public List<Color> DifficultyColorList;
    }
}