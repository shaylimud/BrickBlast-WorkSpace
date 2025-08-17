//
//  ChoiceCMPUtils.m
//  UnityFramework
//
//  Created by Abdul Basit on 22/09/23.
//

#import <Foundation/Foundation.h>
#import "ChoiceCMPUtils.h"

@implementation ChoiceCMPUtils

// Converts an NSString into a const char* ready to be sent to Unity
+(char*) cStringCopy: (NSString*) input
{
    const char* string = [input UTF8String];
    return string ? strdup(string) : NULL;
}

+(NSMutableDictionary<NSString*, NSDictionary<NSString*, id>*>*) getDictionaryFromJson:(const char*) json
{
    NSString* jsonString = GetStringParam(json);
    if (jsonString.length == 0)
        return nil;
    NSMutableDictionary<NSString*, NSDictionary<NSString*, id>*>* dict =
        [NSJSONSerialization JSONObjectWithData:[jsonString dataUsingEncoding:NSUTF8StringEncoding]
                                        options:NSJSONReadingMutableContainers
                                          error:nil];
    return dict.count > 0 ? dict : nil;
}

+(NSDictionary *) dictionaryFromObject:(id)obj
{
    if (obj == nil)
        return nil;
    
    NSMutableDictionary *dict = [NSMutableDictionary dictionary];

    unsigned count;
    objc_property_t *properties = class_copyPropertyList([obj class], &count);

    for (int i = 0; i < count; i++) {
        NSString *key = [NSString stringWithUTF8String:property_getName(properties[i])];
        if([obj valueForKey:key] != nil) {
            if ([[obj valueForKey:key] isKindOfClass:[NSDictionary class]]
                && [self isAllKeysOfTypeNSNumber:[obj valueForKey:key]]) {
                [dict setObject: [self convertDictKeyToString:[obj valueForKey:key]] forKey:key];
            } else {
                [dict setObject:[obj valueForKey:key] forKey:key];
            }
        }
    }

    return [NSDictionary dictionaryWithDictionary:dict];
}

+(NSString *) stringFromDict: (NSDictionary*) dict
{
    if([NSJSONSerialization isValidJSONObject:dict]) {
        NSError *error;
        NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:&error];
        if (error) {
            NSLog(@"error converting the dictionary to data, error: %@", error.localizedDescription);
            return nil;
        }
        NSString * jsonString = [[NSString alloc] initWithData:data
                                                      encoding:NSUTF8StringEncoding];
        return jsonString;
    }
    return nil;
}

+(NSDictionary<NSString *, NSNumber *> *) convertDictKeyToString: (NSDictionary *) dict {
    // Create a new NSMutableDictionary with the desired key type (NSString) and the same values (NSNumber)
    NSMutableDictionary<NSString *, NSNumber *> *newDictionary = [NSMutableDictionary dictionary];

    // Iterate through the original dictionary and convert keys to NSString
    for (NSNumber *key in dict) {
        NSString *stringKey = [key stringValue];
        NSNumber *value = dict[key];
        
        // Add the converted key and value to the new dictionary
        [newDictionary setObject:value forKey:stringKey];
    }

    // Now newDictionary is an NSDictionary<NSString *, NSNumber *>
    return newDictionary;

}

+ (BOOL)isAllKeysOfTypeNSNumber:(NSDictionary *)dictionary {
    for (id key in dictionary) {
        if (![key isKindOfClass:[NSNumber class]]) {
            return NO;
        }
    }
    return YES;
}

+ (NSMutableDictionary *)changeKeyName:(NSString *)fromKey to:(NSString *)toKey inDictionary:(NSDictionary *)dictionary
{
    NSMutableDictionary *mutableDict = [dictionary mutableCopy];
    id value = [mutableDict objectForKey:fromKey];
    
    if (value != nil) {
        // Remove the old key-value pair
        [mutableDict removeObjectForKey:fromKey];

        // Add the value with the new key
        [mutableDict setObject:value forKey:toKey];
    }
    return mutableDict;
}

@end
