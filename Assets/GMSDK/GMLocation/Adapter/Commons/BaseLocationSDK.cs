using System;
using System.Collections.Generic;

using UNBridgeLib.LitJson;
using UnityEngine;
using UNBridgeLib;

namespace GMSDK
{
    /// <summary>
    /// location组件接口
    ///
    /// Location一级错误码使用：GSDK.ErrorCode.LocationXXX
    /// 具体内容请参考 Assets/GSDK/Service/Location/ILocationService.cs 中的 ErrorCode 部分
    /// </summary>
    public class BaseLocationSDK
    {
        public BaseLocationSDK()
        {
#if UNITY_ANDROID
			UNBridge.Call (LocationMethodName.SDKInit, null);
#endif
        }

        /// <summary>
        /// 获取精确定位数据
        /// </summary>
        /// <param name="callback">回调返回数据</param>
        public void GetAccurateLocation(Action<LocationCallback> callback)
        {
            LogUtils.D("ONLocationByIpButtonClicked");
            LocationCallbackHandler unCallBack = new LocationCallbackHandler()
            {
                LocationCallback = callback
            };

            unCallBack.OnSuccess = unCallBack.OnLocationCallBack;
            UNBridge.Call(LocationMethodName.GetAccurateLocation, new JsonData(), unCallBack);
        }

        /// <summary>
        /// LBS获取附近指定范围和数量的poi数据集
        /// </summary>
        /// <param name="dataSet">接入的数据集名称</param>
        /// <param name="maxDistance">最大距离</param>
        /// <param name="minDistance">最小距离</param>
        /// <param name="num">数量</param>
        /// <param name="filters">过滤条件</param>
        /// <param name="callback">回调</param>
        public void GetNearPoiData(string dataSet, double maxDistance, double minDistance, int num,
            Action<GMNearPoiDataResult> callback, List<GMPoiFilter> filters = null)
        {
            LocationCallbackHandler unCallback = new LocationCallbackHandler()
            {
                GETPoiDataCallBack = callback
            };
            JsonData p = new JsonData();
            if (filters != null && filters.Count > 0)
            {
                p["filters"] = JsonMapper.ToObject(JsonMapper.ToJson(filters));
            }
            if (dataSet == null)
            {
                dataSet = "";
            }
            p["dataSet"] = dataSet;
            p["maxDistance"] = maxDistance;
            p["minDistance"] = minDistance;
            p["num"] = num;
            JsonData final = new JsonData();
            final["data"] = p;

            unCallback.OnSuccess = unCallback.OnGetPoiDataCallBack;
            UNBridge.Listen(LocationResultName.SDKGetPoiDataResult, unCallback);
            UNBridge.Call(LocationMethodName.SDKGetNearPoiData, final);
        }

        /// <summary>
        /// LBS获取附近指定范围和数量的poi数据集,支持传入经纬度参数
        /// </summary>
        /// <param name="dataSet">接入的数据集名称</param>
        /// <param name="maxDistance">最大距离</param>
        /// <param name="minDistance">最小距离</param>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="num">数量</param>
        /// <param name="filters">过滤条件</param>
        /// <param name="callback">回调</param>
        public void GetNearPoiData(string dataSet, double maxDistance, double minDistance, double longitude, double latitude, int num,
            Action<GMNearPoiDataResult> callback, List<GMPoiFilter> filters = null)
        {
            LocationCallbackHandler unCallback = new LocationCallbackHandler()
            {
                GETPoiDataCallBack = callback
            };
            JsonData p = new JsonData();
            if (filters != null && filters.Count > 0)
            {
                p["filters"] = JsonMapper.ToObject(JsonMapper.ToJson(filters));
            }
            if (dataSet == null)
            {
                dataSet = "";
            }
            p["dataSet"] = dataSet;
            p["maxDistance"] = maxDistance;
            p["minDistance"] = minDistance;
            p["longitude"]=longitude;
            p["latitude"]=latitude;
            p["num"] = num;
            JsonData final = new JsonData();
            final["data"] = p;

            unCallback.OnSuccess = unCallback.OnGetPoiDataCallBack;
            UNBridge.Listen(LocationResultName.SDKGetPoiDataResult, unCallback);
            UNBridge.Call(LocationMethodName.SDKGetNearPoiData, final);
        }

