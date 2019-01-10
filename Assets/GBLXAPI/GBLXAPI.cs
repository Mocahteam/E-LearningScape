// -------------------------------------------------------------------------------------------------
// GBLXAPI.cs
// Project: GBLXAPI
// Created: 2017/05/16
// Last Updated: 2018/08/24
// Version: 1.04
// Copyright 2018 Dig-It! Games, LLC. All rights reserved.
// This code is licensed under the MIT License (See LICENSE.txt for details)
// -------------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
namespace DIG.GBLXAPI {

	using UnityEngine;
	using System;
	using TinCan;
	using TinCan.Json;
	using System.Collections.Generic;
	using System.Collections;				// coroutines
	using DisruptorUnity3d;					// Thread safe RingBuffer
	using Newtonsoft.Json.Linq;				// functions in loadEducationalStandards()
	using System.Text.RegularExpressions; 	// for UUID
	using System.Security.Cryptography;		// for UUID
	using System.Text;						// for UUID (sb)

	// -------------------------------------------------------------------------------------------------
	// -------------------------------------------------------------------------------------------------
	public class GBLXAPI : MonoBehaviour {

		public static string GAMEOBJECT_NAME = "GBLXAPI";

		// ************************************************************************
		// Monobehaviour singleton
		// ************************************************************************
		private static GBLXAPI instance = null;
		public static GBLXAPI Instance {
			get {
				if (instance == null){
					instance = (new GameObject(GBLXAPI.GAMEOBJECT_NAME)).AddComponent<GBLXAPI>();
				}
				return instance;
			}
		}

		// ------------------------------------------------------------------------
		// set singleton so it persists across scene loads
		// ------------------------------------------------------------------------
		private void Awake(){
			DontDestroyOnLoad(this.gameObject);
		}

		// ------------------------------------------------------------------------
		// NOTE: May not be init, but GO exists
		// ------------------------------------------------------------------------
		public static bool IsActive(){

			if (GameObject.Find(GBLXAPI.GAMEOBJECT_NAME)){
				return true;
			}

			return false;
		}

		// ************************************************************************
		// new QueuedStatement(callback, statement)
		// ************************************************************************
		public struct QueuedStatement {

			public StatementCallbackHandler callback;
			public Statement statement;

			public QueuedStatement(StatementCallbackHandler callback, Statement statement){
				this.callback = callback;
				this.statement = statement;
			}
		}

		// ************************************************************************
		// GBL XAPI Core
		// ************************************************************************
		private RemoteLRSAsync lrsEndpoint; // WebGL/Desktop/Mobile coroutine implementation of RemoteLRS.cs
		public delegate void StatementCallbackHandler(bool result, string resultText);
		public bool useDefaultCallback = false;
		public bool debugStatement = false;
		public JObject standardsJson;

		// queue
		private bool gblReadyToSend = false;
		private RingBuffer<QueuedStatement> gblQueue;

