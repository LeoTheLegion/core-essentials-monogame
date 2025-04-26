using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Collections;
using CoreEssentials.Debugging;

namespace CoreEssentials.Coroutines
{
    /// <summary>
    /// Manages coroutines in a centralized system.
    /// This static class tracks all running coroutines and updates them during the game loop.
    /// </summary>
    public static class CoroutineManager
    {
        /// <summary>
        /// Dictionary of active coroutines mapped by their unique identifiers.
        /// </summary>
        private static readonly Dictionary<Guid, IEnumerator> _activeCoroutines = new Dictionary<Guid, IEnumerator>();
        
        /// <summary>
        /// Dictionary of current yield instructions for coroutines.
        /// </summary>
        private static readonly Dictionary<Guid, IYieldInstruction> _currentYieldInstructions = new Dictionary<Guid, IYieldInstruction>();
        
        /// <summary>
        /// Dictionary mapping coroutines to their nested child coroutines.
        /// </summary>
        private static readonly Dictionary<Guid, Guid> _nestedCoroutines = new Dictionary<Guid, Guid>();
        
        /// <summary>
        /// List of coroutines that need to be removed at the end of the update cycle.
        /// </summary>
        private static readonly List<Guid> _coroutinesToRemove = new List<Guid>();

        // Lock object for thread-safety
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Starts a new coroutine and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public static Guid StartCoroutine(IEnumerator routine)
        {
            lock (_syncLock)
            {
                Guid id = Guid.NewGuid();
                _activeCoroutines.Add(id, routine);
                return id;
            }
        }

        /// <summary>
        /// Stops a coroutine with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the coroutine to stop.</param>
        /// <returns>True if the coroutine was successfully stopped; otherwise, false.</returns>
        public static bool StopCoroutine(Guid id)
        {
            lock (_syncLock)
            {
                if (_activeCoroutines.ContainsKey(id))
                {
                    // If this coroutine has a nested coroutine, stop it as well
                    if (_nestedCoroutines.TryGetValue(id, out Guid childId))
                    {
                        StopCoroutine(childId);
                        _nestedCoroutines.Remove(id);
                    }
                    
                    _activeCoroutines.Remove(id);
                    _currentYieldInstructions.Remove(id);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Updates all active coroutines.
        /// This method should be called once per frame.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            lock (_syncLock)
            {
                _coroutinesToRemove.Clear();
                
                // Use a separate list to iterate through keys safely while modifying the dictionary
                List<Guid> activeCoroutineIds = new List<Guid>(_activeCoroutines.Keys);
                
                // Process each coroutine
                foreach (var id in activeCoroutineIds)
                {
                    // Skip if this coroutine has already been removed
                    if (!_activeCoroutines.ContainsKey(id))
                        continue;
                        
                    IEnumerator routine = _activeCoroutines[id];
                    
                    bool shouldContinue = true;
                    
                    // Skip this coroutine if it's waiting for a nested coroutine to complete
                    if (_nestedCoroutines.TryGetValue(id, out Guid childCoroutineId))
                    {
                        // If the nested coroutine is still active, skip the parent
                        if (_activeCoroutines.ContainsKey(childCoroutineId))
                        {
                            shouldContinue = false;
                        }
                        else
                        {
                            // Nested coroutine is done, remove the reference
                            _nestedCoroutines.Remove(id);
                        }
                    }
                    
                    // Check if the coroutine is waiting on a yield instruction
                    if (shouldContinue && _currentYieldInstructions.TryGetValue(id, out var yieldInstruction))
                    {
                        if (!yieldInstruction.IsComplete(gameTime))
                        {
                            // Yield instruction not complete yet, skip to next coroutine
                            shouldContinue = false;
                        }
                        else
                        {
                            // Yield instruction is complete, remove it and continue execution
                            _currentYieldInstructions.Remove(id);
                        }
                    }
                    
                    if (shouldContinue)
                    {
                        bool hasMoreSteps;
                        
                        try
                        {
                            hasMoreSteps = routine.MoveNext();
                        }
                        catch (Exception e)
                        {
                            // Log exception and remove the faulty coroutine
                            Debug.Console.WriteLine($"Error in coroutine: {e.Message}");
                            _coroutinesToRemove.Add(id);
                            continue;
                        }
                        
                        if (!hasMoreSteps)
                        {
                            // Coroutine has completed
                            _coroutinesToRemove.Add(id);
                            continue;
                        }
                        
                        // Handle yield return values
                        if (routine.Current != null)
                        {
                            if (routine.Current is IYieldInstruction currentYieldInstruction)
                            {
                                _currentYieldInstructions[id] = currentYieldInstruction;
                            }
                            else if (routine.Current is IEnumerator nestedEnumerator)
                            {
                                // Start the nested coroutine
                                Guid newNestedId = StartCoroutine(nestedEnumerator);
                                // Link the parent to the nested coroutine
                                _nestedCoroutines[id] = newNestedId;
                            }
                        }
                    }
                }
                
                // Remove completed coroutines
                foreach (var id in _coroutinesToRemove)
                {
                    if (_activeCoroutines.ContainsKey(id))
                    {
                        _activeCoroutines.Remove(id);
                        _currentYieldInstructions.Remove(id);
                    }
                }
            }
        }
        
        /// <summary>
        /// Stops all active coroutines.
        /// </summary>
        public static void StopAllCoroutines()
        {
            lock (_syncLock)
            {
                _activeCoroutines.Clear();
                _currentYieldInstructions.Clear();
                _nestedCoroutines.Clear();
            }
        }
        
        /// <summary>
        /// Gets the number of active coroutines.
        /// </summary>
        public static int ActiveCoroutineCount
        {
            get
            {
                lock (_syncLock)
                {
                    return _activeCoroutines.Count;
                }
            }
        }
    }
}