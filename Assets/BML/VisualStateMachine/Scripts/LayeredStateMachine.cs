using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.VisualStateMachine.Scripts.Nodes;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
namespace BML.VisualStateMachine.Scripts
{
    public class LayeredStateMachine : MonoBehaviour
{
    [SerializeField] protected StateMachineGraph[] stateMachines;

    protected Dictionary<StateNode, StateMachineGraph> stateNodeDict = new Dictionary<StateNode, StateMachineGraph>();


    #region LifeCycle Methods

    protected virtual void Awake()
    {
        InitStateMachines(true);
        stateMachines.ForEach(s => s.Awaken());
    }

    protected virtual void Update()
    {
        HandleTransitions();
        ExecuteUpdates();
    }

    protected virtual void FixedUpdate()
    {
        ExecuteFixedUpdates();
    }

    protected virtual void ExecuteUpdates()
    {
        stateMachines.ForEach(s => s.ExecuteUpdates());
    }

    protected virtual void ExecuteFixedUpdates()
    {
        stateMachines.ForEach(s => s.ExecuteFixedUpdates());
    }

    private void OnValidate()
    {
        stateMachines.ForEach(s => s.InjectDependencies(this));
    }

    protected void OnDrawGizmos()
    {
        stateMachines.ForEach(s => s.DrawGizmos());
    }

    protected void OnApplicationQuit()
    {
        stateMachines.ForEach(s => s.OnApplicationExit());
    }

    protected void OnDestroy()
    {
        stateMachines.ForEach(s => s.OnDestroy());
    }

    #endregion

    #region Init/Dep Injection

    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Large, Name = "InitStateMachines")]
    public virtual void InitStateMachinesEditor()
    {
        InitStateMachines(false);
        stateMachines.ForEach(i => i.PopulateNodeLists());
    }

    public virtual void InitStateMachines(bool isRuntime)
    {
        stateNodeDict.Clear();

        //Loop through and init nodes
        foreach(var stateMachine in stateMachines)
        {
            stateMachine.InjectDependencies(this);
            stateMachine.PopulateNodeLists();
            PopulateStateNodeDict(stateMachine);
            InjectNodeDependencies(stateMachine);
        }

        //Actually start machine and send over state nodes dict from other machines
        foreach(var stateMachine in stateMachines)
        {
            stateMachine.StartStateMachine(isRuntime);
        }

        Debug.Log("Finished Initializing State Machines");
    }

    protected virtual void InjectNodeDependencies(StateMachineGraph stateMachine)
    {

    }



    //Populate dictionary of State Nodes and their respective State Machine
    protected virtual void PopulateStateNodeDict(StateMachineGraph stateMachine)
    {
        foreach(var stateNode in stateMachine.stateNodes)
        {
            stateNodeDict.Add(stateNode, stateMachine);
        }
    }


    #endregion

    //Send requesting state machine list of active states
    //For transition node valid start states
    public virtual List<StateNode> GetActiveStates(StateMachineGraph requestingStateMachine)
    {
        List<StateNode> activeStates = new List<StateNode>();
        foreach(var stateMachine in stateMachines)
        {
            foreach(var currentState in stateMachine.currentStates)
            {
                activeStates.Add(currentState);
            }
        }

        return activeStates;
    }

    private void HandleTransitions()
    {
        foreach(var stateMachine in stateMachines)
        {
            stateMachine.CheckForValidTransitions();
        }
        foreach(var stateMachine in stateMachines)
        {
            stateMachine.ApplyValidTransitions();
        }
    }

    //Return true if invalid state is in any of its state machine's active states
    public virtual bool CheckInvalidStateActive(List<StateNode> invalidStates)
    {
        Dictionary<StateNode, StateMachineGraph> invalidStateDict = new Dictionary<StateNode, StateMachineGraph>();

        //Populate dict of statemachine and its invalid state
        invalidStates.ForEach(s => {
            if(!stateNodeDict.ContainsKey(s))
                Debug.LogError($"Trying to check invalid start state, {s.name} ,that is not part of state machines!");
            if(!invalidStateDict.ContainsKey(s))
                invalidStateDict.Add(s, stateNodeDict[s]);
        });

        bool result = false;

        //For each statemachine, check that at least 1 (OR) invalid state is active
        foreach(var stateMachine in stateMachines)
        {
            bool isInvalidStateActive = false;
            foreach(var pair in invalidStateDict)
            {
                if(pair.Value == stateMachine)
                {
                    if(stateMachine.currentStates.Contains(pair.Key))
                    {
                        isInvalidStateActive = true;
                        break;
                    }
                }
            }

            result |= isInvalidStateActive;
        }

        return result;
    }

    #region Debug

    #endregion

}
}