		// time tracking for result durations
		private List<DateTime> dateTimeSlots = new List<DateTime>();
		private bool isInit = false;

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void init(List<LRSAddress> addresses, string standardsConfigDefault, string standardsConfigUser, int queueDepth = 1000){

			// lrs init
			this.lrsEndpoint = this.gameObject.AddComponent<RemoteLRSAsync>();
			this.lrsEndpoint.initLRS(addresses);

			// queue init
			this.gblQueue = new RingBuffer<QueuedStatement>(queueDepth);
			this.gblReadyToSend = true;

			// load the educational standard defaults from: Resources/Data/GBLxAPI_Standards_Default.json
			this.standardsJson = null;
			try {
				string jsonTextDefault = Resources.Load<TextAsset>(standardsConfigDefault).text;
				this.standardsJson = JObject.Parse(jsonTextDefault);
			}
			catch {
				Debug.LogError("Missing GBLxAPI default vocabulary! Learning standards cannot be tracked without this file. Run Vocabulary/GBL_Json_Parser.py and ensure that the resulting GBLxAPI_Vocab_Default.json file is moved to the Assets/Resources/Data folder.");
			}

			// load the user configured standards from: Resources/Data/GBLxAPI_Standards_User.json
			JObject standardsJsonUser = null;
			try {
				string jsonTextUser = Resources.Load<TextAsset>(standardsConfigUser).text;
				standardsJsonUser = JObject.Parse(jsonTextUser);
			}
			catch {
				Debug.LogWarning("Missing GBLxAPI vocabulary user overrides. Default vocabulary will be used for learning tracking. To implement user overrides, run Vocabulary/GBL_Json_Parser.py and ensure that the resulting GBLxAPI_Vocab_User.json file is moved to the Assets/Resources/Data folder.");
			}

			// Merge the two json files, letting the user config overwrite the defaults
			if (this.standardsJson != null && standardsJsonUser != null) {
				this.standardsJson.Merge(standardsJsonUser);
			}
            
			// reset duration tracking slots
			this.ResetDurationSlots();

			this.isInit = true;
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public bool IsInit(){

			return this.isInit;
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void ResetDurationSlots(){

			this.dateTimeSlots.Clear();
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void ResetDurationSlot(int slotNumber){

			// add slots up to requested slot number
			if (slotNumber >= this.dateTimeSlots.Count){
				while (slotNumber >= this.dateTimeSlots.Count){
					this.dateTimeSlots.Add(DateTime.Now);
				}
			}

			// and set that slot to now
			if (slotNumber < this.dateTimeSlots.Count){
				this.dateTimeSlots[slotNumber] = DateTime.Now;
			}
			else {
				Debug.LogWarning("ResetDurationSlot: Requested slot " + slotNumber + " less than " + this.dateTimeSlots.Count  + " available slots");
			}
		}

		// ------------------------------------------------------------------------
		// returns difference in seconds
		// ------------------------------------------------------------------------
		public float GetDurationSlot(int slotNumber){

			if (slotNumber < this.dateTimeSlots.Count){
				TimeSpan ts = DateTime.Now - this.dateTimeSlots[slotNumber];
				return (float)ts.TotalSeconds; // fractional
			}

			return -1.0f;
		}

		// ------------------------------------------------------------------------
		// build statement, then queue to be sent asynchronously
		// actor, verb, object, result, context
		// ------------------------------------------------------------------------
		public void QueueStatement(Agent statementActor, Verb statementVerb, Activity statementObject, Result statementResult, Context statementContext, StatementCallbackHandler sendCallback = null){

			// create statement
			Statement statement = new Statement();
			statement.actor = statementActor;
			statement.verb = statementVerb;
			if (statementResult != null){ statement.result = statementResult; }
			statement.target = statementObject;  // statement.target is the object (object is a reserved c# word)
			if (statementContext != null){ statement.context = statementContext; }

			// make sure all are set
			bool invalid = false;
			string invalidReason = "";
			if (statement.actor == null){ 	invalid = true; invalidReason += "ERROR: Agent is null\n"; } // required
			if (statement.verb == null){ 	invalid = true; invalidReason += "ERROR: Verb is null\n"; } // required
			// statement.result is optional and may be null
			if (statement.target == null){	invalid = true; invalidReason += "ERROR: Object is null\n"; } // required
			// statement.context is optional and may be null

			// invalid?
			if (invalid){
				Debug.LogError(invalidReason);
				if (sendCallback != null){
					sendCallback(false, invalidReason);
				}
			}
			// valid - queue
			else {
				// check if space in the ringbuffer queue, if not discard or will hard lock unity
				if (this.gblQueue.Capacity - this.gblQueue.Count > 0){
					this.gblQueue.Enqueue(new QueuedStatement(sendCallback, statement));
				}
				else {
					Debug.LogWarning("QueueStatement: Queue is full. Discarding Statement");
				}
			}
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public void Update(){

			// gbl is open/ready to send?
			if (this.gblReadyToSend){

				// may have stale data, but try anyway
				if (this.gblQueue.Count > 0){

					this.StopAllCoroutines();
					this.StartCoroutine(SendStatementCoroutine());
				}
			}
		}

		// ------------------------------------------------------------------------
		// This coroutine spawns a thread to send the statement to the LRS
		// ------------------------------------------------------------------------
		private IEnumerator SendStatementCoroutine(){

			// lock
			this.gblReadyToSend = false;

			// dequeue statement if exists in queue
			QueuedStatement queuedStatement;
			yield return null;
			bool dequeued = this.gblQueue.TryDequeue(out queuedStatement);
			yield return null;
			if (dequeued){

				// debug statement
				if (this.debugStatement){
					yield return null;
					this.PrintStatement(queuedStatement.statement);
					yield return null;
				}

				yield return null;
				this.lrsEndpoint.SaveStatement(queuedStatement.statement);
				yield return null;

				// wait for the coroutine to finish
				while (this.lrsEndpoint.complete == false){
					yield return null;
				}

				// client callback with result
				if (queuedStatement.callback != null){
					yield return null;
					queuedStatement.callback(this.lrsEndpoint.success, this.lrsEndpoint.response); // result, text
					yield return null;
				}
				// default callback
				else if (this.useDefaultCallback){
					yield return null;
					this.StatementDefaultCallback(this.lrsEndpoint.success, this.lrsEndpoint.response); // result, text
					yield return null;
				}
			}

			// unlock
			this.gblReadyToSend = true;

			yield return null;
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		private void StatementDefaultCallback(bool result, string resultText){

			if (result){
				Debug.Log("GBLXAPI: SUCCESS: " + resultText);
			}
			else {
				Debug.Log("GBLXAPI: ERROR: " + resultText);
			}
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		private void PrintStatement(Statement statement){

			Debug.Log(statement.ToJSON(true));
		}

		// ------------------------------------------------------------------------
		// Generate random UUID based on a unique name
		// ------------------------------------------------------------------------
		public string GenerateActorUUID(string name){
			var saltedName = new StringBuilder();
			string salt = UnityEngine.Random.Range(0, 99999).ToString();
			saltedName.Append(salt);
			saltedName.Append(name);

			byte[] bytes = Encoding.UTF8.GetBytes(saltedName.ToString());
			var sha1 = SHA1.Create();
			byte[] hashBytes = sha1.ComputeHash(bytes);

			// convert to hex string
			var sb = new StringBuilder();
			foreach (byte b in hashBytes){
				var hex = b.ToString("x2");
				sb.Append(hex);
			}

			return sb.ToString();
		}

		// ------------------------------------------------------------------------
		//	"actor": {
		//		"objectType": "Agent",
		//		"account": {
		//			"name": "f1cd58aa-ad22-49e5-8567-d59d97d3b209",
		//			"homepage": "https://dig-itgames.com/"
		//		}
		//		"name": "Joe Schmoe"
		//	}
		//
		//	UUID: f1cd58aa-ad22-49e5-8567-d59d97d3b209
		// 	homepage: https://dig-itgames.com
		// 	name (optional): Joe Schmoe
		// ------------------------------------------------------------------------
		public Agent CreateActorStatement(string UUID, string homepage, string name = null){

			Agent actor = new Agent();
			if ( name != null) { actor.name = name; }
			actor.account = new AgentAccount();
			actor.account.name = UUID;
			actor.account.homePage = new Uri(homepage);

			return actor;
		}

		// ------------------------------------------------------------------------
		//	"verb": {
		//		"id": "https://w3id.org/xapi/dod-isd/verbs/started",
		//		"display": {
		//			"en-US": "started"
		//		}
		//	}
		//
		// statementVerb: started
		// ------------------------------------------------------------------------
		public Verb CreateVerbStatement(string statementVerb){

			statementVerb = statementVerb.ToLower();

			Verb verb = new Verb();

			try {
			verb.id = new Uri((string)this.standardsJson["verb"][statementVerb]["id"]);
			verb.display = new LanguageMap();
			verb.display.Add("en-US", (string)this.standardsJson["verb"][statementVerb]["name"]["en-US"]);
			}
			catch (NullReferenceException) { this.ThrowVocabError("verb", statementVerb); }

			return verb;
		}

		// ------------------------------------------------------------------------
		//	"definition": {
		//		"type": "https://gblxapi.org/object/serious-game",
		//		"name": {
		//			"en-US": "ExoTrex 2"
		//		}
		//	}
		//	activityType: serious-game
		//	activityName: ExoTrex 2
		// ------------------------------------------------------------------------
		public ActivityDefinition CreateActivityDefinitionStatement(string activityType, string activityName){

			activityType = activityType.ToLower();

			ActivityDefinition definition = new ActivityDefinition();

			try {
			definition.type = new Uri((string)this.standardsJson["activity"][activityType]["id"]);
			definition.name = new LanguageMap();
			definition.name.Add("en-US", activityName);
			}
			catch (NullReferenceException) { this.ThrowVocabError("activity type", activityType); }

			return definition;
		}

		// ------------------------------------------------------------------------
		// "context": {
		// 		"extensions": {
		// 			"https://gblxapi.org/c3": [
		// 			{
		// 				"description": {
		// 				"en-US": "Social Studies C3 Standard d2.his.13.6-8"
		// 				},
		// 				"name": {
		// 				"en-US": "d2.his.13.6-8"
		// 				},
		// 				"id": "https://gblxapi.org/c3/d2-his-13-6-8"
		// 			}],
		// 			"https://w3id.org/xapi/gblxapi/extensions/domain": [
		// 			{
		// 				"name": {
		// 				"en-US": "History"
		// 				},
		// 				"id": "https://gblxapi.org/domain/history",
		// 				"description": {
		// 				"en-US": "Actor has been presented or interacted in Social Studies domain experiences in: History"
		// 				}
		// 			}]
		// 		},
		// 		"contextActivities": {
		// 			"category": [
		// 			{
		// 				"id": "https://gblxapi.org/socialstudies",
		// 				"objectType": "Activity"
		// 			}]
		// 			"grouping": [
		// 			{
		// 				"id": "https://dig-itgames.com/",
		// 				"objectType": "Activity"
		// 			},
		// 			{
		// 				"definition": {
		// 				"name": {
		// 					"en-US": "Three Digits - Easy"
		// 				},
		// 				"type": "https://w3id.org/xapi/gblxapi/activities/difficulty"
		// 				},
		// 				"id": "https://dig-itgames.com/three-digits/easy",
		// 				"objectType": "Activity"
		// 			}
		// 			],
		// 			"parent": [
		// 			{
		// 				"definition": {
		// 				"name": {
		// 					"en-US": "Three Digits"
		// 				},
		// 				"type": "https://w3id.org/xapi/seriousgames/activity-types/serious-game"
		// 				},
		// 				"id": "https://dig-itgames.com/three-digits",
		// 				"objectType": "Activity"
		// 			}]}
		// 		}
		//
		// parent: Activity directly related to current Statement Object
		// grouping: Any other related activities
		// category: Educational Subject Area of current Statement Object
		// extensions: Related educational standards
		// ------------------------------------------------------------------------
		public Context CreateContextActivityStatement(List<Activity> parentList, List<Activity> groupingList = null, List<Activity> categoryList = null, TinCan.Extensions contextExtensions = null){

			Context context = new Context();
			context.contextActivities = new ContextActivities();
			if (parentList != null){ context.contextActivities.parent = parentList; }
			if (groupingList != null){ context.contextActivities.grouping = groupingList; }
			if (categoryList != null){ context.contextActivities.category = categoryList; }
			if (contextExtensions != null){ context.extensions = contextExtensions; }

			return context;
		}

		// ------------------------------------------------------------------------
        //  "name": {
        //       "en-US": "Patterns and Relationships"
        //     },
        //     "id": "https://gblxapi.org/skill/patterns-relationships",
        //     "description": {
        //       "en-US": "Actor has been presented or interacted in math skills related to: Patterns and Relationships"
        //     }
        //   }
		//
		// extensionType: Skill
		// extensionName: Patterns and Relationships
		// ------------------------------------------------------------------------
		public JToken CreateContextExtensionStatement(string extensionType, string extensionName){

			extensionType = extensionType.ToLower();
			extensionName = extensionName.ToLower();

			// try/catch statement catches invalid extensionType
			// appended null check catches invalid extensionName
			JToken res = null;
			try { res = this.standardsJson[extensionType][extensionName]; }
			catch (NullReferenceException) {
                res = extensionName;
                //this.ThrowVocabError("extension type", extensionType);
            }

			if (res == null)
            {
                res = extensionName;
                //this.ThrowVocabError("extension", extensionName);
            }

			return res;
		}

		// ------------------------------------------------------------------------
		// This function places a list of context extensions into a Tincan.Extensions object under the correct key.
		//
		// "https://w3id.org/xapi/gblxapi/extensions/focus": [
        //   {
        //     "name": {
        //       "en-US": "Addition/Subtraction"
        //     },
        //     "id": "https://gblxapi.org/focus/addition-subtraction",
        //     "description": {
        //       "en-US": "Actor has been presented or interacted in math experiences involving the focus area of: Addition/Subtraction"
        //     }
        //   }
        // ],
		//
		// extensionType: focus
		// trackedStandards: previously created list of focus extensions
		// target: previously created TinCan.Extensions object
		// ------------------------------------------------------------------------
		public void PackExtension(string extensionType, List<JToken> trackedStandards, TinCan.Extensions target) {

			extensionType = extensionType.ToLower();

			try {
				Uri extURI = new Uri((string)this.standardsJson["extension"][extensionType]["id"]);
				target.Add(extURI, JToken.FromObject(trackedStandards));
			}
			catch (NullReferenceException)
            {
                Uri extURI = new Uri(string.Concat("https://www.lip6.fr/mocah/invalidURI/extensions/", extensionType));
                target.Add(extURI, JToken.FromObject(trackedStandards));
                //this.ThrowVocabError("extension type", extensionType);
            }
		}

		// ------------------------------------------------------------------------
		// Catch-all function for getting the URI associated with any vocabulary term
		//
		// "https://w3id.org/xapi/seriousgames/extensions/progress"
		//
		// type: extension
		// name: progress
		//
		// This is especially useful for adding extensions that are not tracked learning standards and/or extensions that only hold single values,
		// such as the number of collectibles a player got in a level or their progress through the game.
		// ------------------------------------------------------------------------
		public string GetVocabURI (string type, string name) {
			try { return (string)this.standardsJson[type][name]["id"]; }
			catch (NullReferenceException) {
				if (this.standardsJson[type] == null) { this.ThrowVocabError("type", type); }
				else if (this.standardsJson[type][name] == null) { this.ThrowVocabError("vocab term", name); }
				return null;
			}
		}

		// ------------------------------------------------------------------------
		//	"object": {
		//		"objectType": "Activity",
		//		"id": "https://dig-itgames.com/apps/exotrex2",
		//		"definition": {
		//			"type": "https://gblxapi.org/object/serious-game",
		//			"name": {
		//				"en-US": "ExoTrex 2"
		//			}
		//		}
		//	}
		//
		//	activityURI: https://dig-itgames.com/apps/exotrex2
		//	activityType: serious-game
		//	activityName: ExoTrex 2
		// ------------------------------------------------------------------------
		public Activity CreateObjectActivityStatement(string activityURI, string activityType = null, string activityName = null){

			Activity activity = new Activity();
			activity.id = new Uri(activityURI);

			// add definition if requested
			if (activityType != null && activityName != null){
				activity.definition = this.CreateActivityDefinitionStatement(activityType, activityName);
			}

			return activity;
		}

		// ------------------------------------------------------------------------
		//	"result": {
		//		"success": true,
		//		"duration": "PT1.46S",
		//		"response": "0",
		//		"completion": true
		//	},
		// ------------------------------------------------------------------------
		public Result CreateResultStatement(bool? completed, bool? success, float duration, string response = null, Nullable<int> score = null, TinCan.Extensions extensions = null){

			Result resultStatement = new Result();
			resultStatement.completion = completed;
			resultStatement.success = success;
			resultStatement.duration = TimeSpan.FromSeconds(duration);

			if (extensions != null){
				resultStatement.extensions = extensions;
			}

			if (response != null){
				resultStatement.response = response;
			}

			if (score != null){
				Score statementScore = new Score();
				statementScore.raw = score; // scaled, raw, min, max

				resultStatement.score = statementScore;
			}

			return resultStatement;
		}

		// ------------------------------------------------------------------------
		//	"object": {
		//		"objectType": "Activity",
		//		"id": "https://dig-itgames.com/apps/exotrex2/minigame/titan/branchingquiz/4",
		//		"definition": {
		//			"type": "https://gblxapi.org/object/minigame",
		//			"name": {
		//				"en-US": "MiniGame Titan BranchingQuiz 4"
		//			},
		//			"description": {
		//				"en-US": "I'm very interested in your take of Titan's volcanoes. How do they compare to the ones we have on Earth?"
		//			},
		//			"interactionType": "choice",
		//			"correctResponsesPattern": [
		//				"0"
		//			],
		//			"choices": [
		//				{
		//					"id": "0",
		//					"description": {
		//						"en-US": "Titan's volcanoes are called cryovolcanoes because they ooze frozen or slushy mixtures of water and methane from under the moon's crust."
		//					}
		//				},
		//				{
		//					"id": "1",
		//					"description": {
		//						"en-US": "Titan's volcanoes are just like Earth's and the hot lava helps warm the surface."
		//					}
		//				},
		//				{
		//					"id": "2",
		//					"description": {
		//						"en-US": "Titan's volcanoes are called cryovolcanoes because they only emit steam and sparks of light from deep inside of Titan."
		//					}
		//				}
		//			]
		//		}
		//	},
		// REF: https://github.com/adlnet/xAPI-Spec/blob/master/xAPI-Data.md#Appendix2C
		// ------------------------------------------------------------------------
		public Activity CreateObjectStatement(string objectId, string activityName, string objectName, string objectDescription, string interactionType, List<String> objectResponses = null, List<InteractionComponent> objectResponseOptions = null, List<InteractionComponent> objectResponseTargets = null, TinCan.Extensions extensions = null){

			// create definition and add to activity.
			ActivityDefinition definition = new ActivityDefinition();
			try { definition.type = (Uri)this.standardsJson["activity"][activityName.ToLower()]["id"]; }
			catch (NullReferenceException) { this.ThrowVocabError("Activity", activityName); }
			definition.name = new LanguageMap();
			definition.name.Add("en-US", objectName);
			definition.description = new LanguageMap();
			definition.description.Add("en-US", objectDescription);
			definition.interactionType = interactionType;

			if (objectResponses != null){
				definition.correctResponsesPattern = objectResponses;
			}

			if (extensions != null){
				definition.extensions = extensions;
			}

			// definition field type is based on interactionType
			if (objectResponseOptions != null && objectResponseOptions.Count > 0){
				switch (interactionType){
					case "choice":
						definition.choices = objectResponseOptions;
						break;
					case "likert":
						definition.scale = objectResponseOptions;
						break;
					case "matching":
						definition.source = objectResponseOptions;
						definition.target = objectResponseTargets;
						break;
					case "performance":
						definition.steps = objectResponseOptions;
						break;
					case "sequencing":
						definition.choices = objectResponseOptions;
						break;
					default:
						Debug.LogWarning("CreateObjectStatement(): Interaction Type:" + interactionType + " does not use response options (discarded)");
						break;
				}
			}

			// create the activity
			Activity activity = new Activity();
			activity.id = new Uri(objectId);
			activity.definition = definition;

			return activity;
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------
		public InteractionComponent CreateChoiceStatement(string choiceID, string choiceDescription){

			InteractionComponent choice = new InteractionComponent();
			choice.id = choiceID;
			choice.description = new LanguageMap();
			choice.description.Add("en-US", choiceDescription);

			return choice;
		}

		// ------------------------------------------------------------------------
		// ------------------------------------------------------------------------

		private void ThrowVocabError (string termType, string term) {
			Debug.LogError("The " + termType + " \"" + term + "\"" + " does not exist in the GBLxAPI vocabulary. Either fix the typo or add the term to the vocabulary by editing Vocabulary/GBLxAPI_Vocab_User.xlsx, running GBLxAPI_Json_Parser.py, and moving the generated json files to Assets/Resources/Data. See the GBLxAPI documentation for help.");
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false; // gracefully exit the editor so dev can fix the issue
			#endif
		}
	}
}
// eof