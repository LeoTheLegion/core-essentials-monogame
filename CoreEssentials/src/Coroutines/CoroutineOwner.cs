using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Coroutines
{
    /// <summary>
    /// Acts as an owner of coroutines that will be stopped when the owner is disposed.
    /// This allows for automatic cleanup of coroutines when their owner is destroyed.
    /// </summary>
    public class CoroutineOwner : IDisposable
    {
        // Dictionary of active coroutine IDs and their names for this owner
        private readonly Dictionary<Guid, string> _activeCoroutines = new Dictionary<Guid, string>();
        
        // Lock object for thread-safety
        private readonly object _syncLock = new object();
        
        // Flag to prevent double disposal
        private bool _isDisposed = false;

        /// <summary>
        /// Starts a new coroutine and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public Guid StartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine, "Anonymous", true);
        }
        
        /// <summary>
        /// Starts a new coroutine with error handling control and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <param name="allowFailure">If true, exceptions are caught and logged; if false, exceptions are thrown immediately.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public Guid StartCoroutine(IEnumerator routine, bool allowFailure)
        {
            return StartCoroutine(routine, "Anonymous", allowFailure);
        }

        /// <summary>
        /// Starts a new coroutine with a name and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <param name="name">A name for the coroutine, useful for debugging.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public Guid StartCoroutine(IEnumerator routine, string name)
        {
            return StartCoroutine(routine, name, true);
        }
        
        /// <summary>
        /// Starts a new coroutine with a name and error handling control, and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <param name="name">A name for the coroutine, useful for debugging.</param>
        /// <param name="allowFailure">If true, exceptions are caught and logged; if false, exceptions are thrown immediately.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public Guid StartCoroutine(IEnumerator routine, string name, bool allowFailure)
        {
            lock (_syncLock)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("CoroutineOwner", "Cannot start a coroutine on a disposed CoroutineOwner.");
                }
                
                Guid id = CoroutineManager.StartCoroutine(routine, allowFailure);
                _activeCoroutines[id] = name;
                return id;
            }
        }

        /// <summary>
        /// Stops a coroutine with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the coroutine to stop.</param>
        /// <returns>True if the coroutine was successfully stopped; otherwise, false.</returns>
        public bool StopCoroutine(Guid id)
        {
            lock (_syncLock)
            {
                if (_activeCoroutines.ContainsKey(id))
                {
                    _activeCoroutines.Remove(id);
                    return CoroutineManager.StopCoroutine(id);
                }
                return false;
            }
        }

        /// <summary>
        /// Stops all coroutines owned by this instance.
        /// </summary>
        public void StopAllCoroutines()
        {
            lock (_syncLock)
            {
                foreach (var id in _activeCoroutines.Keys)
                {
                    CoroutineManager.StopCoroutine(id);
                }
                _activeCoroutines.Clear();
            }
        }

        /// <summary>
        /// Returns the number of active coroutines owned by this instance.
        /// </summary>
        public int ActiveCoroutineCount
        {
            get
            {
                lock (_syncLock)
                {
                    return _activeCoroutines.Count;
                }
            }
        }

        /// <summary>
        /// Disposes the CoroutineOwner and stops all owned coroutines.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes the CoroutineOwner and stops all owned coroutines.
        /// </summary>
        /// <param name="disposing">True if being called from Dispose(); false if being called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    StopAllCoroutines();
                }
                _isDisposed = true;
            }
        }
        
        /// <summary>
        /// Finalizer to ensure coroutines are stopped if Dispose is not called.
        /// </summary>
        ~CoroutineOwner()
        {
            Dispose(false);
        }
    }
}