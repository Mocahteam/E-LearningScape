// -------------------------------------------------------------------------------------------------
// GBL_Interface.cs
// Project: GBLXAPI-Unity
// Created: 2018/07/06
// Copyright 2018 Dig-It! Games, LLC. All rights reserved.
// This code is licensed under the MIT License (see LICENSE.txt for details)
// -------------------------------------------------------------------------------------------------
using UnityEngine;

// required for GBLXAPI
using DIG.GBLXAPI;
using TinCan;
using System;
using System.Collections.Generic;
using System.Text; // required for StringBuilder()

using Newtonsoft.Json.Linq; // for extensions

public struct LRSAddress
{
    public string lrsURL;       //endpoint
    public string lrsUser;      //key
    public string lrsPassword;  //secret

    public LRSAddress(string url, string user, string password)
    {
        lrsURL = url;
        lrsUser = user;
        lrsPassword = password;
    }
}

// --------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------
public static class GBL_Interface {

		public enum durationSlots
	{
		Application,
		Game,
		Tutorial,
		Level
	};

    // Fill in these fields for GBLxAPI setup.
    //Statements will be sent to all addresses in this list
    public static List<LRSAddress> lrsAddresses = new List<LRSAddress>() {
        new LRSAddress("https://lrsmocah.lip6.fr/data/xAPI", "2da3ea73b1dcf6258c02649d1d3f7a9385b74d61", "90935a12c7eeb44d1d6acefd0f413e4d4c552467")    //default lip6 LRS
    };
	public static string standardsConfigDefault = "data/GBLxAPI_Vocab_Default";
	public static string standardsConfigUser = "data/GBLxAPI_Vocab_User";
	public static string gameURI = "https://www.lip6.fr/mocah/invalidURI/activity-types/serious-game/LearningScape";
	public static string gameName = "E-LearningScape";
	public static string companyURI = "https://www.lip6.fr/mocah/";
	public static string userUUID = "f1cd58aa-ad22-49e5-8567-d59d97d3b209";
    public static string playerName = "player";

    // ------------------------------------------------------------------------
	// Sample Gameplay GBLxAPI Triggers
	// ------------------------------------------------------------------------
	/*
	Here is where you will put functions to be called whenever you want to send a GBLxAPI statement.
	 */
	
	public static void SendStatement(string verb, string activityType, string activityName, Dictionary<string, List<string>> activityExtensions = null)
    {
		Agent statementActor = GBLXAPI.Instance.CreateActorStatement(userUUID, "https://www.lip6.fr/mocah/", playerName);
		Verb statementVerb = GBLXAPI.Instance.CreateVerbStatement(verb);
		Activity statementObject = GBLXAPI.Instance.CreateObjectActivityStatement(string.Concat("https://www.lip6.fr/mocah/invalidURI/",activityType,"/",activityName), activityType, activityName);

        if(activityExtensions != null)
        {
            foreach(string field in activityExtensions.Keys)
            {
                List<JToken> jtList = new List<JToken>();
                foreach (string value in activityExtensions[field])
                    jtList.Add(GBLXAPI.Instance.CreateContextExtensionStatement(field, value));
                statementObject.definition.extensions = new TinCan.Extensions();
                GBLXAPI.Instance.PackExtension(field, jtList, statementObject.definition.extensions);
            }
        }

		Result statementResult = null;
		Context statementContext = null;

		// QueueStatement(Agent statementActor, Verb statementVerb, Activity statementObject, Result statementResult, Context statementContext, StatementCallbackHandler sendCallback = null)
		GBLXAPI.Instance.QueueStatement(statementActor, statementVerb, statementObject, statementResult, statementContext);
	}
	
