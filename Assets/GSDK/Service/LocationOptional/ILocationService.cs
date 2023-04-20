using System.Collections.Generic;

namespace GSDK
{
    #region LocationOptionalDelegate

    /// <summary>
    /// 返回定位信息的回调
    /// </summary>
    /// <param name="result">判断返回是否成功</param>
    /// <param name="locationInfo">定位信息</param>
    public delegate void LocationFetchAccurateLocationDelegate(Result result, LocationInfo locationInfo);

    /// <summary>
    /// 获取附近的信息点的回调
    /// </summary>
    /// <param name="result">
    /// 判断返回是否成功
    /// <para>
    /// 可能返回的错误码：
    /// Success：成功
    /// LocationNotLoginError：没有登陆
    /// LocationParamInvalid：参数不合法
    /// LocationNoGPSPermission：没有GPS相关权限（永久拒绝）
    /// LocationNoGPSPermissionWithoutPromot：没有GPS相关权限（单次拒绝）
    /// LocationLocateFail：定位失败
    /// LocationGpsNotOpen：没有打开GPS
    /// LocationGetNearPoiDataError：获取附近数据失败
    /// </para>
    /// </param>
    /// <param name="nearPoiData">符合条件的附近的信息点</param>
    public delegate void LocationFetchNearPoiDataDelegate(Result result, List<NearPoiData> nearPoiData);

    /// <summary>
    /// 上报用户信息点
    /// </summary>
    /// <param name="result">
    /// 判断返回是否成功
    ///<para>
    /// 可能返回的错误码：
    /// Success：成功
    /// LocationNotLoginError：没有登陆
    /// LocationParamInvalid：参数不合法
    /// LocationNoGPSPermission：没有GPS相关权限（永久拒绝）
    /// LocationNoGPSPermissionWithoutPromot：没有GPS相关权限（单次拒绝）
    /// LocationLocateFail：定位失败
    /// LocationReportLocationError：上报定位数据失败
    /// LocationGpsNotOpen：没有打开GPS
    /// </para>
    /// </param>
    public delegate void LocationReportPoiDataDelegate(Result result);

    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="result">
    /// 判断返回是否成功
    /// <para>
    /// 可能返回的错误码：
    /// Success：成功
    /// LocationNotLoginError：没有登陆
    /// LocationParamInvalid： 参数错误
    /// LocationDelPoiDataError：删除数据失败
    /// </para>
    /// </param>
    public delegate void LocationDeletePoiDataDelegate(Result result);

    #endregion

    /// <summary>
    /// 可选定位组件
    /// 包括获取精确定位和LBS能力（包括上报用户信息点、获取当前用户附近的信息点和删除指定信息点），因LBS需获取经纬度信息，所以调用接口需要定位权限，
    /// 同时也需要登陆后才能使用（防止非法请求恶意篡改）
    /// 这里PointOfInformation（POI）通常指用户信息点
    ///
    /// 所有接口都需申请定位权限，若失败则无法正常返回数据，并返回对应错误码
    /// 当没有权限调用接口，接口内部会申请权限，若授权失败则会返回错误码
    /// </summary>
    public partial interface ILocationService : IService
    {
        /// <summary>
        /// 获取精确定位信息（包含经纬度），需要用户授权定位、读取权限
        ///
        /// 若没有定位权限，调用时会向用户请求定位、读取权限，同意后即返回数据，不同意返回为空
        /// </summary>
        /// <param name="callback">通过回调返回定位数据</param>
        void FetchAccurateLocation(LocationFetchAccurateLocationDelegate callback);

        /// <summary>
        /// 上报单个用户的信息点（需要登陆后使用）
        /// 
        /// 会同时上报当前设备的信息点的经纬度；需要登陆是为了防止非法请求篡改数据
        /// </summary>
        /// <param name="poiData">上传的poi数据类型，其中properties为可选</param>
        /// <param name="reportPoiDataCallback">回调返回成功，或失败时的错误码和信息</param>
        void ReportPoiData(CurrentPoiData poiData,
            LocationReportPoiDataDelegate reportPoiDataCallback);

        /// <summary>
        /// 获取当前用户附近指定数量和距离的信息点（需要登陆后使用）
        ///
        /// 首先会获取当前用户的位置（经纬度）作为中心点，接着再通过传入的距离范围和特征筛选符合条件的信息点集合
        /// </summary>
        /// <param name="targetRangeOfPoi">获取指定范围的信息点数据，其中除了筛选条件Filters为可选传入，其余参数必须传入</param>
        /// <param name="fetchNearPoiDataCallback">返回符合条件的信息点集合</param>
        void FetchNearPoiData(TargetRangeOfPoi targetRangeOfPoi,
            LocationFetchNearPoiDataDelegate fetchNearPoiDataCallback);

        /// <summary>
        /// 删除指定信息点集合（需要登陆后使用）
        /// </summary>
        /// <param name="dataSetName">数据集名称，用于做数据隔离，需要找GSDK中台服务端同学「李春辉」配置</param>
        /// <param name="poiUidList">需要删除的信息点的唯一标识集合</param>
        /// <param name="locationChangePoiDataCallback">回调返回成功，或失败时的错误码和信息</param>
        void DeletePointsOfInformation(string dataSetName, List<string> poiUidList,
            LocationDeletePoiDataDelegate locationChangePoiDataCallback);
    }

