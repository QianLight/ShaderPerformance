using System;

namespace Blueprint.Actor
{

    public class ActorInvoke
    {
        public float time;

        public float repeatRate;

        public Action action;

        public ActorBase actor;

        public void Cancel()
        {
            if (actor == null)
            {
                return ;
            }

            actor.CancelInvoke(this);
        }

    }

}