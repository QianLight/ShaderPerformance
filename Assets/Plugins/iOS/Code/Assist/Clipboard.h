//
//  Clipboard.h
//
//  Created by penghuailiang on 2020/02/17.
//  Copyright © 2020年 Penghuailiang. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface Clipboard : NSObject


+(void) _copyTextToClipboard:(const char*) textList;

+(void)_clearClipboard;

+(NSString*)_getClipboard;


@end
