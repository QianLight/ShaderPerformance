//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace pxr
{
    public class SdfUnregisteredValueListOpVector : global::System.IDisposable, global::System.Collections.IEnumerable
        , global::System.Collections.Generic.IEnumerable<SdfUnregisteredValueListOp>
    {
        private global::System.Runtime.InteropServices.HandleRef swigCPtr;
        protected bool swigCMemOwn;

        internal SdfUnregisteredValueListOpVector(global::System.IntPtr cPtr, bool cMemoryOwn)
        {
            swigCMemOwn = cMemoryOwn;
            swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
        }

        internal static global::System.Runtime.InteropServices.HandleRef getCPtr(SdfUnregisteredValueListOpVector obj)
        {
            return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
        }

        ~SdfUnregisteredValueListOpVector()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            lock (this) {
                if (swigCPtr.Handle != global::System.IntPtr.Zero)
                {
                    if (swigCMemOwn)
                    {
                        swigCMemOwn = false;
                        UsdCsPINVOKE.delete_SdfUnregisteredValueListOpVector(swigCPtr);
                    }
                    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
                }
                global::System.GC.SuppressFinalize(this);
            }
        }

        public SdfUnregisteredValueListOpVector(global::System.Collections.ICollection c) : this()
        {
            if (c == null)
                throw new global::System.ArgumentNullException("c");
            foreach (SdfUnregisteredValueListOp element in c)
            {
                this.Add(element);
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public SdfUnregisteredValueListOp this[int index]
        {
            get
            {
                return getitem(index);
            }
            set
            {
                setitem(index, value);
            }
        }

        public int Capacity
        {
            get
            {
                return (int)capacity();
            }
            set
            {
                if (value < size())
                    throw new global::System.ArgumentOutOfRangeException("Capacity");
                reserve((uint)value);
            }
        }

        public int Count
        {
            get
            {
                return (int)size();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public void CopyTo(SdfUnregisteredValueListOp[] array)
        {
            CopyTo(0, array, 0, this.Count);
        }

        public void CopyTo(SdfUnregisteredValueListOp[] array, int arrayIndex)
        {
            CopyTo(0, array, arrayIndex, this.Count);
        }

        public void CopyTo(int index, SdfUnregisteredValueListOp[] array, int arrayIndex, int count)
        {
            if (array == null)
                throw new global::System.ArgumentNullException("array");
            if (index < 0)
                throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
            if (arrayIndex < 0)
                throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
            if (count < 0)
                throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
            if (array.Rank > 1)
                throw new global::System.ArgumentException("Multi dimensional array.", "array");
            if (index + count > this.Count || arrayIndex + count > array.Length)
                throw new global::System.ArgumentException("Number of elements to copy is too large.");
            for (int i = 0; i < count; i++)
                array.SetValue(getitemcopy(index + i), arrayIndex + i);
        }

        global::System.Collections.Generic.IEnumerator<SdfUnregisteredValueListOp> global::System.Collections.Generic.IEnumerable<SdfUnregisteredValueListOp>.GetEnumerator()
        {
            return new SdfUnregisteredValueListOpVectorEnumerator(this);
        }

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
        {
            return new SdfUnregisteredValueListOpVectorEnumerator(this);
        }

        public SdfUnregisteredValueListOpVectorEnumerator GetEnumerator()
        {
            return new SdfUnregisteredValueListOpVectorEnumerator(this);
        }

        // Type-safe enumerator
        /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
        /// whenever the collection is modified. This has been done for changes in the size of the
        /// collection but not when one of the elements of the collection is modified as it is a bit
        /// tricky to detect unmanaged code that modifies the collection under our feet.
        public sealed class SdfUnregisteredValueListOpVectorEnumerator : global::System.Collections.IEnumerator
            , global::System.Collections.Generic.IEnumerator<SdfUnregisteredValueListOp>
        {
            private SdfUnregisteredValueListOpVector collectionRef;
            private int currentIndex;
            private object currentObject;
            private int currentSize;

            public SdfUnregisteredValueListOpVectorEnumerator(SdfUnregisteredValueListOpVector collection)
            {
                collectionRef = collection;
                currentIndex = -1;
                currentObject = null;
                currentSize = collectionRef.Count;
            }

            // Type-safe iterator Current
            public SdfUnregisteredValueListOp Current
            {
                get
                {
                    if (currentIndex == -1)
                        throw new global::System.InvalidOperationException("Enumeration not started.");
                    if (currentIndex > currentSize - 1)
                        throw new global::System.InvalidOperationException("Enumeration finished.");
                    if (currentObject == null)
                        throw new global::System.InvalidOperationException("Collection modified.");
                    return (SdfUnregisteredValueListOp)currentObject;
                }
            }

            // Type-unsafe IEnumerator.Current
            object global::System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                int size = collectionRef.Count;
                bool moveOkay = (currentIndex + 1 < size) && (size == currentSize);
                if (moveOkay)
                {
                    currentIndex++;
                    currentObject = collectionRef[currentIndex];
                }
                else
                {
                    currentObject = null;
                }
                return moveOkay;
            }

            public void Reset()
            {
                currentIndex = -1;
                currentObject = null;
                if (collectionRef.Count != currentSize)
                {
                    throw new global::System.InvalidOperationException("Collection modified.");
                }
            }

            public void Dispose()
            {
                currentIndex = -1;
                currentObject = null;
            }
        }

        public void Clear()
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_Clear(swigCPtr);
        }

        public void Add(SdfUnregisteredValueListOp x)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_Add(swigCPtr, SdfUnregisteredValueListOp.getCPtr(x));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        private uint size()
        {
            uint ret = UsdCsPINVOKE.SdfUnregisteredValueListOpVector_size(swigCPtr);
            return ret;
        }

        private uint capacity()
        {
            uint ret = UsdCsPINVOKE.SdfUnregisteredValueListOpVector_capacity(swigCPtr);
            return ret;
        }

        private void reserve(uint n)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_reserve(swigCPtr, n);
        }

        public SdfUnregisteredValueListOpVector() : this(UsdCsPINVOKE.new_SdfUnregisteredValueListOpVector__SWIG_0(), true)
        {
        }

        public SdfUnregisteredValueListOpVector(SdfUnregisteredValueListOpVector other) : this(UsdCsPINVOKE.new_SdfUnregisteredValueListOpVector__SWIG_1(SdfUnregisteredValueListOpVector.getCPtr(other)), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public SdfUnregisteredValueListOpVector(int capacity) : this(UsdCsPINVOKE.new_SdfUnregisteredValueListOpVector__SWIG_2(capacity), true)
        {
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        private SdfUnregisteredValueListOp getitemcopy(int index)
        {
            SdfUnregisteredValueListOp ret = new SdfUnregisteredValueListOp(UsdCsPINVOKE.SdfUnregisteredValueListOpVector_getitemcopy(swigCPtr, index), true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        private SdfUnregisteredValueListOp getitem(int index)
        {
            SdfUnregisteredValueListOp ret = new SdfUnregisteredValueListOp(UsdCsPINVOKE.SdfUnregisteredValueListOpVector_getitem(swigCPtr, index), false);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        private void setitem(int index, SdfUnregisteredValueListOp val)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_setitem(swigCPtr, index, SdfUnregisteredValueListOp.getCPtr(val));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void AddRange(SdfUnregisteredValueListOpVector values)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_AddRange(swigCPtr, SdfUnregisteredValueListOpVector.getCPtr(values));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public SdfUnregisteredValueListOpVector GetRange(int index, int count)
        {
            global::System.IntPtr cPtr = UsdCsPINVOKE.SdfUnregisteredValueListOpVector_GetRange(swigCPtr, index, count);
            SdfUnregisteredValueListOpVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new SdfUnregisteredValueListOpVector(cPtr, true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public void Insert(int index, SdfUnregisteredValueListOp x)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_Insert(swigCPtr, index, SdfUnregisteredValueListOp.getCPtr(x));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void InsertRange(int index, SdfUnregisteredValueListOpVector values)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_InsertRange(swigCPtr, index, SdfUnregisteredValueListOpVector.getCPtr(values));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void RemoveAt(int index)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_RemoveAt(swigCPtr, index);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void RemoveRange(int index, int count)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_RemoveRange(swigCPtr, index, count);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public static SdfUnregisteredValueListOpVector Repeat(SdfUnregisteredValueListOp value, int count)
        {
            global::System.IntPtr cPtr = UsdCsPINVOKE.SdfUnregisteredValueListOpVector_Repeat(SdfUnregisteredValueListOp.getCPtr(value), count);
            SdfUnregisteredValueListOpVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new SdfUnregisteredValueListOpVector(cPtr, true);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
            return ret;
        }

        public void Reverse()
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_Reverse__SWIG_0(swigCPtr);
        }

        public void Reverse(int index, int count)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_Reverse__SWIG_1(swigCPtr, index, count);
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }

        public void SetRange(int index, SdfUnregisteredValueListOpVector values)
        {
            UsdCsPINVOKE.SdfUnregisteredValueListOpVector_SetRange(swigCPtr, index, SdfUnregisteredValueListOpVector.getCPtr(values));
            if (UsdCsPINVOKE.SWIGPendingException.Pending) throw UsdCsPINVOKE.SWIGPendingException.Retrieve();
        }
    }
}
