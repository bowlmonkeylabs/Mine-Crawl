using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BML.Scripts.Tasks
{
    [TaskCategory("Unity/Time")]
    [TaskDescription("Check if the time between given time and current time is less than provided delta")]
    public class CompareElapsedTime : Conditional
    {
        public SharedFloat _timeToCheck;
        public SharedFloat _deltaTime;
        
        public override TaskStatus OnUpdate()
        {
            if (_timeToCheck.Value + _deltaTime.Value > Time.time)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}