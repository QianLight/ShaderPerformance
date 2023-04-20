using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLevel
{
    class Bresenham
    {
        public static List<GridLineDesc> GetLine(float x0, float y0, float x1, float y1)
        {
            List<GridLineDesc> ret = new List<GridLineDesc>();
            if(x0 == x1)
            {
                int start = (int)y0;
                int end = (int)y1;

                int direction = (end >= start ? 1 : -1);

                for(int i = 0; i < (end - start)*direction; ++i)
                {
                     ret.Add(new GridLineDesc(direction, 0));
                }
            }
            else
            {
                float k = (y1 - y0) / (x1 - x0);
                float b = y1 - k * x1;

                int xdirection = (x1 > x0 ? 1 : -1);
                int ydirection = (y1 > y0 ? 1 : -1);

                if (Math.Abs(k) <= 1)
                {
                    int x = (int)x0;
                    float y = k * x + b;
                    float d = y - (float)Math.Floor(y);

                    while(x * xdirection < (int)x1 * xdirection)
                    {
                        d = d + k * xdirection;
                        x = x + xdirection;

                        if(k * xdirection >= 0)
                        {
                            if (d >= 1)
                            {
                                d = d - 1;
                                ret.Add(new GridLineDesc(0, ydirection));
                                ret.Add(new GridLineDesc(xdirection, 0));
                            }
                            else
                            {
                                ret.Add(new GridLineDesc(xdirection, 0));
                            }
                        }
                        else
                        {
                            if(d <= 0)
                            {
                                d = d + 1;
                                ret.Add(new GridLineDesc(0, ydirection));
                                ret.Add(new GridLineDesc(xdirection, 0));
                            }
                            else
                            {
                                ret.Add(new GridLineDesc(xdirection, 0));
                            }
                        }
                        
                    }                   
                }
                else
                {
                    int y = (int)y0;
                    float x = (y - b) / k;
                    float d = x - (float)Math.Floor(x);

                    while (y * ydirection < (int)y1 * ydirection)
                    {
                        d = d + 1 / k * ydirection;
                        y = y + ydirection;

                        if (k * ydirection >= 0)
                        {
                            if (d >= 1)
                            {
                                d = d - 1;
                                ret.Add(new GridLineDesc(xdirection, 0));
                                ret.Add(new GridLineDesc(0, ydirection));
                            }
                            else
                            {
                                ret.Add(new GridLineDesc(0, ydirection));
                            }
                        }
                        else
                        {
                            if (d <= 0)
                            {
                                d = d + 1;
                                ret.Add(new GridLineDesc(xdirection, 0));
                                ret.Add(new GridLineDesc(0, ydirection));
                            }
                            else
                            {
                                ret.Add(new GridLineDesc(0, ydirection));
                            }
                        }

                    }
                }

            }
            return ret;
        }
    }
}
