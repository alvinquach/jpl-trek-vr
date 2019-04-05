using System;
using System.Threading;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class ThreadedTask<PROGRESS, RESULT> {

        /// <summary>
        ///     Registers a callback function that is called when progress is updated.
        ///     It is up to the implementing classes to manually call the callback
        ///     function when it updates the progress.
        /// </summary>
        public event Action<PROGRESS> OnProgressChange = progress => { };

        public TaskStatus Status { get; private set; } = TaskStatus.NotStarted;

        public abstract PROGRESS Progress { get; }

        /// <summary>
        ///     Execute the task asynchronously in a new thread.
        /// </summary>
        public void Execute(Action<RESULT> callback = null) {
            if (Status >= TaskStatus.InProgress) {
                // TODO Throw exception.
                return;
            }

            Status = TaskStatus.InProgress;

            ThreadPool.QueueUserWorkItem((state) => {
                try {
                    RESULT result = Task();
                    callback?.Invoke(result);
                    Status = TaskStatus.Completed;
                } catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            });
        }

        /// <summary>
        ///     Execute the task synchronously in the current thread.
        /// </summary>
        public RESULT ExecuteInCurrentThread() {
            return Task();
        }

        protected abstract RESULT Task();

        public static bool operator true(ThreadedTask<PROGRESS, RESULT> o) {
            return o != null;
        }

        public static bool operator false(ThreadedTask<PROGRESS, RESULT> o) {
            return o == null;
        }

        public static bool operator !(ThreadedTask<PROGRESS, RESULT> o) {
            return o ? false : true;
        }

    }

}
