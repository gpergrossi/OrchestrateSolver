using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OrchestrateSolver
{
    public class Verb
    {
        public readonly Verbs.Id Id;
        public readonly uint Bitmask;
        public readonly String Name;
        public readonly char Letter;
        public readonly int[] Resources;

        internal Verb(Verbs.Id id, String name, char letter, Dictionary<Resource, int> resources)
        {
            this.Id = id;
            this.Bitmask = (1u << (int)id);
            this.Name = name;
            this.Letter = letter;
            
            this.Resources = new int[Enum.GetNames(typeof(Resource)).Length];
            foreach (var pair in resources)
            {
                this.Resources[(int) pair.Key] = pair.Value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Letter).Append(": ").Append(this.Name).Append(" {");
            var first = true;
            foreach (var resource in Enum.GetValues(typeof(Resource)).Cast<Resource>())
            {
                if (this.GetResourceProduction(resource) == 0) continue;
                if (!first) sb.Append(',');
                first = false;
                sb.Append(' ').Append(Enum.GetName(resource)).Append(": ").Append(this.GetResourceProduction(resource));
            }
            sb.Append(" }");
            return sb.ToString();
        }

        public int GetResourceProduction(Resource resource) 
        {
            return this.Resources[(int) resource];
        }

        public override int GetHashCode()
        {
            return (int)Id;
        }
    }
}