    /// <summary>
    /// 上报当前信息点的数据
    /// </summary>
    public class CurrentPoiData
    {
        // 数据集名称，用于做数据隔离，需要找GSDK中台服务端同学「李春辉」配置
        public string DataSetName;

        // 单个信息点的唯一标识
        public string PoiUid;

        // （可选）自定义特征，可用于筛选，结构是key-value，可以为昵称和年龄范围，比如"age：10"、"name"："Tom"
        public Dictionary<string, string> Properties = null;

        public double? Longitude;
        public double? Latitude;


        public CurrentPoiData(string dataSetName, string poiUid, Dictionary<string, string> properties = null)
        {
            DataSetName = dataSetName;
            PoiUid = poiUid;
            Properties = properties;
        }

        public CurrentPoiData(string dataSetName, string poiUid, Dictionary<string, string> properties = null, double? longitude = null, double? latitude = null)
        {
            DataSetName = dataSetName;
            PoiUid = poiUid;
            Properties = properties;
            if (longitude != null && latitude != null)
            {
                Longitude = longitude;
                Latitude = latitude;
            }

        }

    }

    /// <summary>
    /// 上报信息点数据（FetchNearPointOfInformation）的数据层
    /// 整个数据范围是一个圆心为当前用户，半径为MaximumDistanceFromPoint减去半径为MinimumDistanceFromPoint的空心圆
    ///
    /// 必传：DataSetName、MinimumDistanceFromPoint、MaximumDistanceFromPoint、LimitedNumberOfSet
    /// 可选：Filters
    /// </summary>
    public class TargetRangeOfPoi
    {
        public const int Uninitialized = -1;

        /// <summary>
        /// 初始化经纬度的值
        /// </summary>
        public const double LatAndLongUninitialized = 360.0;

        /// <summary>
        /// 数据集名称，用于做数据隔离，需要找GSDK中台服务端同学「李春辉」配置
        /// </summary>
        public string DataSetName;

        /// <summary>
        /// 当前距离用户（圆心）的最小范围（单位：米）
        /// </summary>
        public double MinimumDistanceFromPoint = Uninitialized;

        /// <summary>
        /// 当前距离用户（圆心）的最大范围（单位：米）
        /// </summary>
        public double MaximumDistanceFromPoint = Uninitialized;

        /// <summary>
        /// 经度
        /// </summary>
        public double Longitude = LatAndLongUninitialized;

        /// <summary>
        /// 纬度
        /// </summary>
        public double Latitude = LatAndLongUninitialized;

        /// <summary>
        /// 返回符合条件的最大数量
        /// </summary>
        public int LimitedNumberOfSet = Uninitialized;

        /// <summary>
        /// 「可选」自定义筛选特征，与上报时传入的properties参数的key-value对应
        /// 均为字符串类型，结构为key-op-value
        /// key对应上报时的key，op包括 = < > <= >= ，value为需要与上报的value进行比较的值
        ///
        /// 例如：上报了单点A的特征为age:10，点B点特征为age:15，若A和B都在信息点附近，Filers传入age（key） >（op） 12（value），则返回数据会包括点B（过滤了点A）
        /// </summary>
        public List<PointOfInformationFilter> Filters;

        public TargetRangeOfPoi(string dataSetName, double minimumDistanceFromPoint, double maximumDistanceFromPoint,
            int limitedNumberOfSet, List<PointOfInformationFilter> filters = null)
        {
            DataSetName = dataSetName;
            MinimumDistanceFromPoint = minimumDistanceFromPoint;
            MaximumDistanceFromPoint = maximumDistanceFromPoint;
            LimitedNumberOfSet = limitedNumberOfSet;
            Filters = filters;
        }

        public TargetRangeOfPoi(string dataSetName, double minimumDistanceFromPoint, double maximumDistanceFromPoint, double longitude,
            double latitude, int limitedNumberOfSet, List<PointOfInformationFilter> filters = null) :
            this(dataSetName, minimumDistanceFromPoint, maximumDistanceFromPoint, limitedNumberOfSet, filters)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }

    /// <summary>
    /// 信息点过滤条件
    /// </summary>
    public class PointOfInformationFilter
    {
        /// <summary>
        /// 属性：可为age、sex、country、name等自定义特征
        /// </summary>
        public string PropertyName;

        /// <summary>
        /// 运算符：< , > , = 
        /// </summary>
        public string Operator;

        /// <summary>
        /// 常量值：用于筛选
        /// </summary>
        public string ConstantValue;
    }

    /// <summary>
    /// 返回附近的信息点数据
    /// </summary>
    public class NearPoiData
    {
        /// <summary>
        /// 单个信息点的唯一标识
        /// </summary>
        public string UniqueIdentifier;

        /// <summary>
        /// 与查询点的距离
        /// </summary>
        public double DistanceFromPoint;

        /// <summary>
        /// 信息点的自定义特征
        /// </summary>
        public Dictionary<string, string> Properties;

        /// <summary>
        /// 信息点经度
        /// </summary>
        public double Longitude;

        /// <summary>
        /// 信息点纬度
        /// </summary>
        public double Latitude;
    }
}