using System;
using System.Threading;

namespace TrekVRApplication {

    public abstract class ThreadedTask<Progress, Result> {

        protected Action<Progress> _progressCallback;

        public TaskStatus Status { get; private set; } = TaskStatus.NotStarted;

        /// <summary>
        ///     Execute the task asynchronously in a new thread.
        /// </summary>
        public void Execute(Action<Result> callback = null) {
            if (Status >= TaskStatus.Started) {
                // TODO Throw exception.
                return;
            }

            Status = TaskStatus.Started;

            ThreadPool.QueueUserWorkItem((state) => {
                Result result = Task();
                callback?.Invoke(result);
                Status = TaskStatus.Completed;
            });
        }

        /// <summary>
        ///     Execute the task synchronously in the current thread.
        /// </summary>
        public Result ExecuteInSameThread() {
            return Task();
        }

        public abstract Progress GetProgress();

        /// <summary>
        ///     Registers a callback function that is called when progress is updated.
        ///     It is up to the implementing classes to manually call the callback
        ///     function when it updates the progress.
        /// </summary>
        public void OnProgressChange(Action<Progress> callback) {
            _progressCallback = callback;
        }

        protected abstract Result Task();

        public static bool operator true(ThreadedTask<Progress, Result> o) {
            return o != null;
        }

        public static bool operator false(ThreadedTask<Progress, Result> o) {
            return o == null;
        }

        public static bool operator !(ThreadedTask<Progress, Result> o) {
            return o ? false : true;
        }

    }

}
