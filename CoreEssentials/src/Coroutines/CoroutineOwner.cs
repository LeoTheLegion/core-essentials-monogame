using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoreEssentials.Coroutines
{
    /// <summary>
    /// Base class for objects that own coroutines.
    /// Provides methods for starting, stopping, and tracking coroutines.
    /// </summary>
    public class CoroutineOwner
    {
        /// <summary>
        /// Dictionary mapping coroutine IDs to their names for easier debugging.
        /// </summary>
        private readonly Dictionary<Guid, string> _activeCoroutines = new Dictionary<Guid, string>();
        
        /// <summary>
        /// Lock object for thread safety
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Starts a new coroutine and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public Guid StartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine, "Anonymous");
        }

        /// <summary>
        /// Starts a new coroutine with a name and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <param name="name">A name for the coroutine, useful for debugging.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public Guid StartCoroutine(IEnumerator routine, string name)
        {
            lock (_syncLock)
            {
                Guid id = CoroutineManager.StartCoroutine(routine);
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
                    bool result = CoroutineManager.StopCoroutine(id);
                    if (result)
                    {
                        _activeCoroutines.Remove(id);
                    }
                    return result;
                }
                return false;
            }
        }

        /// <summary>
        /// Stops all coroutines owned by this object.
        /// </summary>
        public void StopAllCoroutines()
        {
            lock (_syncLock)
            {
                // Create a copy of IDs to avoid modification during enumeration
                Guid[] ids = _activeCoroutines.Keys.ToArray();
                
                // Stop each coroutine individually
                foreach (var id in ids)
                {
                    CoroutineManager.StopCoroutine(id);
                }
                
                // Clear the collection
                _activeCoroutines.Clear();
            }
        }

        /// <summary>
        /// Gets the number of active coroutines owned by this object.
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
        /// Gets the names of all active coroutines owned by this object.
        /// </summary>
        /// <returns>An array of coroutine names.</returns>
        public string[] GetActiveCoroutineNames()
        {
            lock (_syncLock)
            {
                return _activeCoroutines.Values.ToArray();
            }
        }
    }
}