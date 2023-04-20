using System.Text;

namespace GSDK
{
    /// <summary>
    /// 返回定位信息的回调
    /// </summary>
    /// <param name="result">
    /// 判断返回是否成功
    /// 可能包含的错误码：
    /// <para>
    ///     LocationByIPError: ip定位出错，可能是无网络
    /// </para>
    /// </param>
    /// <param name="location">定位信息</param>
    public delegate void LocationFetchLocationInfoByIpDelegate(Result result, LocationInfo location);

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Location.Service.MethodName();
    /// </summary>
    public static class Location
    {
        public static ILocationService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Location) as ILocationService; }
        }
    }

    /// <summary>
    /// 提供通用定位和可选定位组件，区别是通用定位不需要申请权限，定位数据精确度不高；定位组件需要申请定位权限，精确度高
    ///
    /// Location通用定位能力，提供通过IP和本地获取定位信息，不涉及权限申请，精确度不高（不包含经纬度）
    /// LocationOptional为可选定位组件，需要引入location组件
    /// </summary>
    public partial interface ILocationService : IService
    {
        /// <summary>
        /// 通过IP获取定位信息
        /// </summary>
        /// <param name="fetchLocationInfoByIpCallback">获取正确则返回定位信息，失败则返回错误码</param>
        void FetchLocationInfoByIp(LocationFetchLocationInfoByIpDelegate fetchLocationInfoByIpCallback);

        /// <summary>
        /// 本地获取定位信息
        /// </summary>
        /// <returns>获取保存在本地的定位信息（数据一般是在SDK初始化时通过IP定位存入本地的）</returns>
        LocationInfo GetLocalLocationInfo();
    }

    /// <summary>
    /// 定位信息
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// 国家，例如"中国"
        /// </summary>
        public string Country;

        /// <summary>
        /// 国家级ASCII码，例如"People's Republic of China"
        /// </summary>
        public string CountryASCII;
        
        /// <summary>
        /// 国家码，例如"CN"
        /// </summary>
        public string CountryCode;

        /// <summary>
        /// 省级，例如"广东"
        /// </summary>
        public string AdministrativeArea;

        /// <summary>
        /// 省级ASCII码，例如"Guangdong Sheng"
        /// </summary>
        public string AdministrativeAreaASCII;

        /// <summary>
        /// 市级，例如"深圳"
        /// </summary>
        public string City;

        /// <summary>
        /// 市级ASCII，例如"Shenzhen"
        /// </summary>
        public string CityASCII;

        /// <summary>
        /// 区级，例如"南山"
        /// </summary>
        public string District;

        /// <summary>
        /// 区级ASCII，例如"Nanshan Qu"
        /// </summary>
        public string DistrictASCII;

        /// <summary>
        /// 经纬度
        /// </summary>
        public double Latitude;

        public double Longitude;

        /// <summary>
        /// geoId
        /// </summary>
        public long CountryGeoNameId;

        public long AdministrativeAreaGeoNameId;
        public long CityGeoNameId;
        public long DistrictGeoNameId;

        /// <summary>
        /// 是否为争议地区
        /// </summary>
        public bool IsDisputed;

        public LocationInfo()
        {
            Country = "";
            CountryCode = "";
            AdministrativeArea = "";
            City = "";
            AdministrativeAreaASCII = "";
            CityASCII = "";
            District = "";
            CountryASCII = "";
            DistrictASCII = "";
            Latitude = 0;
            Longitude = 0;
            CityGeoNameId = 0;
            CountryGeoNameId = 0;
            DistrictGeoNameId = 0;
            AdministrativeAreaGeoNameId = 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{")
                .Append("country= ").Append(Country)
                .Append(", countryCode= ").Append(CountryCode)
                .Append(", administrativeArea= ").Append(AdministrativeArea)
                .Append(", city= ").Append(City)
                .Append(", administrativeAreaASCII= ").Append(AdministrativeAreaASCII)
                .Append(", cityASCII= ").Append(CityASCII)
                .Append(", district= ").Append(District)
                .Append(", countryASCII= ").Append(CountryASCII)
                .Append(", districtASCII= ").Append(DistrictASCII)
                .Append(", latitude= ").Append(Latitude)
                .Append(", longitude= ").Append(Longitude)
                .Append(", cityGeoNameId= ").Append(CityGeoNameId)
                .Append(", countryGeoNameId= ").Append(CountryGeoNameId)
                .Append(", districtGeoNameId= ").Append(DistrictGeoNameId)
                .Append(", administrativeAreaGeoNameId= ").Append(AdministrativeAreaGeoNameId)
                .Append(", isDisputed= ").Append(IsDisputed)
                .Append("}");
            return sb.ToString();
        }
    }

    public static partial class ErrorCode
    {
        /// <summary>
        /// 参数错误
        /// </summary>
        public const int LocationParamInvalid = -210001;

        /// <summary>
        /// 定位sdk初始化失败
        /// </summary>
        public const int LocationSDKInitializeFailed = -212002;
        
        /// <summary>
        /// 没有定位权限 
        /// </summary>
        public const int LocationNoLocationPermission = -212003;

        /// <summary>
        /// 没有GPS相关权限 （永久拒绝）
        /// </summary>
        public const int LocationNoGPSPermission = -210004;
        
        /// <summary>
        /// 没有GPS相关权限（单次拒绝）
        /// </summary>
        public const int LocationNoGPSPermissionWithoutPromot = -212015;
        
        /// <summary>
        /// 没有打开GPS
        /// </summary>
        public const int LocationGpsNotOpen = -210005;

        ///<summary>
        /// 定位超时
        /// </summary>
        public const int LocationTimeOut = -212006;
        
        /// <summary>
        /// 定位失败 
        /// </summary>
        public const int LocationLocateFail = -210007;
        
        /// <summary>
        /// 定位信息返回空  
        /// </summary>
        public const int LocationGetLocationInfoNull = -210008;

        /// <summary>
        /// 后台定位失败
        /// </summary>
        public const int LocationGetBackgroundLocFail = -212009;
        
        /// <summary>
        /// 上报定位数据失败
        /// </summary>
        public const int LocationReportLocationError = -210010;
        
        /// <summary>
        /// 获取附近数据失败
        /// </summary>
        public const int LocationGetNearPoiDataError = -210011;

        /// <summary>
        /// 删除数据失败
        /// </summary>
        public const int LocationDelPoiDataError = -210012;
        
        /// <summary>
        /// 争议地区
        /// </summary>
        public const int LocationDisputedAreaError = -212013;
        
        /// <summary>
        /// 获取IP失败 
        /// </summary>
        public const int LocationGetIPFail = -210014;
        
        /// <summary>
        /// 网络错误异常
        /// </summary>
        public const int LocationNetError = -213001;

        /// <summary>
        /// 未登录错误
        /// </summary>
        public const int LocationNotLoginError = -219800;

        /// <summary>
        /// 未知异常
        /// </summary>
        public const int LocationUnknownError = -219999;
    }
}