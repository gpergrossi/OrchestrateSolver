using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OrchestrateSolver
{
    internal class Program
    {
        //! Defines a predicate that judges GameStates as successful or not.
        public delegate bool AcceptStatePredicate(GameState gameState);

        //! Defines a delegate that processes successful GameState results.
        public delegate void ResultHandler(GameState result);
        
        //! Defines a delegate that processes GameStates to be considered in the next wave.
        // Note: Read about the wave-based approach in the comments of DoSolve(...) below.
        public delegate void NextWaveHandler(GameState gameState);
        
        public static void Main(string[] args)
        {
            // Prints some info about the game configuration (As defined in the Resource and Verbs classes)
            // This allows you to sanity check the Solver's assumptions before it gets to work.
            var maxProduction = new Dictionary<Resource, int>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource)))
            {
                maxProduction[resource] = Verbs.All.Select(verb => verb.Resources[(int)resource]).Where(v => v > 0).Sum();
                Console.WriteLine("Max production of " + Enum.GetName(resource) + " is " + maxProduction[resource]);
            }
            foreach (var verb in Verbs.Where(verb => true))
            {
                Console.WriteLine(verb.ToString());
            }
            
            // Prepare output file
            using StreamWriter file = new("Solutions.txt", false);
            void HandleResults(GameState state)
            {
                // ReSharper disable once AccessToDisposedClosure
                file.WriteLine(state.Letters);
            }

            // Call the actual Solve algorithm.
            DoSolve(SuccessPredicate, HandleResults);
            
            // Close the output file
            file.Close();
        }

        //! Our SuccessPredicate returns true if a GameState produces Points.
        // Note: Only GameStates that have no negative resource productions
        // are considered by this delegate, that condition is built in
        // because it greatly prunes the search space.
        public static bool SuccessPredicate(GameState gameState)
        {
            return gameState.GetTotalResourceProduction(Resource.Points) > 0;
        }

        //! Contains some useful information for reporting progress of the Solver.
        public class SolverContext
        {
            public int ItemsScanned;
            public int ResultsFound;
            public int NextMessageItem;
            public int Depth;

            public SolverContext(int itemsScanned = 0, int resultsFound = 0, int nextMessageItem = 10000, int depth = 0)
            {
                this.ItemsScanned = itemsScanned;
                this.ResultsFound = resultsFound;
                this.NextMessageItem = nextMessageItem;
                this.Depth = depth;
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
         * current wave will have already been considered. It also allows the progress messages to give
         * a better idea of how finished the whole process is. There are theoretically 26 waves, but in
         * practice all of the GameStates beyond wave 22 become Un-Viable.
         */ 
        public static void DoSolve(AcceptStatePredicate predicate, ResultHandler resultHandler)
        {
            // The next wave handler will add each discovered GameState to a HashSet.
            var next = new HashSet<uint>();
            NextWaveHandler nextWaveHandler = new NextWaveHandler(state => next.Add(state));
            
            // The initial GameState and first wave will consist of a single GameState with only the
            // Luxuriate Verb active. This verb is necessary for all winning conditions. Adding it to
            // the initial GameState allows the solver to find solutions much more quickly, as it
            // narrows the search scope to Verbs that pay for this Verb's costs.
            var beginState = new GameState().WithVerb(Verbs.Luxuriate);
            var current = new List<GameState>() { beginState };

            // The solver context starts with initial values
            var context = new SolverContext();

            while (current.Any())
            {
                SolveIterative(context, current.GetEnumerator(), predicate, resultHandler, nextWaveHandler);

                current = next.Select(v => new GameState(v)).ToList();
                next.Clear();
                
                context.Depth++;
            }
        }
        
        private static void SolveIterative(SolverContext context, IEnumerator<GameState> currentWaveStates, AcceptStatePredicate predicate, ResultHandler resultHandler, NextWaveHandler nextWaveHandler)
        {
            // Wrap the resultHandler so we can count the number of returned results.
            var resultCounter = new ResultHandler(result =>
            {
                resultHandler(result);
                context.ResultsFound++;
            });

            Console.WriteLine("Begin wave " + (context.Depth+1) + "/22");
            
            // This value represents a total count of all possible game states.
            const uint MaxItemsScannable = 1u << Verbs.Count;
            
            // Iterate over all GameState in the current wave.
            GameState gameState = null;
            while (currentWaveStates.MoveNext())
            {
                // Get the current GameState from the currentWaveStatus Enumerator.
                gameState = currentWaveStates.Current;
                if (gameState == null) throw new Exception("Null state in currentWaveStates!");
                
                // Scan each GameState in the current wave.
                Scan(gameState, predicate, resultCounter, nextWaveHandler);
                
                // Print a status message every once in a while
                context.ItemsScanned++;
                if (context.ItemsScanned >= context.NextMessageItem)
                {
                    context.NextMessageItem += 10000;
                    Console.WriteLine(context.ItemsScanned + " (Max " + MaxItemsScannable + "): " + gameState.ToBinary() + " (" + context.ResultsFound + " solutions found)");
                }
            }

            // Print at least one status message at the end of each wave
            if (gameState != null)
            {
                Console.WriteLine(context.ItemsScanned + " (Max " + MaxItemsScannable + "): " + gameState.ToBinary() + " (" + context.ResultsFound + " solutions found)");
            }
        }
    
        /*!
         * Scans a GameState, checking if it valid and, if not, whether it is possible to fix it by adding more Verbs.
         * @param gameState The GameState to scan
         * @param predicate A predicate used to determine if a game state should be considered successful
         * @param resultHandler A function that processes any GameStates that pass the predicate successfully.
         * @param nextWaveHandler A function that receives any GameStates which should be examined next wave.
         */
        private static void Scan(GameState gameState, AcceptStatePredicate predicate, ResultHandler resultHandler, NextWaveHandler nextWaveHandler)
        {
            // Is this game state valid?
            if (gameState.IsValid)
            {
                // Is this a successful result?
                if (predicate(gameState))
                {
                    resultHandler(gameState);
                }

                // We can keep checking more complex GameStates by adding
                // a GameState to the next wave for each Verb this GameState
                // is not currently using.
                foreach (var verb in gameState.AvailableVerbs())
                {
                    nextWaveHandler(gameState.WithVerb(verb));
                }
            }
            else
            {
                // This GameState is Invalid, can it be restored by adding more Verbs?
                if (!gameState.IsViable(out var impossibleErrorString)) return;
                
                // For each verb that would improve this GameState's deficits, add a 
                // new GameState with that verb, to be examined next wave.
                foreach (var verb in gameState.GetDesiredVerbs())
                {
                    nextWaveHandler(gameState.WithVerb(verb));
                }
            }
        }
    }
}