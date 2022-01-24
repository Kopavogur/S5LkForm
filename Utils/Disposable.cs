using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S5LkForm.Utils
{
    public class Disposable<T>
        where T : IDisposable
    {
        private Func<T> Factory { get; }

        internal Disposable(Func<T> factory)
        {
            this.Factory = factory;
        }

        public void Use(Action<T> action)
        {
            using (T target = this.Factory())
            {
                action(target);
            }
        }

        public TResult Use<TResult>(Func<T, TResult> map)
        {
            using (T target = this.Factory())
            {
                return map(target);
            }
        }
    }

    public static class Disposable
    {
        public static Disposable<T> Of<T>(Func<T> factory)
          where T : IDisposable
          => new Disposable<T>(factory);
    }
}
