// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CubeHack.FrontEnd
{
    /// <summary>
    /// <para>
    /// Implements a single-threaded UI/rendering event queue (similar to the Dispatcher class in WPF), which
    /// acts as a SynchronizationContext for async/await.
    /// </para>
    /// <para>
    /// OpenGL and all window event handling need to be run from a single main thread, but using async/await,
    /// we can offload expensive things to worker threads. This allows us to render and react to user events
    /// while connecting to the server, loading data, etc.
    /// </para>
    /// </summary>
    public class GameLoop
    {
        private readonly object _mutex = new object();
        private readonly Queue<QueuedAction> _queuedActions = new Queue<QueuedAction>();
        private Thread _loopThread;
        private bool _shouldQuit;
        private bool _isRunning;

        [DependencyInjected]
        public GameLoop()
        {
        }

        /// <summary>
        /// Raised when the next frame should be rendered.
        /// </summary>
        internal event Action<RenderInfo> RenderFrame;

        /// <summary>
        /// Resets the game loop to its initial state.
        /// </summary>
        public void Reset()
        {
            lock (_mutex)
            {
                _shouldQuit = false;
            }
        }

        /// <summary>
        /// Quits the game loop. This call always terminates with a GameLoopExitException.
        /// </summary>
        public void Quit()
        {
            lock (_mutex)
            {
                _shouldQuit = true;
            }

            throw new GameLoopExitException();
        }

        /// <summary>
        /// Adds a new action to the end of the event queue, to be run asynchronously.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
        public void Post(Action action)
        {
            if (action == null) return;
            PostInternal(new QueuedAction(action));
        }

        /// <summary>
        /// Runs the game loop, until the window receives a quit event or GameLoop.Quit() is called.
        /// </summary>
        /// <param name="window">The window for which to run the game loop.</param>
        public void Run(GameWindow window)
        {
            lock (_mutex)
            {
                if (_isRunning) throw new InvalidOperationException();

                _isRunning = true;
                _shouldQuit = false;
                _loopThread = Thread.CurrentThread;
            }

            var oldSynchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new GameSynchronizationContext(this));
            try
            {
                while (true)
                {
                    ProcessWindowEvents(window);
                    RenderFrame?.Invoke(new RenderInfo { Width = window.Width, Height = window.Height });
                    ProcessWindowEvents(window);
                    window.SwapBuffers();
                }
            }
            catch (GameLoopExitException)
            {
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldSynchronizationContext);

                List<QueuedAction> actionsToAbort;
                lock (_mutex)
                {
                    _loopThread = null;
                    _shouldQuit = true;
                    _isRunning = false;
                    actionsToAbort = new List<QueuedAction>(_queuedActions);
                    _queuedActions.Clear();
                }

                foreach (var queuedAction in actionsToAbort)
                {
                    queuedAction.Abort();
                }
            }
        }

        /// <summary>
        /// Adds a new action to the end of the event queue, and waits until the action is processed.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
        public void Send(Action action)
        {
            SendInternal(new QueuedAction(action));
        }

        private void PostInternal(QueuedAction queuedAction)
        {
            lock (_mutex)
            {
                if (_shouldQuit) return;
                _queuedActions.Enqueue(queuedAction);
            }
        }

        private void ProcessWindowEvents(GameWindow window)
        {
            if (window.IsExiting) throw new GameLoopExitException();
            window.ProcessEvents();
            while (!window.IsExiting && ProcessSingleQueuedAction()) ;
            if (window.IsExiting) throw new GameLoopExitException();
        }

        private bool ProcessSingleQueuedAction()
        {
            QueuedAction nextAction;
            lock (_mutex)
            {
                if (_shouldQuit) throw new GameLoopExitException();
                if (_queuedActions.Count == 0) return false;
                nextAction = _queuedActions.Dequeue();
            }

            nextAction.Execute();
            return true;
        }

        private void SendInternal(QueuedAction queuedAction)
        {
            ManualResetEventSlim completedEvent = null;

            try
            {
                lock (_mutex)
                {
                    if (_shouldQuit) throw new GameLoopExitException();

                    if (Thread.CurrentThread != _loopThread)
                    {
                        /* If we come from a different thread, create an event so that we can be notified. */
                        completedEvent = new ManualResetEventSlim();
                        queuedAction.SetCompletedEvent(completedEvent);
                    }

                    _queuedActions.Enqueue(queuedAction);
                }

                if (completedEvent != null)
                {
                    /* We come from a different thread. Wait for the event to be processed. */
                    completedEvent.Wait();
                }
                else
                {
                    /* We are in the main thread. Process queued actions until our action is finished. */
                    while (ProcessSingleQueuedAction() && !queuedAction.IsCompleted) ;
                }

                /* Exceptions should bubble up to the caller. */
                queuedAction.ThrowIfNotSuccessful();
            }
            finally
            {
                if (completedEvent != null) completedEvent.Dispose();
            }
        }

        private class GameLoopExitException : Exception
        {
        }

        private class GameSynchronizationContext : SynchronizationContext
        {
            private readonly GameLoop _gameLoop;

            public GameSynchronizationContext(GameLoop gameLoop)
            {
                _gameLoop = gameLoop;
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                if (d == null) return;
                _gameLoop.PostInternal(new QueuedAction(d, state));
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                _gameLoop.SendInternal(new QueuedAction(d, state));
            }
        }

        private class QueuedAction
        {
            private readonly Action _action;
            private readonly SendOrPostCallback _callback;
            private readonly object _state;
            private volatile ManualResetEventSlim _completedEvent;
            private volatile Exception _exception;
            private volatile bool _isCompleted;

            public QueuedAction(Action action)
            {
                _action = action;
            }

            public QueuedAction(SendOrPostCallback callback, object state)
            {
                _callback = callback;
                _state = state;
            }

            public bool IsCompleted { get { return !_isCompleted; } }

            public void Abort()
            {
                _exception = new OperationCanceledException();
                _isCompleted = true;
                if (_completedEvent != null) _completedEvent.Set();
            }

            public void ThrowIfNotSuccessful()
            {
                if (!_isCompleted) throw new InvalidOperationException();
                if (_exception != null) throw _exception;
            }

            public void Execute()
            {
                try
                {
                    _action?.Invoke();
                    _callback?.Invoke(_state);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                    throw;
                }
                finally
                {
                    _isCompleted = true;
                    _completedEvent?.Set();
                }
            }

            public void SetCompletedEvent(ManualResetEventSlim completedEvent)
            {
                _completedEvent = completedEvent;
            }
        }
    }
}
