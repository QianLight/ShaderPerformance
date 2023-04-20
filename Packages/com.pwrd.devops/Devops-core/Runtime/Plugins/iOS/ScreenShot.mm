
#import "ScreenShot.h"

@implementation ScreenShotService

// Take the screenshot
+ (UIImage *) getScreenshot
{
    UIWindow* screenWindow = [[UIApplication sharedApplication] keyWindow];
    UIGraphicsBeginImageContextWithOptions(screenWindow.frame.size, NO, [UIScreen mainScreen].scale);
    [screenWindow.rootViewController.view drawViewHierarchyInRect:screenWindow.rootViewController.view.bounds afterScreenUpdates:NO];
    UIImage* image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();    
    return image;
}
 
+(NSString*) saveScreenshot
{
    UIImage *outputImage = [self getScreenshot];
    NSData* imageData = UIImageJPEGRepresentation(outputImage, 0.5);
    
    if(imageData != nil)
    {
        NSString* encodeData = [imageData base64EncodedStringWithOptions:0];
        return encodeData;
    }
    return @"";
}

@end

char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

 extern "C"
 {
     const char* sreenShot()
     {
         NSString* base64String = [ScreenShotService saveScreenshot];
         if([base64String length] > 0)
         {
             return MakeStringCopy([base64String UTF8String]);
         }
         return NULL;
     }

     void screenShotAsync()
     {          
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT,0), ^{
            UIImage *outputImage = [ScreenShotService getScreenshot];
            NSData* imageData = UIImageJPEGRepresentation(outputImage, 0.5);
            NSString* encodeData = [imageData base64EncodedStringWithOptions:0];
            char* base64String = MakeStringCopy([encodeData UTF8String]);
            dispatch_async(dispatch_get_main_queue(), ^{
                UnitySendMessage("DevopsScreenshot", "ReceiveImageBase64Method", base64String);
            });
        });
     }
 }
 
