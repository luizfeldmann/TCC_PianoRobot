#include "MyReset.h"

Input<RESET_SIGNAL> bResetButton;

void SetupResetButton()
{
  pinMode(RESET_FAKEGROUND, OUTPUT);
  digitalWrite(RESET_FAKEGROUND, LOW);

  pinMode(RESET_FAKESOURCE, OUTPUT);
  digitalWrite(RESET_FAKESOURCE, HIGH);
}

bool CheckResetButton()
{
  return bResetButton;
}

