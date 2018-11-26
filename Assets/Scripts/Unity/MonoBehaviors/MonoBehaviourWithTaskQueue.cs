using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace App.Unity.MonoBehaviors {

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

        /// <summary>Adds a task to the queue.</summary>
        protected void QueueTask(Action task) {
            _taskQueue.Enqueue(task);
        }

    }

    public abstract class MonoBehaviourWithTaskQueue<T> : MonoBehaviour {

        private class TaskWrapper {
            public Action<T> task;
            public T param;
        }

        private readonly ConcurrentQueue<TaskWrapper> _taskQueue = new ConcurrentQueue<TaskWrapper>();

        // If this method is overriden, the overriding method should call this method.
        protected virtual void Update() {
            while (!_taskQueue.IsEmpty) {
                TaskWrapper taskWrapper;
                if (_taskQueue.TryDequeue(out taskWrapper)) {
                    taskWrapper.task.Invoke(taskWrapper.param);
                }
            }
        }

        /// <summary>Adds a task to the queue.</summary>
        protected void QueueTask(Action<T> task, T param) {
            _taskQueue.Enqueue(new TaskWrapper() {
                task = task,
                param = param
            });
        }

    }

}