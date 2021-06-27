using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OrchestrateSolver
{
    internal class Program
    {
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
            
            // Our result handler simply prints the states to a file.
            void HandleResults(GameState state)
            {
                // ReSharper disable once AccessToDisposedClosure
                file.WriteLine(state.Letters);
            }
            
            // Our success predicate accepts any solution that produces points.
            bool SuccessPredicate(GameState gameState) => gameState.GetTotalResourceProduction(Resource.Points) > 0;

            // Call the actual Solve algorithm.
            Solver.Solve(SuccessPredicate, HandleResults);
            
            // Close the output file
            file.Close();
        }
    }
}