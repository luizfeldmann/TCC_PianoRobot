#ifndef _MYSTEPPER_H_
#define _MYSTEPPER_H_

#include "MyEmergency.h"
#include <DirectIO.h>


#define STEPPER1_PUL 26
#define STEPPER1_DIR 24
#define STEPPER2_PUL 30
#define STEPPER2_DIR 28
#define STEPPER_ENABLE 22

#define numSteppers 2
#define Timer1DelayUs 100

void SetupSteppers();
void RunSteppers();
bool bSteppersArrived();
void SteppersSetEmergencyStop();
void SetMotion(int Target[2], long DurationMicro);
void TestSteppers();

#endif
