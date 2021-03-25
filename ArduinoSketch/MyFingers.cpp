#include "MyFingers.h"

bool allUp[] = {false, false, false, false, false, false, false, false, false, false};

Output<5> Finger1(LOW);
Output<6> Finger2(LOW);
Output<11> Finger3(LOW);
Output<8> Finger4(LOW);
Output<3> Finger5(LOW);
Output<12> Finger6(LOW);
Output<4> Finger7(LOW);
Output<10> Finger8(LOW);
Output<9> Finger9(LOW);
Output<7> Finger10(LOW);

void SetupFingers()
{
  // placeholder
  SetAllFingers(allUp);
}

void SetFinger(int index, bool bPressed)
{
  switch(index)
  {
    case 0: Finger1 = bPressed; break;
    case 1: Finger2 = bPressed; break;
    case 2: Finger3 = bPressed; break;
    case 3: Finger4 = bPressed; break;
    case 4: Finger5 = bPressed; break;
    case 5: Finger6 = bPressed; break;
    case 6: Finger7 = bPressed; break;
    case 7: Finger8 = bPressed; break;
    case 8: Finger9 = bPressed; break;
    case 9: Finger10 = bPressed; break;
  }
}

void SetAllFingers(bool bPressed[])
{
  Finger1 = bPressed[0];
  Finger2 = bPressed[1];
  Finger3 = bPressed[2];
  Finger4 = bPressed[3];
  Finger5 = bPressed[4];
  Finger6 = bPressed[5];
  Finger7 = bPressed[6];
  Finger8 = bPressed[7];
  Finger9 = bPressed[8];
  Finger10 = bPressed[9];
}

void TestFingers()
{
  for (int i = 0; i<10; i++)
  {
    SetFinger(i, true);
    delay(500);
    SetFinger(i, false);
    delay(500);
  }
}

void FingersLiftAll()
{
  SetAllFingers(allUp);
}

