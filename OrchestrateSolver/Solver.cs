using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrchestrateSolver
{
    public class Solver
    {
        //! Defines a predicate that judges GameStates as successful or not.
        public delegate bool AcceptStatePredicate(GameState gameState);

        //! Defines a delegate that processes successful GameState results.
        public delegate void ResultHandler(GameState result);
        
        //! Defines a delegate that processes GameStates to be considered in the next wave.
        // Note: Read about the wave-based approach in the comments of Solve(...) below.
        public delegate void NextWaveHandler(GameState gameState);
        
        //! Contains some useful information for reporting progress of the Solver.
        public class SolverProgress
        {
            private int _waveIndex;           //!< Current wave. Equal to the number of Verbs active in all states in the current wave.
            private int _itemsScanned;        //!< How many items have been scanned so far
            private int _itemsEliminated;     //!< How many items have been eliminated (either inviable or no need to scan)
            private int _resultsFound;        //!< How many results have been found
            
            private int _nextMessageItem;     //!< Threshold of ItemsScanned at which another status message will be printed.
            private int _messageFrequency;    //!< How often a status message is printed as number of ItemsScanned.

            //! Total number of all possible game states, used to computer progress percentages.
            private const uint MaxItemsScannable = 1u << Verbs.Count;
            
            // Lock, used for parallel access protection
            private object _lock;
            
            public SolverProgress(int waveIndex = 0, int itemsScanned = 0, int itemsEliminated = 0, int resultsFound = 0, int messageFrequency = 50000)
            {
                this._waveIndex = waveIndex;
                this._itemsScanned = itemsScanned;
                this._itemsEliminated = itemsEliminated;
                this._resultsFound = resultsFound;
                this._nextMessageItem = messageFrequency;
                this._messageFrequency = messageFrequency;
                this._lock = new object();
            }

            public int WaveIndex => _waveIndex;
            
            public int IncrementWaveIndex()
            {
                return Interlocked.Increment(ref _waveIndex);
            }

            public int ItemsScanned => _itemsScanned;
            public int IncrementItemsScanned()
            {
                return Interlocked.Increment(ref _itemsScanned);
            }

            public int ItemsEliminated => _itemsEliminated;
            public int AddItemsEliminated(int count)
            {
                return Interlocked.Add(ref _itemsEliminated, count);
            }

            public int ResultsFound => _resultsFound;
            public int IncrementResultsFound()
            {
                return Interlocked.Increment(ref _resultsFound);
            }

            /**!
             * Prints a status message indicating progress every so often. Will always print if forced is true.
             * @param forced Whether a message should be printed regardless of configured message frequency.
             */
            public void PrintStatus(bool forced = false)
            {
                lock (_lock)
                {
                    if (forced || _itemsScanned >= _nextMessageItem)
                    {
                        while (_itemsScanned >= _nextMessageItem) _nextMessageItem += _messageFrequency;
                        var scannedPercent = _itemsScanned * 100.0 / MaxItemsScannable;
                        var eliminatedPercent = _itemsEliminated * 100.0 / MaxItemsScannable;
                        var totalPercent = (_itemsScanned + _itemsEliminated) * 100.0 / MaxItemsScannable;
                        Console.WriteLine(
                            $"Progress {totalPercent:F1}%: {_resultsFound} solutions. [Scanned {_itemsScanned} ({scannedPercent:F1}%), Eliminated {_itemsEliminated} ({eliminatedPercent:F1}%)]");
                    }
                }
            }
        }
                
        /*!
         * Solves for all GameStates that are stable (no negative resource production) and match
         * the provided predicate. Successful GameStates are returned through the resultHandler delegate.
         *
         * The solver considers GameStates in waves, with the number of active Verbs increasing each wave.
         * In the first wave, only the Luxuriate verb is active. For each GameState in the current wave,
         * more GameStates will be added to the next wave based on whether the current GameState has
         * negative resource production or not (and whether that negative resource production makes
         * the GameState Non-Viable).
         *
         * This wave-based approach allows me to easily keep track of which GameStates have already been
         * considered and avoid considering them twice. All GameStates with fewer Verbs active than the
         * current wave will have already been considered. It also allows the progress messages to compute
         * an exact percentage of how finished the whole process is.
         */ 
        public static void Solve(AcceptStatePredicate predicate, ResultHandler resultHandler, int numThreads = 16)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            // The next wave handler will add each discovered GameState to a HashSet.
            var next = new HashSet<uint>();
            var nextWaveHandler = new NextWaveHandler(state => next.Add(state));

            // Beginning state: no verbs.
            var beginState = new GameState();
            var currentWave = new List<GameState>() { beginState };

            // The solver context starts with initial values
            var context = new SolverProgress();

            while (currentWave.Any())
            {
                Console.WriteLine($"Begin wave {context.WaveIndex}/{Verbs.Count}: {currentWave.Count} states");
                
                SolveWave(context, currentWave, predicate, resultHandler, nextWaveHandler, numThreads);

                // Adopt next wave variables into current wave variables
                currentWave = next.Select(v => new GameState(v)).ToList();
                context.IncrementWaveIndex();
                
                // Clear next wave variables
                next.Clear();
                
                // Eliminate theoretical states that were not added to next wave
                var eliminatedStates = 0;
                if (context.WaveIndex + 1 <= Verbs.Count)
                {
                    var theoreticalStates = NChooseR(Verbs.Count, context.WaveIndex);
                    eliminatedStates = (int)(theoreticalStates - currentWave.Count);
                    context.AddItemsEliminated(eliminatedStates);
                }
                Console.WriteLine($"Wave {context.WaveIndex}/26 completed. Eliminated {eliminatedStates} states.\n");
            }
            
            Console.WriteLine($"Wave {context.WaveIndex} empty. Complete.");
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            var elapsedSeconds = elapsedMs / 1000.0;
            Console.WriteLine($"Solved in {elapsedSeconds:F1} seconds ({elapsedMs} ms).");
        }
        
        //! Combinatorics function used to compute the number of states possible within a given wave.
        private static decimal NChooseR(int n, int r)
        {
            decimal result = 1;
            for (var i = 1; i <= r; i++)
            {
                result *= n - (r - i);
                result /= i;
            }
            return result;
        }

        private static void SolveWave(SolverProgress progress, IEnumerable<GameState> currentWaveStates, AcceptStatePredicate predicate, ResultHandler resultHandler, NextWaveHandler nextWaveHandler, int numThreads)
        {
            var currentWaveResults = new List<GameState>();
            
            // Wrap the resultHandler so we can count the number of returned results.
            var resultCounter = new ResultHandler(result =>
            {
                currentWaveResults.Add(result);
                progress.IncrementResultsFound();
            });

            void ScanState(GameState gameState)
            {
                // Scan each GameState in the current wave.
                Scan(gameState, progress, predicate, resultCounter, nextWaveHandler);
                progress.IncrementItemsScanned();

                // Print a status message every once in a while
                progress.PrintStatus();
            }
            
            // Iterate over all GameState in the current wave.
            if (numThreads > 1) Parallel.ForEach(currentWaveStates, new ParallelOptions() { MaxDegreeOfParallelism = numThreads }, ScanState);
            else foreach (var gameState in currentWaveStates) ScanState(gameState);

            // Aggregate wave results in predictable order.
            currentWaveResults.Sort();
            lock(resultHandler)
            {
                foreach (var state in currentWaveResults)
                {
                    resultHandler(state);
                }
            }
            
            // Print at least one status message at the end of each wave
            progress.PrintStatus(forced: true);
        }

        private static bool IsASubsetOfB(GameState a, GameState b)
        {
            return (a & b) == a;
        }
        
        /*!
         * Scans a GameState, checking if it valid and, if not, whether it is possible to fix it by adding more Verbs.
         * @param gameState The GameState to scan
         * @param predicate A predicate used to determine if a game state should be considered successful
         * @param resultHandler A function that processes any GameStates that pass the predicate successfully.
         * @param nextWaveHandler A function that receives any GameStates which should be examined next wave.
         */
        private static void Scan(GameState gameState, SolverProgress progress, AcceptStatePredicate successPredicate, ResultHandler resultHandler, NextWaveHandler nextWaveHandler)
        {
            // Is this game state valid?
            if (gameState.IsValid)
            {
                
                // Is this a successful result?
                if (successPredicate(gameState))
                {
                    lock(resultHandler) resultHandler(gameState);
                }
            
                // We can keep checking more complex GameStates by adding 
                // a GameState to the next wave for each available Verb.
                foreach (var verb in gameState.AvailableVerbs)
                {
                    lock(nextWaveHandler) nextWaveHandler(gameState.WithVerb(verb));
                }
            }
            else
            {
                // This GameState is Invalid, can it be restored by adding more Verbs?
                if (!gameState.IsViable(out var impossibleErrorString))
                {
                    // State is inviable.
                    return;
                }
            
                // Continue with only verbs that help resolve one of the current deficits.
                foreach (var verb in gameState.GetDesiredVerbs())
                {
                    lock(nextWaveHandler) nextWaveHandler(gameState.WithVerb(verb));
                }
            }
            
        }
    }
}