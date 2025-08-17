
//
//  ChoiceCMPBinding.m
//  Meson
//
//  Copyright (c) 2023 InMobi. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#import "ChoiceCMPManager.h"
#import "ChoiceCMPUtils.h"


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - APIs
void _StartChoice(const char* pCode, bool shouldDisplayIDFA) {
    NSString* code = GetStringParam(pCode);
    [[ChoiceCMPManager shared] startChoiceWithPCode: code shouldDisplayIDFA:shouldDisplayIDFA];
}

void _ShowCCPA(const char* pCode) {
    NSString* code = GetStringParam(pCode);
    [[ChoiceCMPManager shared] showCCPAWithPCode: code];
}

void _ForceDisplayUI() {
    [[ChoiceCMPManager shared] forceDisplayUI];
}

void _StartChoiceFromWeb() {
    [[ChoiceCMPManager shared] startChoiceFromWeb];
}

char* _GetTCString() {
    return [ChoiceCMPUtils cStringCopy:[[ChoiceCMPManager shared] getTCString]];
}
