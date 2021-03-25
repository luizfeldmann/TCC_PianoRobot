#include "MyBuzzer.h"

void SetupBuzzer()
{
  pinMode(BUZZER_PIN, OUTPUT);
  randomSeed(analogRead(5)+analogRead(6)+analogRead(7));
}

float BuzzerGetNoteFrequency(int octave, int pitch)
{
  if (pitch == NOTE_PAUSE)
    return 0;
  float thisa = Base_A4 * pow(2, octave-4);
  return thisa * pow(2, (float)pitch/12);
}

void BuzzerPlayNotes(const BuzzerNote notes[], int len)
{
  for (int i = 0; i< len; i++)
  {
    float durationms = BUZZER_PLAY_TEMPO*0.25*notes[i].beat;
    float freq = BuzzerGetNoteFrequency(notes[i].octave, notes[i].pitch);
    tone(BUZZER_PIN, freq, durationms);
    delay(durationms+5);
  }
}

void BuzzerNotify(int index)
{
  switch (index)
  {
    case 0: // emergency activated
      tone(BUZZER_PIN, 100, BUZZER_NOTIFYSOUND_DURATION);
    break;

    case 1: // emergency lifted
      tone(BUZZER_PIN, 5000, BUZZER_NOTIFYSOUND_DURATION);
    break;
    
    case 3: // reset
      for (long i = 220; i < 20000; i *= 2)
      {
        tone(BUZZER_PIN, i, BUZZER_NOTIFYSOUND_DURATION/8);
        delay(BUZZER_NOTIFYSOUND_DURATION/8 + 5);
      }
    break;
  }
}

void BuzzerPlayRandom()
{
  
  switch (random(0, 4))
  {
    case 0:
      BuzzerPlayNotes(dreamwedding, sizeof(dreamwedding)/sizeof(dreamwedding[0]));
    break;

    case 1:
      BuzzerPlayNotes(furelise, sizeof(furelise)/sizeof(furelise[0]));
    break;

    case 2: 
      BuzzerPlayNotes(rondo1, sizeof(rondo1)/sizeof(rondo1[0]));
    break;

    case 3: 
      BuzzerPlayNotes(rondo2, sizeof(rondo2)/sizeof(rondo2[0]));
    break;

    default:
    break;
  }
}
