using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrchestrateSolver
{
    /**
     * Represents a game state in Orchestrate. The only data is a set of active verbs, which can
     * be represented as a single uint. This makes this class very light weight. A large number
     * of convenience methods are provided to aid in understanding the GameState and making
     * calculations about it.
     */
    public class GameState
    {
        //! Private VerbSet of all the active verbs in this GameState
        private readonly VerbSet _activeVerbs;

        //! Public ReadOnlyList accessor to the active verbs in this GameState
        public IReadOnlyList<Verb> VerbsUsed => _activeVerbs.ToList().AsReadOnly();

        //! Allows implicit conversion from GameState to uint
        public static implicit operator uint(GameState v)  {  return v._activeVerbs.Value;  }
        
        //! Constructor
        public GameState(uint state = 0)
        {
            _activeVerbs = new VerbSet(state);
        }

        //! Hash GameStates based on their uint identity.
        public override int GetHashCode()
        {
            return (int) _activeVerbs.Value;
        }
        
        //! Returns how much of a given resource this GameState produces (or consumes, can be negative)
        public int GetTotalResourceProduction(Resource resource)
        {
            return _activeVerbs.Sum(verb => verb.GetResourceProduction(resource));
        }

        //! Checks if the given Verb is active within this GameState
        public bool IsVerbActive(Verb verb)
        {
            return _activeVerbs.Contains(verb);
        }

        /*! Returns true only if this GameState consumes no Resources
         * (No resource returns a negative value from GetResourceProduction()). */
        public bool IsValid => !NegativeResources.Any();

        //! Returns a string representation of this GameState
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("State[");
            foreach (var verb in this.VerbsUsed)
            {
                sb.Append(verb.Letter);
            }
            sb.Append(']');
            return sb.ToString();
        }
        
        //! Returns a binary string representation of this GameState
        public string ToBinary()
        {
            return Convert.ToString((uint) this, 2).PadLeft(27, '0');
        }
        
        //! Returns a string representation of this GameState showing the active Verb's letters.
        public string Letters {
            get {
                var sb = new StringBuilder();
                foreach (var verb in this.VerbsUsed)
                {
                    sb.Append(verb.Letter);
                }
                return sb.ToString();
            }
        }


        //! Creates a new GameState that is similar to this GameState but with a new Verb active.
        public GameState WithVerb(Verb verb)
        {
            if (this._activeVerbs.Contains(verb)) throw new Exception("Verb " + verb.Name + " is already included!");

            var newState = new GameState(this);
            newState._activeVerbs.Add(verb);

            return newState;
        }

        //! Returns a list of all Verbs that are NOT currently active in this GameState.
        public IEnumerable<Verb> AvailableVerbs()
        {
            return Verbs.Where(verb => !IsVerbActive(verb));
        }
        
        //! Returns a list of Resources that have negative production values in this GameState.
        public IEnumerable<Resource> NegativeResources => Enum.GetValues(typeof(Resource)).Cast<Resource>().Where(resource => this.GetTotalResourceProduction(resource) < 0);

        /*!
         * Checks if this GameState is still a viable. A GameState is considered Non-Viable if
         * one or more of its NegativeResources are too far negative to be fixed by the sum of
         * all AvailableVerbs' productions of that resource.
         * 
         * @return True if this GameState is viable to bring positive again, else false and the search should not continue from this GameState.
         * @param message An error message that describes the reason this GameState was declared Non-Viable, or null if the GameState is Viable.
         */
        public bool IsViable(out string message)
        {
            var impossibleDeficits = (from resource in NegativeResources
                let availableProduction = TargetVerbs(resource)
                    .Select(verb => verb.GetResourceProduction(resource))
                    .Sum()
                where this.GetTotalResourceProduction(resource) + availableProduction < 0
                select resource).ToList();

            if (!impossibleDeficits.Any())
            {
                message = null;
                return true;
            }
            
            // Build the output message, explaining which resources are too negative to fix.
            var sb = new StringBuilder();
            sb.Append("the following resources are too negative:\n");
            foreach (var resource in impossibleDeficits)
            {
                sb.Append('\t').Append(Enum.GetName(resource)).Append(": ");
                sb.Append(this.GetTotalResourceProduction(resource));
                sb.Append(" (Maximum production remaining: ");
                sb.Append(TargetVerbs(resource)
                    .Select(verb => verb.GetResourceProduction(resource))
                    .Sum());
                sb.Append(")\n");
            }
            message = sb.ToString();
            return false;
        }

        //! Returns a list of Verbs that provide a positive production for the target resource.
        public IEnumerable<Verb> TargetVerbs(Resource target) => AvailableVerbs().Where(verb => verb.Resources[(int)target] > 0);

        //! Returns a list of Verbs that provide a positive production to one or more of this GameState's NegativeResources.
        public IEnumerable<Verb> GetDesiredVerbs()
        {
            var set = new VerbSet();
            foreach (var resource in NegativeResources)
            {
                foreach (var verb in TargetVerbs(resource))
                {
                    set.Add(verb);
                }
            }
            return set;
        }
        
    }
}