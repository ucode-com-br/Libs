using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentObjectReference<T> : IDisposable, IComparable<T>, IEquatable<T>
    {
        private T _value;

        private int _lockFlag; // 0 - free

        public virtual bool IsLockable { get; } = true;

        public TimeSpan Timeout = TimeSpan.FromSeconds(1);
        private bool _disposedValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentObjectReference()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentObjectReference(T value) => this.Set(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryLock()
        {
            var x = this.IsLockable && Interlocked.CompareExchange(ref this._lockFlag, 1, 0) == 0;

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WaitLock(TimeSpan? timeSpan = null)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (!this.TryLock() && timer.Elapsed <= this.Timeout)
            {
                Thread.Sleep(1);
            }
            timer.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unlock()
        {
            if (this.IsLockable)
            {
                Interlocked.Decrement(ref this._lockFlag);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForget(T value)
        {
            if (this.TryLock())
            {
                this._value = value;
                this.Unlock();
            }
            else
            {
                Task.Run(() => this.Set(value));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T value)
        {
            this.WaitLock();

            this._value = value;

            this.Unlock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetCurrent() => this._value;

        /// <summary>
        /// Get object
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get()
        {
            T? result;

            this.WaitLock();

            result = this._value;

            this.Unlock();

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)



                    if (this._value != null)
                    {
                        var type = typeof(T);

                        if (type.GetInterfaces().Any(a => a == typeof(IDisposable)))
                        {
                            ((IDisposable)this._value).Dispose();
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConcurrentObjectReference()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }




        public override string ToString() => this.Get().ToString();

        public int CompareTo(T? other)
        {
            var value = this.Get();
            if (value != null)
            {
                var type = typeof(T);

                if (type.GetInterfaces().Any(a => a == typeof(IComparable<T>)))
                {
                    return ((IComparable<T>)value).CompareTo(other);
                }
            }

            throw new NotSupportedException();
        }

        public bool Equals(T? other)
        {
            var value = this.Get();
            if (value != null)
            {
                var type = typeof(T);

                if (type.GetInterfaces().Any(a => a == typeof(IEquatable<T>)))
                {
                    return ((IEquatable<T>)value).Equals(other);
                }
            }

            throw new NotSupportedException();
        }

        public override bool Equals(object? obj) => this.Get().Equals(obj);

        public override int GetHashCode() => this.Get().GetHashCode();

        public static bool operator ==(ConcurrentObjectReference<T> left, ConcurrentObjectReference<T> right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ConcurrentObjectReference<T> left, ConcurrentObjectReference<T> right) => !(left == right);

        public static bool operator <(ConcurrentObjectReference<T> left, ConcurrentObjectReference<T> right) => left is null ? right is not null : left.CompareTo(right.Get()) < 0;


        public static bool operator <=(ConcurrentObjectReference<T> left, ConcurrentObjectReference<T> right) => left is null || left.CompareTo(right.Get()) <= 0;

        public static bool operator >(ConcurrentObjectReference<T> left, ConcurrentObjectReference<T> right) => left is not null && left.CompareTo(right.Get()) > 0;

        public static bool operator >=(ConcurrentObjectReference<T> left, ConcurrentObjectReference<T> right) => left is null ? right is null : left.CompareTo(right.Get()) >= 0;
    }
}
