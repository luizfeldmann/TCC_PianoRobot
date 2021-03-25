#include "MyServos.h"

const int channelList[] = {6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
//float OffsetDeflection[] = {0, 0, 0, 0, 0, 15, 5, 9, 6, 4};
float OffsetDeflection[] = {1, -9, -1, -2, 6,     -8, 0, 7, 2, -2};

int restPosition[] = {900, 900, 900, 900, 900, 900, 900, 900, 900, 900};
#define CONSTRAIN_ANGLE_MIN 65
#define CONSTRAIN_ANGLE_MAX 115



Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();
const int NumServos = 10;
MyServo SERVOS[NumServos];

long MotionDurationMs;
long TimeStart = 0;
long LastIteration = 0;
volatile bool bHasArrived = true;

void SetupServos()
{
  pwm.begin();
  pwm.setPWMFreq(50);

  for (int i = 0; i < NumServos;i++)
  {
    SERVOS[i].CurrentAngle = 90;
    SERVOS[i].Channel = channelList[i];
  }

  SetThetas(restPosition, 500);
}

void SetThetas(int Target[], long Duration)
{
  MotionDurationMs = Duration;

  bool bChange;

  /*do 
  {
    bChange = false;
    for (int i = 0; i<10; i++)
    {
      float ang_min = CONSTRAIN_ANGLE_MIN * 10;
      float ang_max = CONSTRAIN_ANGLE_MAX * 10;

      if (i != 0 && i != 5)
        ang_max = Target[i-1];
      if (i != 4 && i != 9)
        ang_max = Target[i+1];

      float myang = Target[i] * 10;

      if (myang>ang_max)
      {
        myang = 0.5*(myang + ang_max);
        bChange = true;
      }
      else if (myang < ang_min)
      {
        myang = 0.5*(myang+ang_min);
        bChange = true;
      }

      if (bChange)
        Target[i] = (int)(myang/10);
    }
  }
  while (bChange);*/
  
  for (int i = 0; i < NumServos;i++)
  {
    SERVOS[i].StartAngle = SERVOS[i].CurrentAngle;
    SERVOS[i].TargetAngle = constrain((float)(Target[i]) / 10.0, CONSTRAIN_ANGLE_MIN, CONSTRAIN_ANGLE_MAX);
    if (i != 0 && i != 9)
      SERVOS[i].TargetAngle = constrain(SERVOS[i].TargetAngle, SERVOS[i+1].TargetAngle, SERVOS[i-1].TargetAngle);
  }

  TimeStart = millis();
  bHasArrived = false;
}

void StopServos()
{
  bHasArrived = true;
}

void RunServos()
{
  if (bHasArrived)
    return;
  
  long Now = millis();
  if (Now > TimeStart + MotionDurationMs)
  {
    bHasArrived = true;
    return;
  }

  if (Now < LastIteration + ServoPeriodMs)
    return;

  for (int i = 0; i < NumServos; i++)
  {
    float theta = 0.5*(SERVOS[i].StartAngle + SERVOS[i].TargetAngle) + 0.5*(SERVOS[i].TargetAngle-SERVOS[i].StartAngle)*sin((float)(Now - TimeStart)*PI/(float)MotionDurationMs - PI/2);
    SERVOS[i].CurrentAngle = constrain(theta, CONSTRAIN_ANGLE_MIN, CONSTRAIN_ANGLE_MAX);
    pwm.setPWM(SERVOS[i].Channel, 0, map(SERVOS[i].CurrentAngle + OffsetDeflection[i], SERVO_MIN_DEG, SERVO_MAX_DEG, SERVO_MIN_PWM, SERVO_MAX_PWM));
  }
  
  LastIteration = Now;
}

int testIndex = 0;
bool testB = false;
void TestServosSequence()
{
  if (!bHasArrived)
    return;

  if (testIndex >= NumServos)
      testIndex = 0;

  int thetas[] = {900, 900, 900, 900, 900, 900, 900, 900, 900, 900};
  if (testB)
  {
    thetas[testIndex ] = 1200;
    testIndex ++;
  }
  testB = !testB;

  SetThetas(thetas, 500);
}

void TestServosAll()
{
   if (!bHasArrived)
    return;
    
  int thetas[] = {1100, 1000, 950, 800, 700, 1100, 1000, 950, 800, 700};
  
  if (testB)
    SetThetas(thetas, 500);
  else
    SetThetas(restPosition,500);
    
  testB = !testB;
}

