using GMSDK;

namespace GSDK
{
    /// <summary>
    /// 内部检测工具
    /// </summary>
    internal static partial class LocationUtils
    {
        /// <summary>
        /// PointOfInformationFilter转成GMPoiFilter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static GMPoiFilter Convert(PointOfInformationFilter filter)
        {
            if (filter == null)
            {
                GLog.LogError("PointOfInformationFilter is null.");
                return null;
            }

            return new GMPoiFilter
            {
                key = filter.PropertyName,
                op = filter.Operator,
                value = filter.ConstantValue
            };
        }

        public static bool CheckPointOfInformation(TargetRangeOfPoi targetRangeOfPoi)
        {
            if (targetRangeOfPoi == null)
            {
                GLog.LogError("TargetRangeOfPoi is null.");
                return false;
            }

            if (string.IsNullOrEmpty(targetRangeOfPoi.DataSetName))
            {
                GLog.LogError("TargetRangeOfPoi miss DataSetName.");
                return false;
            }

            if (targetRangeOfPoi.LimitedNumberOfSet < 0)
            {
                GLog.LogError("TargetRangeOfPoi miss LimitedNumberOfSet or is invalid(Maybe is negative).");
                return false;
            }

            if (targetRangeOfPoi.MaximumDistanceFromPoint < 0 ||
                targetRangeOfPoi.MinimumDistanceFromPoint < 0)
            {
                GLog.LogError(
                    "TargetRangeOfPoi miss MaximumDistanceFromPoint or MinimumDistanceFromPoint, or is invalid(Maybe is negative).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// GMNearPoiData转成NearPointOfInformation
        /// </summary>
        /// <param name="nearPoiData"></param>
        /// <returns></returns>
        public static NearPoiData Convert(GMNearPoiData nearPoiData)
        {
            if (nearPoiData != null)
                return new NearPoiData
                {
                    UniqueIdentifier = nearPoiData.primaryKey,
                    DistanceFromPoint = nearPoiData.distance,
                    Properties = nearPoiData.properties,
                    Latitude = nearPoiData.latLng.latitude,
                    Longitude = nearPoiData.latLng.longitude
                };

            GLog.LogError("GMNearPoiData is null.");
            return null;
        }
    }
}