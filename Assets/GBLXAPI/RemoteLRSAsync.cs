// -------------------------------------------------------------------------------------------------
// RemoteLRSAsync.cs
// Project: GBLXAPI
// Created: 2017/05/30
// Copyright 2017 Dig-It! Games, LLC. All rights reserved.
// This code is licensed under the MIT License (see LICENSE.txt for details)
//
// NOTE:
// This is a slim async version of RemoteLRS.cs for WebGL that only saves statements
// There is a desktop/mobile version that uses threading and the full RemoteLRS to make async.
// -------------------------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Text;	// encoding
using System.Collections;	// coroutines
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TinCan;
using TinCan.Json;

namespace TinCan {

	// ------------------------------------------------------------------------
	// ------------------------------------------------------------------------
	public class RemoteLRSAsync : MonoBehaviour {

		// config
		public string endpoint { get; set; }
		public TCAPIVersion version { get; set; }
		public String auth { get; set; }

		// state
		public bool complete { get; set; }
		public bool success { get; set; }
		public string response { get; set; }

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void initLRS(String endpoint, String username, String password){

			this.endpoint = endpoint;
			// endpoint should have trailing /
			if (this.endpoint[this.endpoint.Length - 1] != '/'){
				this.endpoint += "/";
			}
			this.version = TCAPIVersion.latest();
			this.auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

			this.initState();
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void initState(){

			this.complete = false;
			this.success = false;
			this.response = "";
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void SaveStatement(Statement statement){

			// reinit state
			this.initState();

			// post header
			Dictionary<string,string> postHeader = new Dictionary<string, string>();
			postHeader.Add("Content-Type", "application/json");
			postHeader.Add("X-Experience-API-Version", this.version.ToString());
			postHeader.Add("Authorization", auth);

			// form data
			byte[] formBytes = Encoding.UTF8.GetBytes(statement.ToJSON(this.version));

			// https://learninglocker.dig-itgames.com/data/xAPI/statements?statementId=58098b7c-3353-4f9c-b812-1bddb08876fd
			string queryURL = this.endpoint + "statements";

			/*
			// debug
			foreach (string key in postHeader.Keys){
				string val = postHeader[key];
				Debug.Log(key + ": " + val);
			}
			Debug.Log(statement.ToJSON());
			Debug.Log("Starting WWW: " + queryURL + " FORM:" + Encoding.UTF8.GetString(formBytes));
			*/

			// post via www
			WWW www = new WWW(queryURL, formBytes, postHeader);
			StartCoroutine(WaitForRequest(www));
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		IEnumerator WaitForRequest(WWW data){

			yield return data; // Wait until the download is done

			// ok
			if (data.error == null){
				this.success = true;
				JArray ids = JArray.Parse(data.text);
				this.response = ids[0].ToString();
			}
			// fail
			else {
				this.success = false;
				this.response = data.error;
			}

			// finished
			this.complete = true;
		}
	}
}
