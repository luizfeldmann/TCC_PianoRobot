#ifndef _MYLCD_H_
#define _MYLCD_H_

#include <Arduino.h>
#include <LiquidCrystal_I2C.h>

#define TEXT_LUIZ "LUIZ G PFITSCHER\n   e FELDMANN   "
#define TEXT_NAME "PIANOPLEX - LGPF\n     STANDBY    "
#define TEXT_EMER "  !! E-STOP !!  \nCANC. EMERCENCIA"
#define TEXT_RSET "|     RESET    ||     =====    |"
#define TEXT_ERRR "UNKNOWN MESSAGE \n#!THIS IS A BUG!"


void SetupLCD();
void WriteLCD(const char text[]);
void PresetLCD(int i);

#endif
