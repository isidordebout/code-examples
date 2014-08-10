    interface IValidation<TResult, TError> { }

    class Failure<R, E> : IValidation<R, E>
    {
        public readonly IEnumerable<E> errors;
        internal Failure(IEnumerable<E> errors)
        {
        }
    }

    class Success<R,E> : IValidation<R, E>
    {
        public readonly R result;
        internal Success(R result)
        {
        }
    }

    static class Validations
    {
        public static IValidation<R, E> Success<R, E>(R r) { return new Success<R, E>(r); }
        public static IValidation<R, E> Failure<R, E>(params E[] errors) { return new Failure<R, E>(errors); }
        public static IValidation<R, E> Failure<R, E>(IEnumerable<E> errors) { return new Failure<R, E>(errors); }

        public static T Match<R, E, T>(this IValidation<R, E> v, Func<Success<R,E>, T> success, Func<Failure<R, E>, T> failure)
        {
            if (v is Success<E, R>)
                return success(v as Success<R, E>);
            else
                return failure(v as Failure<R, E>);
        }

        public static IValidation<R2, E> Apply<R1, R2, E>(this IValidation<Func<R1, R2>, E> f, IValidation<R1, E> v)
        {
            return f.Match(
                sf => v.Match(sv => Success<R2, E>(sf.result(sv.result)), fv => Failure<R2, E>(fv.errors)),
                ff => v.Match(sv => Failure<R2, E>(ff.errors), fv => Failure<R2, E>(ff.errors.Concat(fv.errors))));
        }

        public static IValidation<R2, E> Select<R1, R2, E>(Func<R1, R2> f, IValidation<R1, E> v)
        {
            return Success<Func<R1, R2>, E>(f).Apply(v);
        }

        public static IValidation<R3, E> Select<R1, R2, R3, E>(Func<R1, R2, R3> f, IValidation<R1, E> v1, IValidation<R2, E> v2)
        {
            return Apply(Select(Curry(f), v1), v2);
        }

        public static IValidation<R4, E> Select<R1, R2, R3, R4, E>(Func<R1, R2, R3, R4> f, IValidation<R1, E> v1, IValidation<R2, E> v2, IValidation<R3, E> v3)
        {
            return Apply(Apply(Select(Curry(f), v1), v2), v3);
        }

        public static Func<T1, Func<T2, T3>> Curry<T1, T2, T3>(Func<T1, T2, T3> f)
        {
            return t1 => t2 => f(t1, t2);
        }

        public static Func<T1, Func<T2, Func<T3, T4>>> Curry<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f)
        {
            return t1 => t2 => t3 => f(t1, t2, t3);
        }

        public static IValidation<string, string> ValidateName(string n)
        {
            return !String.IsNullOrWhiteSpace(n) && !n.Any(c => char.IsDigit(c))
                ? Success<string, string>(n)
                : Failure<string, string>(n + " is not a correct name format");
        }

        public static IValidation<DateTime, string> ValidateDate(string n)
        {
            DateTime d;
            return DateTime.TryParse(n, out d) 
                ? Success<DateTime, string>(d) 
                : Failure<DateTime, string>(n + " is not a correct date format");
        }

        public static IValidation<int, string> ValidateWeight(string s)
        {
            int n;
            return Int32.TryParse(s, out n) && n > 0 && n < 300
                ? Success<int, string>(n)
                : Failure<int, string>(s + " is not a correct weight");
        }

        public static IValidation<Dictionary<string, dynamic>, string> ValidatePersonInfo(string name, string date, string weight)
        {
            return Select((n, d, w) => new Dictionary<string, dynamic> { {"name", n}, {"date of birth", d}, {"weight in kg", w} }, 
                ValidateName(name), 
                ValidateDate(date), 
                ValidateWeight(weight));
        }
    }
