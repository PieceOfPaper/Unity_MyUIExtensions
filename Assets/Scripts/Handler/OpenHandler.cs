using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public class OpenHandler : MonoBehaviour
    {
        [SerializeField] private Animator m_Animator;
        [SerializeField] private string m_AnimatorLayer = "Base Layer";
        [SerializeField] private string m_AnimatorOpenState = "Open";
        [SerializeField] private string m_AnimatorCloseState = "Close";

        public Animator animator
        {
            get => m_Animator;
            set
            {
                if (m_Animator == value)
                    return;

                m_Animator = value;
                if (Application.isPlaying == true)
                {
                    if (m_Animator != null && autoDisableAnimator == true) m_Animator.enabled = false;
                    m_AnimatorLayerIndex = m_Animator == null ? 0 : m_Animator.GetLayerIndex(m_AnimatorLayer);
                }
            }
        }
        public string animatorLayer
        {
            get => m_AnimatorLayer;
            set
            {
                if (m_AnimatorLayer == value)
                    return;

                m_AnimatorLayer = value;
                if (Application.isPlaying == true)
                    m_AnimatorLayerIndex = m_Animator == null ? 0 : m_Animator.GetLayerIndex(m_AnimatorLayer);
            }
        }
        public string animatorOpenState
        {
            get => m_AnimatorOpenState;
            set
            {
                if (m_AnimatorOpenState == value)
                    return;

                m_AnimatorOpenState = value;
                if (Application.isPlaying == true)
                    m_AnimatorOpenStateHash = Animator.StringToHash(m_AnimatorOpenState);
            }
        }
        public string animatorCloseState
        {
            get => m_AnimatorCloseState;
            set
            {
                if (m_AnimatorCloseState == value)
                    return;

                m_AnimatorCloseState = value;
                if (Application.isPlaying == true)
                    m_AnimatorCloseStateHash = Animator.StringToHash(m_AnimatorCloseState);
            }
        }
        
        
#if DOTWEEN
        public DG.Tweening.DOTweenAnimation[] doTweenAnimOpen;
        public DG.Tweening.DOTweenAnimation[] doTweenAnimClose;
#endif
        
        
        public float minimumDuration = 0f;
        public float minimumUnscaledDuration = 0f;
        public bool autoDisableAnimator = true;
        public bool openOnStart = false;
        public bool controlActive = true;
        public bool reverseOpenTweenOnClose = false;


        public ChangeStateEvent onChangeState = new ChangeStateEvent();
        [System.Serializable] public class ChangeStateEvent : UnityEvent<State> { }
        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();
        
        
        public enum State
        {
            Unknown = 0,
            Opening,
            Opened,
            Closing,
            Closed,
        }



        private State m_State;
        public State state
        {
            get => m_State;
            private set
            {
                if (m_State == value)
                    return;

                m_State = value;
                onChangeState?.Invoke(m_State);
            }
        }



        private int m_AnimatorOpenStateHash = 0;
        private int m_AnimatorCloseStateHash = 0;
        private int m_AnimatorLayerIndex = 0;

        private Coroutine m_WaitEndRoutine;
        

        private void Awake()
        {
            if (m_Animator != null && autoDisableAnimator == true) m_Animator.enabled = false;
            m_AnimatorOpenStateHash = Animator.StringToHash(m_AnimatorOpenState);
            m_AnimatorCloseStateHash = Animator.StringToHash(m_AnimatorCloseState);
            m_AnimatorLayerIndex = m_Animator == null ? 0 : m_Animator.GetLayerIndex(m_AnimatorLayer);

#if DOTWEEN
            foreach (var tweenAnim in doTweenAnimOpen)
            {
                if (tweenAnim == null) continue;
                tweenAnim.autoPlay = false;
                tweenAnim.autoGenerate = false;
                tweenAnim.autoKill = false;
                tweenAnim.RecreateTween();
            }
            foreach (var tweenAnim in doTweenAnimClose)
            {
                if (tweenAnim == null) continue;
                tweenAnim.autoPlay = false;
                tweenAnim.autoGenerate = false;
                tweenAnim.autoKill = false;
                tweenAnim.RecreateTween();
            }
#endif
        }
        
        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            if (m_WaitEndRoutine != null) StopCoroutine(m_WaitEndRoutine);
            switch (state)
            {
                case State.Opening:
                    OnEndOpen();
                    break;
                case State.Closing:
                    OnEndClose();
                    break;
            }
        }

        private void Start()
        {
            if (openOnStart == true)
                Open();
        }


        public void Open()
        {
            ProcessOpen();
        }

        private void ProcessOpen()
        {
            if (state == State.Opening || state == State.Opened)
                return;
            
            if (controlActive == true)
                gameObject.SetActive(true);

            if (isActiveAndEnabled == false)
            {
                state = State.Opening;
                OnEndOpen();
                return;
            }

            if (m_Animator != null && m_AnimatorOpenStateHash != 0)
            {
                if (autoDisableAnimator == true)
                    m_Animator.enabled = true;
                if (m_Animator.enabled == true)
                    m_Animator.Play(m_AnimatorOpenStateHash, m_AnimatorLayerIndex);
            }
            
#if DOTWEEN
            foreach (var tweenAnim in doTweenAnimOpen)
            {
                if (tweenAnim == null) continue;
                tweenAnim.DORewind();
                tweenAnim.DOPlayForward();
            }
#endif
            
            state = State.Opening;
            if (m_WaitEndRoutine != null) StopCoroutine(m_WaitEndRoutine);
            m_WaitEndRoutine = StartCoroutine(WaitEndRoutine());
        }

        private void OnEndOpen()
        {
            if (m_Animator != null && autoDisableAnimator == true) m_Animator.enabled = false;
            
            state = State.Opened;
            onOpen.Invoke();
        }
        
        
        public void Close()
        {
            ProcessClose();
        }
        
        private void ProcessClose()
        {
            if (state == State.Closing || state == State.Closed)
                return;
            
            if (isActiveAndEnabled == false)
            {
                state = State.Closing;
                OnEndClose();
                return;
            }

            if (m_Animator != null && m_AnimatorCloseStateHash != 0)
            {
                if (autoDisableAnimator == true)
                    m_Animator.enabled = true;
                if (m_Animator.enabled == true)
                    m_Animator.Play(m_AnimatorCloseStateHash, m_AnimatorLayerIndex);
            }
            
#if DOTWEEN
            if (reverseOpenTweenOnClose == true)
            {
                foreach (var tweenAnim in doTweenAnimOpen)
                {
                    if (tweenAnim == null) continue;
                    tweenAnim.DOPlayBackwards();
                }
            }
            foreach (var tweenAnim in doTweenAnimClose)
            {
                if (tweenAnim == null) continue;
                tweenAnim.DORewind();
                tweenAnim.DOPlayForward();
            }
#endif
            
            state = State.Closing;
            if (m_WaitEndRoutine != null) StopCoroutine(m_WaitEndRoutine);
            m_WaitEndRoutine = StartCoroutine(WaitEndRoutine());
        }

        private void OnEndClose()
        {
            if (controlActive == true)
                gameObject.SetActive(false);
            
            if (m_Animator != null && autoDisableAnimator == true) m_Animator.enabled = false;
            
            state = State.Closed;
            onClose.Invoke();
        }


        public void Switch()
        {
            switch (state)
            {
                case State.Opening:
                case State.Opened:
                    Close();
                    break;
                case State.Unknown:
                case State.Closing:
                case State.Closed:
                    Open();
                    break;
            }
        }


        private IEnumerator WaitEndRoutine()
        {
            var duration = 0f;
            var unscaledDuration = 0f;

            duration = Mathf.Max(duration, minimumDuration);
            unscaledDuration = Mathf.Max(duration, minimumUnscaledDuration);
            
            if (m_Animator != null && m_Animator.enabled == true &&
                ((state == State.Opening && m_AnimatorOpenStateHash != 0) || (state == State.Closing && m_AnimatorCloseStateHash != 0)))
            {
                yield return null;
                var stateInfo = m_Animator.GetCurrentAnimatorStateInfo(m_AnimatorLayerIndex);
                if (stateInfo.loop == false)
                {
                    switch (m_Animator.updateMode)
                    {
                        case AnimatorUpdateMode.UnscaledTime:
                            unscaledDuration = Mathf.Max(unscaledDuration, stateInfo.length / stateInfo.speed);
                            break;
                        default:
                            duration = Mathf.Max(duration, stateInfo.length / stateInfo.speed);
                            break;
                    }
                }
            }
            
#if DOTWEEN
            switch (state)
            {
                case State.Opening:
                    foreach (var tweenAnim in doTweenAnimOpen)
                    {
                        if (tweenAnim == null) continue;
                        if (tweenAnim.isIndependentUpdate)
                            unscaledDuration = Mathf.Max(unscaledDuration, tweenAnim.delay + tweenAnim.duration * tweenAnim.loops);
                        else
                            duration = Mathf.Max(duration, tweenAnim.delay + tweenAnim.duration * tweenAnim.loops);
                    }
                    break;
                case State.Closing:
                    if (reverseOpenTweenOnClose == true)
                    {
                        foreach (var tweenAnim in doTweenAnimOpen)
                        {
                            if (tweenAnim == null) continue;
                            if (tweenAnim.isIndependentUpdate)
                                unscaledDuration = Mathf.Max(unscaledDuration, tweenAnim.delay + tweenAnim.duration * tweenAnim.loops);
                            else
                                duration = Mathf.Max(duration, tweenAnim.delay + tweenAnim.duration * tweenAnim.loops);
                        }
                    }
                    foreach (var tweenAnim in doTweenAnimClose)
                    {
                        if (tweenAnim == null) continue;
                        if (tweenAnim.isIndependentUpdate)
                            unscaledDuration = Mathf.Max(unscaledDuration, tweenAnim.delay + tweenAnim.duration * tweenAnim.loops);
                        else
                            duration = Mathf.Max(duration, tweenAnim.delay + tweenAnim.duration * tweenAnim.loops);
                    }
                    break;
            }
#endif

            var startTime = Time.time;
            var startUnscaledTime = Time.unscaledTime;
            while ((Time.time - startTime) < duration || (Time.unscaledTime - startUnscaledTime) < unscaledDuration)
                yield return null;
            yield return null; // 혹시모르니 한프레임 더 추가요

            switch (state)
            {
                case State.Opening:
                    OnEndOpen();
                    break;
                case State.Closing:
                    OnEndClose();
                    break;
            }
        }
    }
}