	public static void SendStatementWithResult(string verb, string activityType, string activityName, Dictionary<string, List<string>> activityExtensions = null,
        Dictionary<string, List<string>> resultExtensions = null, bool ? completed = null, bool? success = null, string response = null, int? score = null,
        float duration = 0)
    {
		Agent statementActor = GBLXAPI.Instance.CreateActorStatement(userUUID, "https://www.lip6.fr/mocah/", playerName);
		Verb statementVerb = GBLXAPI.Instance.CreateVerbStatement(verb);
		Activity statementObject = GBLXAPI.Instance.CreateObjectActivityStatement(string.Concat("https://www.lip6.fr/mocah/invalidURI/",activityType,"/",activityName), activityType, activityName);

        if (activityExtensions != null)
        {
            foreach (string field in activityExtensions.Keys)
            {
                List<JToken> jtList = new List<JToken>();
                foreach (string value in activityExtensions[field])
                    jtList.Add(GBLXAPI.Instance.CreateContextExtensionStatement(field, value));
                statementObject.definition.extensions = new TinCan.Extensions();
                GBLXAPI.Instance.PackExtension(field, jtList, statementObject.definition.extensions);
            }
        }

        Result statementResult = GBLXAPI.Instance.CreateResultStatement(completed, success, duration, response, score);

        if (resultExtensions != null)
        {
            foreach (string field in resultExtensions.Keys)
            {
                List<JToken> jtList = new List<JToken>();
                foreach (string value in resultExtensions[field])
                    jtList.Add(GBLXAPI.Instance.CreateContextExtensionStatement(field, value));
                statementResult.extensions = new TinCan.Extensions();
                GBLXAPI.Instance.PackExtension(field, jtList, statementResult.extensions);
            }
        }

        Context statementContext = null;

		// QueueStatement(Agent statementActor, Verb statementVerb, Activity statementObject, Result statementResult, Context statementContext, StatementCallbackHandler sendCallback = null)
		GBLXAPI.Instance.QueueStatement(statementActor, statementVerb, statementObject, statementResult, statementContext);
	}
	
	public static void SendTestStatementStarted(){

		Agent statementActor = GBLXAPI.Instance.CreateActorStatement(GBL_Interface.userUUID, "https://dig-itgames.com/", "Test User");
		Verb statementVerb = GBLXAPI.Instance.CreateVerbStatement("started");
		Activity statementObject = GBLXAPI.Instance.CreateObjectActivityStatement("https://dig-itgames.com/apps/GBLXAPITEST", "serious-game", "GBLXAPI TEST");
		Result statementResult = null;

		// context can be created right in the statement functions
		List<Activity> parentList = new List<Activity>();
		parentList.Add(GBLXAPI.Instance.CreateObjectActivityStatement("https://dig-itgames.com/apps/GBLXAPITEST", "serious-game", "GBLXAPI TEST"));

		List<Activity> groupingList = new List<Activity>();
		groupingList.Add(GBLXAPI.Instance.CreateObjectActivityStatement("https://dig-itgames.com/"));

		Context statementContext = GBLXAPI.Instance.CreateContextActivityStatement(parentList, groupingList);

		// QueueStatement(Agent statementActor, Verb statementVerb, Activity statementObject, Result statementResult, Context statementContext, StatementCallbackHandler sendCallback = null)
		GBLXAPI.Instance.QueueStatement(statementActor, statementVerb, statementObject, statementResult, statementContext);
	}

	public static void SendTestStatementCompleted(){

		Agent statementActor = GBLXAPI.Instance.CreateActorStatement(GBL_Interface.userUUID, "https://dig-itgames.com/", "Test User");
		Verb statementVerb = GBLXAPI.Instance.CreateVerbStatement("completed");
		Activity statementObject = GBLXAPI.Instance.CreateObjectActivityStatement("https://dig-itgames.com/apps/GBLXAPITEST", "serious-game", "GBLXAPI TEST");

		float durationSeconds = GBLXAPI.Instance.GetDurationSlot((int)durationSlots.Application); // get delta time since start of application
		Result statementResult = GBLXAPI.Instance.CreateResultStatement(false, false, durationSeconds);

		// this time using a helper function to create context
		Context statementContext = CreateTestContext();

		GBLXAPI.Instance.QueueStatement(statementActor, statementVerb, statementObject, statementResult, statementContext);
	}

