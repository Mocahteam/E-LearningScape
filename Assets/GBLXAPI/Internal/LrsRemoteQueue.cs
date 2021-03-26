using DisruptorUnity3d;
using System;
using System.Collections;
using System.Collections.Generic;
using TinCan;
using UnityEngine;

namespace DIG.GBLXAPI.Internal
{
    public class LrsRemoteQueue : MonoBehaviour
    {
		public const string GAMEOBJECT_NAME = "GBLXAPI";

		// ************************************************************************
		// Monobehaviour singleton
		// ************************************************************************
		private static LrsRemoteQueue instance = null;
		public static LrsRemoteQueue Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (new GameObject(GAMEOBJECT_NAME)).AddComponent<LrsRemoteQueue>();
				}

				return instance;
			}
		}

		public bool useDefaultCallback = false;

		private List<RemoteLRSAsync> _lrsEndpoints; // WebGL/Desktop/Mobile coroutine implementation of RemoteLRS.cs

		private RingBuffer<QueuedStatement> _statementQueue;

		// ------------------------------------------------------------------------
		// Set singleton so it persists across scene loads
		// ------------------------------------------------------------------------
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		public void Init(List<GBLConfig> configs, int queueDepth = 1000)
		{
            _lrsEndpoints = new List<RemoteLRSAsync>();
            foreach (GBLConfig config in configs)
                _lrsEndpoints.Add(new RemoteLRSAsync(config.lrsURL, config.lrsUser, config.lrsPassword));
			_statementQueue = new RingBuffer<QueuedStatement>(queueDepth);
		}

		private void Update()
		{
            if (_statementQueue == null || _statementQueue.Count == 0)
                return;

            foreach (RemoteLRSAsync endPoint in _lrsEndpoints)
            {
                // Dequeue statement if exists in queue
                if (_statementQueue.TryDequeue(out QueuedStatement queuedStatement))
                {
                    // Debug statement
                    if (GBLXAPI.debugMode)
                    {
                        Debug.Log(queuedStatement.statement.ToJSON(true));
                    }
                    StartCoroutine(SendStatementCoroutine(endPoint, queuedStatement));
                }
            }
		}

		public void EnqueueStatement(Statement statement, Action<string, bool, string> sendCallback = null)
		{
			// Make sure all required fields are set
			bool valid = true;
			string invalidReason = "";
			if (statement.actor == null) { valid = false; invalidReason += "ERROR: Agent is null\n"; }
			if (statement.verb == null) { valid = false; invalidReason += "ERROR: Verb is null\n"; }
			if (statement.target == null) { valid = false; invalidReason += "ERROR: Object is null\n"; }

			// Use default callback if none was given
			if (sendCallback == null && useDefaultCallback)
			{
				sendCallback = StatementDefaultCallback;
			}

			if (valid)
			{
				// Check if space in the ringbuffer queue, if not discard or will hard lock unity
				if (_statementQueue.Capacity - _statementQueue.Count > 0)
				{
					_statementQueue.Enqueue(new QueuedStatement(statement, sendCallback));
				}
				else
				{
					Debug.LogWarning("QueueStatement: Queue is full. Discarding Statement");
				}
			}
			else
			{
				sendCallback?.Invoke("", false, invalidReason);
			}
		}

		// ------------------------------------------------------------------------
		// This coroutine spawns a thread to send the statement to the LRS
		// ------------------------------------------------------------------------
		private IEnumerator SendStatementCoroutine(RemoteLRSAsync endPoint, QueuedStatement queuedStatement)
		{

            int idState = endPoint.PostStatement(queuedStatement.statement);

			// Wait for the coroutine to finish
			while (!endPoint.states[idState].complete) { yield return null; }

			// Client callback with result
			queuedStatement.callback?.Invoke(endPoint.endpoint, endPoint.states[idState].success, endPoint.states[idState].response);
		}

		private void StatementDefaultCallback(string endpoint, bool result, string resultText)
		{
			if (result) { Debug.Log("GBLXAPI: "+ endpoint + " SUCCESS: " + resultText); }
			else { Debug.Log("GBLXAPI: "+endpoint+" ERROR: " + resultText); }
		}

		public struct QueuedStatement
		{
			public Statement statement;
			public Action<string, bool, string> callback;

			public QueuedStatement(Statement statement, Action<string, bool, string> callback)
			{
				this.statement = statement;
				this.callback = callback;
			}
		}
	}
}
