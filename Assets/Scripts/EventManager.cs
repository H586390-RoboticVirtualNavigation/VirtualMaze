using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

	private Dictionary<string, UnityEvent> eventDictionary;
	private static EventManager _eventManager;
	public static EventManager eventManager {
		get{
			if(!_eventManager){
				_eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
				if(!_eventManager){
					Debug.LogError("There needs to be one active EventManager script on a GameObject");
				}else{
					_eventManager.Init();
				}
			}
			return _eventManager;
		}
	}
	
	void Init(){
		if (eventDictionary == null) {
			eventDictionary = new Dictionary<string, UnityEvent>();	
		}
	}

	public static void StartListening (string eventNameToListen, UnityAction listener){

		UnityEvent thisEvent = null;
		if (eventManager.eventDictionary.TryGetValue (eventNameToListen, out thisEvent)) {
			thisEvent.AddListener (listener);
		} else {
			thisEvent = new UnityEvent();
			thisEvent.AddListener(listener);
			eventManager.eventDictionary.Add(eventNameToListen,thisEvent);
		}
	}

	public static void StopListening (string eventNameToStopListening, UnityAction listener){

		UnityEvent thisEvent = null;
		if(eventManager.eventDictionary.TryGetValue(eventNameToStopListening, out thisEvent)){
			thisEvent.RemoveListener(listener);
		}
	}

	public static void TriggerEvent(string eventNameToTrigger){
		UnityEvent thisEvent = null;
		if (eventManager.eventDictionary.TryGetValue (eventNameToTrigger, out thisEvent)) {
			thisEvent.Invoke();
		}
	}
}








































