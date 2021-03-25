#ifndef _MY_COMMS_H_
#define _MY_COMMS_H_

#include <Arduino.h>

void SetupConnection();
bool SerialTalk();
int CommReadInt();
bool CommReadBool();
byte CommReadByte();
int CommFrameLength();

#endif



