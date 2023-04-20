/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Threading;

namespace Zeus
{
    public class AsyncManager : Singleton<AsyncManager>, IModule
    {
        public void Init()
        {
        }

        public void Update()
        {
            if (SystemMessageQueue.Instance.GetCount() == 0)
            {
                return;
            }

            // Deal message in system queue.
            IInternalMessage message = SystemMessageQueue.Instance.Poll();
            if (null != message)
            {
                switch (message.GetMessageId())
                {
                    case AsyncTaskMessage.AsyncMessageId:
                        {
                            AsyncTaskMessage asyncTaskMessage = message as AsyncTaskMessage;
                            AsyncState asyncState = asyncTaskMessage.State;
                            IAsyncTask asyncTask = asyncTaskMessage.AsyncTask;

                            if (asyncState == AsyncState.Doing)
                            {
                                throw new ApplicationException(string.Format("AsyncTask {0} [AS_Doing] looped.", asyncState.GetType().FullName));
                            }

                            ExecuteAsyncTask(asyncState, asyncTask);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        public void ExecuteAsyncTask(IAsyncTask asyncTask)
        {
            this.ExecuteAsyncTask(AsyncState.Before, asyncTask);
        }

        // Execute asynchronous task by current state.
        public void ExecuteAsyncTask(AsyncState asyncState, IAsyncTask asyncTask)
        {
            switch (asyncState)
            {
                case AsyncState.Before:
                    {
                        AsyncState newState = asyncTask.BeforeAsyncTask();
                        if (newState == AsyncState.Before)
                        {
                            throw new ApplicationException(string.Format("AsyncTask {0} [AS_Before] looped.", asyncState.GetType().FullName));
                        }
                        ExecuteAsyncTask(newState, asyncTask);
                        break;
                    }
                case AsyncState.Doing:
                    {
                        // Do the task on a new thread.
                        ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncExecute), asyncTask);
                        break;
                    }
                case AsyncState.After:
                    {
                        AsyncState newState = asyncTask.AfterAsyncTask();
                        if (newState == AsyncState.After)
                        {
                            throw new ApplicationException(string.Format("AsyncTask {0} [AS_After] looped.", asyncState.GetType().FullName));
                        }
                        ExecuteAsyncTask(newState, asyncTask);
                        break;
                    }
                default:
                    break;
            }
        }

        // Task will be executed on thread.
        private void AsyncExecute(object o)
        {
            IAsyncTask asyncTask = o as IAsyncTask;
            // Do the thing.
            AsyncState asyncState = asyncTask.DoAsyncTask();

            // Offer an new message to system queue.
            AsyncTaskMessage message = new AsyncTaskMessage();
            message.State = asyncState;
            message.AsyncTask = asyncTask;
            SystemMessageQueue.Instance.Offer(message);
        }
    }
}