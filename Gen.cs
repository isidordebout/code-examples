using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.CSP
{
    public delegate T Gen<out T>(Random rnd);
    public delegate Tuple<TState, Result> Command<TState>(TState m);

    public static class Specs
    {
        private static Result Evaluate(Func<Result> f)
        {
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                return Result.ExceptionThrown(ex);
            }
        }

        private static Func<T, Gen<Result>> Safe<T>(Func<T, Gen<Result>> f)
        {
            return arg => rnd => Evaluate(() => f(arg)(rnd));
        }

        public static Gen<Result> Assert(bool b)
        {
            return Constant(new Result(b));
        }

        public static Gen<Result> ForAll<T>(Gen<T> g, Func<T, Gen<Result>> f)
        {
            return g.SelectMany(Safe(f));
        }

        public static Gen<Result> Label(this Gen<Result> g, String label)
        {
            return g.Select(r => r.WithStamps(label));
        }

        public static Gen<Result> Classify(this Gen<Result> g, bool condition, string label)
        {
            return condition ? g.Label(label) : g;
        }

        public static Gen<TResult> Select<T, TResult>(this Gen<T> g, Func<T, TResult> selector)
        {
            return rnd => selector(g(rnd));
        }

        public static Gen<TResult> SelectMany<T1, T2, TResult>(this Gen<T1> source, Func<T1, Gen<T2>> f, Func<T1, T2, TResult> select)
        {
            return source.Select(x => f(x).Select(y => select(x, y))).Join();
        }

        public static Gen<TResult> SelectMany<T, TResult>(this Gen<T> source, Func<T, Gen<TResult>> f)
        {
            return source.Select(v => f(v)).Join();
        }

        public static Gen<T> Join<T>(this Gen<Gen<T>> g)
        {
            return rnd => g(rnd)(rnd);
        }

        public static Gen<IEnumerable<T>> Sequence<T>(IEnumerable<Gen<T>> gs)
        {
            return rnd => gs.Select(g => g(rnd));
        }

        public static Gen<T> OneOf<T>(params Gen<T>[] gs)
        {
            return rnd => gs[rnd.Next(gs.Length)](rnd);
        }

        public static Gen<T> OneOf<T>(IEnumerable<Gen<T>> gs)
        {
            return OneOf(gs.ToArray());
        }

        static Gen<T> ValueIn<T>(IEnumerable<T> vs)
        {
            return OneOf(vs.Select(Constant));
        }

        public static Gen<T> Frequency<T>(IEnumerable<Tuple<int, Gen<T>>> weightedGs)
        {
            var generators = weightedGs.SelectMany(t => Enumerable.Repeat(t.Item2, t.Item1)).ToArray();
            return OneOf(generators);
        }

        public static Gen<T> Constant<T>(T v)
        {
            return rnd => v;
        }

        private static IEnumerable<KeyValuePair<T, int>> Histogram<T>(IEnumerable<T> xs) where T : IEquatable<T>
        {
            return xs.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        }

        public static Gen<Result> ForAll<T>(Gen<T> g, Func<T, Gen<IEnumerable<T>>> shrink, Func<T, Gen<Result>> f)
        {
            return rnd =>
            {
                var input = g(rnd);
                var res = f(input)(rnd);
                if (res.IsFailed())
                {
                    while (true)
                    {
                        var failed = shrink(input)(rnd).Select(v => Tuple.Create(v, f(v)(rnd))).FirstOrDefault(t => t.Item2.IsFailed());
                        if (failed == null)
                            return res;
                        input = failed.Item1;
                        res = failed.Item2;
                    }
                }
                return res;
            };
        }

        public static Gen<Command<TState>> Label<TState>(this Gen<Command<TState>> g, string label)
        {
            return g.Select(f => new Command<TState>((state) => { var res = f(state); return Tuple.Create(res.Item1, res.Item2.WithStamps(label)); }));
        }

        public static Gen<Command<TState>> Classify<TState>(this Gen<Command<TState>> g, bool condition, string label)
        {
            return condition ? g.Label(label) : g;
        }

        public static Func<TState, Gen<Result>> OneOfCmds<TState>(params Gen<Command<TState>>[] cmds)
        {
            return state => OneOf(cmds).Select(cmd => { var res = cmd(state); state = res.Item1; return res.Item2; });
        }

        public static Gen<Command<TState>> ForAllCommand<TArg, TState>(
            Gen<TArg> g, 
            Func<TArg, Gen<Command<TState>>> cmd)
        {
            return g.SelectMany(cmd);
        }

        public static Gen<Command<TState>> Command<TState, TResult>(
            Func<TState, bool> precondition,
            Func<TResult> cmd,
            Func<TState, TState> next,
            Func<TState, TState, TResult, bool> postcondition)
        {
            return rnd => state => 
                {
                    if (!precondition(state)) 
                        return Tuple.Create(state, Result.Undefined());
                    var newstate = next(state);
                    return Tuple.Create(newstate, Evaluate(() => new Result(postcondition(state, newstate, cmd()))));
                };
        }


        private static void test()
        {
            ForAll(rnd => rnd.NextDouble(),
                   a => Assert(Math.Abs(a - Math.Sqrt(a) * Math.Sqrt(a)) < double.Epsilon)
                       .Classify(a < 0, "negative"))
                       .Label("Check Sqrt");
            Dictionary<int, bool> d = new Dictionary<int,bool>();
            var cmdAdd = ForAllCommand(
                rnd => rnd.Next(),
                n =>  ForAllCommand(
                    rnd => rnd.Next() % 2 == 0,
                    b => Command<IList<KeyValuePair<int, bool>>, IDictionary<int, bool>>(
                        _ => true,
                        () => { d.Add(n, b); return d; },
                        l => { l.Add(new KeyValuePair<int, bool>(n, b)); return l; }, 
                        (lold, lnew, dico) => dico.SequenceEqual(lnew))));

            var cmdRead = ForAllCommand(
                rnd => rnd.Next(),
                n =>  ForAllCommand(
                    rnd => rnd.Next() % 2 == 0,
                    b => Command<IList<KeyValuePair<int, bool>>, bool>(
                            l => l.Any(kv => kv.Key == n && kv.Value == b),
                            () => d[n],
                            l => l,
                            (lold, lnew, val) => val == b
                            )).Label("n=" + n));

        }
    }

    public class Result
    {
        public readonly bool? OK;
        public readonly Exception Exception;
        public readonly IEnumerable<string> Stamps;
        public Result(bool? ok, Exception ex)
            : this(ok, ex, Enumerable.Empty<string>())
        {
        }

        public Result(bool? ok) : this(ok, null) { }

        public Result(bool? ok, Exception ex, IEnumerable<string> stamps)
        {
            this.OK = ok;
            this.Exception = ex;
            this.Stamps = stamps;
        }

        public static Result Undefined()
        {
            return new Result(null, null);
        }

        public static Result Ok()
        {
            return new Result(true, null);
        }

        public static Result ExceptionThrown(Exception ex)
        {
            return new Result(false, ex);
        }

        public bool IsOk()
        {
            return OK.HasValue && OK.Value;
        }

        public bool IsFailed()
        {
            return OK.HasValue && !OK.Value;
        }

        public bool IsUndefined()
        {
            return !OK.HasValue;
        }

        public Result WithStamps(params string[] stamps)
        {
            return new Result(this.OK, this.Exception, stamps.Concat(Stamps));
        }
    }

    public static class Shrinks
    {
        private static long NextLong(this Random rnd, long min, long max)
        {
            byte[] buf = new byte[8];
            rnd.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        private static IEnumerable<long> Shrink(Random rnd, long v)
        {
            var n = Math.Abs(v);
            while(n != 0)
            {
                n = rnd.NextLong(n / 2, n);
                yield return n*Math.Sign(v);
            }
        }

        private static IEnumerable<double> Shrink(Random rnd, double v)
        {
            var n = Math.Abs(v);
            while (n > double.Epsilon)
            {
                n = rnd.NextDouble() * (n/2.0) +  (n/2.0);
                yield return n * Math.Sign(v);
            }
        }

        public static Gen<IEnumerable<long>> Shrink(long v)
        {
            return rnd => Shrink(rnd, v);
        }

        public static Gen<IEnumerable<DateTime>> Shrink(DateTime d)
        {
            return Shrink(d.Ticks).Select(xs => xs.Select(x => new DateTime(x)));
        }

        public static Gen<IEnumerable<int>> Shrink(int n)
        {
            return Shrink((long)n).Select(xs => xs.Select(x => (int)x));
        }

        public static Gen<IEnumerable<double>> Shrink(double n)
        {
            return rnd => Shrink(rnd, n);
        }

        public static Gen<IEnumerable<char>> Shrink(char n)
        {
            return Shrink((long)n).Select(xs => xs.Select(x => (char)x));
        }

        // remove chunks of variable size at variable position
        private static Gen<IEnumerable<IEnumerable<T>>> Shrink<T>(IEnumerable<T> xs)
        {
            return rnd =>
            {
                var n = xs.Count();
                return 
                    new[] { n }.Concat(Shrink(n)(rnd))
                    .SelectMany(size => Enumerable.Range(0, n - size).Select(pos => xs.Take(pos).Concat(xs.Skip(pos + size))));
            };
        }

        public static Gen<IEnumerable<string>> Shrink<T>(string s)
        {
            return Shrink(s).Select(css => css.Select(cs => new String(cs.ToArray())));
        }
    }
}
