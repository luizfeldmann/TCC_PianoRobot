#ifndef _MYSERVO_H_
#define _MYSERVO_H_

#include <Arduino.h>
#include <Adafruit_PWMServoDriver.h>

#define SERVO_MIN_PWM 111
#define SERVO_MAX_PWM 491
#define SERVO_MIN_DEG 0
#define SERVO_MAX_DEG 180
#define ServoPeriodMs 50

struct MyServo
{
  volatile float CurrentAngle;
  volatile float TargetAngle;
  volatile float StartAngle;
  int Channel;
};

void SetupServos();
void SetThetas(int Target[], long Duration);
void StopServos();
void RunServos();
void TestServosSequence();
void TestServosAll();

#endif

