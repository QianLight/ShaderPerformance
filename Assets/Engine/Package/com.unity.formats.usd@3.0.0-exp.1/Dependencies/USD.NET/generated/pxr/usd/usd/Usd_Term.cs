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
    public class Usd_Term : global::System.IDisposable
    {
        private global::System.Runtime.InteropServices.HandleRef swigCPtr;
        protected bool swigCMemOwn;

        internal Usd_Term(global::System.IntPtr cPtr, bool cMemoryOwn)
        {
            swigCMemOwn = cMemoryOwn;
            swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
        }

        internal static global::System.Runtime.InteropServices.HandleRef getCPtr(Usd_Term obj)
        {
            return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
        }

        ~Usd_Term()
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
                        UsdCsPINVOKE.delete_Usd_Term(swigCPtr);
                    }
                    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
                }
                global::System.GC.SuppressFinalize(this);
            }
        }

        public Usd_Term(Usd_PrimFlags flag) : this(UsdCsPINVOKE.new_Usd_Term__SWIG_0((int)flag), true)
        {
        }

        public Usd_Term(Usd_PrimFlags flag, bool negated) : this(UsdCsPINVOKE.new_Usd_Term__SWIG_1((int)flag, negated), true)
        {
        }

        public Usd_PrimFlags flag
        {
            set
            {
                UsdCsPINVOKE.Usd_Term_flag_set(swigCPtr, (int)value);
            }
            get
            {
                Usd_PrimFlags ret = (Usd_PrimFlags)UsdCsPINVOKE.Usd_Term_flag_get(swigCPtr);
                return ret;
            }
        }

        public bool negated
        {
            set
            {
                UsdCsPINVOKE.Usd_Term_negated_set(swigCPtr, value);
            }
            get
            {
                bool ret = UsdCsPINVOKE.Usd_Term_negated_get(swigCPtr);
                return ret;
            }
        }
    }
}
