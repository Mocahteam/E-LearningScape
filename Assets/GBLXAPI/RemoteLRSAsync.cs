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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinCan;
using TinCan.Json;

namespace TinCan {

	// ------------------------------------------------------------------------
	// ------------------------------------------------------------------------
	public class RemoteLRSAsync : MonoBehaviour {

		// config
		public TCAPIVersion version { get; set; }
        public List<LRSAddress> lrsAddresses;

		// state
		public bool complete { get; set; }
		public bool success { get; set; }
		public string response { get; set; }

        private Queue<string> unsentStatements;
        private bool sendingFromQueue = false;
        private bool connexionFailed = false;
        private float retryTimer = float.MaxValue;
        private byte[] tmpByteArray = null;

        public RemoteLRSAsync()
        {
            unsentStatements = new Queue<string>();
            //Send all unsent data stored
            if (File.Exists("Data/UnsentData.txt"))
            {
                List<string> unsentStoredData = new List<string>(File.ReadAllLines("Data/UnsentData.txt"));
                for(int i = unsentStoredData.Count - 1; i > -1; i--)
                    if (unsentStoredData[i] == "")
                        unsentStoredData.RemoveAt(i);
                File.WriteAllLines("Data/UnsentData.txt", unsentStoredData.ToArray());
                for (int i = 0; i < unsentStoredData.Count; i++)
                {
                    if (unsentStoredData[i] != "")
                        unsentStatements.Enqueue(unsentStoredData[i]);
                }
            }
        }

        private void Update()
        {
            //if there are unsent statement, try to send them every 30 seconds
            if (unsentStatements.Count > 0)
                if (((connexionFailed && Time.time - retryTimer > 30) || !connexionFailed) && !sendingFromQueue)
                {
                    connexionFailed = false;
                    sendingFromQueue = true;
                    tmpByteArray = Encoding.UTF8.GetBytes(unsentStatements.Peek());
                    SaveStatement(tmpByteArray, true);
                }
        }

        // ------------------------------------------------------------------------
        // ------------------------------------------------------------------------
        public void initLRS(List<LRSAddress> addresses){
            
            lrsAddresses = new List<LRSAddress>(addresses);
			this.version = TCAPIVersion.latest();

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
            //send statements to each address in the LRS config file
            foreach(LRSAddress address in lrsAddresses)
            {
                // reinit state
                this.initState();

                // post header
                Dictionary<string, string> postHeader = new Dictionary<string, string>();
                postHeader.Add("Content-Type", "application/json");
                postHeader.Add("X-Experience-API-Version", this.version.ToString());
                if( address.lrsUser != "" && address.lrsPassword == "")
                    postHeader.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(address.lrsUser)));
                else if( address.lrsUser == "" && address.lrsPassword != "")
                    postHeader.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(address.lrsPassword)));
                else if( address.lrsUser != "" && address.lrsPassword != "")
                    postHeader.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(address.lrsUser, ":", address.lrsPassword))));

                // form data
                byte[] formBytes = Encoding.UTF8.GetBytes(statement.ToJSON(this.version));

                string queryURL;
                // endpoint should have trailing /
                if (address.lrsURL[address.lrsURL.Length - 1] != '/')
                    queryURL = address.lrsURL + "/statements";
                else
                    // https://learninglocker.dig-itgames.com/data/xAPI/statements?statementId=58098b7c-3353-4f9c-b812-1bddb08876fd
                    queryURL = address.lrsURL + "statements";

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
                StartCoroutine(WaitForRequest(www, formBytes, false));
            }
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void SaveStatement(byte[] formBytes, bool statementFromQueue = false)
        {
            //send statements to each address in the LRS config file
            foreach (LRSAddress address in lrsAddresses)
            {
                // reinit state
                this.initState();

                // post header
                Dictionary<string, string> postHeader = new Dictionary<string, string>();
                postHeader.Add("Content-Type", "application/json");
                postHeader.Add("X-Experience-API-Version", this.version.ToString());
                if (address.lrsUser != "" && address.lrsPassword == "")
                    postHeader.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(address.lrsUser)));
                else if (address.lrsUser == "" && address.lrsPassword != "")
                    postHeader.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(address.lrsPassword)));
                else if (address.lrsUser != "" && address.lrsPassword != "")
                    postHeader.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(address.lrsUser, ":", address.lrsPassword))));

                string queryURL;
                // endpoint should have trailing /
                if (address.lrsURL[address.lrsURL.Length - 1] != '/')
                    queryURL = address.lrsURL + "/statements";
                else
                    // https://learninglocker.dig-itgames.com/data/xAPI/statements?statementId=58098b7c-3353-4f9c-b812-1bddb08876fd
                    queryURL = address.lrsURL + "statements";

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
                StartCoroutine(WaitForRequest(www, formBytes, statementFromQueue));
            }
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		IEnumerator WaitForRequest(WWW data, byte[] formBytes, bool statementFromQueue)
        {

            yield return data; // Wait until the download is done

            // ok
            if (data.error == null){
				this.success = true;
				JArray ids = JArray.Parse(data.text);
				this.response = ids[0].ToString();
                if (statementFromQueue)
                {
                    sendingFromQueue = false;
                    unsentStatements.Dequeue();
                    List<string> unsentStoredData = new List<string>(File.ReadAllLines("Data/UnsentData.txt"));
                    unsentStoredData.Remove(Encoding.UTF8.GetString(tmpByteArray));
                    File.WriteAllLines("Data/UnsentData.txt", unsentStoredData.ToArray());
                }
			}
			// fail
			else {
                connexionFailed = true;
                retryTimer = Time.time;
                if (!statementFromQueue)
                {
                    //if the sending failed and the statement wasn't in the waiting queue, add the statement to the queue and save it in the file
                    unsentStatements.Enqueue(Encoding.UTF8.GetString(formBytes));
                    if (!File.Exists("Data/UnsentData.txt") || File.ReadAllText("Data/UnsentData.txt") == "")
                        File.WriteAllText("Data/UnsentData.txt", Encoding.UTF8.GetString(formBytes));
                    else
                        File.AppendAllText("Data/UnsentData.txt", string.Concat(System.Environment.NewLine, Encoding.UTF8.GetString(formBytes)));
                }
                else
                    sendingFromQueue = false;

                this.success = false;
				this.response = data.error;
			}

			// finished
			this.complete = true;
		}
	}
}
