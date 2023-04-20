/*
 * @yankang.nj
 * Frame监听，在每一帧空闲的时候执行Do方法，需要保证Do方法的轻量
 */

namespace GSDK.RNU
{
    public interface IFrameIdleListener
    {
        void Do();
    }
}