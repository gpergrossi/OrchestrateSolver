using System;
using System.Collections.Generic;

namespace OrchestrateSolver
{
    // A mutable class which constructs the immutable Verb instances.
    // Can call Create() manually, or rely on implicit conversion.
    public class VerbFactory
    {
        public static VerbFactory Begin(Verbs.Id id, char letter, string name)
        {
            return new VerbFactory(id, name, letter);
        }

        private readonly Verbs.Id _id;
        private readonly string _name;
        private readonly char _letter;
        private readonly Dictionary<Resource, int> _resources;
        
        public VerbFactory(Verbs.Id id, String name, char letter)
        {
            this._id = id;
            this._name = name;
            this._letter = letter;
            this._resources = new Dictionary<Resource, int>();
        }

        public VerbFactory With(Resource resource, int amount)
        {
            if (_resources.ContainsKey(resource))
            {
                _resources[resource] += amount;
            }
            else
            {
                _resources[resource] = amount;
            }
            return this;
        }

        public Verb Create()
        {
            return new Verb(_id, _name, _letter, _resources);
        }
        
        public static implicit operator Verb(VerbFactory v)  {  return v.Create();  }
    }
}