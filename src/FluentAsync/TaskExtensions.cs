// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Threading.Tasks;

namespace FluentAsync {
    public static class TaskExtensions {
        public static Task<Tuple<TR1, TR2, TR3>> InvokeAsync<T1, TR1, TR2, TR3>(this Action<T1, Action<Exception>, Action<TR1, TR2, TR3>> action, T1 t1) {
            var source = new DisposableTaskCompletionSource<Tuple<TR1, TR2, TR3>>(TaskCreationOptions.AttachedToParent);
            try {
                action(t1, source.SetException, (tr1, tr2, tr3) => source.SetResult(Tuple.Create(tr1, tr2, tr3)));
            }
            catch (Exception ex) {
                source.SetException(ex);
            }
            return source.Task;
        }


        public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> continuation) {
            return task.ContinueWith(t => continuation(t.Result),
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.OnlyOnRanToCompletion |
                        TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task Then(this Task task, Action continuation) {
            return task.ContinueWith(t => continuation(),
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.OnlyOnRanToCompletion |
                        TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task Then<T>(this Task<T> task, Action<T> continuation) {
            return task.ContinueWith(t => continuation(t.Result),
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.OnlyOnRanToCompletion |
                        TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task Then<T1, T2, T3>(this Task<Tuple<T1, T2, T3>> task, Action<T1, T2, T3> continuation) {
            return task.ContinueWith(t => continuation(t.Result.Item1, t.Result.Item2, t.Result.Item3),
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.OnlyOnRanToCompletion |
                        TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task<T> Catch<T>(this Task<T> task, Func<Exception, T> handler) {
            return task.ContinueWith(t => t.IsFaulted ? handler(t.Exception) : t.Result,
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task Catch(this Task task, Action<Exception> handler) {
            return task.ContinueWith(t => { if (t.IsFaulted) handler(t.Exception); },
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task Finally(this Task task, Action continuation) {
            return task.ContinueWith(t => continuation(),
                TaskContinuationOptions.AttachedToParent |
                    TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}