#import "Permission.h"
#import <AVFoundation/AVFoundation.h>
#import <AVFoundation/AVCaptureDevice.h>
#import <AssetsLibrary/AssetsLibrary.h>
//#import <Photos/Photos.h>
//#import <CoreTelephony/CTCellularData.h>

#import "UnityInterface.h"
#import <CoreLocation/CoreLocation.h>
#import <CoreLocation/CLLocationManager.h>
@interface Permission()<CLLocationManagerDelegate>
@property (nonatomic, strong) CLLocationManager *locationManager;
@end

@implementation Permission

- (instancetype)init {
    
    self = [super init];
    if (self) {
        NSLog(@"%@",@"Permission constructor");
        [self initializeLocationService];
    }
    return self;
}

- (void)initializeLocationService 
{
    // 初始化定位管理器
    _locationManager = [[CLLocationManager alloc] init];
    
    // 设置代理
    _locationManager.delegate = self;
    // 设置定位精确度到米
    //_locationManager.desiredAccuracy = kCLLocationAccuracyBest;
    // 设置过滤器为无
    //_locationManager.distanceFilter = kCLDistanceFilterNone;
    // 开始定位
    //[_locationManager startUpdatingLocation];//开始定位之后会不断的执行代理方法更新位置会比较费电所以建议获取完位置即时关闭更新位置服务
    //初始化地理编码器
    //_geocoder = [[CLGeocoder alloc] init];
    [_locationManager requestWhenInUseAuthorization];
    //[_locationManager requestAlwaysAuthorization];//iOS8必须，这两行必须有一行执行，否则无法获取位置信息，和定位
}

//首次调用获取麦克风权限
extern "C" void GetMicrophonePermission()
{
     [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
          if (granted) {
               NSLog(@"%@",@"GetMicrophonePermission granted");
               UnitySendMessage("LoomUtils", "GetMicrophonePermissionCallback", "3");
          }else{
               NSLog(@"%@",@"GetMicrophonePermission not granted");
               UnitySendMessage("LoomUtils", "GetMicrophonePermissionCallback", "2");
          }
     }];
}

//获取麦克风权限准许状态
extern "C" int GetMicrophonePermissionState()
{
    AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
    NSLog(@"%d",status);
    return (int)status;
}


//首次调用获取相机权限
extern "C" void GetCameraPermission()
{
    [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
        if (granted) {
            NSLog(@"%@",@"相机准许");
        }else{
            NSLog(@"%@",@"相机不准许");
        }
    }];
}

//获取相机权限准许状态
extern "C" bool GetCameraPermissionState()
{
    GetCameraPermission();
     AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
        if (status == AVAuthorizationStatusAuthorized){
            NSLog(@"%@",@"相机准许");
        }else{
            NSLog(@"%@",@"相机不准许");
        }
    return (status == AVAuthorizationStatusAuthorized);
}
/*
//首次调用获取相册权限
extern "C" void GetPhotoPermission()
{
    [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
        if (status == PHAuthorizationStatusAuthorized) {
            NSLog(@"%@",@"相册准许");
        }else{
            NSLog(@"%@",@"相册不准许");
        }
        }];
}

//获取相册权限准许状态
extern "C" bool GetPhotoPermissionState()
{
    GetPhotoPermission();
    PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
        if (status == PHAuthorizationStatusAuthorized) {
            NSLog(@"%@",@"相册准许");
        }else{
            NSLog(@"%@",@"相册不准许");
        }
        return (status == PHAuthorizationStatusAuthorized);
}
*/

//打开本程序的设置
extern "C" void OpenAppSettings()
{
    //GetMicrophonePermission();
    //GetCameraPermission();
    NSURL*url=[NSURL URLWithString:UIApplicationOpenSettingsURLString];
    if([[UIApplication sharedApplication]canOpenURL:url]){
        if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 10.0) {
        //设备系统为IOS 10.0或者以上的
        [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:nil];
        }else{
            //设备系统为IOS 10.0以下的
                [[UIApplication sharedApplication]openURL:url];
        }
    }
}



- (int)RequesetLocationPermission
{
    int code = -1;
    BOOL isLocation = [CLLocationManager locationServicesEnabled];
    if(!isLocation)
    {
        NSLog(@"not turn on the location");
        code = 0;
    }
    else
    {
        CLAuthorizationStatus authStatus = CLLocationManager.authorizationStatus;
        switch( authStatus)
        {
            case kCLAuthorizationStatusNotDetermined:
                NSLog(@"kCLAuthorizationStatusNotDetermined");
                code = 1;
                break;
            case kCLAuthorizationStatusRestricted:
                NSLog(@"kCLAuthorizationStatusRestricted");
                code = 2;
                break;
            case kCLAuthorizationStatusAuthorizedAlways:
                NSLog(@"kCLAuthorizationStatusAuthorizedAlways");
                code = 3;
                break;
            case kCLAuthorizationStatusAuthorizedWhenInUse:
                NSLog(@"kCLAuthorizationStatusAuthorizedWhenInUse");
                code = 4;
                break;
            //case kCLAuthorizationStatusAuthorized:
            //  NSLog(@"kCLAuthorizationStatusAuthorized");
            //  break;
            case kCLAuthorizationStatusDenied:
                NSLog(@"kCLAuthorizationStatusDenied");
                code = 5;
                break;
        }
    }
    NSLog(@"RequesetLocationPermission@%d", code);
    if(code == 1)
    {
        [_locationManager requestWhenInUseAuthorization];
    }
    return code;
}

- (void)locationManager:(CLLocationManager *)manager didChangeAuthorizationStatus:(CLAuthorizationStatus)status 
{
   switch( status)
    {
        case kCLAuthorizationStatusNotDetermined:
            NSLog(@"status change callback kCLAuthorizationStatusNotDetermined");
            break;
        case kCLAuthorizationStatusRestricted:
            NSLog(@"status change callback kCLAuthorizationStatusRestricted");
            break;
        case kCLAuthorizationStatusAuthorizedAlways:
            NSLog(@"status change callback kCLAuthorizationStatusAuthorizedAlways");
            break;
        case kCLAuthorizationStatusAuthorizedWhenInUse:
            NSLog(@"status change callback kCLAuthorizationStatusAuthorizedWhenInUse");
            break;
        //case kCLAuthorizationStatusAuthorized:
        //  NSLog(@"kCLAuthorizationStatusAuthorized");
        //  break;
        case kCLAuthorizationStatusDenied:
            NSLog(@"status change callback kCLAuthorizationStatusDenied");
            break;
    }
}

static Permission *permisson;
extern "C" int GetLocationPermissionState()
{
    if(permisson == nil)
    {
        permisson = [[Permission alloc] init];
    }
    int code = [permisson RequesetLocationPermission];
    return code;
}
@end

