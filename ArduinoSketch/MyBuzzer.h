#ifndef _BUZZER_H_

#include <Arduino.h>

#define _BUZZER_H_

#define NOTE_C -9
#define NOTE_Cs -8
#define NOTE_D -7
#define NOTE_Ds -6
#define NOTE_E -5
#define NOTE_F -4
#define NOTE_Fs -3
#define NOTE_G -2
#define NOTE_Gs -1
#define NOTE_A 0
#define NOTE_As 1
#define NOTE_B 2
#define NOTE_PAUSE -10

#define NOTE_WHOLE 16
#define NOTE_HALF 8
#define NOTE_QUARTER 4
#define NOTE_EIGHTH 2
#define NOTE_SIXTEENTH 1

#define Base_A4 440
#define BUZZER_NOTIFYSOUND_DURATION 1000
#define BUZZER_PLAY_TEMPO 200
#define BUZZER_PIN 13

typedef struct  {int pitch; int octave; int beat; } BuzzerNote;
const BuzzerNote dreamwedding[] = {
  {NOTE_G, 4, NOTE_EIGHTH},
  {NOTE_G, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_As, 4, NOTE_EIGHTH},
  {NOTE_As, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_G, 4, NOTE_EIGHTH},
  {NOTE_G, 4, NOTE_EIGHTH},
  {NOTE_D, 4, NOTE_EIGHTH},
  {NOTE_D, 4, NOTE_EIGHTH},
  {NOTE_As, 3, NOTE_EIGHTH},
  {NOTE_As, 3, NOTE_EIGHTH},
  {NOTE_G, 3, NOTE_EIGHTH},
  {NOTE_G, 3, NOTE_EIGHTH},
  {NOTE_F, 4, NOTE_EIGHTH},
  {NOTE_F, 4, NOTE_EIGHTH},
  {NOTE_Ds, 4, NOTE_EIGHTH},
  {NOTE_Ds, 4, NOTE_EIGHTH},
  {NOTE_D, 4, NOTE_EIGHTH},
  {NOTE_Ds, 4, NOTE_EIGHTH},
  {NOTE_F, 4, NOTE_EIGHTH},
  {NOTE_Ds, 4, NOTE_HALF}
  };

const BuzzerNote furelise[] = {
  {NOTE_E, 5, NOTE_EIGHTH},
  {NOTE_Ds, 5, NOTE_EIGHTH},
  {NOTE_E, 5, NOTE_EIGHTH},
  {NOTE_Ds, 5, NOTE_EIGHTH},
  {NOTE_E, 5, NOTE_EIGHTH},
  {NOTE_B, 4, NOTE_EIGHTH},
  {NOTE_D, 5, NOTE_EIGHTH},
  {NOTE_C, 5, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_A, 3, NOTE_EIGHTH},
  {NOTE_C, 3, NOTE_EIGHTH},
  {NOTE_E, 3, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_B, 4, NOTE_QUARTER},
  {NOTE_PAUSE, 0, NOTE_EIGHTH},
  {NOTE_E, 4, NOTE_EIGHTH},
  {NOTE_C, 5, NOTE_EIGHTH},
  {NOTE_B, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_QUARTER}
};

const BuzzerNote rondo1[] = {
  {NOTE_B, 3, NOTE_EIGHTH},
  {NOTE_A, 3, NOTE_EIGHTH},
  {NOTE_As, 3, NOTE_EIGHTH},
  {NOTE_A, 3, NOTE_EIGHTH},
  {NOTE_C, 4, NOTE_HALF},

  {NOTE_D, 4, NOTE_EIGHTH},
  {NOTE_C, 4, NOTE_EIGHTH},
  {NOTE_B, 3, NOTE_EIGHTH},
  {NOTE_C, 4, NOTE_EIGHTH},
  {NOTE_E, 4, NOTE_HALF},

  {NOTE_F, 4, NOTE_EIGHTH},
  {NOTE_E, 4, NOTE_EIGHTH},
  {NOTE_Ds, 4, NOTE_EIGHTH},
  {NOTE_E, 4, NOTE_EIGHTH},

  {NOTE_B, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_As, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_B, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_As, 4, NOTE_EIGHTH},
  {NOTE_A, 4, NOTE_EIGHTH},
  {NOTE_C, 5, NOTE_HALF}
  
};

const BuzzerNote rondo2[] = {
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_C, 5, NOTE_QUARTER},
  {NOTE_B, 4, NOTE_QUARTER},
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_G, 4, NOTE_QUARTER},
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_B, 4, NOTE_QUARTER},
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_G, 4, NOTE_QUARTER},
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_B, 4, NOTE_QUARTER},
  {NOTE_A, 4, NOTE_QUARTER},
  {NOTE_G, 4, NOTE_QUARTER},
  {NOTE_Fs, 4, NOTE_QUARTER},
  {NOTE_E, 4, NOTE_HALF},
  };

float BuzzerGetNoteFrequency(int octave, int pitch);
void BuzzerPlayNotes(const BuzzerNote notes[], int len);
void BuzzerPlayRandom();
void SetupBuzzer();
void BuzzerNotify(int index);

#endif
