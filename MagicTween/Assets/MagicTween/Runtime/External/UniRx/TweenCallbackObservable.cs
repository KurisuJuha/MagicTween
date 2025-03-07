#if MAGICTWEEN_SUPPORT_UNIRX
using System;
using System.Collections.Generic;
using UniRx;

namespace MagicTween
{
    internal sealed class TweenCallbackObservable : IObservable<Unit>
    {
        public enum CallbackType
        {
            OnStart,
            OnPlay,
            OnUpdate,
            OnPause,
            OnStepComplete,
            OnComplete,
            OnKill
        }

        public TweenCallbackObservable(Tween tween, CallbackType callbackType)
        {
            var callbacks = tween.GetOrAddCallbackActions();

            static void OnNext(List<IObserver<Unit>> observers)
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    observers[i].OnNext(Unit.Default);
                }
            }
            static void OnCompleted(List<IObserver<Unit>> observers)
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    observers[i].OnCompleted();
                }
            }

            switch (callbackType)
            {
                case CallbackType.OnStart:
                    callbacks.onStart += () => OnNext(observers);
                    break;
                case CallbackType.OnPlay:
                    callbacks.onPlay += () => OnNext(observers);
                    break;
                case CallbackType.OnUpdate:
                    callbacks.onUpdate += () => OnNext(observers);
                    break;
                case CallbackType.OnPause:
                    callbacks.onPause += () => OnNext(observers);
                    break;
                case CallbackType.OnStepComplete:
                    callbacks.onStepComplete += () => OnNext(observers);
                    break;
                case CallbackType.OnComplete:
                    callbacks.onComplete += () => OnNext(observers);
                    break;
                case CallbackType.OnKill:
                    callbacks.onKill += () =>
                    {
                        OnNext(observers);
                        OnCompleted(observers);
                    };
                    break;
            }

            if (callbackType != CallbackType.OnKill)
            {
                callbacks.onKill += () => OnCompleted(observers);
            }
        }

        readonly List<IObserver<Unit>> observers = new();

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            observers.Add(observer);
            return new Subscription(this, observer);
        }

        sealed class Subscription : IDisposable
        {
            readonly TweenCallbackObservable observable;
            readonly IObserver<Unit> item;

            public Subscription(TweenCallbackObservable observable, IObserver<Unit> item)
            {
                this.observable = observable;
                this.item = item;
            }

            public void Dispose()
            {
                observable.observers.Remove(item);
            }
        }

    }
}
#endif