	// // ------------------------------------------------------------------------
	// // Sample Context Generators
	// // ------------------------------------------------------------------------
    /*
    Since context generation can be many lines of code, it is often helpful to separate it out into helper functions. 
    These functions will be responsible for creating Context Activities, Context Extensions, and assigning them to a singular Context object.
     */

	public static Context CreateTestContext() {

		// CONTEXT ACTIVITIES

		// parent contains the activity just above this one in the hierarchy
		List<Activity> parentList = new List<Activity>();
		parentList.Add (GBLXAPI.Instance.CreateObjectActivityStatement(gameURI, "serious-game", gameName));

		// grouping contains all other related activities to this one
		List<Activity> groupingList = new List<Activity> ();
		groupingList.Add (GBLXAPI.Instance.CreateObjectActivityStatement (companyURI));

		// category is used in GBLxAPI to report on the subject area
		List<Activity> categoryList = new List<Activity> ();;
		categoryList.Add(GBLXAPI.Instance.CreateObjectActivityStatement("https://gblxapi.org/socialstudies"));
		categoryList.Add(GBLXAPI.Instance.CreateObjectActivityStatement("https://gblxapi.org/math"));

		// CONTEXT EXTENSIONS

		// grade
		List<JToken> gradeList = new List<JToken>();
		gradeList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Grade", "Grade 4 level")); 

		// domain
		List<JToken> domainList = new List<JToken>();
		domainList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Domain", "History"));
		domainList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Domain", "Number and Operations in Base Ten"));

		// subdomain
		List<JToken> subdomainList = new List<JToken>();
		subdomainList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Subdomain", "Problem Solving"));

		// skill
		List<JToken> skillList = new List<JToken>();
		skillList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Skill","Patterns and Relationships"));
		skillList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Skill","Calculation and Computation"));
		
		// topic
		List<JToken> topicList = new List<JToken>();
		topicList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Topic","Arithmetic"));
		
		// focus
		List<JToken> focusList = new List<JToken>();
		focusList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Focus","Addition/Subtraction"));
		
		// action
		List<JToken> actionList = new List<JToken>();
		actionList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("Action","Solve Problems"));
		
		// c3/ccss
		List<JToken> c3List = new List<JToken>(); 
		c3List.Add(GBLXAPI.Instance.CreateContextExtensionStatement("C3 Framework", "d2.His.13.6-8."));

		List<JToken> ccList = new List<JToken>();
		ccList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("CC-MATH", "CCSS.Math.Content.4.NBT.B.4"));
		ccList.Add(GBLXAPI.Instance.CreateContextExtensionStatement("CC-MATH", "CCSS.Math.Content.5.NBT.A.1"));

		// creating TinCan.Extensions object to pack the lists into
		TinCan.Extensions contextExtensions = new TinCan.Extensions();
		// adding lists to extensions JObject
		GBLXAPI.Instance.PackExtension("domain", domainList, contextExtensions);
		GBLXAPI.Instance.PackExtension("subdomain", subdomainList, contextExtensions);
		GBLXAPI.Instance.PackExtension("topic", topicList, contextExtensions);
		GBLXAPI.Instance.PackExtension("focus", focusList, contextExtensions);
		GBLXAPI.Instance.PackExtension("action", actionList, contextExtensions);
		GBLXAPI.Instance.PackExtension("skill", skillList, contextExtensions);
		GBLXAPI.Instance.PackExtension("grade", gradeList, contextExtensions);
		GBLXAPI.Instance.PackExtension("cc", ccList, contextExtensions);
		GBLXAPI.Instance.PackExtension("c3", c3List, contextExtensions);

		// Folding all of the above into our Context object to be used in the statement
		Context statementContext = GBLXAPI.Instance.CreateContextActivityStatement(parentList, groupingList, categoryList, contextExtensions);
		return statementContext;
	}
}