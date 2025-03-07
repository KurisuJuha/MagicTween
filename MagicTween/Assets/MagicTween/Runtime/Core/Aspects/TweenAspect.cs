using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using MagicTween.Core.Components;

namespace MagicTween.Core
{
    [BurstCompile]
    public readonly partial struct TweenAspect : IAspect
    {
        public readonly Entity entity;
        readonly RefRW<TweenStatus> statusRefRW;

        readonly RefRW<TweenPosition> positionRefRW;
        readonly RefRW<TweenCompletedLoops> completedLoopsRefRW;
        readonly RefRW<TweenProgress> progressRefRW;

        readonly RefRO<TweenParameterDuration> durationRefRO;
        readonly RefRO<TweenParameterDelay> delayRefRW;
        readonly RefRO<TweenParameterLoops> loopsRefRW;
        readonly RefRO<TweenParameterLoopType> loopTypeRefRW;
        readonly RefRO<TweenParameterPlaybackSpeed> playbackSpeedRefRW;

        readonly RefRO<TweenParameterEase> easeRefRO;
        readonly RefRW<TweenParameterCustomEasingCurve> customEasingCurveRefRW;

        readonly RefRO<TweenParameterAutoPlay> autoPlayFlagRefRO;
        readonly RefRO<TweenParameterAutoKill> autoKillFlagRefRO;
        readonly RefRO<TweenParameterIgnoreTimeScale> ignoreTimeScaleFlagRefRO;
        readonly RefRO<TweenParameterIsRelative> isRelativeFlagRefRO;
        readonly RefRW<TweenParameterInvertMode> invertModeRefRW;

        readonly RefRW<TweenInvertFlag> invertedFlagRefRW;
        readonly RefRW<TweenStartedFlag> flagsRefRW;
        readonly RefRW<TweenCallbackFlags> callbackFlagsRefRW;
        readonly RefRW<TweenAccessorFlags> accessorFlagsRefRW;

        public float position
        {
            get => positionRefRW.ValueRO.value;
            set => positionRefRW.ValueRW.value = value;
        }

        public int completedLoops
        {
            get => completedLoopsRefRW.ValueRO.value;
            set => completedLoopsRefRW.ValueRW.value = value;
        }

        public bool started
        {
            get => flagsRefRW.ValueRO.value;
            set => flagsRefRW.ValueRW.value = value;
        }

        public bool inverted
        {
            get => invertedFlagRefRW.ValueRO.value;
            set => invertedFlagRefRW.ValueRW.value = value;
        }

        public TweenStatusType status
        {
            get => statusRefRW.ValueRO.value;
            set => statusRefRW.ValueRW.value = value;
        }

        public float progress
        {
            get => progressRefRW.ValueRO.value;
            set => progressRefRW.ValueRW.value = value;
        }

        public readonly float duration => durationRefRO.ValueRO.value;
        public readonly float delay => delayRefRW.ValueRO.value;
        public readonly int loops => loopsRefRW.ValueRO.value;
        public readonly LoopType loopType => loopTypeRefRW.ValueRO.value;
        public readonly float playbackSpeed => playbackSpeedRefRW.ValueRO.value;

        public readonly bool autoPlay => autoPlayFlagRefRO.ValueRO.value;
        public readonly bool autoKill => autoKillFlagRefRO.ValueRO.value;

        public readonly InvertMode invertMode => invertModeRefRW.ValueRO.value;

        public readonly bool isRelative => isRelativeFlagRefRO.ValueRO.value;
        public readonly bool ignoreTimeScale => ignoreTimeScaleFlagRefRO.ValueRO.value;

        public CallbackFlags callbackFlags
        {
            get => callbackFlagsRefRW.ValueRO.flags;
            set => callbackFlagsRefRW.ValueRW.flags = value;
        }

        public AccessorFlags accessorFlags
        {
            get => accessorFlagsRefRW.ValueRO.flags;
            set => accessorFlagsRefRW.ValueRW.flags = value;
        }

        [BurstCompile]
        public float GetEasedValue(float t)
        {
            if (easeRefRO.ValueRO.value == Ease.Custom) return customEasingCurveRefRW.ValueRW.value.Evaluate(t);
            return EaseUtility.Evaluate(t, easeRefRO.ValueRO.value);
        }

        [BurstCompile]
        public void Update(float currentPosition, ref NativeQueue<Entity>.ParallelWriter parallelWriter)
        {
            UpdateCore(this, currentPosition, ref parallelWriter);
        }

        [BurstCompile]
        public void Kill(ref NativeQueue<Entity>.ParallelWriter parallelWriter)
        {
            status = TweenStatusType.Killed;
            accessorFlags = AccessorFlags.None;
            if (customEasingCurveRefRW.ValueRW.value.IsCreated)
            {
                customEasingCurveRefRW.ValueRW.value.Dispose();
            }
            callbackFlags |= CallbackFlags.OnKill;
            parallelWriter.Enqueue(entity);
        }

        [BurstCompile]
        static void UpdateCore(in TweenAspect aspect, float currentPosition, ref NativeQueue<Entity>.ParallelWriter parallelWriter)
        {
            aspect.callbackFlags = CallbackFlags.None;
            aspect.accessorFlags = AccessorFlags.None;

            switch (aspect.status)
            {
                case TweenStatusType.Invalid:
                case TweenStatusType.Killed:
                    return;
                case TweenStatusType.WaitingForStart:
                    if (!aspect.autoPlay || currentPosition <= 0f) break;

                    aspect.callbackFlags |= CallbackFlags.OnPlay;
                    aspect.callbackFlags |= CallbackFlags.OnStartUp;

                    if (aspect.delay > 0f)
                    {
                        aspect.status = TweenStatusType.Delayed;
                    }
                    else
                    {
                        aspect.status = TweenStatusType.Playing;
                        aspect.callbackFlags |= CallbackFlags.OnStart;
                        aspect.started = true;
                    }
                    break;
                case TweenStatusType.Completed:
                    if (!aspect.autoKill) break;
                    aspect.Kill(ref parallelWriter);
                    return;
            }

            if (aspect.status is not (TweenStatusType.Playing or TweenStatusType.Delayed or TweenStatusType.RewindCompleted or TweenStatusType.Completed)) return;

            // cache parameters  -----------------------------------------------------------------------

            int loops = aspect.loops;
            float duration = aspect.duration;

            // get current progress  -------------------------------------------------------------------

            float currentTime = currentPosition - aspect.delay;
            float currentProgress;

            int prevCompletedLoops = aspect.completedLoops;
            int currentCompletedLoops;
            int clampedCompletedLoops;

            bool isCompleted;

            if (duration == 0f)
            {
                isCompleted = currentTime > 0f;

                if (isCompleted)
                {
                    currentProgress = 1f;
                    currentCompletedLoops = loops;
                }
                else
                {
                    currentProgress = 0f;
                    currentCompletedLoops = currentTime < 0f ? -1 : 0;
                }
                clampedCompletedLoops = loops < 0 ? math.max(0, currentCompletedLoops) : math.clamp(currentCompletedLoops, 0, loops);
            }
            else
            {
                currentCompletedLoops = (int)math.floor(currentTime / aspect.duration);
                clampedCompletedLoops = loops < 0 ? math.max(0, currentCompletedLoops) : math.clamp(currentCompletedLoops, 0, loops);
                isCompleted = loops >= 0 && clampedCompletedLoops > loops - 1;

                if (isCompleted)
                {
                    currentProgress = 1f;
                }
                else
                {
                    float currentLoopTime = currentTime - duration * clampedCompletedLoops;
                    currentProgress = math.clamp(currentLoopTime / duration, 0f, 1f);
                }
            }

            // set position and completedLoops --------------------------------------------------------

            aspect.position = math.max(currentPosition, 0f);
            aspect.completedLoops = currentCompletedLoops;

            switch (aspect.loopType)
            {
                case LoopType.Restart:
                    aspect.progress = aspect.GetEasedValue(currentProgress);
                    break;
                case LoopType.Yoyo:
                    aspect.progress = aspect.GetEasedValue(currentProgress);
                    if ((clampedCompletedLoops + (int)currentProgress) % 2 == 1) aspect.progress = 1f - aspect.progress;
                    break;
                case LoopType.Incremental:
                    aspect.progress = aspect.GetEasedValue(1f) * clampedCompletedLoops + aspect.GetEasedValue(math.fmod(currentProgress, 1f));
                    break;
            }

            // set status & callback flags ----------------------------------------------------------------------

            if (isCompleted)
            {
                if (aspect.status != TweenStatusType.Completed) aspect.callbackFlags |= CallbackFlags.OnComplete;
                aspect.status = TweenStatusType.Completed;
            }
            else
            {
                if (currentTime < 0f)
                {
                    if (aspect.started && currentCompletedLoops <= 0)
                    {
                        if (prevCompletedLoops > currentCompletedLoops)
                        {
                            aspect.callbackFlags |= CallbackFlags.OnRewind;
                            aspect.status = TweenStatusType.RewindCompleted;
                        }
                        else
                        {
                            aspect.started = false;
                            aspect.status = TweenStatusType.WaitingForStart;
                        }
                    }
                    else
                    {
                        aspect.status = TweenStatusType.Delayed;
                    }

                    if (currentPosition >= 0f) aspect.callbackFlags |= CallbackFlags.OnUpdate;
                }
                else
                {
                    if (aspect.status == TweenStatusType.Delayed)
                    {
                        aspect.callbackFlags |= CallbackFlags.OnStart;
                        aspect.started = true;
                    }

                    aspect.status = TweenStatusType.Playing;
                    aspect.callbackFlags |= CallbackFlags.OnUpdate;

                    if (prevCompletedLoops < currentCompletedLoops && currentCompletedLoops > 0)
                    {
                        aspect.callbackFlags |= CallbackFlags.OnStepComplete;
                    }
                }
            }

            // set accessor flags ----------------------------------------------------------------------

            if ((aspect.callbackFlags & CallbackFlags.OnStartUp) == CallbackFlags.OnStartUp)
            {
                aspect.accessorFlags |= AccessorFlags.Getter;
            }
            if ((aspect.callbackFlags & (CallbackFlags.OnUpdate | CallbackFlags.OnComplete | CallbackFlags.OnRewind)) != 0)
            {
                aspect.accessorFlags |= AccessorFlags.Setter;
            }

            // set from  -------------------------------------------------------------------------------

            switch (aspect.invertMode)
            {
                case InvertMode.None:
                    aspect.inverted = false;
                    break;
                case InvertMode.Immediate:
                    aspect.inverted = true;
                    break;
                case InvertMode.AfterDelay:
                    aspect.inverted = currentTime >= 0f;
                    break;
            }
        }
    }
}