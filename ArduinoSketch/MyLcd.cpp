#include "MyLcd.h"

LiquidCrystal_I2C lcd(0x3F,2,1,0,4,5,6,7,3, POSITIVE);

void SetupLCD()
{
  // I2C p/ LCD
  lcd.begin(16,2);
}

void WriteLCD(const char text[])
{
  bool bswitchline = false;
  lcd.clear();
  lcd.setCursor(0,0);

  int i;
  for (i = 0; i<33;i++)
  {
    char c = text[i];
    if ((i==16 || c == '\n') && !bswitchline)
    {
      lcd.setCursor(0,1);
      bswitchline = true;
    }
    else if (c == '\0')
      break;

    if (c != '\n' && c!='\0')
      lcd.print(c);
  }
}

void PresetLCD(int i)
{
  switch (i)
  {
    case 0: WriteLCD(TEXT_NAME); break;
    case 1: WriteLCD(TEXT_LUIZ); break;
    case 2: WriteLCD(TEXT_EMER); break;
    case 3: WriteLCD(TEXT_RSET); break;

    default:WriteLCD(TEXT_ERRR); break;
  }
}

