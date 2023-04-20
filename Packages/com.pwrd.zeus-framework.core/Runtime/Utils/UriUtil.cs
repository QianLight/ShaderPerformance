/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Zeus.Core
{
	public class UriUtil
	{
        static char[] trims = new char[] { '\\', '/' };
        public static string CombineUri(params string[] uriParts)
        {
            string uri = string.Empty;
            if (uriParts != null && uriParts.Length > 0)
            {
                for (int i = 0; i < uriParts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(uriParts[i]))
                    {
                        if (string.IsNullOrEmpty(uri))
                        {
                            uri = uriParts[i];
                        }
                        else
                        {
                            uri = string.Format("{0}/{1}", uri.TrimEnd(trims), uriParts[i].TrimStart(trims));
                        }
                    }
                }
            }
            return uri.Replace('\\', '/');
        }
    }
}