        /// <summary>
        /// LBS从指定dataSet删除指定primaryKey集合的数据
        /// </summary>
        /// <param name="dataSet">接入的数据集名称</param>
        /// <param name="primaryKeys">需要删除的primaryKeys</param>
        /// <param name="callback">回调</param>
        public void DeletePoiData(string dataSet, List<string> primaryKeys, Action<CallbackResult> callback)
        {
            LocationCallbackHandler unCallback = new LocationCallbackHandler
            {
                deletePoiDataCallBack = callback
            };
            if (dataSet == null)
            {
                dataSet = "";
            }
            JsonData data = new JsonData();
            if (primaryKeys != null && primaryKeys.Count > 0)
            {
                foreach (var primaryKey in primaryKeys)
                {
                    data.Add(primaryKey);
                }
            }

            JsonData finalParam = new JsonData();
            finalParam["primaryKeys"] = data;
            finalParam["dataSet"] = dataSet;
            unCallback.OnSuccess = unCallback.OnDeletePoiDataCallBack;
            UNBridge.Call(LocationMethodName.SDKDeltePoiData, finalParam, unCallback);
        }

        /// <summary>
        /// LBS上报单个数据
        /// </summary>
        /// <param name="dataSet">接入数据集</param>
        /// <param name="primaryKey">单个poi数据唯一标识</param>
        /// <param name="callback">回调</param>
        /// <param name="properties">其他自定义key-value对数据(可选)，可用于查询、筛选等,如头像、昵称、年龄范围(用于筛选)</param>
        public void SdkReportPoiData(string dataSet, string primaryKey, Action<CallbackResult> callback, JsonData properties = null)
        {
            LocationCallbackHandler unCallback = new LocationCallbackHandler()
            {
                reportPoiDataCallBack = callback
            };
            JsonData p = new JsonData();
            if (dataSet == null)
            {
                dataSet = "";
            }
            p["dataSet"] = dataSet;
            if (primaryKey == null)
            {
                primaryKey = "";
            }
            p["primaryKey"] = primaryKey;
            p["properties"] = properties;
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnReportPoiDataCallBack);
            UNBridge.Listen(LocationResultName.SDKReportPoiDataResult, unCallback);
            UNBridge.Call(LocationMethodName.SDKReportPoiData, finalParam);
        }

        /// <summary>
        /// LBS上报单个数据（自定义经纬度）
        /// </summary>
        /// <param name="dataSet">接入数据集</param>
        /// <param name="primaryKey">单个poi数据唯一标识</param>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="callback">回调</param>
        /// <param name="properties">其他自定义key-value对数据(可选)，可用于查询、筛选等,如头像、昵称、年龄范围(用于筛选)</param>
        public void SdkReportPoiData(string dataSet, string primaryKey, double longitude, double latitude, Action<CallbackResult> callback, JsonData properties = null)
        {
            LocationCallbackHandler unCallback = new LocationCallbackHandler()
            {
                reportPoiDataCallBack = callback
            };
            JsonData p = new JsonData();
            if (dataSet == null)
            {
                dataSet = "";
            }
            p["dataSet"] = dataSet;
            if (primaryKey == null)
            {
                primaryKey = "";
            }
            p["primaryKey"] = primaryKey;
            p["properties"] = properties;
            p["longitude"] = longitude;
            p["latitude"] = latitude;
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnReportPoiDataCallBack);
            UNBridge.Listen(LocationResultName.SDKReportPoiDataResult, unCallback);
            UNBridge.Call(LocationMethodName.SDKReportPoiData, finalParam);
        }

    }
}
