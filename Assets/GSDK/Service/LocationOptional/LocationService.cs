using System.Collections.Generic;
using System.Linq;
using GMSDK;
using GMSDK.GMLocation;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public partial class LocationService : ILocationService
    {
        public void FetchAccurateLocation(LocationFetchAccurateLocationDelegate locationFetchAccurateLocationCallback)
        {
            GMLocationMgr.instance.SDK.GetAccurateLocation(locationCallback =>
            {
                var result = InnerTools.ConvertToResult(locationCallback);
                var locationInfo = LocationUtils.Convert(locationCallback.Location);

                InnerTools.SafeInvoke((() => { locationFetchAccurateLocationCallback.Invoke(result, locationInfo); }));
            });
        }

        public void ReportPoiData(CurrentPoiData poiData,
            LocationReportPoiDataDelegate reportPoiDataCallback)
        {
            if (poiData == null)
            {
                GLog.LogError("CurrentPoiData is null.");
                return;
            }

            JsonData propertiesJson = null;
            if (poiData.Properties != null)
            {
                propertiesJson = JsonMapper.ToObject(JsonMapper.ToJson(poiData.Properties));
            }
            if (poiData.Latitude != null && poiData.Longitude != null)
            {
                double longitude = (double)poiData.Longitude;
                double latitude = (double)poiData.Latitude;
                GMLocationMgr.instance.SDK.SdkReportPoiData(poiData.DataSetName, poiData.PoiUid, longitude, latitude, callback =>
                {
                    InnerTools.SafeInvoke((() =>
                    {
                        var result = InnerTools.ConvertToResult(callback);
                        reportPoiDataCallback.Invoke(result);
                    }));
                },
                propertiesJson);
            }
            else
            {
                GMLocationMgr.instance.SDK.SdkReportPoiData(poiData.DataSetName, poiData.PoiUid, callback =>
                {
                    InnerTools.SafeInvoke((() =>
                    {
                        var result = InnerTools.ConvertToResult(callback);
                        reportPoiDataCallback.Invoke(result);
                    }));
                },
                propertiesJson);
            }
                
        }

        public void FetchNearPoiData(TargetRangeOfPoi targetRangeOfPoi,
            LocationFetchNearPoiDataDelegate fetchNearPoiDataCallback)
        {
            // 缺少必传参数
            if (!LocationUtils.CheckPointOfInformation(targetRangeOfPoi))
            {
                return;
            }

            var filters = targetRangeOfPoi.Filters;
            List<GMPoiFilter> gmPoiFilters = null;
            // filters可空
            if (filters != null)
            {
                gmPoiFilters = new List<GMPoiFilter>();
                gmPoiFilters.AddRange(targetRangeOfPoi.Filters.Select(LocationUtils.Convert));
            }
            
            if (targetRangeOfPoi.Latitude.Equals(TargetRangeOfPoi.LatAndLongUninitialized) &&
               targetRangeOfPoi.Longitude.Equals(TargetRangeOfPoi.LatAndLongUninitialized))
            {
                GLog.LogInfo("GetNearPoiData without longitude and latitude");
                GMLocationMgr.instance.SDK.GetNearPoiData(targetRangeOfPoi.DataSetName,
                targetRangeOfPoi.MaximumDistanceFromPoint, targetRangeOfPoi.MinimumDistanceFromPoint,
                targetRangeOfPoi.LimitedNumberOfSet,
                nearPoiDataResult =>
                {
                    var result = InnerTools.ConvertToResult(nearPoiDataResult);

                    var nearPoiList = new List<NearPoiData>();
                    if (nearPoiDataResult.data != null && nearPoiDataResult.data.points != null)
                    {
                        nearPoiList.AddRange(nearPoiDataResult.data.points.Select(LocationUtils.Convert));
                    }
                    else
                    {
                        GLog.LogError("Get Near PoiData return null, see error code and message.");
                    }

                    InnerTools.SafeInvoke((() => { fetchNearPoiDataCallback.Invoke(result, nearPoiList); }));
                }, gmPoiFilters);
            }
            else
            {
                GLog.LogInfo("GetNearPoiData with longitude and latitude");
                GMLocationMgr.instance.SDK.GetNearPoiData(targetRangeOfPoi.DataSetName, targetRangeOfPoi.MaximumDistanceFromPoint,
                targetRangeOfPoi.MinimumDistanceFromPoint, targetRangeOfPoi.Longitude, targetRangeOfPoi.Latitude, targetRangeOfPoi.LimitedNumberOfSet,
                nearPoiDataResult =>
                {
                    var result = InnerTools.ConvertToResult(nearPoiDataResult);

                    var nearPoiList = new List<NearPoiData>();
                    if (nearPoiDataResult.data != null && nearPoiDataResult.data.points != null)
                    {
                        nearPoiList.AddRange(nearPoiDataResult.data.points.Select(LocationUtils.Convert));
                    }
                    else
                    {
                        GLog.LogError("Get Near PoiData return null, see error code and message.");
                    }

                    InnerTools.SafeInvoke((() => { fetchNearPoiDataCallback.Invoke(result, nearPoiList); }));
                }, gmPoiFilters);
            }
        }

        public void DeletePointsOfInformation(string dataSetName, List<string> poiUidList,
            LocationDeletePoiDataDelegate locationChangePoiDataCallback)
        {
            GMLocationMgr.instance.SDK.DeletePoiData(dataSetName, poiUidList, deleteResult =>
            {
                var result = InnerTools.ConvertToResult(deleteResult);
                InnerTools.SafeInvoke((() => { locationChangePoiDataCallback.Invoke(result); }));
            });
        }
    }
}