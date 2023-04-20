using GMSDK;

namespace GSDK
{
    /// <summary>
    /// 内部检测工具
    /// </summary>
    internal static partial class LocationUtils
    {

        /// <summary>
        /// 将GMLocationModel转成LocationInfo
        /// </summary>
        /// <param name="locationModel"></param>
        /// <returns></returns>
        public static LocationInfo Convert(GMLocationModel locationModel)
        {
            if (locationModel == null)
            {
                return null;
            }

            return new LocationInfo
            {
                AdministrativeArea = locationModel.administrativeArea,
                AdministrativeAreaASCII = locationModel.administrativeAreaAsci,
                AdministrativeAreaGeoNameId = locationModel.administrativeAreaGeoNameId,
                City = locationModel.city,
                CityASCII = locationModel.cityAsci,
                CityGeoNameId = locationModel.cityGeoNameId,
                Country = locationModel.country,
                CountryASCII = locationModel.countryAsci,
                CountryCode = locationModel.countryCode,
                CountryGeoNameId = locationModel.countryGeoNameId,
                District = locationModel.district,
                DistrictASCII = locationModel.districtAsci,
                DistrictGeoNameId = locationModel.districtGeoNameId,
                Latitude = locationModel.latitude,
                Longitude = locationModel.longitude,
                IsDisputed = locationModel.isDisputed
            };
        }
    }
}