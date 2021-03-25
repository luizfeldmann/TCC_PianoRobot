#ifndef _FINGER_H_
#define _FINGER_H_

#include <Arduino.h>
#include <DirectIO.h>

void SetupFingers();
void SetFinger(int index, bool bPressed);
void SetAllFingers(bool bPressed[]);
void FingersLiftAll();
void TestFingers();

#endif
