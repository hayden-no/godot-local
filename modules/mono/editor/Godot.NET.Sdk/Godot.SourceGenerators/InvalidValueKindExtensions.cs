using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Godot.SourceGenerators;

public static class InvalidValueKindExtensions
{
    extension(InvalidValueKindFlags)
    {
        public static bool IsFlaggedKind<T>(T? obj, InvalidValueKindFlags flags)
        {
            if (flags.HasFlag(InvalidValueKindFlags.Null))
            {
                if (obj is null) return true;
            }

            if (flags.HasFlag(InvalidValueKindFlags.Empty))
            {
                if (obj is ICollection collection && collection.Count == 0) return true;
                if (obj is string str && string.IsNullOrEmpty(str)) return true;
            }

            if (flags.HasFlag(InvalidValueKindFlags.WhiteSpace))
            {
                if (obj is string str && string.IsNullOrWhiteSpace(str)) return true;
            }

            if (flags.HasFlag(InvalidValueKindFlags.DefaultValue))
            {
                var type = obj?.GetType();

                IEqualityComparer<T?> comparer = EqualityComparer<T?>.Default;
                if (comparer.Equals(obj, default)) return true;
            }

            return false;
        }

        public static Predicate<T> GetPredicate<T>(InvalidValueKindFlags flags, bool negate = false)
        {
            if (flags == InvalidValueKindFlags.None) return  obj => negate;
            return obj =>
            {
                var result = IsFlaggedKind(obj, flags);
                return negate ? !result : result;
            };
        }

        public static Func<T, bool> GetPredicateFunc<T>(InvalidValueKindFlags flags, bool negate = false)
        {
            if (flags == InvalidValueKindFlags.None) return  obj => negate;
            return obj =>
            {
                var result = IsFlaggedKind(obj, flags);
                return negate ? !result : result;
            };
        }
    }

    extension<T>(IEnumerable<T> enumerable)
    {
        public bool Any(InvalidValueKindFlags flags, bool negate = false) =>
            enumerable.Any(InvalidValueKindFlags.GetPredicateFunc<T>(flags, negate));
        public IEnumerable<T> Where(InvalidValueKindFlags flags, bool negate = false) =>
            enumerable.Where(InvalidValueKindFlags.GetPredicateFunc<T>(flags, negate));
        public bool All(InvalidValueKindFlags flags, bool negate = false) =>
            enumerable.All(InvalidValueKindFlags.GetPredicateFunc<T>(flags, negate));
    }
}