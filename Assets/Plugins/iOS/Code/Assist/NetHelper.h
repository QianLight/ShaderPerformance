//
//  NetHelper.h
//  YIosPlugins
//
//  Created by qyg on 2018/7/21.
//  Copyright © 2018年 YaoQiShan. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface NetHelper : NSObject
    
+(NSString *)getIPv6 : (const char *)mHost :(const char *)mPort; 
    
@end
