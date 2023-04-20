//
//  Clipboard.mm
//
//  Created by penghuailiang on 2020/02/17.
//  Copyright © 2020年 Penghuailiang. All rights reserved.
//

#import "Clipboard.h"

#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

void _clearClipboard()
{
     UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
     pasteboard.string =@"";
}

   
void _copyTextToClipboard(const char *textList)
{   
    NSString *text = [NSString stringWithUTF8String: textList] ;
   
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    pasteboard.string = text;
}


const char* _getClipboard()
{   
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    NSString *text = pasteboard.string;
    return MakeStringCopy(text);
}
