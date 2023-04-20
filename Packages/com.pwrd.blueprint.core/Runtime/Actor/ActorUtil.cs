using UnityEngine;

namespace Blueprint.Actor
{
    public static class ActorUtil
    {
        /// <summary>
        /// 根据name查找场景中的actor
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ActorBase FindActor(string name)
        {
            GameObject actor = GameObject.Find(name);

            if (actor != null && actor.GetComponent<BlueprintActor>())
            {
                return actor.GetComponent<BlueprintActor>().BpActor;
            }

            return null;
        }

        /// <summary>
        /// 根据GameObject获取actor
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// 
        public static ActorBase FindActor(GameObject obj)
        {
            GameObject actor = obj;

            if (actor != null && actor.GetComponent<BlueprintActor>())
            {
                return actor.GetComponent<BlueprintActor>().BpActor;
            }

            return null;
        }
    }
}