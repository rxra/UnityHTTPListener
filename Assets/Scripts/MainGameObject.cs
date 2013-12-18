using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class MainGameObject : MonoBehaviour {
	
	public List<string> objects;
	
	private List<Action> _actions = new List<Action>();
	private List<Action> _currentActions = new List<Action>();
	
	private static MainGameObject s_Instance = null;
	public static MainGameObject Instance()
	{
		return s_Instance;
	}
	
	public static void QueueOnMainThread(Action action)
	{
		lock (s_Instance._actions)
		{
			s_Instance._actions.Add(action);
		}
	}
	
	public string ObjectByID(int idx)
	{
		if (idx<0 || idx>=objects.Count) {
			return null;
		}
		
		return objects[idx];
	}
	
	void Awake () {
		s_Instance = this;
	}
		
	// Update is called once per frame
	void Update () {
	
		lock (_actions)
		{
			_currentActions.Clear();
			_currentActions.AddRange(_actions);
			_actions.Clear();
		}
		foreach(var a in _currentActions)
		{
			a();
		}
		
	}
}
