
#ifndef _MY_GAUGE_
#define _MY_GAUGE_

#include <Arduino.h>

#define PressureNumSamples 200
#define GaugePin 15

void SetupPressureGauge();
int ReadPressure();

#endif
