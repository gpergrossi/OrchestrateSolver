using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrchestrateSolver
{
    /**!
     * A simple implementation of the ISet<> interface.
     * Uses a single uint encoded as bitmask based on the Verbs class Ids and Verb.Bitmask in particular.
     */
    public class VerbSet : ISet<Verb>
    {
        private uint _elements;

        public uint Value => _elements;
        public VerbSet()
        {
            _elements = 0;
        }
        public VerbSet(uint value)
        {
            _elements = value;
        }
        
        public VerbSet(VerbSet other)
        {
            _elements = other._elements;
        }
        
        public IEnumerator<Verb> GetEnumerator()
        {
            return Verbs.All.Where(verb => (verb.Bitmask & _elements) != 0).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<Verb>.Add(Verb item)
        {
            this.Add(item);
        }

        private uint CombinedBitmask(IEnumerable<Verb> verbs)
        {
            return verbs.Aggregate<Verb, uint>(0, (current, verb) => current | verb.Bitmask);
        }
        
        public void ExceptWith(IEnumerable<Verb> other)
        {
            _elements &= ~CombinedBitmask(other);
        }

        public void IntersectWith(IEnumerable<Verb> other)
        {
            _elements &= CombinedBitmask(other);
        }

        public bool IsProperSubsetOf(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            return (_elements != combined) && (_elements & combined) == _elements;
        }

        public bool IsProperSupersetOf(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            return (_elements != combined) && (_elements & combined) == combined;
        }

        public bool IsSubsetOf(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            return (_elements & combined) == _elements;
        }

        public bool IsSupersetOf(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            return (_elements & combined) == combined;
        }

        public bool Overlaps(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            return (_elements & combined) != 0;
        }

        public bool SetEquals(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            return _elements == combined;
        }

        public void SymmetricExceptWith(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            _elements ^= combined;
        }

        public void UnionWith(IEnumerable<Verb> other)
        {
            uint combined = CombinedBitmask(other);
            _elements |= combined;
        }

        bool ISet<Verb>.Add(Verb item)
        {
            return this.Add(item);
        }
        
        public bool Add(Verb item)
        {
            if (item == null) throw new ArgumentNullException();
            if (!this.Contains(item))
            {
                _elements |= item.Bitmask;
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _elements = 0;
        }

        public bool Contains(Verb item)
        {
            if (item == null) return false;
            return (_elements & item.Bitmask) != 0;
        }

        public void CopyTo(Verb[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException();
            if (arrayIndex < 0) throw new IndexOutOfRangeException();

            using IEnumerator<Verb> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (arrayIndex >= array.Length) throw new ArgumentException();
                array[arrayIndex] = enumerator.Current;
                arrayIndex++;
            }
        }

        public List<Verb> ToList()
        {
            List<Verb> verbs = new List<Verb>();
            using IEnumerator<Verb> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                verbs.Add(enumerator.Current);
            }

            return verbs;
        }

        public bool Remove(Verb item)
        {
            if (item != null && this.Contains(item))
            {
                _elements &= ~item.Bitmask;
                return true;
            }
            return false;
        }

        public int Count => ToList().Count();
        
        public bool IsReadOnly => false;
    }
}