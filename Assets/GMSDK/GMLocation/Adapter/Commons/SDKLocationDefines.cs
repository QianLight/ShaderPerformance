using System;
using System.Collections.Generic;

namespace GMSDK
{
    public static class LocationMethodName
    {
        public const string SDKInit = "registerLocation";
        public const string GetAccurateLocation = "requestAndUpdateCurrentLocation";
        public const string SDKGetNearPoiData = "requestGetNearPoiData";
        public const string SDKDeltePoiData = "requestDeletePoiData";
        public const string SDKReportPoiData = "requestReportPoiData";
    }

    public class LocationCallback : CallbackResult
    {
        public GMLocationModel Location;

        public LocationCallback(GMLocationModel location)
        {
            Location = location;
        }
    }

    public class LocationResultName
    {
        public const string SDKGetPoiDataResult = "requestGetPoiDataResult";
        public const string SDKReportPoiDataResult = "requestReportPoiDataResult";

    }

    //lbs过滤条件
    public class GMPoiFilter
    {
        public string key;
        public string op;
        public string value;
    }
    //lbs获取附近的人回调结果
    public class GMNearPoiDataResult : CallbackResult
    {
        public GMNearPoiDataModel data;

    }

    public class GMNearPoiDataModel
    {
        public List<GMNearPoiData> points;
        public string nextPageToken;
    }
    public class GMNearPoiData
    {
        public string primaryKey;
        public double distance;
        public Dictionary<String, String> properties;
        public GMLatLng latLng;
    }
    public class GMLatLng
    {
        public double longitude;
        public double latitude;
    }
}
