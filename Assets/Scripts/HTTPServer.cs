using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;


public class HTTPServer : MonoBehaviour {

	public delegate void RequestHandler (Match match, HttpListenerResponse response);

	private Dictionary<Regex,RequestHandler> _requestHandlers = new Dictionary<Regex, RequestHandler>();

	void Awake () {
		// create the dictionnary of Regex and RequestHandler
		_requestHandlers[new Regex(@"^/characters$")] = HandleCharacters;
		_requestHandlers[new Regex(@"^/character/(\d+)$")] = HandleCharacter;
	}

	// Use this for initialization
	void Start () {
 
		_listener = new HttpListener();
		_listener.Prefixes.Add ("http://127.0.0.1:8080/");

		_listener.Start();

		_listener.BeginGetContext(new AsyncCallback(ListenerCallback),_listener);
	}
	
	void Destroy () {
		if (_listener!=null) {
			_listener.Close();
		}
	}

	private static void HandleCharacter(Match match, HttpListenerResponse response)
	{
		// here we are running in a thread different from the main thread
		int pid = Convert.ToInt32(match.Groups[1].Value);
		
		string responseString = "";

		// event used to wait the answer from the main thread.
		AutoResetEvent autoEvent = new AutoResetEvent(false);

		// var to store the character we are looking for
		Character character = null;
		// this bool is to check character is valid ... explanation below
		bool found = false;

		// we queue an 'action' to be executed in the main thread
		MainGameObject.QueueOnMainThread(()=>{
			// here we are in the main thread (see Update() in MainGameObject.cs)
			// retrieve the character
			character = MainGameObject.Instance().CharacterByID(pid);
			// if the character is null set found to false
			// have to do this because cannot call "character==null" elsewhere than the main thread
			// do not know why (yet?)
			// so if found this "trick"
			found = (character!=null?true:false);
			// set the event to "unlock" the thread
			autoEvent.Set();
		});

		// wait for the end of the 'action' executed in the main thread
		autoEvent.WaitOne();

		// generate the HTTP answer

		if (found==false) {
			responseString = "<html><body>character: not found (" + pid + ")</body></html>";
		} else {
			responseString = "<html><body>";
			responseString += "<img src='data:image/jpg;base64," + character.imageB64 + "'></img></br>";
			responseString += "name: " +  character.name + "</br>";
			responseString += "life: " +  character.life + "</br>";
			responseString += "streght " +  character.streght + "</br>";
			responseString += "dexterity " +  character.dexterity + "</br>";
			responseString += "consitution " +  character.consitution + "</br>";
			responseString += "intelligence " +  character.intelligence + "</br>";
			responseString += "</body></html>";
		}
		
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
		// Get a response stream and write the response to it.
		response.ContentLength64 = buffer.Length;
		System.IO.Stream output = response.OutputStream;
		output.Write(buffer,0,buffer.Length);
		// You must close the output stream.
		output.Close();
	}

	private static void HandleCharacters(Match match, HttpListenerResponse response)
	{
		string responseString = "<html><body><div align='center'>";

		foreach (Character c in MainGameObject.Instance().characters) {
			responseString += "<p><a href='/character/" + c.cid + "'><img src=\"data:image/jpg;base64," + c.imageB64 + "\"></img></br>" + c.name + "</br></a></p>";
		}

		responseString += "</div></body></html>";
		
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
		// Get a response stream and write the response to it.
		response.ContentLength64 = buffer.Length;
		System.IO.Stream output = response.OutputStream;
		output.Write(buffer,0,buffer.Length);
		// You must close the output stream.
		output.Close();
	}
	
	private void ListenerCallback(IAsyncResult result)
	{
    	HttpListener listener = (HttpListener) result.AsyncState;
    	// Call EndGetContext to complete the asynchronous operation.
    	HttpListenerContext context = listener.EndGetContext(result);
    	HttpListenerRequest request = context.Request;
    	// Obtain a response object.
    	HttpListenerResponse response = context.Response;

		foreach (Regex r in _requestHandlers.Keys) {
			Match m = r.Match(request.Url.AbsolutePath);
			if (m.Success) {
				(_requestHandlers[r])(m,response);
				_listener.BeginGetContext(new AsyncCallback(ListenerCallback),_listener);
				return;
			}
		}

		response.StatusCode = 404;
		response.Close();
	}
	
	HttpListener _listener;
	
}
