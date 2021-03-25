#include "MyEmergency.h"

Input<ESTOP_SIGNAL> bStopButton;
volatile bool bLastState = false;

void SetupEStopButton()
{
  pinMode(ESTOP_FAKEGROUND, OUTPUT);
  digitalWrite(ESTOP_FAKEGROUND, LOW);

  pinMode(ESTOP_FAKESOURCE, OUTPUT);
  digitalWrite(ESTOP_FAKESOURCE, HIGH);
}

void ButtonPressed()
{
  Serial.write('E');
  SteppersSetEmergencyStop();
  FingersLiftAll();
  StopServos();
  
  PresetLCD(2);
  BuzzerNotify(0);
}

void ButtonReleased()
{
  PresetLCD(0);
  BuzzerNotify(1);
}

bool bEmergency()
{
  return bLastState;
}

bool CheckEmergency()
{
  bool currentButton = bStopButton;
  if (bLastState != currentButton)
  {
    if (currentButton)
      ButtonPressed();
    else
      ButtonReleased();

    bLastState = currentButton;
    delay(ESTOP_TOGGLE_DELAY);
  }

  return currentButton;
}

