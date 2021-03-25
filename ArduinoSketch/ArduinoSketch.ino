
#include "MyEmergency.h"
#include "MyReset.h"
#include "MyLcd.h"
#include "MyFingers.h"
#include "MyServos.h"
#include "MyStepper.h"
#include "MyComms.h"
#include "MyBuzzer.h"
#include "MyPressureGauge.h"

void setup()
{
  // Inicializar botão de emergência
  SetupEStopButton();
  SetupResetButton();

  while (CheckResetButton()) {delay(100);} // Aguardar liberação do botão reset
  
  // Iniciar LCD
  SetupLCD();
  PresetLCD(1);

  // Inicializar buzzer
  SetupBuzzer();
  BuzzerPlayRandom();
  PresetLCD(0);

  // Inicializar dedos
  SetupFingers();

  // Iniciar Servos
  SetupServos();

  // Inicializar motores de passo
  SetupSteppers();

  // Iniciar pressostato
  SetupPressureGauge();

  // Estabelecer comunicação
  SetupConnection();
}

void loop()
{
  // interromper loop principal caso botão acionado
  if (CheckEmergency())
    return;

  if (CheckResetButton())
  {
    Serial.write('R');
    PresetLCD(3);
    BuzzerNotify(3);
    setup();
    return;
  }

  // comunicar-se via serial e atrata comandos recebidos
  if (SerialTalk())
  {
    byte command = CommReadByte();
    if (command == 'T')
    {
        char text[33];
        for (int i = 0; i < min(CommFrameLength(),33); i++)
          text[i] = char(CommReadByte());
          
        WriteLCD(text);
    }
    else if (command == 'F')
    {
        bool fing[10];
        for (int i = 0; i<10;i++)
          fing[i] = CommReadBool();
        SetAllFingers(fing);
    }
    else if (command == 'M')
    {
        long frameDurationMs = (long)CommReadInt();
        long frameDurationUs = frameDurationMs*1000;
        int posLeft = CommReadInt();
        int posRight = CommReadInt();
        int posArray[2] = {posLeft, posRight};
        
        /*Serial.print("\r\n");
        Serial.print("T=");
        Serial.print(frameDurationMs);
        Serial.print(" A=");
        Serial.print(posLeft);
        Serial.print(" B=");
        Serial.print(posRight);*/
        //Serial.print("\r\n");
        
        SetMotion(posArray, frameDurationUs);
    }
    else if (command == 'R')
    {
        long frameDurationMs = CommReadInt();
      
        int thetas[10];
        
        //Serial.print("\r\n");
        for (int i = 0; i < 10; i++)
        {
          thetas[i] = CommReadInt();
          //Serial.print(thetas[i]);
          //Serial.print(" - ");
        }
        //Serial.print("\r\n");
          
        SetThetas(thetas, frameDurationMs);
    }
    else if (command == 'E')
    {
        FingersLiftAll();
        SteppersSetEmergencyStop();
        StopServos();
        PresetLCD(0);
    }
  }

  RunServos();

  //TestServosAll();
  //TestServos();
  
  //TestSteppers();
  //TestFingers();
}
