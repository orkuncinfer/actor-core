using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine; 

[CreateAssetMenu]
public  class BE_State : ScriptableObject
{
   public List<Transition> Transitions = new List<Transition>();

   public StateActions[] Actions;

   public void OnEnter(StateManager states,StateActions[] actions)
   {
      for (int i = 0; i < actions.Length; i++)
      {
         actions[i].OnEnter();
         actions[i].Manager = states;
      }
   }

   public void OnExit(StateManager states,StateActions[] actions)
   {
      for (int i = 0; i < actions.Length; i++)
      {
         actions[i].OnExit();
         actions[i].Manager = states;
      }
   }

   public void OnTick(StateManager states, StateActions[] actions)
   {
      for (int i = 0; i < actions.Length; i++)
      {
         actions[i].OnTick();
         actions[i].Manager = states;
      }
   }

  

   public Transition AddTransition()
   {
      Transition retVal = new Transition();
      Transitions.Add(retVal);
      return retVal;
   }
}