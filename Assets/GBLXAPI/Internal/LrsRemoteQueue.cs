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

        private int batchTreshold = 1000;
		private int totalqueuedStatement = 0;

		// ------------------------------------------------------------------------
		// Set singleton so it persists across scene loads
		// ------------------------------------------------------------------------
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		public void Init(List<GBLConfig> configs, int queueDepth = 2000)
		{
            _lrsEndpoints = new List<RemoteLRSAsync>();
            foreach (GBLConfig config in configs)
                _lrsEndpoints.Add(new RemoteLRSAsync(config.lrsURL, config.lrsUser, config.lrsPassword));
			_statementQueue = new RingBuffer<QueuedStatement>(queueDepth);
		}

		private void Update()
        {
            if (_statementQueue == null || _statementQueue.Count < batchTreshold)
                return;
            flushQueuedStatements(true);
        }

        private void OnDestroy()
        {
			// flush statements
            flushQueuedStatements(false);
			Debug.Log("Total statements sent:" + totalqueuedStatement);
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
				flushQueuedStatements(true);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
				flushQueuedStatements(true);
		}


		public void flushQueuedStatements(bool waitComplete)
        {
			if (_statementQueue != null)
			{
				// Dequeue statements if exists in queue
				List<QueuedStatement> batchStatements = new List<QueuedStatement>();
				while (_statementQueue.Count > 0)
				{
					if (_statementQueue.TryDequeue(out QueuedStatement queuedStatement))
					{
						// Debug statement
						if (GBLXAPI.debugMode)
						{
							Debug.Log(queuedStatement.statement.ToJSON(true));
						}
						batchStatements.Add(queuedStatement);
					}
				}
				// Send to each endPoint
				foreach (RemoteLRSAsync endPoint in _lrsEndpoints)
				{
					if (waitComplete)
						StartCoroutine(SendStatementCoroutine(endPoint, batchStatements));
					else
						SendStatementsImmediate(endPoint, batchStatements);
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
					totalqueuedStatement++;
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

        //return idState of posted statements
        private int SendStatementsImmediate(RemoteLRSAsync endPoint, List<QueuedStatement> queuedStatements)
        {
            List<Statement> statements = new List<Statement>();
            foreach (QueuedStatement qs in queuedStatements)
                statements.Add(qs.statement);
			int idState = endPoint.PostStatements(statements);
			Debug.Log(statements.Count+" statements with new idState "+idState+" sent. ");
			return idState;
        }

        // ------------------------------------------------------------------------
        // This coroutine spawns a thread to send the statement to the LRS
        // ------------------------------------------------------------------------
        private IEnumerator SendStatementCoroutine(RemoteLRSAsync endPoint, List<QueuedStatement> queuedStatements)
		{
			int idState = SendStatementsImmediate(endPoint, queuedStatements);
			// Wait answer
			while (!endPoint.states[idState].complete) { yield return null; }

			if (endPoint.states[idState].success)
				Debug.Log("Statements with idState " + idState + " sent with success");
			else
				Debug.LogWarning("Statements with idState " + idState + " failed with error: " + endPoint.states[idState].response);

			// Client callback with result
			foreach (QueuedStatement qs in queuedStatements)
                qs.callback?.Invoke(endPoint.endpoint, endPoint.states[idState].success, endPoint.states[idState].response);
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
