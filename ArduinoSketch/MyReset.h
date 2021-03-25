#ifndef _MY_RESET_H_
#define _MY_RESET_H_

#include <Arduino.h>
#include <DirectIO.h>

#define RESET_FAKEGROUND 35
#define RESET_FAKESOURCE 39
#define RESET_SIGNAL 37

bool CheckResetButton();
void SetupResetButton();

#endif
