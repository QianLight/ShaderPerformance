﻿using UnityEngine;
using System.Collections;
using GMSDK;
using UNBridgeLib;
using System.Collections.Generic;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class BulletinInnerTools
    {

        public static Result ConvertBulletinRet(BulletinRet ret)
        {
            return new Result(ret.code, ret.message, ret.extraErrorCode, ret.extraErrorMessage, ret.additionalInfo);
        }

		public static BulletinInfo ConvertGMBulletin(GMBulletin gmBulletin)
        {
            if (gmBulletin == null || gmBulletin.bulletinItems == null) return null;
            //Debug.Log("Bulletin result:" + JsonMapper.ToJson(gmBulletin));
            BulletinInfo bulletin = new BulletinInfo();
			List<BulletinItem> bulletinItems = new List<BulletinItem>();

			foreach(GMBulletinItem gmBulletinItem in gmBulletin.bulletinItems)
			{
                BulletinItem bulletinItem = ConvertGMBulletinItem(gmBulletinItem);
                if (bulletinItem != null)
                {
                    bulletinItems.Add(bulletinItem);
                }
			}
            bulletin.BulletinItems = bulletinItems;
			bulletin.TotalPage = gmBulletin.totalPage;
			bulletin.CurrentPage = gmBulletin.currentPage;
			bulletin.PageSize = gmBulletin.pageSize;
			bulletin.Filters = gmBulletin.filters == null ? null : JsonMapper.ToJson(gmBulletin.filters);
			return bulletin;
		}

		public static BulletinItem ConvertGMBulletinItem(GMBulletinItem gmBulletinItem)
		{
			if (gmBulletinItem == null) return null;
			BulletinItem bulletinItem = new BulletinItem();
			bulletinItem.BID = gmBulletinItem.bID;
			bulletinItem.Language = gmBulletinItem.language;
			bulletinItem.Scene = gmBulletinItem.scene;
			bulletinItem.Title = gmBulletinItem.title;
			bulletinItem.Content = gmBulletinItem.content;
			bulletinItem.TargetURL = gmBulletinItem.targetURL;
			bulletinItem.Priority = gmBulletinItem.priority;
			bulletinItem.ImageURL = gmBulletinItem.imageURL;
			bulletinItem.Encoding = gmBulletinItem.encoding;
			bulletinItem.ButtonText = gmBulletinItem.buttonText;
			bulletinItem.StartTime = gmBulletinItem.startTime;
			bulletinItem.ExpireTime = gmBulletinItem.expireTime;
			bulletinItem.Times = gmBulletinItem.times;
			bulletinItem.Tab = gmBulletinItem.tab;
			bulletinItem.ImageInfoJson = gmBulletinItem.imageInfoJson;
			
			List<ImageItem> imageItems = new List<ImageItem>();
			foreach(GMImageItem gmImageItem in gmBulletinItem.imageList)
			{
				ImageItem imageItem = ConvertGMImageItem(gmImageItem);
				if (imageItem != null)
				{
					imageItems.Add(imageItem);
				}
			}
			bulletinItem.ImageList = imageItems;
			bulletinItem.TabLabel = gmBulletinItem.tabLabel;
			bulletinItem.BadgeSwitch = gmBulletinItem.badgeSwitch;
			bulletinItem.Extra = gmBulletinItem.extra;
			return bulletinItem;
		}

		public static ImageItem ConvertGMImageItem(GMImageItem gmImageItem)
		{
			if (gmImageItem == null) return null;
			ImageItem imageItem = new ImageItem();
			imageItem.imageLink = gmImageItem.imageLink;
			imageItem.imageJumpLink = gmImageItem.imageJumpLink;
			imageItem.imageInfoJson = gmImageItem.imageInfoJson;
			return imageItem;
		}
    }
}
