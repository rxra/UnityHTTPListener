using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;


public class HTTPServer : MonoBehaviour {
	
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
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private void ListenerCallback(IAsyncResult result)
	{
    	HttpListener listener = (HttpListener) result.AsyncState;
    	// Call EndGetContext to complete the asynchronous operation.
    	HttpListenerContext context = listener.EndGetContext(result);
    	HttpListenerRequest request = context.Request;
    	// Obtain a response object.
    	HttpListenerResponse response = context.Response;

		System.Text.RegularExpressions.Regex myRegex = new Regex(@"^/object/(\d+)$");
		Match m = myRegex.Match(request.Url.AbsolutePath);
		if (!m.Success) {
			response.OutputStream.Close();
			return;
		}
		
		int pid = Convert.ToInt32(m.Groups[1].Value);
		
		string responseString = "";
		
		AutoResetEvent autoEvent = new AutoResetEvent(false);
		string objectName = null;
		
		MainGameObject.QueueOnMainThread(()=>{
			objectName = MainGameObject.Instance().ObjectByID(pid);
			autoEvent.Set();
        });
		
		autoEvent.WaitOne();
		
		if (objectName==null) {
			responseString = "<html><body>object: not found (" + pid + ")</body></html>";
		} else {
			responseString = "<html><body>object: " +  objectName + " (" + pid + ")</body></html>";
		}

		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
    	// Get a response stream and write the response to it.
    	response.ContentLength64 = buffer.Length;
    	System.IO.Stream output = response.OutputStream;
    	output.Write(buffer,0,buffer.Length);
    	// You must close the output stream.
    	output.Close();
		
		_listener.BeginGetContext(new AsyncCallback(ListenerCallback),_listener);
	}
	
	HttpListener _listener;
	
}
