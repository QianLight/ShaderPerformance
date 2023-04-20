/*
 * @author yankang.nj
 * 我们只支持 JS测 已Promise的形式调用， 这里表示一个JS测的Promise
 */
using System.Collections;

namespace GSDK.RNU
{
    public class Promise
    {
        private static string ERROR_MAP_KEY_CODE = "code";
        private static string ERROR_MAP_KEY_MESSAGE = "message"; 
        
        private Callback resolveCallback;
        private Callback rejectCallback;
        public Promise(Callback resolve, Callback reject)
        {
            resolveCallback = resolve;
            rejectCallback = reject;
        }
        
        //TODO 需要完善错误信息
        public void Reject(string errorMessage)
        {
            if (rejectCallback == null) {
                resolveCallback = null;
                return;
            }

            Hashtable errorInfo = new Hashtable();
            
            errorInfo.Add(ERROR_MAP_KEY_CODE, -1);
            errorInfo.Add(ERROR_MAP_KEY_MESSAGE, errorMessage);
            
            rejectCallback.Invoke(new ArrayList(){errorInfo});
            resolveCallback = null;
            rejectCallback = null;
        }

        public void Resolve(object value)
        {
            if (resolveCallback != null)
            {
                resolveCallback.Invoke(new ArrayList(){value});

                rejectCallback = null;
                resolveCallback = null;
            }
        }
    }
}