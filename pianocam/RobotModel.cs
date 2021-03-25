using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;
using System.Diagnostics;

namespace pianocam
{
	public class RobotPlayer
	{
		public ArduinoSession controller;
		private InstrumentTrack track;
		private Timeline timeline;
		private PianoKey[] KeysReference;
		private int offsetWidget;

		public RobotPlayer(string name, InstrumentTrack it, Timeline tim, PianoKey[] reference)
		{
			controller = new ArduinoSession();
			controller.WriteLCD(name);

			track = it;
			timeline = tim;

			KeysReference = reference;
			offsetWidget = KeysReference.First(x => x.MidiNumber == RobotStateHelper.CoordinateSystemOffset).WidgetOrder;
		}

		public void End()
		{
			controller.End();
			controller.Dispose();
		}

        Keyframe NextFrame;
		bool[] savedFingerState = new bool[10];
        public int[] ActiveMidicodes { get; set; }
		public void ProcessInstant(int nowmilis)
		{
			var state = timeline.InterpolateState(nowmilis);

			bool[] newFingerState = new bool[10];
            List<int> OnNotes = new List<int>();
			for (int h = 0; h < 2; h++)
			{
				RobotHand hand = h == 0 ? state.LeftHand : state.RightHand;
				for (int i = 0; i < 5; i++)
				{
					RobotFinger fin = hand.Fingers[i];                 

					if (fin.bMoving)
						continue;

					int coord = (int)Math.Round(hand.Position + i + fin.Deflection);


					int midi = KeysReference.First(x => x.WidgetOrder == offsetWidget + coord).MidiNumber;

                    if (track.Notes.Any(x => x.NoteNumber == midi && x.StartTime < nowmilis && x.EndTime > nowmilis))
                    {
                        newFingerState[5 * h + i] = true;
                        OnNotes.Add(midi);
                    }
                     
				}
			}

            if(!Enumerable.SequenceEqual(newFingerState, savedFingerState))
            {
                savedFingerState = newFingerState;
                controller.SetAllFingers(newFingerState);
            }
            ActiveMidicodes = OnNotes.ToArray();

			Keyframe next = timeline.GetNext(nowmilis);

			if (next == null)
				return;
			
			if (next == NextFrame)
				return;         

			Keyframe prev = timeline.GetPrevious(next.Time-1);
			if (prev == null)
				return;

			if (prev == next)
				return;

			NextFrame = next;

			int DurationMs = NextFrame.Time - prev.Time;

			if (DurationMs <= 10)
				return;

			int[] TargetStep = new int[2];
			float[] TargetTheta = new float[10];

            //Debug.WriteLine("TARGET: {0} | {1} ", NextFrame.State.LeftHand.Position, NextFrame.State.RightHand.Position);
			TargetStep[0] = RobotStateHelper.CoordinateToStep(NextFrame.State.LeftHand.Position - (RobotStateHelper.InitialState.LeftHand.Position));
			TargetStep[1] = RobotStateHelper.CoordinateToStep(NextFrame.State.RightHand.Position - (RobotStateHelper.InitialState.RightHand.Position));

			NextFrame.State.LeftHand.Fingers.Select(x => RobotStateHelper.DeflectionToThetaDeg(x.Deflection)).ToArray().CopyTo(TargetTheta, 0);
			NextFrame.State.RightHand.Fingers.Select(x => RobotStateHelper.DeflectionToThetaDeg(x.Deflection)).ToArray().CopyTo(TargetTheta, 5);
            
			controller.InterpolateToState(DurationMs, TargetStep, TargetTheta);
		}
	}

    [Serializable]
    public class Timeline
    {
        private List<Keyframe> Keyframes = new List<Keyframe>();
        public Timeline()
        {
            Keyframes.Add(new Keyframe() { Time = 0, State = RobotStateHelper.InitialState });
        }

        public Keyframe NewKeyframe(int time)
        {
            var kf = new Keyframe() { Time = time};
            Keyframes.Add(kf);
            Keyframes.Sort((k1, k2) => k1.Time.CompareTo(k2.Time));
            return kf;
        }

        public void DeleteKeyframe(Keyframe kf)
        {
            Keyframes.Remove(kf);
        }

        public Keyframe GetPrevious(int time)
        {
            if (!Keyframes.Any(x => x.Time < time))
                return null;
            
            return Keyframes.OrderByDescending(x => x.Time).First(x => x.Time <= time);
        }

        public Keyframe GetNext(int time)
        {
            if (!Keyframes.Any(x => x.Time > time))
                return null;
            
            return Keyframes.OrderByDescending(x => x.Time).Last(x => x.Time > time);
        }

        public Keyframe GetAt(int time)
        {
            return Keyframes.FirstOrDefault(x => x.Time == time);
        }

        private RobotHand InterpolateHand(float time, float prevtime, float nexttime, RobotHand prev, RobotHand next)
        {
            RobotHand hand = new RobotHand();
            double velocity;
            hand.Position = (float)Interpolator.Sigmoid(time, new InterpolationPoint() { x = prevtime, y = prev.Position }, new InterpolationPoint { x = nexttime, y = next.Position }, out velocity);
            hand.Velocity = (float)velocity;

            hand.Fingers = new RobotFinger[prev.Fingers.Length];
            for (int i = 0; i < hand.Fingers.Length; i++)
            {
                double fingerVelocity;
                hand.Fingers[i] = new RobotFinger()
                {
                    Deflection = (float)Interpolator.Sigmoid(time, new InterpolationPoint {x = prevtime, y = prev.Fingers[i].Deflection}, new InterpolationPoint {x = nexttime, y = next.Fingers[i].Deflection}, out fingerVelocity)
                };


                hand.Fingers[i].bMoving = Math.Pow((hand.Fingers[i].Deflection + hand.Position) - (next.Fingers[i].Deflection + next.Position), 2) + Math.Pow((next.Fingers[i].Deflection + next.Position) - (prev.Fingers[i].Deflection + prev.Position),2) > 0.01f;
            }

            return hand;
        }

