using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Collections;

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

        /// <summary>
        /// Set of coroutine IDs that are marked as unfailable (exceptions should be rethrown).
        /// </summary>
        private static readonly HashSet<Guid> _unfailableCoroutines = new HashSet<Guid>();

        // Lock object for thread-safety
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Starts a new coroutine and returns its identifier.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public static Guid StartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine, true);
        }
        
        /// <summary>
        /// Starts a new coroutine and returns its identifier, with control over exception handling.
        /// </summary>
        /// <param name="routine">The enumerator representing the coroutine.</param>
        /// <param name="allowFailure">If true, exceptions are caught and logged; if false, exceptions are thrown immediately.</param>
        /// <returns>A unique identifier for the coroutine.</returns>
        public static Guid StartCoroutine(IEnumerator routine, bool allowFailure)
        {
            lock (_syncLock)
            {
                Guid id = Guid.NewGuid();
                _activeCoroutines.Add(id, routine);
                
                // If this coroutine doesn't allow failure, track it separately
                if (!allowFailure)
                {
                    _unfailableCoroutines.Add(id);
                }
                
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
                    _unfailableCoroutines.Remove(id);
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
                        bool isUnfailable = _unfailableCoroutines.Contains(id);
                        
                        try
                        {
                            hasMoreSteps = routine.MoveNext();
                        }
                        catch (Exception e)
                        {
                            // Check if this is an unfailable coroutine
                            if (isUnfailable)
                            {
                                // Make sure to remove the coroutine from tracking collections before rethrowing
                                _activeCoroutines.Remove(id);
                                _currentYieldInstructions.Remove(id);
                                _unfailableCoroutines.Remove(id);
                                
                                // Also remove any nested coroutines associated with this one
                                if (_nestedCoroutines.TryGetValue(id, out Guid childId))
                                {
                                    StopCoroutine(childId);
                                    _nestedCoroutines.Remove(id);
                                }
                                
                                throw; // Rethrow the exception
                            }
                            
                            // For normal coroutines, just log the error
                            Console.WriteLine($"Error in coroutine: {e.Message}");
                            _coroutinesToRemove.Add(id);
                            continue;
                        }
                        
                        if (!hasMoreSteps)
                        {
                            // Coroutine has completed
                            _coroutinesToRemove.Add(id);
                            
                            // Also remove from unfailable list if present
                            if (isUnfailable)
                            {
                                _unfailableCoroutines.Remove(id);
                            }
                            
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
                                // Pass down the allowFailure setting from the parent
                                Guid newNestedId = StartCoroutine(nestedEnumerator, !isUnfailable);
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
                        _unfailableCoroutines.Remove(id);
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
                // Clear all tracking collections at once to ensure consistency
                _activeCoroutines.Clear();
                _currentYieldInstructions.Clear();
                _nestedCoroutines.Clear();
                _coroutinesToRemove.Clear();
                _unfailableCoroutines.Clear();
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