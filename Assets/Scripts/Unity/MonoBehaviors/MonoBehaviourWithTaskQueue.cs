using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace TrekVRApplication {

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
        ///     Adds a task to the queue. This is needed when a task needs
        ///     to be executed after another task has completed outside of
        ///     the main thread. Since modification of MonoBehaviour objects
        ///     are limited outisde of the main thread, any additional tasks
        ///     that update MonoBehaviour objects need to be queued up to be
        ///     executed on the main thread.
        /// </summary>
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

        /// <summary>
        ///     Adds a task to the queue. This is needed when a task needs
        ///     to be executed after another task has completed outside of
        ///     the main thread. Since modification of MonoBehaviour objects
        ///     are limited outisde of the main thread, any additional tasks
        ///     that update MonoBehaviour objects need to be queued up to be
        ///     executed on the main thread.
        /// </summary>
        protected void QueueTask(Action<T> task, T param) {
            _taskQueue.Enqueue(new TaskWrapper() {
                task = task,
                param = param
            });
        }

    }

}