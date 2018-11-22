using System;
using System.Collections.Concurrent;
using UnityEngine;

public abstract class MonoBehaviourWithTaskQueue : MonoBehaviour {

    private readonly ConcurrentQueue<Action> _taskQueue = new ConcurrentQueue<Action>();

    // If this method is overriden, the overriding method should call this method.
    protected virtual void Update() {
        while(!_taskQueue.IsEmpty) {
            Action task;
            if (_taskQueue.TryDequeue(out task)) {
                task.Invoke();
            }
        }
    }

    /// <summary>
    ///     Adds a task to the queue.
    /// </summary>
    protected void QueueTask(Action task) {
        _taskQueue.Enqueue(task);
    }

}