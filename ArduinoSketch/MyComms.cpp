#include "MyComms.h"

const byte SLIP_END      =       0xC0;
const byte SLIP_ESC      =       0xDB;
const byte SLIP_ESC_END  =       0xDC;
const byte SLIP_ESC_ESC  =       0xDE;

byte serialBuffer[128];
int bufferPosition = 0;

int frame_size;
int frame_read_position;

void SetupConnection()
{
  Serial.begin(57600, SERIAL_8N2);
}

bool SerialTalk()
{
  if (Serial.available() <= 0)
    return false;

  byte incoming = Serial.read();

  switch (incoming)
  {
    case SLIP_END:
      frame_read_position = 0;
      frame_size = bufferPosition;
      bufferPosition = 0;
      
      //Serial.println("PACKET:");
      
      return (frame_size > 0);
    break;
    
    case SLIP_ESC:
      incoming = Serial.read();
      if (incoming == SLIP_ESC_END)
        incoming = SLIP_END;
      else if (incoming == SLIP_ESC_ESC)
        incoming = SLIP_ESC;
      else
      { Serial.println("Protocol violation");}
      
    //break; FALL THROUGH
    default:
      serialBuffer[bufferPosition] = incoming;
      bufferPosition++;
      //Serial.print(incoming, HEX);
      //Serial.print("-");
    break;
  }

  return false;
}

bool CommReadBool()
{
  return (serialBuffer[frame_read_position++] == 0xFF);
}


int CommReadInt()
{
  int i = 256 * serialBuffer[frame_read_position+1] + serialBuffer[frame_read_position];
  frame_read_position += 2;

  return i;
}

int CommFrameLength()
{
  return frame_size;
}


byte CommReadByte()
{
  frame_read_position++;
  return serialBuffer[frame_read_position-1];
}

