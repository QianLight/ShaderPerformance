
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    /// <summary>
    /// Command Buffer Pool
    /// </summary>
    public static class CommandBufferPool
    {

        /// <summary>
        /// Get a new Command Buffer.
        /// </summary>
        /// <returns></returns>
        public static CommandBuffer Get()
        {
            var cmd = CommonObject<CommandBuffer>.Get();
            cmd.name = "Unnamed Command Buffer";
            return cmd;
        }

        /// <summary>
        /// Get a new Command Buffer and assign a name to it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CommandBuffer Get(string name)
        {
            var cmd = CommonObject<CommandBuffer>.Get();
            cmd.name = name;
            return cmd;
        }

        /// <summary>
        /// Release a Command Buffer.
        /// </summary>
        /// <param name="buffer"></param>
        public static void Release(CommandBuffer buffer)
        {
            buffer.Clear();
            CommonObject<CommandBuffer>.Release(buffer);
        }
    }
}
