/*
 * @author yankang.nj
 * 表示一个JS测的回调
 */
using System.Collections;

namespace GSDK.RNU
{
    public class Callback
    {
        private int callbackId;
        private bool invoked = false;
        public Callback(int callbackId)
        {
            this.callbackId = callbackId;
        }
        
        public void Invoke(ArrayList args) {
            if (invoked) {
                //TODO 
            }
            
            RNUMainCore.InvokeJSCallback(callbackId, args);
            invoked = true;
        }
    }
}