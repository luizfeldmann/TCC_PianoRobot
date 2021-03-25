#include "MyStepper.h"
#include "TimerOne.h"

struct StructSTEPPER
{
  OutputPin *PUL;
  OutputPin *DIR;
  bool bInverted;

  volatile int TargetPosition;
  volatile int CurrentPosition;
  volatile int StartPosition;
  int Increment;

  volatile int Counter;
};

StructSTEPPER STEPPERS[2];
Output<STEPPER_ENABLE> EN = LOW;

float MotionDuration;

void SetupSteppers()
{
  EN = true; // Disable Outputs

  STEPPERS[0].PUL = new OutputPin(STEPPER1_PUL);
  STEPPERS[0].DIR = new OutputPin(STEPPER1_DIR);
  STEPPERS[0].bInverted = false;
  STEPPERS[0].TargetPosition = 0;
  STEPPERS[0].CurrentPosition = 0;
  STEPPERS[0].StartPosition = 0;
  STEPPERS[0].Counter = 0;

  STEPPERS[1].PUL = new OutputPin(STEPPER2_PUL);
  STEPPERS[1].DIR = new OutputPin(STEPPER2_DIR);
  STEPPERS[1].bInverted = true;
  STEPPERS[1].TargetPosition = 0;
  STEPPERS[1].CurrentPosition = 0;
  STEPPERS[1].StartPosition = 0;
  STEPPERS[1].Counter = 0;

  MotionDuration = 0;
  

  Timer1.initialize(Timer1DelayUs);
  Timer1.attachInterrupt(RunSteppers);
}

void SetMotion(int Target[], long DurationMicro)
{
  Timer1.stop();
  
  MotionDuration = DurationMicro;

  for (int i = 0; i < numSteppers; i++)
  {
    STEPPERS[i].TargetPosition = Target[i];
    STEPPERS[i].StartPosition = STEPPERS[i].CurrentPosition;
    STEPPERS[i].Counter = 0;
    
    if (STEPPERS[i].TargetPosition > STEPPERS[i].CurrentPosition)
    {
      STEPPERS[i].Increment = 1;
      STEPPERS[i].DIR -> write(STEPPERS[i].bInverted);
    }
    else
    {
      STEPPERS[i].Increment = -1;
      STEPPERS[i].DIR -> write(!STEPPERS[i].bInverted);
    }
  }

  EN = false; // enable low = activate drivers

  Timer1.restart();
}


int GetCounter(float x, float delta)
{
  float omega = PI/MotionDuration;
  float X = constrain(x, 10, delta-10);
  float speed = omega*sqrt(delta*X - X*X);
  float del = 1/speed;
  
  return round(del/Timer1DelayUs);
  
}

void RunSteppers()
{
  int ready = 0;
  for (int i = 0; i < numSteppers; i++)
  {
    if (STEPPERS[i].CurrentPosition == STEPPERS[i].TargetPosition)
      {
          ready++;
          continue;
      }

      if (STEPPERS[i].Counter != 0)
      {
        STEPPERS[i].Counter--;
        continue;
      }

      float delta = abs(STEPPERS[i].StartPosition - STEPPERS[i].TargetPosition);
      float x = abs(STEPPERS[i].StartPosition - STEPPERS[i].CurrentPosition);

      STEPPERS[i].Counter = GetCounter(x, delta);

      #ifdef CONSTANTSPEED
        STEPPERS[i].Counter = MotionDuration/(delta*Timer1DelayUs);
      #endif
      
      STEPPERS[i].PUL -> pulse();
      STEPPERS[i].CurrentPosition += STEPPERS[i].Increment;
  }

  if (ready >= numSteppers && !EN)
  {
    EN = true;
    Timer1.stop();
  }
}

bool bSteppersArrived()
{
  return EN;
}

void SteppersSetEmergencyStop()
{
  EN = true; // disable
  Timer1.stop();
}

long test_last_time = 0;
bool test_flag = false;
long test_duration = 2000000;
int test_target_1[] = {-400, 800};
int test_target_2[] = {0, 0};

void TestSteppers()
{
  if (!bSteppersArrived())
    return;

  if (!test_flag)
    SetMotion(test_target_1, test_duration);
  else
    SetMotion(test_target_2, test_duration);
    
  test_flag = !test_flag;

  long now = millis();
  Serial.println(now - test_last_time);
  test_last_time = now;
}

