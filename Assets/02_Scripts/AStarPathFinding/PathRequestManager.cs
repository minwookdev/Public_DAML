// ReSharper disable InvalidXmlDocComment
/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using System;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using UnityEngine;

/// NOTE:
///
namespace CoffeeCat.Pathfinding2D {
	public class PathRequestManager : SceneSingleton<PathRequestManager> {
		// Components
		private Pathfinder pathFinder = null;
	
		// PathFind Process Queue
		private Queue<PathRequest> requestQueue;
		private PathRequest currentRequest;
		private bool isProcessingPathFind;

		protected override void Initialize() {
			base.Initialize();
		}

		private void Start() {
			pathFinder = GetComponent<Pathfinder>();
			requestQueue = new Queue<PathRequest>();
		}

		public void RequestPathAsync(Vector2 pathStart, Vector2 pathEnd, Action<Vector2[], bool> completed) {
			PathRequest request = new PathRequest(pathStart, pathEnd, completed);
			requestQueue.Enqueue(request);
			TryProcessNext();
		}

		void TryProcessNext() {
			if (!isProcessingPathFind && requestQueue.Count > 0) {
				currentRequest = requestQueue.Dequeue();
				isProcessingPathFind = true;
				pathFinder.StartFindPath(currentRequest.pathStart, currentRequest.pathEnd);
			}
		}

		public void FinishedProcessingRequest(Vector2[] path, bool isSuccessed) {
			currentRequest.OnCompleted?.Invoke(path, isSuccessed);
			isProcessingPathFind = false;
			TryProcessNext();
		}

		public struct PathRequest {
			public Vector2 pathStart;
			public Vector2 pathEnd;
			public Action<Vector2[], bool> OnCompleted;

			public PathRequest(Vector2 startPosition, Vector2 endPosition, Action<Vector2[], bool> onCompleted) {
				pathStart = startPosition;
				pathEnd = endPosition;
				OnCompleted = onCompleted;
			}
		}
	}
}
