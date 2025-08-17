
//
//  ChoiceCMPManager.h
//  InMobi
//
//  Copyright (c) 2023 InMobi. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <ChoiceMobile/ChoiceMobile-Swift.h>


@interface ChoiceCMPManager : NSObject <ChoiceCmpDelegate, CCPADelegate>

+ (ChoiceCMPManager*) shared;

+ (void)sendUnityEvent:(NSString*)eventName withArgs:(NSString*)args;

- (void)sendUnityEvent:(NSString*)eventName;

- (void)startChoiceWithPCode:(NSString*)pCode shouldDisplayIDFA:(BOOL)shouldDisplayIDFA;

- (void)showCCPAWithPCode:(NSString*)pCode;

- (NSString*)getTCString;

- (void)forceDisplayUI;

- (void)startChoiceFromWeb;

@end
