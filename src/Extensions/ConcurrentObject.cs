using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Represents a reference to an object of type <typeparamref name="T"/> 
    /// that is safe to use across multiple threads, implementing 
    /// <see cref="IDisposable"/> to release resources, 
    /// <see cref="IComparable{T}"/> for comparing instances, 
    /// and <see cref="IEquatable{T}"/> for equality comparisons.
    /// </summary>
    /// <typeparam name="T">The type of the object referenced.</typeparam>
    public class ConcurrentObjectReference<T> : IDisposable, IComparable<T>, IEquatable<T>
    {
        private T _value;

        private int _lockFlag; // 0 - free

        /// <summary>
        /// Gets a value indicating whether the current object can be locked.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is lockable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is virtual, allowing derived classes to override it to provide their own locking behavior.
        /// The default value is <c>true</c>, indicating that the object can be locked.
        /// </remarks>
        public virtual bool IsLockable { get; } = true;

        public TimeSpan Timeout = TimeSpan.FromSeconds(1);
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObjectReference"/> class.
        /// This constructor is designed to provide an efficient initialization of 
        /// the concurrent object reference without any additional overhead.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentObjectReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentObjectReference{T}"/> class 
        /// with the specified value.
        /// </summary>
        /// <param name="value">The initial value to set for the object reference.</param>
        /// <remarks>
        /// This constructor uses aggressive inlining to optimize performance for 
        /// frequently called methods. It calls the <see cref="Set(T)"/> method 
        /// to assign the value to the object reference.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentObjectReference(T value) => this.Set(value);

        /// <summary>
        /// Attempts to acquire a lock if the current instance is lockable.
        /// </summary>
        /// <returns>
        /// Returns true if the lock was successfully acquired; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryLock()
        {
            var x = this.IsLockable && Interlocked.CompareExchange(ref this._lockFlag, 1, 0) == 0;

            return x;
        }

        /// <summary>
        /// Attempts to acquire a lock, waiting for a specified duration if necessary.
        /// </summary>
        /// <param name="timeSpan">An optional time span to limit the wait time for acquiring the lock.</param>
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

        /// <summary>
        /// Unlocks the resource if it is currently lockable by decrementing the lock flag.
        /// </summary>
        /// <remarks>
        /// This method is marked with <see cref="MethodImplOptions.AggressiveInlining"/> to suggest that the compiler inline the method for performance improvements. 
        /// It checks the state of <see cref="IsLockable"/> before proceeding with the unlocking process.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an attempt is made to unlock a resource that is not lockable.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unlock()
        {
            if (this.IsLockable)
            {
                Interlocked.Decrement(ref this._lockFlag);
            }
        }

        /// <summary>
        /// Sets the value of a field while ensuring thread safety. 
        /// If the lock is acquired successfully, it assigns the provided 
        /// value to the field. If the lock cannot be acquired, it 
        /// queues a task to set the value asynchronously.
        /// </summary>
        /// <param name="value">The value to be set.</param>
        /// <remarks>
        /// This method uses aggressive inlining to optimize performance 
        /// in scenarios where it is called frequently. The locking mechanism 
        /// ensures that only one thread can modify the value at a time, 
        /// preventing race conditions.
        /// </remarks>
        /// <exception cref="System.Exception">
        /// This method may throw exceptions related to task execution
        /// if running asynchronously fails.
        /// </exception>
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

        /// <summary>
        /// Sets the value of the current instance to the specified value.
        /// This method acquires a lock before updating the value to ensure
        /// thread-safety, then releases the lock after the update.
        /// </summary>
        /// <param name="value">The value to set for the current instance.</param>
        /// <remarks>
        /// The method is marked with <see cref="MethodImpl(MethodImplOptions.AggressiveInlining)"/> 
        /// to suggest that the compiler optimize the method call by inlining the method if possible,
        /// which may improve performance in scenarios where the method is called frequently.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T value)
        {
            this.WaitLock();

            this._value = value;

            this.Unlock();
        }

        /// <summary>
        /// Gets the current value of the instance.
        /// </summary>
        /// <returns>
        /// The current value of type <typeparamref name="T"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetCurrent() => this._value;

        /// <summary>
        /// Retrieves the current value of the instance.
        /// </summary>
        /// <returns>
        /// The current value of type <typeparamref name="T"/> if it exists; otherwise, <c>null</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get()
        {
            T? result;

            this.WaitLock();

            result = this._value;

            this.Unlock();

            return result;
        }

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">A boolean value that indicates whether the method 
        /// was called directly or by the runtime from inside the finalizer.</param>
        /// <remarks>
        /// If disposing is true, the method has been called directly or 
        /// through the Dispose method. Managed and unmanaged resources can be 
        /// disposed. If disposing is false, the method has been called by the 
        /// runtime from inside the finalizer and only unmanaged resources should 
        /// be released.
        /// </remarks>
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

        /// <summary>
        /// Finalizer for the ConcurrentObjectReference class.
        /// This finalizer is called by the garbage collector to clean up unmanaged resources
        /// if Dispose has not been called explicitly. The finalizer should only free unmanaged
        /// resources. If the Dispose(bool disposing) method has code to free unmanaged resources,
        /// it should safely handle the case when disposing is false.
        /// </summary>
        /// <remarks>
        /// This finalizer should not be changed directly. Cleanup logic should reside 
        /// within the Dispose(bool disposing) method.
        /// </remarks>
        /// <example>
        /// <code>
        /// ~ConcurrentObjectReference() 
        /// { 
        ///     Dispose(disposing: false); 
        /// }
        /// </code>
        /// </example>
        ~ConcurrentObjectReference() 
        { 
            Dispose(disposing: false); 
        }
        
        /// <summary>
        /// Releases all resources used by the ConcurrentObjectReference class.
        /// This method can be called to manually release resources held by 
        /// the instance before the garbage collector reclaims it. 
        /// It calls the Dispose(bool disposing) method to clean up both managed 
        /// and unmanaged resources when disposing is true.
        /// </summary>
        /// <remarks>
        /// Call this method when you are finished using the ConcurrentObjectReference.
        /// After calling Dispose, you should not use the instance until it is 
        /// re-initialized.
        /// </remarks>
        /// <example>
        /// <code>
        /// 
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }




        /// <summary>
        /// Returns a string that represents the current object.
        /// This method overrides the default implementation of the 
        /// <see cref="Object.ToString"/> method and calls the 
        /// <see cref="Get"/> method to retrieve a representation 
        /// of the object which is then converted to a string.
        /// </summary>
        /// <returns>
        /// A string that represents the current object, obtained 
        /// by calling the <see cref="Get"/> method and converting 
        /// its result to a string.
        /// </returns>
        public override string ToString() => this.Get().ToString();

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// This method attempts to compare the current instance with the provided 
        /// instance of type T using the IComparable<T> interface.
        /// </summary>
        /// <param name="other">An instance of type T to compare with the current instance.</param>
        /// <returns>
        /// A signed integer that indicates the relative order of the objects being compared. 
        /// The return value is less than zero if this instance precedes the other object, 
        /// zero if they are considered equal, and greater than zero if this instance follows the other object.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the type T does not implement the IComparable<T> interface or 
        /// if the value is null and cannot be compared.
        /// </exception>
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

        /// <summary>
        /// Compares the current instance with another object of the same type to determine if they are equal.
        /// This method checks if the current instance implements the <see cref="IEquatable{T}"/> interface
        /// and uses the interface's <see cref="IEquatable{T}.Equals(T?)"/> method for comparison when applicable.
        /// </summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <returns>
        /// Returns true if the current instance is equal to the specified <paramref name="other"/> object; 
        /// otherwise, throws a <see cref="NotSupportedException"/> if the instance does not support equality comparison.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the current instance is not supported for equality comparison.
        /// </exception>
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. Can be null.</param>
        /// <returns>
        /// True if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method overrides the default Equals method to provide a custom comparison logic.
        /// It retrieves the current instance's value using the Get() method and compares it to the specified object.
        /// </remarks>
        public override bool Equals(object? obj) => this.Get().Equals(obj);

        /// <summary>
        /// Returns a hash code for the current instance.
        /// <para>This method overrides the default implementation of <see cref="object.GetHashCode"/>.</para>
        /// </summary>
        /// <returns>
        /// A hash code for this instance, which is generated by calling <see cref="object.GetHashCode"/> 
        /// on the result of the <see cref="Get()"/> method.
        /// </returns>
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
