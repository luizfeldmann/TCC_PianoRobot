#ifndef _MY_EMERGENCY_H_
#define _MY_EMERGENCY_H_

#include <Arduino.h>
#include <DirectIO.h>
#include "MyLcd.h"
#include "MyBuzzer.h"
#include "MyStepper.h"
#include "MyFingers.h"
#include "MyServos.h"

#define ESTOP_FAKEGROUND 27
#define ESTOP_FAKESOURCE 29
#define ESTOP_SIGNAL 31
#define ESTOP_TOGGLE_DELAY 1000

bool CheckEmergency();
bool bEmergency();
void SetupEStopButton();

#endif
