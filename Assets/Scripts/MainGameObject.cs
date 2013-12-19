using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

[System.Serializable]
public class Character
{
	public int cid;
	public string name;
	public Texture2D image;
	public int life;
	public int streght;
	public int dexterity;
	public int consitution;
	public int intelligence;

	[HideInInspector]
	public string imageB64
	{
		get
		{
			return _imageB64;
		}
	}

	public string _imageB64;
}

public class MainGameObject : MonoBehaviour {
	
	public List<Character> characters;
	
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
	
	public Character CharacterByID(int id)
	{
		foreach (Character c in characters) {
			if (c.cid==id) {
				return c;
			}
		}
		return null;
	}
	
	void Awake () {
		s_Instance = this;
		foreach (Character c in MainGameObject.Instance().characters) {
			Debug.Log(c.cid);
			if (c._imageB64==null || c._imageB64.Length==0) {
				byte[] data = c.image.EncodeToPNG();
				c._imageB64 = Convert.ToBase64String(data);
			}
		}
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