        public RobotState InterpolateState(int time)
        {
            Keyframe prev = GetPrevious(time);
            Keyframe next = GetNext(time);

            if (prev == null)
                prev = GetAt(time);

            if (next == null || next == prev)
                return prev.State;

            RobotState state = new RobotState();
            state.LeftHand = InterpolateHand(time, prev.Time, next.Time, prev.State.LeftHand, next.State.LeftHand);
            state.RightHand = InterpolateHand(time, prev.Time, next.Time, prev.State.RightHand, next.State.RightHand);

            return state;
        }

        public Keyframe[] GetKeyframes()
        {
            return Keyframes.ToArray();
        }
    }

    [Serializable]
    public class Keyframe
    {
        public RobotState State;
        public int Time;
    }

    public static class RobotStateHelper
    {
        public static readonly int HandSpacingMinimum = Configs.ReadInt("MinimumHandSpacing");
        public static readonly int MaxDeflection = Configs.ReadInt("MaxFingerDeflectionKeys");
        public static readonly int MinPosition = Configs.ReadInt("LinearPositionMin");
        public static readonly int MaxPosition = Configs.ReadInt("LinearPositionMax");
        public static readonly int CoordinateSystemOffset = Configs.ReadInt("CoordinateSystemOriginMidi");
        public static readonly int FirstKeyMidi = Configs.ReadInt("PianoFirstKey");
        public static readonly int LastKeyMidi = Configs.ReadInt("PianoLastKey");
        public static readonly int ProgramResolutionMs = Configs.ReadInt("ProgramResolutionMs");
        public static readonly float FingerRadiusMM = Configs.ReadFloat("FingerRadiusMM");
        public static readonly float KeyWidthMM = Configs.ReadFloat("KeyWidthMM");
        public static readonly int PulleyZ = Configs.ReadInt("PulleyZ");
		public static readonly int BeltPitch = Configs.ReadInt("BeltPitchMM");
		public static readonly int StepsPerRevolution = Configs.ReadInt("StepsPerRevolution");

        public static RobotState InitialState = new RobotState()
        {
            LeftHand = new RobotHand()
            {
                Position = -10,
                Velocity = 0,
                Fingers = new RobotFinger[] {
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0}
                        }
            },
            RightHand = new RobotHand()
            {
                Position = 10,
                Velocity = 0,
                Fingers = new RobotFinger[] {
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0},
                            new RobotFinger() { Deflection = 0}
                        }
            }
        };

        public static float DeflectionToThetaDeg(float x)
		{
			return (float)(90.0 - Math.Asin(x*KeyWidthMM/FingerRadiusMM)*180.0/Math.PI);
		}

        public static int CoordinateToStep(float x)
		{
			return (int)Math.Round((float)StepsPerRevolution * KeyWidthMM * x / (PulleyZ * BeltPitch));
		}

        public static RobotStateError CheckState(RobotState state)
        {
            if (state.LeftHand.Position >= state.RightHand.Position || Math.Abs(state.RightHand.Position - state.LeftHand.Position) < HandSpacingMinimum)
                return RobotStateError.RSE_HandsCrossover;
            if (state.LeftHand.Position < MinPosition || state.RightHand.Position > MaxPosition)
                return RobotStateError.RSE_HandsOutOfRange;

            for (int j = 0; j < 2; j++)
            {
                RobotHand hand = (j == 0 ? state.LeftHand : state.RightHand);
                for (int i = 1; i < hand.Fingers.Length-1; i++)
                {
                    if (hand.Fingers[i].Deflection > hand.Fingers[i + 1].Deflection /*- FingerSpacingMinimum*/ || hand.Fingers[i].Deflection < hand.Fingers[i - 1].Deflection /*+ FingerSpacingMinimum*/)
                        return RobotStateError.RSE_FingersCrossover;

                    if (Math.Abs(hand.Fingers[i].Deflection) > MaxDeflection)
                        return RobotStateError.RSE_FingerOutOfRange;
                }
            }

            return RobotStateError.RSE_None;
        }

        public static RobotFinger GetFinger(int index, RobotState state)
        {
            return (index < 5 ? state.LeftHand : state.RightHand).Fingers[index % 5];
        }

        public static float GetFingerPosition(int index, RobotState state)
        {
            return (index < 5 ? state.LeftHand : state.RightHand).Position + (index < 5 ? state.LeftHand : state.RightHand).Fingers[index % 5].Deflection + (index % 5);
        }
    }

    [Serializable]
    public struct RobotState
    {
        public RobotHand LeftHand;
        public RobotHand RightHand;
    }

    [Serializable]
    public struct RobotHand
    {
        public RobotFinger[] Fingers;
        public float Position { get; set; }
        public float Velocity { get; set; }
    }

    [Serializable]
    public struct RobotFinger
    {
        public float Deflection;
        public bool bMoving;
    }

    [Serializable]
    public enum RobotStateError
    {
        RSE_HandsCrossover = 1,
        RSE_FingersCrossover = 2,
        RSE_HandsOutOfRange = 3,
        RSE_FingerOutOfRange = 4,
        RSE_InvalidChange = 5,
        RSE_None = 0
    }
}