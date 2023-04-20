#import <Foundation/Foundation.h>

void closeapplication (char *str1, char *str2)
{
  NSLog(@"###%@", [NSString stringWithFormat:@"%@ %@", "CloseApp", "CloseApp"]);
  exit(0);
}