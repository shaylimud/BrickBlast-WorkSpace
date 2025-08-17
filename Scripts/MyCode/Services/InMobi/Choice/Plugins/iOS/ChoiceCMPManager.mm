
//
//  ChoiceCMPManager.m
//  InMobi
//
//  Copyright (c) 2023 InMobi. All rights reserved.
//

#import "ChoiceCMPManager.h"
#import <ChoiceMobile/ChoiceMobile-Swift.h>
#import <Foundation/Foundation.h>
#import "ChoiceCMPUtils.h"

#ifdef __cplusplus
extern "C" {
#endif
    // life cycle management
    void UnityPause(int pause);
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
#ifdef __cplusplus
}
#endif

@implementation ChoiceCMPManager

// Manager to be used for methods that do not require a specific adunit to operate on.
+ (ChoiceCMPManager*)shared
{
    static ChoiceCMPManager* sharedManager = nil;

    if (!sharedManager)
        sharedManager = [[ChoiceCMPManager alloc] init];

    return sharedManager;
}

+ (void)sendUnityEvent:(NSString*)eventName withArgs:(NSString*)args
{
    UnitySendMessage("ChoiceCMPManager", eventName.UTF8String, args.UTF8String);
}

- (void)sendUnityEvent:(NSString*)eventName
{
    [[self class] sendUnityEvent:eventName withArgs:@""];
}

- (void)startChoiceWithPCode:(NSString *)pCode shouldDisplayIDFA:(BOOL)shouldDisplayIDFA {
    [[ChoiceCmp shared] startChoiceWithPcode: pCode delegate: self ccpaDelegate: self shouldDisplayIDFA: shouldDisplayIDFA];
}

- (void)showCCPAWithPCode:(NSString *)pCode {
    [[ChoiceCmp shared] startCCPAWithPcode:pCode ccpaDelegate:self];
}

- (NSString*)getTCString {
    return  [[ChoiceCmp shared] getTCString];
}

- (void)forceDisplayUI {
    [[ChoiceCmp shared] forceDisplayUI];
}

- (void)startChoiceFromWeb {
    [[ChoiceCmp shared] startChoiceFromWeb];
}

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - ChoiceCmpDelegate

- (void)cmpDidErrorWithError:(NSError * _Nonnull)error {
    [[self class] sendUnityEvent:@"EmitCMPDidErrorEvent" withArgs:error.localizedDescription];
}

- (void)cmpDidLoadWithInfo:(PingResponse * _Nonnull)info {
    NSDictionary *dict = [ChoiceCMPUtils dictionaryFromObject:info];
    
    NSMutableDictionary *mutableDict = [ChoiceCMPUtils changeKeyName:@"doesGdprApply" to:@"gdprApplies" inDictionary:dict];
    
    NSString *pingResponseStr = [ChoiceCMPUtils stringFromDict:mutableDict];
    
    if (pingResponseStr != nil)
        [[self class] sendUnityEvent:@"EmitCMPDidLoadEvent" withArgs:pingResponseStr];
     else
         [self sendUnityEvent:@"EmitCMPDidLoadEvent"];
}

- (void)cmpDidShowWithInfo:(PingResponse * _Nonnull)info {
    NSDictionary *dict = [ChoiceCMPUtils dictionaryFromObject:info];
    
    NSMutableDictionary *mutableDict = [ChoiceCMPUtils changeKeyName:@"doesGdprApply" to:@"gdprApplies" inDictionary:dict];
    
    NSString *pingResponseStr = [ChoiceCMPUtils stringFromDict:mutableDict];
    
    if (pingResponseStr != nil)
        [[self class] sendUnityEvent:@"EmitCMPDidShowEvent" withArgs:pingResponseStr];
     else
         [self sendUnityEvent:@"EmitCMPDidShowEvent"];
}

- (void)didReceiveAdditionalConsentWithAcData:(ACData * _Nonnull)acData updated:(BOOL)updated {
    NSDictionary *dict = [ChoiceCMPUtils dictionaryFromObject:acData];
    NSString *acDataStr = [ChoiceCMPUtils stringFromDict:dict];
    
    if (acDataStr != nil)
        [[self class] sendUnityEvent:@"EmitCMPDidReceiveAdditionalConsentEvent" withArgs:acDataStr];
     else
         [self sendUnityEvent:@"EmitCMPDidReceiveAdditionalConsentEvent"];
    
}

- (void)didReceiveIABVendorConsentWithTcData:(TCData * _Nonnull)tcData updated:(BOOL)updated {
    NSMutableDictionary *dictionary = [NSMutableDictionary dictionary];
    [dictionary setObject:tcData.tcString forKey:@"tcString"];
    
    NSString *tcDataStr = [ChoiceCMPUtils stringFromDict:dictionary];
    
    if (tcDataStr != nil)
        [[self class] sendUnityEvent:@"EmitCMPDidReceiveIABVendorConsentEvent" withArgs:tcDataStr];
     else
         [self sendUnityEvent:@"EmitCMPDidReceiveIABVendorConsentEvent"];
}

- (void)didReceiveNonIABVendorConsentWithNonIabData:(NonIABData * _Nonnull)nonIabData updated:(BOOL)updated {
    NSDictionary *dict = [ChoiceCMPUtils dictionaryFromObject:nonIabData];
    NSString *nonIabDataStr = [ChoiceCMPUtils stringFromDict:dict];
    
    if (nonIabDataStr != nil)
        [[self class] sendUnityEvent:@"EmitCMPDidReceiveNonIABVendorConsentEvent" withArgs:nonIabDataStr];
     else
         [self sendUnityEvent:@"EmitCMPDidReceiveNonIABVendorConsentEvent"];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - CCPADelegate
- (void)didReceiveCCPAConsentWithString:(NSString * _Nonnull)string {
    [[self class] sendUnityEvent:@"EmitCMPDidReceiveCCPAConsentEvent" withArgs:string];
}

@end
